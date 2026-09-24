using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Nutrition.Dtos;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Nutrition;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Nutrition;

public class NutritionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NutritionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_nutr")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("NutritionCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, string Token)> SetupClientAsync(string coachPrefix = "nutr_coach")
    {
        var token = await RegisterAndLoginCoachAsync(coachPrefix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createClientReq = new CreateClientRequestDto(
            FirstName: "Kareem",
            LastName: "Hassan",
            Email: $"kareem_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201011223355",
            DateOfBirth: new DateTime(1996, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Fat Loss", 16, "Lose body fat"),
            IntakeNotes: "Healthy client.");

        var clientResp = await _client.PostAsJsonAsync("/api/clients", createClientReq);
        clientResp.EnsureSuccessStatusCode();
        var client = await clientResp.Content.ReadFromJsonAsync<ClientDto>();

        return (client!.Id, token);
    }

    [Fact]
    public async Task CalculateTargets_ReturnsDeterministicMifflinStJeorAndHypothesisFlags()
    {
        var (clientId, token) = await SetupClientAsync("calc_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var req = new CalculateNutritionTargetsRequestDto(
            ClientId: clientId,
            WeightKg: 80,
            HeightCm: 180,
            Age: 30,
            Gender: Gender.Male,
            ActivityLevel: ActivityLevel.ModeratelyActive,
            GoalType: NutritionGoalType.Hypertrophy,
            ApplyToProfile: true);

        var response = await _client.PostAsJsonAsync("/api/nutrition/calculate-targets", req);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CalculateNutritionTargetsResponseDto>();
        result.Should().NotBeNull();
        result!.Bmr.Should().Be(1780);
        result.Tdee.Should().Be(2759);
        result.EstimatedCalories.Should().Be(2750);
        result.GoalTargetCalories.Should().Be(3000); // 2750 rounded + 250 = 3000
        result.TargetProteinGrams.Should().Be(160);  // 80 * 2.0
        result.IsHypothesis.Should().BeTrue();
        result.Uncertainty.Should().Contain("±15-25%");

        // Verify applied to profile
        var profile = await _client.GetFromJsonAsync<ClientNutritionProfileDto>($"/api/nutrition/clients/{clientId}");
        profile.Should().NotBeNull();
        profile!.CurrentCalorieTarget.Should().Be(3000);
        profile.CurrentProteinTargetGrams.Should().Be(160);
    }

    [Fact]
    public async Task ProfileLifecycle_CreateGetAndCalibrate_WorksEndToEnd()
    {
        var (clientId, token) = await SetupClientAsync("profile_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Profile
        var createProfileReq = new CreateOrUpdateNutritionProfileRequestDto(
            ClientId: clientId,
            BudgetTier: BudgetTier.Constrained,
            DietaryPreferences: new List<string> { "High Protein" },
            FoodExclusions: new List<string> { "Shellfish" },
            MealsPerDay: 3);

        var profileResp = await _client.PostAsJsonAsync("/api/nutrition/profiles", createProfileReq);
        profileResp.EnsureSuccessStatusCode();

        var profile = await profileResp.Content.ReadFromJsonAsync<ClientNutritionProfileDto>();
        profile.Should().NotBeNull();
        profile!.ClientId.Should().Be(clientId);
        profile.BudgetTier.Should().Be(BudgetTier.Constrained);
        profile.MealsPerDay.Should().Be(3);

        // Set baseline target
        var targetReq = new CalculateNutritionTargetsRequestDto(
            ClientId: clientId,
            WeightKg: 80,
            HeightCm: 180,
            Age: 30,
            Gender: Gender.Male,
            ActivityLevel: ActivityLevel.ModeratelyActive,
            GoalType: NutritionGoalType.FatLoss,
            ApplyToProfile: true);
        await _client.PostAsJsonAsync("/api/nutrition/calculate-targets", targetReq);

        // 2. Get Profile
        var getResp = await _client.GetAsync($"/api/nutrition/clients/{clientId}");
        getResp.EnsureSuccessStatusCode();
        var fetchedProfile = await getResp.Content.ReadFromJsonAsync<ClientNutritionProfileDto>();
        fetchedProfile!.CurrentCalorieTarget.Should().Be(2350); // 2750 - 400

        // 3. Add Calibration Record (Fat loss too slow -> recommendation: Decrease)
        var calibReq = new RecordCalibrationRequestDto(
            WeightKg: 80,
            WeeklyWeightAverages: new List<decimal> { 80.0m, 79.9m },
            GoalType: NutritionGoalType.FatLoss);

        var calibResp = await _client.PostAsJsonAsync($"/api/nutrition/clients/{clientId}/calibrations", calibReq);
        calibResp.EnsureSuccessStatusCode();
        var calibRecord = await calibResp.Content.ReadFromJsonAsync<NutritionCalibrationRecordDto>();
        calibRecord.Should().NotBeNull();
        calibRecord!.AdjustmentRecommendation.Should().Be(AdjustmentRecommendation.Decrease);
        calibRecord.CoachDecision.Should().Be(CalibrationDecision.Pending);

        // 4. Coach Applies Decision and Updates Profile Target
        var decisionReq = new RecordCalibrationDecisionRequestDto(
            Decision: CalibrationDecision.Applied,
            CoachNote: "Decreasing calorie target per calibration feedback.");

        var decisionResp = await _client.PostAsJsonAsync($"/api/nutrition/calibrations/{calibRecord.Id}/decision", decisionReq);
        decisionResp.EnsureSuccessStatusCode();
        var updatedCalib = await decisionResp.Content.ReadFromJsonAsync<NutritionCalibrationRecordDto>();
        updatedCalib!.CoachDecision.Should().Be(CalibrationDecision.Applied);

        // 5. Verify Profile Target Adjusted
        var profileAfterAdj = await _client.GetFromJsonAsync<ClientNutritionProfileDto>($"/api/nutrition/clients/{clientId}");
        profileAfterAdj!.CurrentCalorieTarget.Should().BeLessThan(2350);
    }

    [Fact]
    public async Task TenantIsolation_CoachCannotAccessAnotherCoachsClientNutrition()
    {
        var (clientAId, tokenA) = await SetupClientAsync("coachA_nutr");
        var (clientBId, tokenB) = await SetupClientAsync("coachB_nutr");

        // Set profile for Client A under Coach A
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.PostAsJsonAsync("/api/nutrition/profiles", new CreateOrUpdateNutritionProfileRequestDto(
            ClientId: clientAId,
            BudgetTier: BudgetTier.Flexible));

        // Coach B attempts to access Client A's profile
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var getResp = await _client.GetAsync($"/api/nutrition/clients/{clientAId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Coach B attempts to modify Client A's profile
        var updateResp = await _client.PostAsJsonAsync("/api/nutrition/profiles", new CreateOrUpdateNutritionProfileRequestDto(
            ClientId: clientAId,
            BudgetTier: BudgetTier.Constrained));
        updateResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetEgyptianFoods_ReturnsListWithoutErrors()
    {
        var token = await RegisterAndLoginCoachAsync("food_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.GetAsync("/api/nutrition/foods?budgetTier=1");
        resp.EnsureSuccessStatusCode();
        var foods = await resp.Content.ReadFromJsonAsync<IReadOnlyList<EgyptianFoodDto>>();
        foods.Should().NotBeNull();
    }
}

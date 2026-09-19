using AiCoachOs.Domain.Exercises;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Persistence;

public static class ExerciseLibrarySeeder
{
    // Deterministic GUIDs for relational integrity
    public static class Patterns
    {
        public static readonly Guid Squat = new("11111111-1111-1111-1111-111111111101");
        public static readonly Guid Hinge = new("11111111-1111-1111-1111-111111111102");
        public static readonly Guid HorizontalPush = new("11111111-1111-1111-1111-111111111103");
        public static readonly Guid HorizontalPull = new("11111111-1111-1111-1111-111111111104");
        public static readonly Guid VerticalPush = new("11111111-1111-1111-1111-111111111105");
        public static readonly Guid VerticalPull = new("11111111-1111-1111-1111-111111111106");
        public static readonly Guid KneeExtension = new("11111111-1111-1111-1111-111111111107");
        public static readonly Guid KneeFlexion = new("11111111-1111-1111-1111-111111111108");
        public static readonly Guid ShoulderAbduction = new("11111111-1111-1111-1111-111111111109");
        public static readonly Guid ElbowFlexion = new("11111111-1111-1111-1111-111111111110");
        public static readonly Guid ElbowExtension = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TrunkCore = new("11111111-1111-1111-1111-111111111112");
    }

    public static class Equip
    {
        public static readonly Guid Barbell = new("22222222-2222-2222-2222-222222222201");
        public static readonly Guid Dumbbell = new("22222222-2222-2222-2222-222222222202");
        public static readonly Guid Cable = new("22222222-2222-2222-2222-222222222203");
        public static readonly Guid Machine = new("22222222-2222-2222-2222-222222222204");
        public static readonly Guid Bench = new("22222222-2222-2222-2222-222222222205");
        public static readonly Guid SquatRack = new("22222222-2222-2222-2222-222222222206");
        public static readonly Guid Bodyweight = new("22222222-2222-2222-2222-222222222207");
    }

    public static class Musc
    {
        public static readonly Guid Quadriceps = new("33333333-3333-3333-3333-333333333301");
        public static readonly Guid Hamstrings = new("33333333-3333-3333-3333-333333333302");
        public static readonly Guid GluteusMaximus = new("33333333-3333-3333-3333-333333333303");
        public static readonly Guid PectoralisMajor = new("33333333-3333-3333-3333-333333333304");
        public static readonly Guid LatissimusDorsi = new("33333333-3333-3333-3333-333333333305");
        public static readonly Guid MiddleTrapezius = new("33333333-3333-3333-3333-333333333306");
        public static readonly Guid AnteriorDeltoid = new("33333333-3333-3333-3333-333333333307");
        public static readonly Guid LateralDeltoid = new("33333333-3333-3333-3333-333333333308");
        public static readonly Guid PosteriorDeltoid = new("33333333-3333-3333-3333-333333333309");
        public static readonly Guid BicepsBrachii = new("33333333-3333-3333-3333-333333333310");
        public static readonly Guid TricepsBrachii = new("33333333-3333-3333-3333-333333333311");
        public static readonly Guid RectusAbdominis = new("33333333-3333-3333-3333-333333333312");
    }

    private static readonly SemaphoreSlim _seedLock = new(1, 1);

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await _seedLock.WaitAsync();
        try
        {
            if (await context.MovementPatternsDbSet.AnyAsync())
                return;

        // 1. Movement Patterns
        var patterns = new List<MovementPattern>
        {
            new(Patterns.Squat, "Squat / Knee-Dominant", "Bilateral and unilateral knee and hip flexion movements"),
            new(Patterns.Hinge, "Hip Hinge", "Hip-dominant flexion and extension with minimal knee displacement"),
            new(Patterns.HorizontalPush, "Horizontal Push", "Pushing resistance forward perpendicular to torso"),
            new(Patterns.HorizontalPull, "Horizontal Pull", "Rowing resistance toward torso in horizontal plane"),
            new(Patterns.VerticalPush, "Vertical Push", "Pushing resistance overhead in line with torso"),
            new(Patterns.VerticalPull, "Vertical Pull", "Pulling resistance down toward torso in vertical plane"),
            new(Patterns.KneeExtension, "Knee Extension", "Isolated knee extension around fixed axis"),
            new(Patterns.KneeFlexion, "Knee Flexion", "Isolated knee flexion around fixed axis"),
            new(Patterns.ShoulderAbduction, "Shoulder Abduction", "Lateral humeral abduction in scapular plane"),
            new(Patterns.ElbowFlexion, "Elbow Flexion", "Forearm flexion around elbow axis"),
            new(Patterns.ElbowExtension, "Elbow Extension", "Forearm extension around elbow axis"),
            new(Patterns.TrunkCore, "Trunk / Core", "Sagittal, frontal, or rotational spinal stabilization")
        };
        await context.MovementPatternsDbSet.AddRangeAsync(patterns);

        // 2. Equipment
        var equipmentList = new List<Equipment>
        {
            new(Equip.Barbell, "Barbell", "Free Weights"),
            new(Equip.Dumbbell, "Dumbbell", "Free Weights"),
            new(Equip.Cable, "Cable Machine", "Cables"),
            new(Equip.Machine, "Selectorized / Plate-Loaded Machine", "Machines"),
            new(Equip.Bench, "Adjustable Bench", "Support"),
            new(Equip.SquatRack, "Squat / Power Rack", "Support"),
            new(Equip.Bodyweight, "Bodyweight / Pull-up Bar", "Bodyweight")
        };
        await context.EquipmentDbSet.AddRangeAsync(equipmentList);

        // 3. Muscles
        var muscles = new List<Muscle>
        {
            new(Musc.Quadriceps, "Quadriceps Femoris", "Quads", "Legs"),
            new(Musc.Hamstrings, "Hamstrings", "Hamstrings", "Legs"),
            new(Musc.GluteusMaximus, "Gluteus Maximus", "Glutes", "Hips"),
            new(Musc.PectoralisMajor, "Pectoralis Major", "Chest", "Chest"),
            new(Musc.LatissimusDorsi, "Latissimus Dorsi", "Lats", "Back"),
            new(Musc.MiddleTrapezius, "Trapezius & Rhomboids", "Mid Back", "Back"),
            new(Musc.AnteriorDeltoid, "Anterior Deltoid", "Front Delts", "Shoulders"),
            new(Musc.LateralDeltoid, "Lateral Deltoid", "Side Delts", "Shoulders"),
            new(Musc.PosteriorDeltoid, "Posterior Deltoid", "Rear Delts", "Shoulders"),
            new(Musc.BicepsBrachii, "Biceps Brachii", "Biceps", "Arms"),
            new(Musc.TricepsBrachii, "Triceps Brachii", "Triceps", "Arms"),
            new(Musc.RectusAbdominis, "Rectus Abdominis", "Abs", "Core")
        };
        await context.MusclesDbSet.AddRangeAsync(muscles);
        await context.SaveChangesAsync();

        // 4. Exercises
        var subGroupSquat = Guid.NewGuid();
        var subGroupHinge = Guid.NewGuid();
        var subGroupChestPress = Guid.NewGuid();
        var subGroupHorizPull = Guid.NewGuid();
        var subGroupVertPull = Guid.NewGuid();
        var subGroupSideDelt = Guid.NewGuid();
        var subGroupTriceps = Guid.NewGuid();

        // Ex 1: Barbell Back Squat
        var squatId = new Guid("44444444-4444-4444-4444-444444444401");
        var squat = new Exercise(squatId, "Barbell Back Squat", ExerciseCategory.Compound, Patterns.Squat,
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange,
            "Squat, Back Squat", "Knee extension, hip extension", subGroupSquat);
        squat.AddMuscle(Musc.Quadriceps, true);
        squat.AddMuscle(Musc.GluteusMaximus, false);
        squat.AddEquipment(Equip.Barbell, true);
        squat.AddEquipment(Equip.SquatRack, true);

        // Ex 2: Hack Squat
        var hackSquatId = new Guid("44444444-4444-4444-4444-444444444402");
        var hackSquat = new Exercise(hackSquatId, "Hack Squat Machine", ExerciseCategory.Machine, Patterns.Squat,
            QualitativeRating.High, QualitativeRating.Moderate, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened,
            "Machine Squat", "Knee extension, hip extension", subGroupSquat);
        hackSquat.AddMuscle(Musc.Quadriceps, true);
        hackSquat.AddMuscle(Musc.GluteusMaximus, false);
        hackSquat.AddEquipment(Equip.Machine, true);

        // Ex 3: Romanian Deadlift (Barbell)
        var rdlId = new Guid("44444444-4444-4444-4444-444444444403");
        var rdl = new Exercise(rdlId, "Barbell Romanian Deadlift", ExerciseCategory.Compound, Patterns.Hinge,
            QualitativeRating.Moderate, QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened,
            "RDL, Stiff-Leg Deadlift", "Hip extension with knee fixed", subGroupHinge);
        rdl.AddMuscle(Musc.Hamstrings, true);
        rdl.AddMuscle(Musc.GluteusMaximus, true);
        rdl.AddEquipment(Equip.Barbell, true);

        // Ex 4: Dumbbell Romanian Deadlift
        var dbRdlId = new Guid("44444444-4444-4444-4444-444444444404");
        var dbRdl = new Exercise(dbRdlId, "Dumbbell Romanian Deadlift", ExerciseCategory.Compound, Patterns.Hinge,
            QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened,
            "DB RDL", "Hip extension with knee fixed", subGroupHinge);
        dbRdl.AddMuscle(Musc.Hamstrings, true);
        dbRdl.AddMuscle(Musc.GluteusMaximus, true);
        dbRdl.AddEquipment(Equip.Dumbbell, true);

        // Ex 5: Barbell Bench Press
        var benchId = new Guid("44444444-4444-4444-4444-444444444405");
        var bench = new Exercise(benchId, "Barbell Flat Bench Press", ExerciseCategory.Compound, Patterns.HorizontalPush,
            QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange,
            "Flat Bench, Chest Press", "Shoulder horizontal adduction, elbow extension", subGroupChestPress);
        bench.AddMuscle(Musc.PectoralisMajor, true);
        bench.AddMuscle(Musc.AnteriorDeltoid, false);
        bench.AddMuscle(Musc.TricepsBrachii, false);
        bench.AddEquipment(Equip.Barbell, true);
        bench.AddEquipment(Equip.Bench, true);

        // Ex 6: Dumbbell Flat Bench Press
        var dbBenchId = new Guid("44444444-4444-4444-4444-444444444406");
        var dbBench = new Exercise(dbBenchId, "Dumbbell Flat Bench Press", ExerciseCategory.Compound, Patterns.HorizontalPush,
            QualitativeRating.Low, QualitativeRating.Moderate, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened,
            "DB Bench Press", "Shoulder horizontal adduction, elbow extension", subGroupChestPress);
        dbBench.AddMuscle(Musc.PectoralisMajor, true);
        dbBench.AddMuscle(Musc.AnteriorDeltoid, false);
        dbBench.AddMuscle(Musc.TricepsBrachii, false);
        dbBench.AddEquipment(Equip.Dumbbell, true);
        dbBench.AddEquipment(Equip.Bench, true);

        // Ex 7: Seated Cable Row
        var cableRowId = new Guid("44444444-4444-4444-4444-444444444407");
        var cableRow = new Exercise(cableRowId, "Seated Cable Row", ExerciseCategory.Compound, Patterns.HorizontalPull,
            QualitativeRating.High, QualitativeRating.Low, QualitativeRating.High, QualitativeRating.Low,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange,
            "Cable Row, Horizontal Row", "Shoulder extension/retraction, elbow flexion", subGroupHorizPull);
        cableRow.AddMuscle(Musc.MiddleTrapezius, true);
        cableRow.AddMuscle(Musc.LatissimusDorsi, true);
        cableRow.AddMuscle(Musc.BicepsBrachii, false);
        cableRow.AddEquipment(Equip.Cable, true);

        // Ex 8: Chest-Supported T-Bar Row
        var tbarRowId = new Guid("44444444-4444-4444-4444-444444444408");
        var tbarRow = new Exercise(tbarRowId, "Chest-Supported T-Bar Row", ExerciseCategory.Machine, Patterns.HorizontalPull,
            QualitativeRating.High, QualitativeRating.Low, QualitativeRating.High, QualitativeRating.Low,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange,
            "T-Bar Row", "Shoulder retraction, elbow flexion", subGroupHorizPull);
        tbarRow.AddMuscle(Musc.MiddleTrapezius, true);
        tbarRow.AddMuscle(Musc.LatissimusDorsi, false);
        tbarRow.AddEquipment(Equip.Machine, true);

        // Ex 9: Lat Pulldown
        var latPulldownId = new Guid("44444444-4444-4444-4444-444444444409");
        var latPulldown = new Exercise(latPulldownId, "Lat Pulldown", ExerciseCategory.Compound, Patterns.VerticalPull,
            QualitativeRating.High, QualitativeRating.Low, QualitativeRating.High, QualitativeRating.Low,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened,
            "Cable Pulldown", "Shoulder adduction, elbow flexion", subGroupVertPull);
        latPulldown.AddMuscle(Musc.LatissimusDorsi, true);
        latPulldown.AddMuscle(Musc.BicepsBrachii, false);
        latPulldown.AddEquipment(Equip.Cable, true);

        // Ex 10: Pull-Up
        var pullUpId = new Guid("44444444-4444-4444-4444-444444444410");
        var pullUp = new Exercise(pullUpId, "Pull-Up", ExerciseCategory.Bodyweight, Patterns.VerticalPull,
            QualitativeRating.Low, QualitativeRating.High, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened,
            "Chin-up, Bodyweight Pull-up", "Shoulder adduction, elbow flexion", subGroupVertPull);
        pullUp.AddMuscle(Musc.LatissimusDorsi, true);
        pullUp.AddMuscle(Musc.BicepsBrachii, false);
        pullUp.AddEquipment(Equip.Bodyweight, true);

        // Ex 11: Dumbbell Lateral Raise
        var dbLatRaiseId = new Guid("44444444-4444-4444-4444-444444444411");
        var dbLatRaise = new Exercise(dbLatRaiseId, "Dumbbell Lateral Raise", ExerciseCategory.Isolation, Patterns.ShoulderAbduction,
            QualitativeRating.Low, QualitativeRating.Low, QualitativeRating.Low, QualitativeRating.Low,
            QualitativeRating.Moderate, QualitativeRating.Moderate, ResistanceProfile.Shortened,
            "Side Lateral Raise", "Humeral abduction", subGroupSideDelt);
        dbLatRaise.AddMuscle(Musc.LateralDeltoid, true);
        dbLatRaise.AddEquipment(Equip.Dumbbell, true);

        // Ex 12: Cable Lateral Raise
        var cableLatRaiseId = new Guid("44444444-4444-4444-4444-444444444412");
        var cableLatRaise = new Exercise(cableLatRaiseId, "Cable Lateral Raise", ExerciseCategory.Isolation, Patterns.ShoulderAbduction,
            QualitativeRating.Moderate, QualitativeRating.Low, QualitativeRating.Low, QualitativeRating.Low,
            QualitativeRating.High, QualitativeRating.Moderate, ResistanceProfile.Lengthened,
            "Cable Side Delt Raise", "Humeral abduction", subGroupSideDelt);
        cableLatRaise.AddMuscle(Musc.LateralDeltoid, true);
        cableLatRaise.AddEquipment(Equip.Cable, true);

        // Ex 13: Triceps Cable Pushdown
        var pushdownId = new Guid("44444444-4444-4444-4444-444444444413");
        var pushdown = new Exercise(pushdownId, "Triceps Cable Pushdown", ExerciseCategory.Isolation, Patterns.ElbowExtension,
            QualitativeRating.High, QualitativeRating.Low, QualitativeRating.Low, QualitativeRating.Low,
            QualitativeRating.High, QualitativeRating.Moderate, ResistanceProfile.Shortened,
            "Cable Triceps Pressdown", "Elbow extension", subGroupTriceps);
        pushdown.AddMuscle(Musc.TricepsBrachii, true);
        pushdown.AddEquipment(Equip.Cable, true);

        // Ex 14: Cable Overhead Triceps Extension
        var overheadTricepsId = new Guid("44444444-4444-4444-4444-444444444414");
        var overheadTriceps = new Exercise(overheadTricepsId, "Cable Overhead Triceps Extension", ExerciseCategory.Isolation, Patterns.ElbowExtension,
            QualitativeRating.Moderate, QualitativeRating.Low, QualitativeRating.Low, QualitativeRating.Low,
            QualitativeRating.High, QualitativeRating.Moderate, ResistanceProfile.Lengthened,
            "Overhead Cable Extension", "Elbow extension in shoulder flexion", subGroupTriceps);
        overheadTriceps.AddMuscle(Musc.TricepsBrachii, true);
        overheadTriceps.AddEquipment(Equip.Cable, true);

        // Add substitutions preserving training intent
        squat.AddSubstitution(hackSquatId, "Machine-guided squat preserves quad stimulus with reduced spinal loading and greater stability.");
        hackSquat.AddSubstitution(squatId, "Free-weight bilateral squat matches knee-dominant stimulus with higher axial stability requirement.");

        rdl.AddSubstitution(dbRdlId, "Dumbbell variant preserves lengthened hamstring loading with neutral wrist/arm path.");
        dbRdl.AddSubstitution(rdlId, "Barbell variant preserves lengthened hamstring loading with greater absolute loading capacity.");

        bench.AddSubstitution(dbBenchId, "Dumbbell press preserves horizontal push with greater convergent range of motion.");
        dbBench.AddSubstitution(benchId, "Barbell bench press preserves horizontal push with higher external stability.");

        cableRow.AddSubstitution(tbarRowId, "Chest-supported row preserves horizontal pull with complete spinal deloading.");
        tbarRow.AddSubstitution(cableRowId, "Cable row preserves horizontal pull with customizable handle attachments.");

        latPulldown.AddSubstitution(pullUpId, "Bodyweight vertical pull maintains vertical pull stimulus where bodyweight strength permits.");
        pullUp.AddSubstitution(latPulldownId, "Lat pulldown provides scalable vertical pull load and higher stability.");

        dbLatRaise.AddSubstitution(cableLatRaiseId, "Cable lateral raise shifts resistance profile to lengthened/even across ROM.");
        cableLatRaise.AddSubstitution(dbLatRaiseId, "Dumbbell lateral raise provides convenient alternative when cables are occupied.");

        pushdown.AddSubstitution(overheadTricepsId, "Overhead extension places triceps long head into greater stretch/lengthened bias.");
        overheadTriceps.AddSubstitution(pushdownId, "Pushdown emphasizes shortened range with lower shoulder mobility requirement.");

        await context.ExercisesDbSet.AddRangeAsync(new[]
        {
            squat, hackSquat, rdl, dbRdl, bench, dbBench, cableRow,
            tbarRow, latPulldown, pullUp, dbLatRaise, cableLatRaise, pushdown, overheadTriceps
        });

        await context.SaveChangesAsync();
        }
        finally
        {
            _seedLock.Release();
        }
    }
}

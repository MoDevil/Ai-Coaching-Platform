using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ApplicationValidationException = AiCoachOs.Application.Common.Exceptions.ValidationException;

namespace AiCoachOs.Api.Filters;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> against the action's bound arguments.
/// </summary>
/// <remarks>
/// Validators are discovered from DI by argument type, so registering a validator with
/// <c>AddValidatorsFromAssembly</c> is enough to make it effective. Arguments without a
/// registered validator are passed through untouched.
/// <para>
/// Model binding failures (malformed JSON, wrong primitive types) are handled separately by
/// <c>[ApiController]</c> and are not re-reported here.
/// </para>
/// </remarks>
public sealed class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.Value.GetType());
            var validator = services.GetService(validatorType) as IValidator;
            if (validator is null)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument.Value);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                throw new ApplicationValidationException(result.Errors);
            }
        }

        await next();
    }
}

/// <summary>Registers <see cref="FluentValidationFilter"/> as a global MVC filter.</summary>
public static class FluentValidationFilterExtensions
{
    public static IMvcBuilder AddApplicationValidationFilter(this IMvcBuilder builder)
        => builder.AddMvcOptions(options => options.Filters.Add<FluentValidationFilter>());
}

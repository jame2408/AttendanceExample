using FluentValidation;
using FluentValidation.Results;

namespace Attendance.Api.Helpers;

public interface IValidationService
{
    Task<ValidationResult> ValidateAsync<T>(T request);
}

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ValidationResult> ValidateAsync<T>(T request)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        return await validator!.ValidateAsync(request);
    }
}
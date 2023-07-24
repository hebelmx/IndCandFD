using ConfigDataService;
using FluentValidation;

namespace BlazorApp1.Views;
public class ConfigDataValidator : AbstractValidator<FramesData>
{
    private readonly IConfigDataService _service;

    public ConfigDataValidator(IConfigDataService service)
    {
        _service = service;

        RuleFor(x => x.ID)
            .InclusiveBetween(1, 7)
            .Must(id =>
            {
                // Get command length from service
                if (_service.CommandLengths.TryGetValue(id, out int commandLength))
                {
                    return commandLength >= id.ToString().Length;
                }

                // If command length not found, default to allowing all lengths
                return true;
            })
            .WithMessage(x => $"Value length exceeds command length of {GetCommandLength(x.ID)}");

        RuleFor(x => x.Data)
            .NotEmpty()
            .Matches(@"^[A-Fa-f0-9\s]*$")
            .WithMessage("Invalid data format. Only hexa characters and white spaces allowed.");
    }

    private int GetCommandLength(int id)
    {
        if (_service.CommandLengths.TryGetValue(id, out int commandLength))
        {
            return commandLength;
        }

        // If command length not found, default to allowing all lengths
        return int.MaxValue;
    }
}

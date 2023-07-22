using ConfigDataApp;
using FluentValidation;

public class ConfigDataValidator : AbstractValidator<ConfigData>
{


    private readonly ICommandLengthService _service;


    public ConfigDataValidator(ICommandLengthService service)
    {
        _service = service;

        RuleFor(x => x.ID)
            .InclusiveBetween(1, 7)
            .Must(id =>
            {
                // Get command length from service
                // Get command length from service
                var commandLength = _service.GetCommandLengthAsync(id).GetAwaiter().GetResult();

                return commandLength >= id.ToString().Length;

            })
            .WithMessage(x => $"Value length exceeds command length");

        RuleFor(x => x.Data)
            .NotEmpty()
            .Matches(@"^[A-Fa-f0-9\s]*$")
            .WithMessage("Invalid data format. Only hex characters and white spaces allowed.");
    }

    
}
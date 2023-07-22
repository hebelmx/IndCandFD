using System.ComponentModel.DataAnnotations;
using ConfigDataApp;

namespace BlazorApp1.Views;
public class ConfigDataView
{
    [Required]
    public int ID { get; set; }

    [Required]
    [RegularExpression(@"^[A-Fa-f0-9\s]*$", ErrorMessage = "Only hex characters and spaces are allowed.")]
    [StringLengthFromCommandLength]
    public string Data { get; set; }

    [Required]
    public DateTime DateTime { get; set; }

    public string UserName { get; set; }
}

public class StringLengthFromCommandLength : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Value cannot be null");
        }

        int id = (int)validationContext.ObjectInstance.GetType().GetProperty("ID").GetValue(validationContext.ObjectInstance, null);

        var service = (IConfigDataService)validationContext.GetService(typeof(IConfigDataService));

        if (service == null)
        {
            return new ValidationResult("Could not retrieve IConfigDataService from context");
        }

        if (!service.CommandLengths.TryGetValue(id, out int commandLength))
        {
            return new ValidationResult($"No command length found for ID: {id}");
        }

        if (value.ToString().Length > commandLength)
        {
            return new ValidationResult($"Value length exceeds command length of {commandLength}");
        }

        return ValidationResult.Success;
    }
}
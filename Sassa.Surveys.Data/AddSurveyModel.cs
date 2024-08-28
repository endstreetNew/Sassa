using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sassa.Surveys.Data
{
    public class AddSurveyModel: IValidatableObject
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        // Note for Blazor to validate nested complex type, this annotation is required
        // which is defined in the NuGet package Microsoft.AspNetCore.Components.DataAnnotations.Validation, version 3.2
        // and it is still in "experimental mode"
        // See https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0#nested-models-collection-types-and-complex-types
        //[ValidateComplexType]
        public List<OptionModel> Options { get; init; } = new List<OptionModel>();

        public void RemoveOption(OptionModel option) => this.Options.Remove(option);

        public void AddOption() => this.Options.Add(new OptionModel());

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.Options.Count < 2 )
            {
                yield return new ValidationResult("A survey requires at least 2 options.");
            }
        }
    }

    public class OptionModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int SurveyId { get; set; }
        [MaxLength(150)]
        public string OptionName { get; set; }
        public OptionType optionType { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ZMClassLibrary
{
    /// <summary>
    /// This class validates provice code as per Canadian postal pattern
    /// </summary>
    public class PostalCodeValidation: ValidationAttribute
    {
        public PostalCodeValidation(): base("{0} is not a valid Canadian postal pattern"){} //Overridding the base constructor

        protected override ValidationResult IsValid(object value, System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;
            Regex patter = new Regex(@"^[ABCEGHJKLMNPRSTVXY]\d[A-Z]\s?\d[A-Z]\d$",RegexOptions.IgnoreCase);
            if(patter.IsMatch(value.ToString()))
                return ValidationResult.Success;
            else
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));//return validation result with error message
            
        }
    }
}

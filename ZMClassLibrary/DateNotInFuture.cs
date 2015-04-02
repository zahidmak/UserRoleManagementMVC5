using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMClassLibrary
{
    /// <summary>
    /// This class validates for date not being in future
    /// </summary>
    public class DateNotInFuture : ValidationAttribute
    {
        public DateNotInFuture() : base("{0} cannot be in future") { }//overidding base class constructor

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if ((DateTime)value > DateTime.Now)//checking if date is in future

                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            else
                return ValidationResult.Success;
 
        }
    }
}

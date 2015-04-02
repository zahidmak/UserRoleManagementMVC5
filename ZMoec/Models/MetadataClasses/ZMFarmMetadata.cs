using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZMClassLibrary;

namespace ZMoec.Models
{
    [MetadataType(typeof(ZMFarmMetadata))]
    public partial class farm : IValidatableObject
    {

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //There are two types of yield 
            //   1. yield return 
            //      This will return and add the result to ienumerator                                                         
            //   2. yield break
            //      This will end the iteration
            yield return ValidationResult.Success;

            if ((county == null || county.Trim() == "") && (town == null || town.Trim() == ""))
                yield return new ValidationResult("at least one of town or county must be provided", new string[] { "county", "town" });
            if (provinceCode != null)
                provinceCode = provinceCode.ToUpper();//Changing the province code to upper case
            if (postalCode.Length == 6)//if postal code is provided without space then add space and upper case it.
            {
                postalCode = postalCode.Substring(0, 3).ToUpper() + " " + postalCode.Substring(3, 3).ToUpper();
                yield return ValidationResult.Success;
            }
            else
            {
                postalCode = postalCode.ToUpper();
                yield return ValidationResult.Success;
            }

            if (homePhone != null)//extracting digits irrespective of ')' or '-' and formatting like 222-222-2222 to store in database
            {
                string _tempHomePhone = new String(homePhone.ToCharArray().Where(a => Char.IsDigit(a)).ToArray());
                if (_tempHomePhone.Length == 10)
                {
                    homePhone = _tempHomePhone.Substring(0, 3) + "-" + _tempHomePhone.Substring(3, 3) + "-" + _tempHomePhone.Substring(6, 4);
                }
                else
                {
                    yield return new ValidationResult("home phone must have 10 digits (including area code)", new string[] { "homePhone" });
                }
            }
            if (cellPhone != null)//extracting digits irrespective of ')' or '-' and formatting like 222-222-2222 to store in database
            {
                string _tempCellPhone = new String(cellPhone.ToCharArray().Where(a => Char.IsDigit(a)).ToArray());
                if (_tempCellPhone.Length == 10)
                {
                    cellPhone = _tempCellPhone.Substring(0, 3) + "-" + _tempCellPhone.Substring(3, 3) + "-" + _tempCellPhone.Substring(6, 4);
                }
                else
                {
                    yield return new ValidationResult("cell phone must have 10 digits (including area code)", new string[] { "cellPhone" });
                }
            }
            if (dateJoined > lastContactDate)//last contact date cannot be before date joined
            {
                yield return new ValidationResult("farm cannot be contacted about plots before they have joined the program");
            }
            if (lastContactDate != null && dateJoined == null) //if last contact date is provided then date joined is mandatory
                yield return new ValidationResult("You have provided last contact date. So you must also have date joined");



        }
    }

    public class ZMFarmMetadata
    {
        public int farmId { get; set; }

        [Display(Name = "Farm Name")]
        [Required]
        public string name { get; set; }

        [Display(Name = "Address")]
        public string address { get; set; }

        [Display(Name = "Town")]
        public string town { get; set; }

        [Display(Name = "County")]
        public string county { get; set; }

        [Display(Name = "Province")]
        [Remote("checkProvinceCode", "Farm")]
        public string provinceCode { get; set; }

        [Display(Name = "Postal Code")]
        [Required]
        [PostalCodeValidation]
        public string postalCode { get; set; }

        [Display(Name = "Home Phone")]
        public string homePhone { get; set; }

        [Display(Name = "Cell Phone")]
        public string cellPhone { get; set; }

        [Display(Name = "Directions")]
        public string directions { get; set; }

        [Display(Name = "Date Joined")]
        [DateNotInFuture(ErrorMessage = "{0} cannot be in future")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> dateJoined { get; set; }

        [Display(Name = "Last Contact Date")]
        [DateNotInFuture(ErrorMessage = "{0} cannot be in future")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> lastContactDate { get; set; }
    }
}
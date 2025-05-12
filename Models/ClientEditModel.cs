using System.ComponentModel.DataAnnotations;

namespace FreedomITAS.Models
{
    public class ClientEditModel
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string NumberStreet { get; set; }
        [Required]
        public string City { get; set; }

        [Required]
        public string StateName { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Postcode { get; set; }

        [Required, Phone]
        public string CompanyPhone { get; set; }

        [Required]
        public string CompanyABN { get; set; }

        [Required]
        public string ContactFirstName { get; set; }

        public string? ContactMiddleName { get; set; }

        [Required]
        public string ContactLastName { get; set; }

        [Required, EmailAddress]
        public string ContactEmail { get; set; }

        public string? ContactMobile { get; set; }

       
    }
}


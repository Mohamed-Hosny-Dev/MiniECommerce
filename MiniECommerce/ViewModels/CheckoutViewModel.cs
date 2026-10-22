using Microsoft.AspNetCore.Mvc.ModelBinding;
using MiniECommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace MiniECommerce.ViewModels
{
    public class CheckoutViewModel : IValidatableObject
    {
        public static readonly string[] Governorates =
        {
            "Cairo", "Giza", "Alexandria", "Dakahlia", "Red Sea", "Beheira", "Fayoum", "Gharbia",
            "Ismailia", "Menofia", "Minya", "Qalyubia", "New Valley", "Suez", "Aswan", "Assiut",
            "Beni Suef", "Port Said", "Damietta", "Sharkia", "South Sinai", "Kafr El Sheikh",
            "Matrouh", "Luxor", "Qena", "North Sinai", "Sohag"
        };

        [Required(ErrorMessage = "Please enter the customer name")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the phone number")]
        [RegularExpression(@"^01[0125][0-9]{8}$",
            ErrorMessage = "Enter a valid Egyptian mobile number, e.g. 01012345678")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the governorate")]
        [Display(Name = "Governorate")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the city")]
        [StringLength(60, ErrorMessage = "City must be at most 60 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the address")]
        [StringLength(300, ErrorMessage = "Address must be at most 300 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        // Never trust the cart sent by the browser: it is always re-read from the session.
        [BindNever]
        public CartViewModel Cart { get; set; } = new CartViewModel();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Governorate) && !Governorates.Contains(Governorate))
            {
                yield return new ValidationResult(
                    "Please select a valid governorate.",
                    new[] { nameof(Governorate) });
            }
        }
    }
}

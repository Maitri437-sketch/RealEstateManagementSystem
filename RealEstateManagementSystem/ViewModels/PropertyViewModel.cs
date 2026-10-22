using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstateManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.ViewModels
{
    public class PropertyViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        [Display(Name = "Property Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Property type is required")]
        [Display(Name = "Property Type")]
        public string PropertyType { get; set; } = "House";

        [Required(ErrorMessage = "Listing type is required")]
        [Display(Name = "Listing Type")]
        public string ListingType { get; set; } = "Sale"; // Sale, Rent

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 999999999.99, ErrorMessage = "Please enter a valid price")]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Street Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        [Display(Name = "State")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required")]
        [StringLength(20)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Range(0, 50, ErrorMessage = "Bedrooms must be 0 or more")]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; } = 1;

        [Range(0, 50, ErrorMessage = "Bathrooms must be 0 or more")]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; } = 1;

        [Range(1, 1000000, ErrorMessage = "Area must be at least 1 sq ft")]
        [Display(Name = "Area (Sq Ft)")]
        public double AreaSqFt { get; set; }

        [Display(Name = "Year Built")]
        public int? YearBuilt { get; set; }

        [Display(Name = "Featured Property")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Listing Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Assign Agent")]
        public string? AgentId { get; set; }

        public string? SellerId { get; set; }

        [Display(Name = "Upload Property Images")]
        public List<IFormFile>? Images { get; set; }

        public List<PropertyImage>? ExistingImages { get; set; }

        public IEnumerable<SelectListItem>? Agents { get; set; }
    }
}

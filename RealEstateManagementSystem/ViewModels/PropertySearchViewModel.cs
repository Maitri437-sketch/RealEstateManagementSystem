using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.ViewModels
{
    public class PropertySearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? City { get; set; }
        public string? PropertyType { get; set; }
        public string? ListingType { get; set; } // Sale, Rent, or All
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public string SortBy { get; set; } = "newest"; // newest, price_asc, price_desc, area_desc

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<Property> Properties { get; set; } = new List<Property>();

        // Wishlist property IDs for the current user (to highlight active wishlist hearts in UI)
        public HashSet<int> UserWishlistPropertyIds { get; set; } = new HashSet<int>();

        // Comparison property IDs for the current user
        public HashSet<int> UserComparisonPropertyIds { get; set; } = new HashSet<int>();
    }
}

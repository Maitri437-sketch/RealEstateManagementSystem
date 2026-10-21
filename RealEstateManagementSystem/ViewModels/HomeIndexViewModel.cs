using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Property> FeaturedProperties { get; set; } = new List<Property>();
        public List<Property> LatestProperties { get; set; } = new List<Property>();
        public Dictionary<string, int> CategoryCounts { get; set; } = new Dictionary<string, int>();

        public HashSet<int> UserWishlistIds { get; set; } = new HashSet<int>();
        public HashSet<int> UserComparisonIds { get; set; } = new HashSet<int>();

        // Hero Search form fields
        public string? SearchTerm { get; set; }
        public string? City { get; set; }
        public string? PropertyType { get; set; }
        public string? ListingType { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}

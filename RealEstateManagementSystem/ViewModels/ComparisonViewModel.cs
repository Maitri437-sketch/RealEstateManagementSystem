using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.ViewModels
{
    public class ComparisonViewModel
    {
        public List<Property> Properties { get; set; } = new List<Property>();

        public const int MaxComparisonCount = 4;
        public bool CanAddMore => Properties.Count < MaxComparisonCount;
    }
}

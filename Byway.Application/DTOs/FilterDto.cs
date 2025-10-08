using Byway.Application.Services.Enums;
using Byway.Core.Entities.Enums;

namespace Byway.Application.DTOs
{
    public class FilterDto
    {
        public SortBy SortBy { get; set; } = SortBy.HighestRated;
        public List<InstructorCategory>? Categories{ get; set; }
        public NumOfLectures? NumOfLecturesOption { get; set; }
        public int Rate { get; set; } = 0;
        public double MinimumPrice { get; set; } = 0;
        public double MaximumPrice { get; set; } = double.MaxValue;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }
}

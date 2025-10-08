namespace Byway.Application.DTOs
{
    public class ReceiptDto
    {
        public int Id { get; set; }
        public List<CourseReceiptItemDto> Courses { get; set; } = new List<CourseReceiptItemDto>();
        public double TotalPrice { get; set; }
    }
}

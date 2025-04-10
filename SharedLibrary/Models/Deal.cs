using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class Deal
    {
        [Key]
        public int DealID { get; set; }
        public int? ContactID { get; set; }
        public string? DealName { get; set; }
        public float? Value { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Remarks { get; set; }

        public Contact? Contact { get; set; }
        public ICollection<Task>? Tasks { get; set; }
    }
}

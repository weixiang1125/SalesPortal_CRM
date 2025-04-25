using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class Deal
    {
        [Key]
        public int DealID { get; set; }
        public int? ContactID { get; set; }
        public string? DealName { get; set; }
        public decimal? Value { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string? Remarks { get; set; }

        public Contact? Contact { get; set; }
        public ICollection<Task>? Tasks { get; set; }
    }
}

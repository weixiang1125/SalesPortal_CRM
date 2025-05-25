using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class Task
    {
        [Key]
        public int TaskID { get; set; }
        public int? ContactID { get; set; }
        public int? DealID { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public Contact? Contact { get; set; }
        public Deal? Deal { get; set; }

        public Users? CreatedByUser { get; set; }
        public Users? UpdatedByUser { get; set; }
    }
}

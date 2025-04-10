using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class Task
    {
        [Key]
        public int TaskID { get; set; }
        public int ContactID { get; set; }
        public int DealID { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

        public Contact? Contact { get; set; }
        public Deal? Deal { get; set; }
    }
}

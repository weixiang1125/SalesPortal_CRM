using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Company { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }

        public ICollection<Deal>? Deals { get; set; }
        public ICollection<Task>? Tasks { get; set; }
        public ICollection<ChatChannel>? ChatChannels { get; set; }
    }
}

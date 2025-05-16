using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class ChatChannel
    {
        [Key]
        public int ChannelID { get; set; }
        public int? UserID { get; set; }
        public int? ContactID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        public Users? User { get; set; }
        public Contact? Contact { get; set; }
        public ICollection<ChatMessage>? ChatMessages { get; set; }
    }
}

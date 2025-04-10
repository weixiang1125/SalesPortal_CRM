using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class ChatChannel
    {
        [Key]
        public int ChannelID { get; set; }
        public int UserID { get; set; }
        public int ContactID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        public Users? User { get; set; }
        public Contact? Contact { get; set; }
        public ICollection<ChatMessage>? ChatMessages { get; set; }
    }
}

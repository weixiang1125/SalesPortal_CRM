using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageID { get; set; }
        public int ChannelID { get; set; }
        public string? MessageText { get; set; }
        public string? MessageType { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaMimeType { get; set; }
        public bool? IsSender { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        public ChatChannel? Channel { get; set; }
    }
}

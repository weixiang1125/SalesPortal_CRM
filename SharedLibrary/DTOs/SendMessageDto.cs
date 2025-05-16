public class SendMessageDTO
{
    public string ContactPhone { get; set; } = null!;
    public string MessageText { get; set; } = null!;
    public string? MediaUrl { get; set; }
    public string MessageType { get; set; } = "text";
}

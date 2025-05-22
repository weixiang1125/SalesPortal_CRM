public class ReceiveMessageDTO
{
    public string From { get; set; } = "";           // customer
    public string To { get; set; } = "";             // your WhatsApp number
    public string Message { get; set; } = "";        // the actual message text
    public string MessageType { get; set; } = "text";
    public DateTime? Timestamp { get; set; }        // optional: when it was sent
}

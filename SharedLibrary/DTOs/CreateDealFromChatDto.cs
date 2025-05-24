namespace SharedLibrary.DTOs
{
    public class CreateDealFromChatDto
    {
        public string DealName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Stage { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ExpectedCloseDate { get; set; }
        public string ContactPhone { get; set; } = string.Empty;
    }

}

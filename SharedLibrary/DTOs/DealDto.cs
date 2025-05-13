namespace SharedLibrary.DTOs
{
    public class DealDto
    {
        public int DealID { get; set; }
        public string? DealName { get; set; }
        public decimal? Value { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string? CreatedByUsername { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedByUsername { get; set; }

        public int ContactID { get; set; }
        public string? ContactName { get; set; }
    }
}

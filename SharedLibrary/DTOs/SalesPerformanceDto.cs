namespace SharedLibrary.DTOs
{
    public class SalesPerformanceDto
    {
        public int? UserId { get; set; }
        public decimal TotalSales { get; set; }
        public string Username { get; set; }

        public int DealCount { get; set; }
        public int WonCount { get; set; }
        public int LostCount { get; set; }
        public double SuccessRate { get; set; }
    }

}

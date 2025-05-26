namespace SharedLibrary.DTOs
{
    public class TaskDto
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; } = "";
        public string? TaskDescription { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Not Started";

        public int? ContactID { get; set; }
        public int? DealID { get; set; }

        public string? ContactName { get; set; }
        public string? DealName { get; set; }
    }

}

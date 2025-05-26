namespace SharedLibrary.DTOs
{
    public class SidebarChatItem
    {
        public string Phone { get; set; } = "";
        public string? Text { get; set; }
        public DateTime? Date { get; set; }
        public string Group { get; set; } = "";
        public bool IsActive { get; set; }
        public string? AgentName { get; set; }
        public string? ContactName { get; set; }

    }

}

namespace SharedLibrary.DTOs
{
    public class ContactDto
    {
        public int ContactID { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedByUsername { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedByUsername { get; set; }
    }
}

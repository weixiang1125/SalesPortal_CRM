using SharedLibrary.DTOs;

namespace CRM_Web.Models
{
    public class DealModel
    {
        public List<DealDto> Deals { get; set; } = new();
        public DealDto NewDeal { get; set; } = new();
        public DealDto DealToEdit { get; set; } = new();
    }
}

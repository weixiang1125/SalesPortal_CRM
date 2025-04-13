using SharedLibrary.Models;

namespace CRM_Web.Pages.Deals
{
    public class DealModel
    {
        public List<Deal> Deals { get; set; } = new();
        public Deal NewDeal { get; set; } = new(); // Add this if it's not there
        public Deal DealToEdit { get; set; } = new();
    }
}

namespace Assignment3_Backend.ViewModels
{
    public class CreateProductViewModel
    {
        public string name { get; set; }
        public decimal price { get; set; }
        public int brandId { get; set; }
        public int productTypeId { get; set; }
        public string description { get; set; }
        public IFormFile image { get; set; }
    }
}

using myapplication.Models;

namespace myapplication.Dto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string? ProductNumber{ get; set; }
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public int? ProductModelId { get; set; }
        public string ProductModelName { get; set; }
        public string Description { get; set; }
        public decimal StandardCost { get; set; }
        public decimal ListCost { get; set; }
        public byte[] ProductPhoto { get; set; }
        public string ProductPhotoName { get; set; }

    }
}

namespace myapplication.Dto
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        public string Reviews { get; set; }
        public decimal StandardCost { get; set; }
        public decimal ListCost { get; set; }
        public byte[] ProductPhoto { get; set; }
        public string Location { get; set; }
        public string Shelf { get; set; }
        public int TotalQuantity { get; set; }
    }
}

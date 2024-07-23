namespace myapplication.Dto
{
    public class PaginatedProductsDto
    {
        public List<ProductDto> Products { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

    }
}

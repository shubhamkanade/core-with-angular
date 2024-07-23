using Microsoft.AspNetCore.Mvc;
using myapplication.Dto;
using myapplication.Models;

namespace myapplication.Services
{
    public interface IProductService
    {
        Task<PaginatedProductsDto> GetAllProducts(string? productName,string? productCategory, decimal? minCost, decimal? maxCost, string? sortBy, string? sortDirection, int currentPage, int pageSize);

        Task<ProductDto?> GetProductByID(int id);

        Task<int> AddProduct(AddProductDto obj);

        Task<UpdateProductDto> UpdateProduct(int id, UpdateProductDto obj);

        Task<List<ProductModel>> GetProductModels();

        Task<ProductDetailDto> GetProductDetail(int id);
    }
}

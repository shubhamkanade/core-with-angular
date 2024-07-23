using myapplication.Dto;
using myapplication.Models;

namespace myapplication.Services
{
    public interface ICategoryService
    {
        Task<List<ProductCategory>> GetAllCategories();

        Task<List<ProductSubcategory>> GetAllSubcategories(int productCategoryId);
    }
}

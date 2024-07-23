using Microsoft.EntityFrameworkCore;
using myapplication.Dto;
using myapplication.Models;
using System.Linq;

namespace myapplication.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AdventureWorks2022Context _context;

        public CategoryService(AdventureWorks2022Context context)
        {
            _context = context;
        }

        public async Task<List<ProductCategory>> GetAllCategories()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        public async Task<List<ProductSubcategory>> GetAllSubcategories(int productCategoryId)
        {
            return await _context.ProductSubcategories.Where(x => x.ProductCategoryId == productCategoryId).ToListAsync();
        }
    }
}

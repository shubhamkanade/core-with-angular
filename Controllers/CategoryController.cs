using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using myapplication.Models;
using myapplication.Services;

namespace myapplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService productService)
        {
            _categoryService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductCategory>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategories();
            return Ok(categories);
        }

        [HttpGet("sub")]
        public async Task<ActionResult<List<ProductCategory>>> GetAllSubcategories([FromQuery] int productCategoryId)
        {
            var subCategories = await _categoryService.GetAllSubcategories(productCategoryId);
            return Ok(subCategories);
        }
    }
}

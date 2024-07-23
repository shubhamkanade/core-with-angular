using Microsoft.AspNetCore.Mvc;
using myapplication.Dto;
using myapplication.Services;

namespace myapplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
       
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? productName,[FromQuery] string? productCategory,[FromQuery] decimal? minCost,[FromQuery] decimal? maxCost, [FromQuery] string? sortBy, [FromQuery] string? sortDirection, int currentPage = 1, int pageSize = 10)
        {
            var products = await _productService.GetAllProducts(productName,productCategory,minCost,maxCost, sortBy, sortDirection,currentPage, pageSize);

            if (products == null)
            {
                return NotFound();
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByID(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] AddProductDto addProduct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productId = await _productService.AddProduct(addProduct);

            return CreatedAtAction(nameof(GetProduct), new { id = productId }, productId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto productUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedProduct = await _productService.UpdateProduct(id, productUpdateDto);
            if (updatedProduct == null)
            {
                return NotFound();
            }

            return Ok(updatedProduct);
        }
       
        [HttpGet("productmodel")]
        public async Task<IActionResult> GetProductModel()
        {
            var productModel = await _productService.GetProductModels();

            if (productModel == null)
            {
                return NotFound();
            }

            return Ok(productModel);
        }
        [HttpGet("{id}/productdetail")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var productModel = await _productService.GetProductDetail(id);

            if (productModel == null)
            {
                return NotFound();
            }

            return Ok(productModel);
        }
    }
}

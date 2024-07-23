using Microsoft.EntityFrameworkCore;
using myapplication.Dto;
using myapplication.Models;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace myapplication.Services
{
    public class ProductService : IProductService
    {
        private readonly AdventureWorks2022Context _context;
        public ProductService(AdventureWorks2022Context context)
        {
            _context = context;
        }

        public async Task<PaginatedProductsDto> GetAllProducts(string? productName, string? productCategory, decimal? minCost, decimal? maxCost, string sortBy, string sortDirection, int currentPage, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.ProductSubcategory)
                    .ThenInclude(ps => ps.ProductCategory)
                .Include(p => p.ProductModel)
                    .ThenInclude(pm => pm.ProductModelProductDescriptionCultures)
                        .ThenInclude(pmpdc => pmpdc.ProductDescription)
                .Include(p => p.ProductProductPhotos)
                    .ThenInclude(ppp => ppp.ProductPhoto)
                    .OrderBy(p => p.ProductId);


            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(p => p.Name.Contains(productName)).OrderBy(p => p.ProductId);
            }

            if (!string.IsNullOrEmpty(productCategory))
            {
                query = query.Where(p => p.ProductSubcategory.ProductCategory.Name.Contains(productCategory)).OrderBy(p => p.ProductId);
            }

            if (minCost.HasValue && minCost != 0)
            {
                query = query.Where(p => p.StandardCost >= minCost.Value).OrderBy(p => p.ProductId);
            }

            if (maxCost.HasValue && maxCost != 0)
            {
                query = query.Where(p => p.StandardCost <= maxCost.Value).OrderBy(p => p.ProductId);
            } 

            if (minCost.HasValue && minCost != 0)
            {
                query = query.Where(p => p.ListPrice >= minCost.Value).OrderBy(p => p.ProductId);
            }

            if (maxCost.HasValue && maxCost != 0)
            {
                query = query.Where(p => p.ListPrice <= maxCost.Value).OrderBy(p => p.ProductId);
            }

            switch (sortBy)
            {
                case "productName":
                    query = sortDirection == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                    break;
                case "category":
                    query = sortDirection == "desc" ? query.OrderByDescending(p => p.ProductSubcategory.ProductCategory.Name) : query.OrderBy(p => p.ProductSubcategory.ProductCategory.Name);
                    break;
                case "standardCost":
                    query = sortDirection == "desc" ? query.OrderByDescending(p => p.StandardCost) : query.OrderBy(p => p.StandardCost);
                    break;
                case "listCost":
                    query = sortDirection == "desc" ? query.OrderByDescending(p => p.ListPrice) : query.OrderBy(p => p.ListPrice);
                    break;
                default:
                    query = query.OrderBy(p => p.ProductId);
                    break;
            }

            var productsDto =
                await query.Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    ProductName = p.Name,
                    CategoryId = p.ProductSubcategory.ProductCategoryId,
                    CategoryName = p.ProductSubcategory.ProductCategory.Name,
                    ProductModelId = p.ProductModelId,
                    ProductModelName = p.ProductModel.Name,
                    Description = p.ProductModel.ProductModelProductDescriptionCultures
                                    .FirstOrDefault().ProductDescription.Description,
                    StandardCost = p.StandardCost,
                    ListCost = p.ListPrice,
                    ProductPhoto = p.ProductProductPhotos.FirstOrDefault().ProductPhoto.ThumbNailPhoto,
                    ProductPhotoName = p.ProductProductPhotos.FirstOrDefault().ProductPhoto.ThumbnailPhotoFileName
                })
                .ToListAsync();


            var paginatedProductsDto = new PaginatedProductsDto
            {
                Products = productsDto,
                TotalItems = _context.Products.Count(),
                PageNumber = currentPage,
                PageSize = pageSize
            };

            return paginatedProductsDto;
        }


        public async Task<ProductDto> GetProductByID(int id)
        {
            var productDto = await _context.Products
                .Include(p => p.ProductSubcategory)
                    .ThenInclude(ps => ps.ProductCategory)
                .Include(p => p.ProductModel)
                    .ThenInclude(pm => pm.ProductModelProductDescriptionCultures)
                        .ThenInclude(pmpdc => pmpdc.ProductDescription)
                .Include(p => p.ProductProductPhotos)
                    .ThenInclude(ppp => ppp.ProductPhoto)
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDto
                {
                    ProductName = p.Name,
                    SubCategoryId = p.ProductSubcategoryId,
                    SubCategoryName = p.ProductSubcategory.Name,
                    CategoryId = p.ProductSubcategory.ProductCategoryId,
                    CategoryName = p.ProductSubcategory.ProductCategory.Name,
                    ProductNumber = p.ProductNumber,
                    ProductModelId = p.ProductModelId,
                    ProductModelName = p.ProductModel.Name,
                    Description = p.ProductModel.ProductModelProductDescriptionCultures.OrderByDescending(x => x.ModifiedDate)
                                     .Select(pmpdc => pmpdc.ProductDescription.Description)
                                     .FirstOrDefault(),
                    StandardCost = p.StandardCost,
                    ListCost = p.ListPrice,
                    ProductPhoto = p.ProductProductPhotos
                                        .FirstOrDefault().ProductPhoto.ThumbNailPhoto,
                    ProductPhotoName = p.ProductProductPhotos
                                        .FirstOrDefault().ProductPhoto.ThumbnailPhotoFileName
                })
                .FirstOrDefaultAsync();

            return productDto;
        }


        public async Task<int> AddProduct(AddProductDto createProductDto)
        {
            try
            {
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await createProductDto.ProductPhoto.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var productPhoto = new ProductPhoto
                {
                    ThumbNailPhoto = fileBytes,
                    ModifiedDate = DateTime.Now,
                    ThumbnailPhotoFileName = createProductDto.ProductPhoto.FileName
                };

                var productModelProductDescriptionCulture = new ProductModelProductDescriptionCulture
                {
                    CultureId = "en",
                    ModifiedDate = DateTime.Now,
                    ProductModelId = createProductDto.ProductModelId
                };

                var productDescription =
                    new ProductDescription
                    {
                        Description = createProductDto.Description,
                        ModifiedDate = DateTime.Now,
                        Rowguid = new Guid(),
                        ProductModelProductDescriptionCultures = new List<ProductModelProductDescriptionCulture> { productModelProductDescriptionCulture }
                    };

                var product = new Product
                {
                    ProductNumber = createProductDto.ProductNumber,
                    Name = createProductDto.ProductName,
                    StandardCost = createProductDto.StandardCost,
                    ListPrice = createProductDto.ListCost,
                    SafetyStockLevel = 4,
                    ProductSubcategoryId = createProductDto.SubCategoryId,
                    ReorderPoint = 3,
                    Rowguid = new Guid(),
                    DaysToManufacture = 0,
                    SellStartDate = DateTime.Now,
                    ProductModelId = productModelProductDescriptionCulture.ProductModelId,
                    ModifiedDate = DateTime.Now,
                    ProductProductPhotos = new List<ProductProductPhoto> {
                     new ProductProductPhoto { ProductPhoto  = productPhoto, ModifiedDate = DateTime.Now}
                    }
                };

                _context.Products.Add(product);
                _context.ProductDescriptions.Add(productDescription);
                await _context.SaveChangesAsync();
                return product.ProductId;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                throw;
            }
        }
        public async Task<UpdateProductDto> UpdateProduct(int id, UpdateProductDto obj)
        {
            try
            {
                var product = await _context.Products
                   .Include(p => p.ProductSubcategory)
                       .ThenInclude(ps => ps.ProductCategory)
                   .Include(p => p.ProductModel)
                       .ThenInclude(pm => pm.ProductModelProductDescriptionCultures)
                           .ThenInclude(pmpdc => pmpdc.ProductDescription)
                   .Include(p => p.ProductProductPhotos)
                       .ThenInclude(ppp => ppp.ProductPhoto)
                   .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return null;
                }

                if (obj.ProductPhoto != null)
                {

                    byte[] fileBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await obj.ProductPhoto.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }

                    var productPhoto = product.ProductProductPhotos
                       .OrderByDescending(ppp => ppp.ModifiedDate).Where(p => p.ProductId == id)
                       .FirstOrDefault()?.ProductPhoto;

                    if (productPhoto != null)
                    {
                        productPhoto.ThumbNailPhoto = fileBytes;
                        productPhoto.ModifiedDate = DateTime.Now;
                        productPhoto.ThumbnailPhotoFileName = obj.ProductPhoto.FileName;
                    }
                }

                product.ProductNumber = obj.ProductNumber;
                product.Name = obj.ProductName;
                product.StandardCost = obj.StandardCost;
                product.ListPrice = obj.ListCost;
                product.ModifiedDate = DateTime.Now;
                product.ProductSubcategoryId = obj.SubCategoryId;




                if (product.ProductModel != null)
                {

                    var productModelProductDescriptionCulture = product.ProductModel.ProductModelProductDescriptionCultures.OrderByDescending(x => x.ModifiedDate)
                        .FirstOrDefault(pmpdc => pmpdc.ProductModelId == product.ProductModelId);

                    if (productModelProductDescriptionCulture != null)
                    {
                        var productDescription = productModelProductDescriptionCulture.ProductDescription;
                        if (productDescription != null)
                        {
                            productDescription.Description = obj.Description;
                            productDescription.ModifiedDate = DateTime.Now;
                        }
                    }
                }
                else
                {
                    product.ProductModelId = obj.ProductModelId;
                    var productModelProductDescriptionCulture = new ProductModelProductDescriptionCulture
                    {
                        CultureId = "en",
                        ModifiedDate = DateTime.Now,
                        ProductModelId = obj.ProductModelId
                    };

                    var productDescription =
                        new ProductDescription
                        {
                            Description = obj.Description,
                            ModifiedDate = DateTime.Now,
                            Rowguid = new Guid(),
                            ProductModelProductDescriptionCultures = new List<ProductModelProductDescriptionCulture> { productModelProductDescriptionCulture }
                        };
                    _context.ProductDescriptions.Add(productDescription);
                }

                await _context.SaveChangesAsync();
                return new UpdateProductDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                throw;
            }
        }

        public async Task<List<ProductModel>> GetProductModels()
        {
            return _context.ProductModels.ToList();
        }

        public async Task<ProductDetailDto> GetProductDetail(int id)
        {
            var productDetail = await _context.Products
           .Include(p => p.ProductSubcategory)
          .ThenInclude(ps => ps.ProductCategory)
          .Include(p => p.ProductModel)
              .ThenInclude(pm => pm.ProductModelProductDescriptionCultures)
                  .ThenInclude(pmpdc => pmpdc.ProductDescription)
          .Include(p => p.ProductReviews)
          .Include(p => p.ProductProductPhotos)
              .ThenInclude(ppp => ppp.ProductPhoto)
          .Include(p => p.ProductInventories)
              .ThenInclude(pi => pi.Location)
          .Where(p => p.ProductId == id)
          .Select(p => new ProductDetailDto
          {
              ProductId = p.ProductId,
              ProductName = p.Name,
              Category = p.ProductSubcategory.ProductCategory.Name,
              Description = p.ProductModel.ProductModelProductDescriptionCultures.OrderByDescending(x => x.ModifiedDate)
                              .FirstOrDefault().ProductDescription.Description,
              Reviews = p.ProductReviews.FirstOrDefault().Comments,
              StandardCost = p.StandardCost,
              ListCost = p.ListPrice,
              ProductPhoto = p.ProductProductPhotos
                              .OrderByDescending(ppp => ppp.ModifiedDate)
                              .FirstOrDefault().ProductPhoto.LargePhoto,
              Location = p.ProductInventories
                          .FirstOrDefault().Location.Name,
              Shelf = p.ProductInventories
                        .FirstOrDefault().Shelf,
              TotalQuantity = p.ProductInventories.Sum(pi => pi.Quantity)
          })
          .FirstOrDefaultAsync();

            return productDetail;
        }
    }
}

using Assignment3_Backend.Models;
using Assignment3_Backend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Assignment3_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IRepository _repository;

        public ProductController(IRepository repository)
        {
            _repository = repository;
        }



        [HttpGet]
        [Route("GetProducts")]
        public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetAllProducts()
        {
            var products = await _repository.Query<Product>()
                                    .Include(p => p.Brand)
                                    .Include(p => p.ProductType)
                                    .Where(p => p.IsActive && !p.IsDeleted) // Ensuring we only fetch active, non-deleted products
                                    .Select(p => new ProductViewModel
                                    {
                                        Name = p.Name,
                                        Description = p.Description,
                                        Price = p.Price,
                                        Brand = p.Brand.Name,
                                        ProductType = p.ProductType.Name,
                                        Image = p.Image
                                    })
                                    .ToListAsync();

            if (!products.Any())
            {
                return NotFound("No products found.");
            }

            return Ok(products);
        }


        [HttpPost]
        [Route("AddProduct")]
        public async Task<IActionResult> AddProduct([FromForm] CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var imageBase64 = await ConvertToBase64(model.image);
                var product = new Product
                {
                    Name = model.name,
                    Price = model.price,
                    BrandId = model.brandId,
                    ProductTypeId = model.productTypeId,
                    Description = model.description,
                    Image = imageBase64,
                    DateCreated = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Add(product);
                await _repository.SaveChangesAsync();

                return Ok(new { message = $"{model.name} created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private async Task<string> ConvertToBase64(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

        [HttpGet]
        [Route("Brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _repository.Query<Brand>().Where(b => b.IsActive && !b.IsDeleted).ToListAsync();
            if (!brands.Any())
            {
                return NotFound("No brands found.");
            }
            return Ok(brands);
        }

        [HttpGet]
        [Route("ProductTypes")]
        public async Task<IActionResult> GetProductTypes()
        {
            var productTypes = await _repository.Query<ProductType>().Where(pt => pt.IsActive && !pt.IsDeleted).ToListAsync();
            if (!productTypes.Any())
            {
                return NotFound("No product types found.");
            }
            return Ok(productTypes);
        }

        [HttpGet]
        [Route("ProductCountsByBrand")]
        public async Task<IActionResult> GetProductCountsByBrand()
        {
            var productCounts = await _repository.Query<Product>()
                .Where(p => p.IsActive && !p.IsDeleted)
                .GroupBy(p => p.Brand.Name)
                .Select(group => new { Brand = group.Key, Count = group.Count() })
                .ToListAsync();

            return Ok(productCounts);
        }

        [HttpGet]
        [Route("ProductCountsByProductType")]
        public async Task<IActionResult> GetProductCountsByProductType()
        {
            var productCounts = await _repository.Query<Product>()
                .Where(p => p.IsActive && !p.IsDeleted)
                .GroupBy(p => p.ProductType.Name)
                .Select(group => new { ProductType = group.Key, Count = group.Count() })
                .ToListAsync();

            return Ok(productCounts);
        }

        [HttpGet]
        [Route("ActiveProductsReport")]
        public async Task<IActionResult> GetActiveProductsReport()
        {
            var products = await _repository.Query<Product>()
                .Include(p => p.Brand)  
                .Include(p => p.ProductType)
                .Where(p => p.IsActive && !p.IsDeleted)
                .ToListAsync();

            var groupedData = products
                .GroupBy(p => p.Brand.Name)
                .Select(group => new
                {
                    Brand = group.Key,
                    ProductTypes = group
                        .GroupBy(p => p.ProductType.Name)
                        .Select(subGroup => new
                        {
                            ProductType = subGroup.Key,
                            Products = subGroup.Select(p => new
                            {
                                p.Name,
                                p.Description,
                                p.Price
                            })
                        })
                });

            return Ok(groupedData);
        }

    }
}

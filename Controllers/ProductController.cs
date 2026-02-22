using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductController(IProductRepository productRepository, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{productId:int}", Name ="GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);
            if(product == null) return NotFound($"El producto con el Id {productId} no existe");

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if(createProductDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError","El producto ya existe");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError",$"La categoria con el id {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al guardar el registro. {product.Name}" );
                return StatusCode(500, ModelState);
            }
            var createProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createProduct);
            return CreatedAtRoute("GetProduct", new {productId = product.ProductId}, productDto);
        }

        [HttpGet("searchProductByCategory/{categorytId:int}", Name ="GetProductForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductForCategory(int categorytId)
        {
            var products = _productRepository.GetProductsForCategory(categorytId);
            if(products.Count == 0) return NotFound($"No se encontraron productos con el siguiente Id: {categorytId}.");

            var productDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDto);
        }

        [HttpGet("searchProductByNameDescription/{name}", Name ="SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string name)
        {
            var products = _productRepository.SearchProduct(name);
            if(products.Count == 0) return NotFound($"Los productos con el nombre: {name} no existen.");

            var productDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDto);
        }

    }
}

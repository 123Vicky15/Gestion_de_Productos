using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models.DTO;
using WebApi.Models.Entities;
using WebApi.Tools;
using WebApi.Tools.IRepository;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IProductoRepositorio _productoRepositorio;
        public ProductoController(AppDbContext dbContext, IProductoRepositorio productoRepositorio)
        {
            this.dbContext = dbContext;
            _productoRepositorio = productoRepositorio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetAllProductos([FromQuery] PaginacionParams paginacionParams)
        {
            var productos = await _productoRepositorio.GetAllProductsAsync(paginacionParams);

            return Ok(new
            {
                productos.CurrentPage,
                productos.TotalPages,
                productos.PageSize,
                productos.TotalCount,
                Items = productos
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDTO>> GetProducto(Guid id)
        {

            var producto = await _productoRepositorio.GetProductByIdAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductoDTO>> PostProducto(ProductoCreateDTO productoDTO)
        {
            var producto = await _productoRepositorio.AddProductAsync(productoDTO);

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, productoDTO);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]  
        public async Task<IActionResult> PutProducto(Guid id, ProductoUpdateDTO productoUpdateDTO)
        {
            var result = await _productoRepositorio.UpdateProductAsync(id, productoUpdateDTO);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]  // Solo los usuarios con rol de Admin pueden acceder
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await dbContext.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            dbContext.Productos.Remove(producto);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoExists(Guid id)
        {
            return dbContext.Productos.Any(e => e.Id == id);
        }
    }

}


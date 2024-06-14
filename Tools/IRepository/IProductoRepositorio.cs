using WebApi.Models.DTO;
using WebApi.Models.Entities;

namespace WebApi.Tools.IRepository
{
    public interface IProductoRepositorio
    {
        Task<PagedList<ProductoDTO>> GetAllProductsAsync(PaginacionParams paginacionParams);
        Task<ProductoDTO> GetProductByIdAsync(Guid id);
        Task<ProductoDTO> AddProductAsync(ProductoCreateDTO producto);
        Task<bool> UpdateProductAsync(Guid id, ProductoUpdateDTO productoUpdateDTO);
        Task<IEnumerable<Producto>> SearchProductsAsync(string searchTerm);
        Task<PaginatedList<Producto>> GetProductsPaginatedAsync(int pageIndex, int pageSize);
    }
}

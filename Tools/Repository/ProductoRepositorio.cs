using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebApi.Data;
using WebApi.Models.DTO;
using WebApi.Models.Entities;
using WebApi.Tools.IRepository;

namespace WebApi.Tools.Repository
{
    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly AppDbContext dbContext;

        public ProductoRepositorio(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PagedList<ProductoDTO>>GetAllProductsAsync(PaginacionParams paginacionParams)
        {
            var query = dbContext.Productos.AsQueryable();

            // Aplicar filtro de búsqueda
            if (!string.IsNullOrEmpty(paginacionParams.SearchTerm))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(paginacionParams.SearchTerm) ||
                    p.Descripcion.Contains(paginacionParams.SearchTerm));
            }

            // Seleccionar los campos necesarios para el DTO
            var productoQuery = query.Select(p => new ProductoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                FechaCreacion = p.FechaCreacion,
                FechaActualizacion = p.FechaActualizacion
            });

            return await PagedList<ProductoDTO>.CreateAsync(productoQuery, paginacionParams.PageNumber, paginacionParams.PageSize);
            //var productos = await dbContext.Productos
            //    .Select(p => new ProductoDTO
            //    {
            //        Id = p.Id,
            //        Nombre = p.Nombre,
            //        Descripcion = p.Descripcion,
            //        Precio = p.Precio,
            //        Stock = p.Stock,
            //        FechaCreacion = p.FechaCreacion,
            //        FechaActualizacion = p.FechaActualizacion
            //    })
            //    .ToListAsync();

            //return productos;
        }

        public async Task<ProductoDTO> GetProductByIdAsync(Guid id)
        {
            var producto = await dbContext.Productos
                .Select(p => new ProductoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            return producto;
        }

        public async Task<ProductoDTO> AddProductAsync(ProductoCreateDTO productoCreateDTO)
        {
            var producto = new Producto
            {
                Nombre = productoCreateDTO.Nombre,
                Descripcion = productoCreateDTO.Descripcion,
                Precio = productoCreateDTO.Precio,
                Stock = productoCreateDTO.Stock,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            dbContext.Productos.Add(producto);
            await dbContext.SaveChangesAsync();

            var productoDTO = new ProductoDTO
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                FechaCreacion = producto.FechaCreacion,
                FechaActualizacion = producto.FechaActualizacion
            };

            return productoDTO;
        }

        public async Task<bool> UpdateProductAsync(Guid id, ProductoUpdateDTO productoUpdateDTO)
        {
            var producto = await dbContext.Productos.FindAsync(id);
            if (producto == null)
            {
                return false;
            }

            producto.Nombre = productoUpdateDTO.Nombre;
            producto.Descripcion = productoUpdateDTO.Descripcion;
            producto.Precio = productoUpdateDTO.Precio;
            producto.Stock = productoUpdateDTO.Stock;
            producto.FechaActualizacion = DateTime.Now;

            dbContext.Entry(producto).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
        private bool ProductoExists(Guid id)
        {
            return dbContext.Productos.Any(e => e.Id == id);
        }

        public async Task<IEnumerable<Producto>> SearchProductsAsync(string searchTerm)
        {
            return await dbContext.Productos
                .Where(p => p.Nombre.Contains(searchTerm) || p.Descripcion.Contains(searchTerm))
            .ToListAsync();
        }

        public async Task<PaginatedList<Producto>> GetProductsPaginatedAsync(int pageIndex, int pageSize)
        {
            var query = dbContext.Productos.AsQueryable();
            return await PaginatedList<Producto>.CreateAsync(query, pageIndex, pageSize);
        }
    }
}

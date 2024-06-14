using Microsoft.AspNetCore.Mvc;
using WebApi.Models.DTO;
using WebApi.Models.Entities;

namespace WebApi.Tools.IRepository
{
    public interface IUsuarioRepositorio
    {
        Task<UsuarioDTO> Register(UsuarioRegisterDTO usuarioRegisterDTO);
        Task<UsuarioDTO> GetUsuario(Guid id);
        Task<bool> VerifyPasswordAsync(string nombreUsuario, string password);
        Task<Usuario> GetUsuarioByNombreUsuario(string nombreUsuario);
    }
}

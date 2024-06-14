using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using WebApi.Data;
using WebApi.Models.DTO;
using WebApi.Models.Entities;
using WebApi.Tools.IRepository;

namespace WebApi.Tools.Repository
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly AppDbContext dbContext;
        public UsuarioRepositorio(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<UsuarioDTO> GetUsuario(Guid id)
        {
            var usuario = await dbContext.Usuarios
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    NombreUsuario = u.NombreUsuario,
                    Email = u.Email,
                    Rol = u.Rol
                })
                .FirstOrDefaultAsync(u => u.Id == id);
            return usuario;
        }

        public async Task<UsuarioDTO> Register(UsuarioRegisterDTO usuarioRegisterDTO)
        {
            //var usuario = new Usuario
            //{
            //    NombreUsuario = usuarioRegisterDTO.NombreUsuario,
            //    Contraseña = BCrypt.Net.BCrypt.HashPassword(usuarioRegisterDTO.Contraseña),
            //    Email = usuarioRegisterDTO.Email,
            //    Rol = "Usuario"
            //};

            //byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            //string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            //password: Contraseña!,
            //salt: salt,
            //prf: KeyDerivationPrf.HMACSHA256,
            //iterationCount: 100000,
            //numBytesRequested: 256 / 8));

            //dbContext.Usuarios.Add(usuario);
            //await dbContext.SaveChangesAsync();

            //var usuarioDTO = new UsuarioDTO
            //{
            //    Id = usuario.Id,
            //    NombreUsuario = usuario.NombreUsuario,
            //    Email = usuario.Email,
            //    Rol = usuario.Rol
            //};
            // Generar el salt
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            // Hash de la contraseña
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: usuarioRegisterDTO.Contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Convertir el salt a Base64 para almacenamiento
            string saltBase64 = Convert.ToBase64String(salt);

            var usuario = new Usuario
            {
                NombreUsuario = usuarioRegisterDTO.NombreUsuario,
                Contraseña = hashed,
                Email = usuarioRegisterDTO.Email,
                Rol = "Usuario",
                Salt = saltBase64 // Suponiendo que tienes un campo Salt en la entidad Usuario
            };

            dbContext.Usuarios.Add(usuario);
            await dbContext.SaveChangesAsync();

            var usuarioDTO = new UsuarioDTO
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                Rol = usuario.Rol
            };
            return usuarioDTO;

        }
        public async Task<bool> VerifyPasswordAsync(string nombreUsuario, string password)
        {
            var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
            if (usuario == null) return false;

            return VerifyPassword(password, usuario.Contraseña, usuario.Salt);
        }
        public async Task<Usuario> GetUsuarioByNombreUsuario(string nombreUsuario)
        {
            return await dbContext.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }
        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed == storedHash;
        }
    }
}

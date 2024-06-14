using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Data;
using WebApi.Models.DTO;
using WebApi.Models.Entities;
using WebApi.Tools.IRepository;
using WebApi.Tools.Repository;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public UsuariosController(AppDbContext context, IConfiguration configuration, IUsuarioRepositorio usuarioRepositorio)
        {
            _context = context;
            _configuration = configuration;
            _usuarioRepositorio = usuarioRepositorio;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UsuarioDTO>> Register(UsuarioRegisterDTO usuarioRegisterDTO)
        {

            var usuario = await _usuarioRepositorio.Register(usuarioRegisterDTO);

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuarioRegisterDTO);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UsuarioLoginDTO usuarioLoginDTO)
        {
            var isValidUser = await _usuarioRepositorio.VerifyPasswordAsync(usuarioLoginDTO.NombreUsuario, usuarioLoginDTO.Contraseña);

            if (!isValidUser)
            {
                return Unauthorized("Usuario o contraseña incorrectos");
            }

            var dbUser = await _usuarioRepositorio.GetUsuarioByNombreUsuario(usuarioLoginDTO.NombreUsuario);
            var token = GenerateJwtToken(dbUser);

            return Ok(token);
        }

        //[HttpPost("login")]
        //public async Task<ActionResult<string>> Login(UsuarioLoginDTO usuarioLoginDTO)
        //{
        //    var dbUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuarioLoginDTO.NombreUsuario);

        //    if (dbUser == null || !BCrypt.Net.BCrypt.Verify(usuarioLoginDTO.Contraseña, dbUser.Contraseña))
        //    {
        //        return Unauthorized("Usuario o contraseña incorrectos");
        //    }

        //    var token = GenerateJwtToken(dbUser);

        //    return Ok(token);
        //}

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(Guid id)
        {
            var usuario = await _usuarioRepositorio.GetUsuario(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.NombreUsuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
    


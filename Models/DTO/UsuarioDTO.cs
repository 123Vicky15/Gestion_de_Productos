﻿namespace WebApi.Models.DTO
{
    public class UsuarioDTO
    {
        public Guid Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
    }
}

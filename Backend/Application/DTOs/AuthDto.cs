namespace SistemaDeGestionDeClub.Application.DTOs;

public class LoginDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UsuarioDto Usuario { get; set; } = null!;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
}

public class CrearUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty; // "admin" o "recepcionista"
}

public class ActualizarUsuarioDto
{
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool? EstaActivo { get; set; }
}

public class UsuarioDetalleDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ActualizarPerfilDto
{
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Password { get; set; }
    public string? PasswordActual { get; set; }
}

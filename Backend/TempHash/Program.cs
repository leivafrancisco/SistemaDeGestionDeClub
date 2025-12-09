using BCrypt.Net;

// Genera hash para la contraseña que quieras usar
string password = "admin123";  // Cambia esto por tu contraseña real
string hash = BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine("===========================================");
Console.WriteLine($"Contraseña: {password}");
Console.WriteLine($"Hash BCrypt: {hash}");
Console.WriteLine("===========================================");
Console.WriteLine();
Console.WriteLine("Ejecuta este SQL en tu base de datos:");
Console.WriteLine();
Console.WriteLine($"UPDATE usuarios SET contrasena_hash = '{hash}' WHERE nombre_usuario = 'TU_USUARIO_ADMIN';");
Console.WriteLine();

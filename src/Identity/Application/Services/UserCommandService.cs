using Energix.API.Identity.Domain.Entities;
using Energix.API.Identity.Domain.Repositories;
using Energix.API.Identity.Domain.Services;

namespace Energix.API.Identity.Application.Services;

/// <summary>
/// Application service for User command operations
/// </summary>
public class UserCommandService : IUserCommandService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashingService _hashingService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public UserCommandService(
        IUserRepository userRepository,
        IHashingService hashingService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _hashingService = hashingService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string Message, int? UserId)> SignUpAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string dni,
        string district)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(email))
            return (false, "El email es requerido", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña es requerida", null);

        if (string.IsNullOrWhiteSpace(firstName))
            return (false, "El nombre es requerido", null);

        if (string.IsNullOrWhiteSpace(lastName))
            return (false, "El apellido es requerido", null);

        if (string.IsNullOrWhiteSpace(dni))
            return (false, "El DNI es requerido", null);

        if (string.IsNullOrWhiteSpace(district))
            return (false, "El distrito es requerido", null);

        email = email.Trim().ToLowerInvariant();
        dni = dni.Trim();
        
        if (dni.Length != 8 || !dni.All(char.IsDigit))
            return (false, "El DNI debe tener exactamente 8 dígitos", null);

        // Check if user already exists by email
        if (await _userRepository.ExistsByEmailAsync(email))
            return (false, "Ya existe un usuario con ese email", null);

        // Note: DNI uniqueness is enforced at database level
        // (see UserEntityConfiguration with HasIndex(u => u.Dni).IsUnique())

        // Hash password
        var passwordHash = _hashingService.HashPassword(password);

        // Generate username from email (e.g., "john.doe@example.com" -> "john.doe")
        var username = email.Split('@')[0];

        // Create user
        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Dni = dni,
            District = district.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Usuario registrado exitosamente", user.Id);
    }

    public async Task<(bool Success, string Message, string? Token, object? User)> SignInAsync(
        string email,
        string password)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(email))
            return (false, "El email es requerido", null, null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña es requerida", null, null);

        email = email.Trim().ToLowerInvariant();

        // Find user
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null)
            return (false, "Usuario no encontrado, regístrate primero", null, null);

        // Verify password
        if (string.IsNullOrEmpty(user.PasswordHash))
            return (false, "Credenciales inválidas", null, null);

        // Security: Only accept BCrypt hashed passwords
        // BCrypt hashes always start with "$2a$", "$2b$", "$2x$" or "$2y$"
        if (!user.PasswordHash.StartsWith("$2"))
        {
            // If you need to migrate legacy passwords, create a separate migration endpoint
            // or a background job that rehashes passwords on first successful login.
            // DO NOT allow plain text password comparison in production code.
            return (false, "Formato de contraseña inválido. Contacte al administrador.", null, null);
        }

        bool verified;
        try
        {
            verified = _hashingService.VerifyPassword(password, user.PasswordHash);
        }
        catch
        {
            return (false, "Error al verificar contraseña", null, null);
        }

        if (!verified)
            return (false, "Credenciales inválidas", null, null);

        // Generate token
        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Username, null, null, false);

        var userDto = new
        {
            id = user.Id,
            email = user.Email,
            username = user.Username,
            firstName = user.FirstName,
            lastName = user.LastName,
            dni = user.Dni,
            district = user.District
        };

        return (true, "Login exitoso", token, userDto);
    }
}


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
        string username,
        string firstName,
        string lastName)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(email))
            return (false, "El email es requerido", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña es requerida", null);

        if (string.IsNullOrWhiteSpace(username))
            return (false, "El nombre de usuario es requerido", null);

        email = email.Trim().ToLowerInvariant();
        username = username.Trim();

        // Check if user already exists
        if (await _userRepository.ExistsByEmailAsync(email))
            return (false, "Ya existe un usuario con ese email", null);

        if (await _userRepository.ExistsByUsernameAsync(username))
            return (false, "Ya existe un usuario con ese nombre de usuario", null);

        // Hash password
        var passwordHash = _hashingService.HashPassword(password);

        // Create user
        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
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

        bool verified;
        try
        {
            // Support both BCrypt hashes and plain text (for migration purposes only)
            if (user.PasswordHash.StartsWith("$2"))
            {
                verified = _hashingService.VerifyPassword(password, user.PasswordHash);
            }
            else
            {
                // Plain text comparison (INSECURE - only for migration/testing)
                verified = string.Equals(password, user.PasswordHash, StringComparison.Ordinal);
            }
        }
        catch
        {
            return (false, "Error al verificar contraseña", null, null);
        }

        if (!verified)
            return (false, "Credenciales inválidas", null, null);

        // Generate token
        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Username);

        var userDto = new
        {
            id = user.Id,
            email = user.Email,
            username = user.Username,
            firstName = user.FirstName,
            lastName = user.LastName
        };

        return (true, "Login exitoso", token, userDto);
    }
}


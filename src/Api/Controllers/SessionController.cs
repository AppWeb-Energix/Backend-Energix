using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Interfaces.REST.DTOs;
using Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Controllers;

[ApiController]
[Route("api/v1/session")]
public class SessionController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly ITokenService _tokenService;
    private readonly GetSubscriptionByUserQueryHandler _getSubscriptionQueryHandler;

    public SessionController(
        IUserCommandService userCommandService,
        ITokenService tokenService,
        GetSubscriptionByUserQueryHandler getSubscriptionQueryHandler)
    {
        _userCommandService = userCommandService;
        _tokenService = tokenService;
        _getSubscriptionQueryHandler = getSubscriptionQueryHandler;
    }
    
    /// <summary>
    /// Registra un nuevo usuario y retorna JWT sin información de plan (planSelectionPending=true)
    /// </summary>
    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        if (request == null)
            return BadRequest(new { error = "Request body es requerido" });

        // 2. Registrar usuario
        var (success, message, userId) = await _userCommandService.SignUpAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Dni,
            request.District
        );

        if (!success)
            return BadRequest(new { error = message });

        if (userId == null)
            return StatusCode(500, new { error = "Error al crear usuario" });

        // 3. NO crear suscripción aquí (según context2.md)

        // 4. Intentar hacer SignIn automático para obtener token y user
        var (signInSuccess, _, _, user) = await _userCommandService.SignInAsync(
            request.Email,
            request.Password
        );

        if (!signInSuccess)
        {
            // Si falla el auto-login, responder 201 con mensaje
            return StatusCode(201, new
            {
                id = userId.Value,
                message = "Usuario creado, inicie sesión"
            });
        }

        // Extraer información del usuario
        int userIdFromSignIn;
        string email;
        string username;
        try
        {
            var userDynamic = (dynamic)user!;
            userIdFromSignIn = (int)userDynamic.id;
            email = (string)userDynamic.email;
            username = $"{userDynamic.firstName} {userDynamic.lastName}";
        }
        catch
        {
            return StatusCode(500, new { error = "Error al procesar información del usuario" });
        }

        // 5. Generar token sin claims de plan (planSelectionPending=true)
        var token = _tokenService.GenerateToken(
            userIdFromSignIn,
            email,
            username,
            null, // Sin planType
            null, // Sin billingPeriod
            true  // planSelectionPending = true
        );

        // Responder 201 con payload coherente con session/sign-in
        return StatusCode(201, new
        {
            token,
            user,
            plan = (object?)null,
            planSelectionPending = true,
            message = "Usuario registrado exitosamente"
        });
    }
    
    /// <summary>
    /// Inicia sesión y retorna JWT con información de suscripción si existe
    /// </summary>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest(new { error = "Request body es requerido" });
        
        // Autenticar usuario (sin generar token aún)
        var (success, message, _, user) = await _userCommandService.SignInAsync(
            request.Email,
            request.Password
        );

        if (!success)
            return Unauthorized(new { error = message });
        
        // Extraer información del usuario
        int userId;
        string email;
        string username;
        try
        {
            var userDynamic = (dynamic)user!;
            userId = (int)userDynamic.id;
            email = (string)userDynamic.email;
            username = $"{userDynamic.firstName} {userDynamic.lastName}";
        }
        catch
        {
            return StatusCode(500, new { error = "Error al procesar información del usuario" });
        }

        // Consultar suscripción
        var subscription = await _getSubscriptionQueryHandler.HandleAsync(
            new GetSubscriptionByUserQuery 
            { 
                UserId = userId,
                IncludePaymentMethods = false 
            },
            cancellationToken
        );
        
        // Generar token con o sin información de plan
        string token;
        object? planInfo;
        bool planSelectionPending;

        if (subscription != null)
        {
            // Usuario tiene suscripción activa
            token = _tokenService.GenerateToken(
                userId, 
                email, 
                username, 
                subscription.PlanType, 
                subscription.BillingPeriod,
                false
            );

            planInfo = new
            {
                type = subscription.PlanType,
                uiKey = GetPlanUiKey(subscription.PlanType),
                displayName = subscription.PlanDisplayName,
                billingPeriod = subscription.BillingPeriod
            };

            planSelectionPending = false;
        }
        else
        {
            // Usuario sin suscripción
            token = _tokenService.GenerateToken(
                userId, 
                email, 
                username, 
                null, 
                null,
                true
            );

            planInfo = null;
            planSelectionPending = true;
        }

        return Ok(new
        {
            token,
            user,
            plan = planInfo,
            planSelectionPending
        });
    }
    
    private string GetPlanUiKey(string planType)
    {
        return planType switch
        {
            "Basic" => "basic",
            "Student" => "student",
            "FamilyPremium" => "family",
            _ => "basic"
        };
    }
}

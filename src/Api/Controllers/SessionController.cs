using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Interfaces.REST.DTOs;
using Energix.Subscriptions.Application.Commands.Subscribe;
using Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;
using Energix.Subscriptions.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Controllers;

[ApiController]
[Route("api/v1/session")]
public class SessionController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly GetSubscriptionByUserQueryHandler _getSubscriptionQueryHandler;
    private readonly SubscribeCommandHandler _subscribeCommandHandler;

    public SessionController(
        IUserCommandService userCommandService,
        GetSubscriptionByUserQueryHandler getSubscriptionQueryHandler,
        SubscribeCommandHandler subscribeCommandHandler)
    {
        _userCommandService = userCommandService;
        _getSubscriptionQueryHandler = getSubscriptionQueryHandler;
        _subscribeCommandHandler = subscribeCommandHandler;
    }
    
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest(new { error = "Request body es requerido" });
        
        var (success, message, token, user) = await _userCommandService.SignInAsync(
            request.Email,
            request.Password
        );

        if (!success)
            return Unauthorized(new { error = message });
        
        // Extraer id del objeto anónimo
        int userId;
        try
        {
            var userDynamic = (dynamic)user!;
            userId = (int)userDynamic.id;
        }
        catch
        {
            return StatusCode(500, new { error = "Error al procesar información del usuario" });
        }

        var subscription = await _getSubscriptionQueryHandler.HandleAsync(
            new GetSubscriptionByUserQuery 
            { 
                UserId = userId,
                IncludePaymentMethods = false 
            },
            cancellationToken
        );
        
        var planInfo = subscription != null 
            ? new
            {
                type = subscription.PlanType,
                uiKey = GetPlanUiKey(subscription.PlanType),
                displayName = subscription.PlanDisplayName
            }
            : new
            {
                type = "Basic",
                uiKey = "basic",
                displayName = "Basic Plan"
            };

        return Ok(new
        {
            token,
            user,
            plan = planInfo
        });
    }
    
    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest(new { error = "Request body es requerido" });

        // 1. Registrar usuario (Identity BC)
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
        
        var subscribeCommand = new SubscribeCommand
        {
            UserId = userId.Value,
            PlanType = PlanType.Basic,
            BillingPeriod = BillingPeriod.Monthly
        };

        var (subSuccess, subMessage, _, _) = 
            await _subscribeCommandHandler.HandleAsync(subscribeCommand, cancellationToken);
        
        if (!subSuccess)
        {
            Console.WriteLine($"Warning: No se pudo crear suscripción para usuario {userId}: {subMessage}");
        }
        
        var (signInSuccess, _, token, user) = await _userCommandService.SignInAsync(
            request.Email,
            request.Password
        );

        if (!signInSuccess)
        {
            return Created($"/api/v1/users/{userId}", new 
            { 
                id = userId, 
                message = "Usuario creado exitosamente. Por favor inicie sesión.",
                warning = "No se pudo generar token automáticamente"
            });
        }
        
        // Extraer id del objeto anónimo
        int userIdFromSignIn;
        try
        {
            var userDynamic = (dynamic)user!;
            userIdFromSignIn = (int)userDynamic.id;
        }
        catch
        {
            return StatusCode(500, new { error = "Error al procesar información del usuario" });
        }

        var subscription = await _getSubscriptionQueryHandler.HandleAsync(
            new GetSubscriptionByUserQuery 
            { 
                UserId = userIdFromSignIn,
                IncludePaymentMethods = false 
            },
            cancellationToken
        );
        
        var planInfo = subscription != null 
            ? new
            {
                type = subscription.PlanType,
                uiKey = GetPlanUiKey(subscription.PlanType),
                displayName = subscription.PlanDisplayName
            }
            : new
            {
                type = "Basic",
                uiKey = "basic",
                displayName = "Basic Plan"
            };

        return Created($"/api/v1/users/{userId}", new
        {
            token,
            user,
            plan = planInfo
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

using Energix.API;
using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Infrastructure.Authorization.Extensions;
using Energix.Subscriptions.Application.Commands.CancelPlan;
using Energix.Subscriptions.Application.Commands.ChangePlan;
using Energix.Subscriptions.Application.Commands.RenewPlan;
using Energix.Subscriptions.Application.Commands.Subscribe;
using Energix.Subscriptions.Application.DTOs;
using Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;
using Energix.Subscriptions.Domain.Enums;
using Energix.Subscriptions.Infrastructure.PaymentGateway;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Energix.API.Controllers;

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly GetSubscriptionByUserQueryHandler _getSubscriptionByUserHandler;
    private readonly SubscribeCommandHandler _subscribeHandler;
    private readonly ChangePlanCommandHandler _changePlanHandler;
    private readonly RenewPlanCommandHandler _renewPlanHandler;
    private readonly CancelPlanCommandHandler _cancelPlanHandler;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ITokenService _tokenService;

    public SubscriptionsController(
        AppDbContext context,
        GetSubscriptionByUserQueryHandler getSubscriptionByUserHandler,
        SubscribeCommandHandler subscribeHandler,
        ChangePlanCommandHandler changePlanHandler,
        RenewPlanCommandHandler renewPlanHandler,
        CancelPlanCommandHandler cancelPlanHandler,
        IPaymentGateway paymentGateway,
        ITokenService tokenService)
    {
        _context = context;
        _getSubscriptionByUserHandler = getSubscriptionByUserHandler;
        _subscribeHandler = subscribeHandler;
        _changePlanHandler = changePlanHandler;
        _renewPlanHandler = renewPlanHandler;
        _cancelPlanHandler = cancelPlanHandler;
        _paymentGateway = paymentGateway;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Obtiene la suscripción de un usuario con sus métodos de pago
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionByUser(
        int userId,
        [FromQuery] bool includePaymentMethods = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubscriptionByUserQuery
        {
            UserId = userId,
            IncludePaymentMethods = includePaymentMethods
        };

        var subscription = await _getSubscriptionByUserHandler.HandleAsync(query, cancellationToken);

        if (subscription == null)
            return NotFound(new { message = "Suscripción no encontrada" });

        return Ok(subscription);
    }

    /// <summary>
    /// Mejora o crea una suscripción procesando el pago en una transacción
    /// </summary>
    [HttpPost("upgrade")]
    [ProducesResponseType(typeof(UpgradeSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpgradeSubscription(
        [FromBody] UpgradeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Obtener userId del token JWT
        var userId = HttpContext.GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized(new { error = "Usuario no autenticado" });

        // Validar request
        if (request == null)
            return BadRequest(new ErrorResponse
            {
                Message = "Request inválido",
                Errors = new List<string> { "El cuerpo de la petición es requerido" }
            });

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Obtener información del usuario para el token
            var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            // 2. Verificar si existe suscripción activa
            var existingSubscription = await _getSubscriptionByUserHandler.HandleAsync(
                new GetSubscriptionByUserQuery
                {
                    UserId = userId.Value,
                    IncludePaymentMethods = false
                },
                cancellationToken);

            // 3. Calcular monto a cobrar basado en el plan
            var planSchema = Energix.Subscriptions.Domain.Schemas.PlanSchema.GetPlanSchema(request.PlanType);
            var amount = planSchema.GetPrice(request.BillingPeriod);

            // 4. Procesar pago
            var paymentRequest = new PaymentRequest
            {
                UserId = userId.Value,
                Amount = amount.Amount,
                Currency = amount.Currency,
                CardNumber = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Cvv = request.Cvv,
                Description = existingSubscription == null 
                    ? $"Suscripción a plan {request.PlanType}" 
                    : $"Cambio a plan {request.PlanType}"
            };

            var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

            if (!paymentResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(new ErrorResponse
                {
                    Message = "Error al procesar el pago",
                    Errors = new List<string> { paymentResult.Message, paymentResult.ErrorCode }
                });
            }

            // 5. Crear o cambiar suscripción
            string message;
            if (existingSubscription == null)
            {
                // Crear nueva suscripción
                var subscribeCommand = new SubscribeCommand
                {
                    UserId = userId.Value,
                    PlanType = request.PlanType,
                    BillingPeriod = request.BillingPeriod
                };

                var (success, msg, errors, _) = await _subscribeHandler.HandleAsync(subscribeCommand, cancellationToken);
                
                if (!success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Error al crear suscripción",
                        Errors = errors
                    });
                }
                
                message = msg;
            }
            else
            {
                // Cambiar plan existente
                var changePlanCommand = new ChangePlanCommand
                {
                    UserId = userId.Value,
                    NewPlanType = request.PlanType,
                    NewBillingPeriod = request.BillingPeriod
                };

                var (success, msg, errors) = await _changePlanHandler.HandleAsync(changePlanCommand, cancellationToken);
                
                if (!success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Error al cambiar plan",
                        Errors = errors
                    });
                }
                
                message = msg;
            }

            // 6. Guardar cambios y commit
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 7. Generar JWT actualizado con claims de plan
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", $"{user.FirstName} {user.LastName}"),
                new Claim("planType", request.PlanType.ToString()),
                new Claim("billingPeriod", request.BillingPeriod.ToString()),
                new Claim("planSelectionPending", "false"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = _tokenService.GenerateToken(claims);

            // 8. Preparar respuesta con detalles del plan
            var planUiKey = GetPlanUiKey(request.PlanType);

            return Ok(new UpgradeSubscriptionResponse
            {
                Token = token,
                Plan = new PlanInfo
                {
                    Type = request.PlanType.ToString(),
                    UiKey = planUiKey,
                    DisplayName = planSchema.Name,
                    BillingPeriod = request.BillingPeriod.ToString()
                },
                PlanSelectionPending = false,
                Message = message
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(new ErrorResponse
            {
                Message = "Error al procesar la solicitud",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Cambia el plan de suscripción de un usuario
    /// </summary>
    [HttpPut("change-plan")]
    [ProducesResponseType(typeof(ChangePlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePlan(
        [FromBody] ChangePlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var (success, message, errors) = await _changePlanHandler.HandleAsync(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Message = message,
                Errors = errors
            });
        }

        return Ok(new ChangePlanResponse
        {
            Message = message,
            UserId = command.UserId,
            NewPlanType = command.NewPlanType.ToString()
        });
    }

    /// <summary>
    /// Renueva la suscripción del usuario procesando un pago
    /// </summary>
    [HttpPost("renew")]
    [ProducesResponseType(typeof(RenewPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RenewPlan(
        [FromBody] RenewPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var (success, message, errors) = await _renewPlanHandler.HandleAsync(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Message = message,
                Errors = errors
            });
        }

        return Ok(new RenewPlanResponse
        {
            Message = message,
            UserId = command.UserId
        });
    }

    /// <summary>
    /// Cancela la suscripción del usuario
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(CancelPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelPlan(
        [FromBody] CancelPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var (success, message, errors) = await _cancelPlanHandler.HandleAsync(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Message = message,
                Errors = errors
            });
        }

        return Ok(new CancelPlanResponse
        {
            Message = message,
            UserId = command.UserId
        });
    }

    private string GetPlanUiKey(PlanType planType)
    {
        return planType switch
        {
            PlanType.Basic => "basic",
            PlanType.Student => "student",
            PlanType.FamilyPremium => "family",
            _ => "basic"
        };
    }
}

// DTOs
public class UpgradeSubscriptionRequest
{
    public PlanType PlanType { get; set; }
    public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Monthly;
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Cvv { get; set; } = string.Empty;
}

public class UpgradeSubscriptionResponse
{
    public string Token { get; set; } = string.Empty;
    public PlanInfo Plan { get; set; } = new();
    public bool PlanSelectionPending { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PlanInfo
{
    public string Type { get; set; } = string.Empty;
    public string UiKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
}

public class ChangePlanResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string NewPlanType { get; set; } = string.Empty;
}

public class RenewPlanResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class CancelPlanResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}


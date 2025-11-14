using Energix.Subscriptions.Application.Commands.AddPaymentMethods;
using Energix.Subscriptions.Application.Commands.CancelPlan;
using Energix.Subscriptions.Application.Commands.ChangePlan;
using Energix.Subscriptions.Application.Commands.RemovePaymentMethod;
using Energix.Subscriptions.Application.Commands.RenewPlan;
using Energix.Subscriptions.Application.Commands.SetDefaultPaymentMethod;
using Energix.Subscriptions.Application.Queries.GetPaymentMethodsByUser;
using Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;
using Energix.Subscriptions.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly AddPaymentMethodCommandHandler _addPaymentMethodHandler;
    private readonly GetPaymentMethodsByUserQueryHandler _getPaymentMethodsByUserHandler;
    private readonly GetSubscriptionByUserQueryHandler _getSubscriptionByUserHandler;
    private readonly RemovePaymentMethodCommandHandler _removePaymentMethodHandler;
    private readonly SetDefaultPaymentMethodCommandHandler _setDefaultPaymentMethodHandler;

    public PaymentMethodsController(
        AddPaymentMethodCommandHandler addPaymentMethodHandler,
        GetPaymentMethodsByUserQueryHandler getPaymentMethodsByUserHandler,
        GetSubscriptionByUserQueryHandler getSubscriptionByUserHandler,
        RemovePaymentMethodCommandHandler removePaymentMethodHandler,
        SetDefaultPaymentMethodCommandHandler setDefaultPaymentMethodHandler)
    {
        _addPaymentMethodHandler = addPaymentMethodHandler;
        _getPaymentMethodsByUserHandler = getPaymentMethodsByUserHandler;
        _getSubscriptionByUserHandler = getSubscriptionByUserHandler;
        _removePaymentMethodHandler = removePaymentMethodHandler;
        _setDefaultPaymentMethodHandler = setDefaultPaymentMethodHandler;
    }

    /// <summary>
    /// Añade un nuevo método de pago a la suscripción del usuario
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddPaymentMethodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPaymentMethod(
        [FromBody] AddPaymentMethodCommand command,
        CancellationToken cancellationToken)
    {
        var (success, paymentMethodId, errors) = await _addPaymentMethodHandler.HandleAsync(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Message = "No se pudo añadir el método de pago",
                Errors = errors
            });
        }

        return CreatedAtAction(
            nameof(GetPaymentMethod),
            new { id = paymentMethodId },
            new AddPaymentMethodResponse
            {
                PaymentMethodId = paymentMethodId!.Value,
                Message = "Método de pago añadido correctamente"
            });
    }

    /// <summary>
    /// Obtiene todos los métodos de pago de un usuario
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<PaymentMethodDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethodsByUser(
        Guid userId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentMethodsByUserQuery
        {
            UserId = userId,
            IncludeInactive = includeInactive
        };

        var paymentMethods = await _getPaymentMethodsByUserHandler.HandleAsync(query, cancellationToken);
        return Ok(paymentMethods);
    }

    /// <summary>
    /// Obtiene un método de pago por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentMethod(
        Guid id,
        CancellationToken cancellationToken)
    {
        var paymentMethod = await _getPaymentMethodsByUserHandler.HandleAsync(
            new GetPaymentMethodsByUserQuery { UserId = Guid.Empty },
            cancellationToken);

        var method = paymentMethod.FirstOrDefault(pm => pm.Id == id);
        
        if (method == null)
            return NotFound(new { message = "Método de pago no encontrado" });

        return Ok(method);
    }

    /// <summary>
    /// Elimina (desactiva) un método de pago
    /// </summary>
    [HttpDelete("{paymentMethodId}")]
    [ProducesResponseType(typeof(RemovePaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePaymentMethod(
        Guid paymentMethodId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemovePaymentMethodCommand
        {
            UserId = userId,
            PaymentMethodId = paymentMethodId
        };

        var (success, message, errors) = await _removePaymentMethodHandler.HandleAsync(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Message = message,
                Errors = errors
            });
        }

        return Ok(new RemovePaymentMethodResponse
        {
            Message = message,
            PaymentMethodId = paymentMethodId
        });
    }

    /// <summary>
    /// Establece un método de pago como predeterminado
    /// </summary>
    [HttpPut("{paymentMethodId}/set-default")]
    [ProducesResponseType(typeof(SetDefaultPaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultPaymentMethod(
        Guid paymentMethodId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new SetDefaultPaymentMethodCommand
        {
            UserId = userId,
            PaymentMethodId = paymentMethodId
        };

        var (success, message, errors) = await _setDefaultPaymentMethodHandler.HandleAsync(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Message = message,
                Errors = errors
            });
        }

        return Ok(new SetDefaultPaymentMethodResponse
        {
            Message = message,
            PaymentMethodId = paymentMethodId
        });
    }
}

public class RemovePaymentMethodResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid PaymentMethodId { get; set; }
}

public class SetDefaultPaymentMethodResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid PaymentMethodId { get; set; }
}

[ApiController]
[Route("api/v1/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly GetSubscriptionByUserQueryHandler _getSubscriptionByUserHandler;
    private readonly ChangePlanCommandHandler _changePlanHandler;
    private readonly RenewPlanCommandHandler _renewPlanHandler;
    private readonly CancelPlanCommandHandler _cancelPlanHandler;

    public SubscriptionsController(
        GetSubscriptionByUserQueryHandler getSubscriptionByUserHandler,
        ChangePlanCommandHandler changePlanHandler,
        RenewPlanCommandHandler renewPlanHandler,
        CancelPlanCommandHandler cancelPlanHandler)
    {
        _getSubscriptionByUserHandler = getSubscriptionByUserHandler;
        _changePlanHandler = changePlanHandler;
        _renewPlanHandler = renewPlanHandler;
        _cancelPlanHandler = cancelPlanHandler;
    }

    /// <summary>
    /// Obtiene la suscripción de un usuario con sus métodos de pago
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionByUser(
        Guid userId,
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
            NewPlanType = command.NewPlanType
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
            UserId = command.UserId,
            RenewalMonths = command.RenewalMonths
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
}

public class ChangePlanResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string NewPlanType { get; set; } = string.Empty;
}

public class RenewPlanResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int RenewalMonths { get; set; }
}

public class CancelPlanResponse
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class AddPaymentMethodResponse
{
    public Guid PaymentMethodId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}


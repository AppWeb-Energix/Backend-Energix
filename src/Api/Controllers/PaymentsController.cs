using Energix.Subscriptions.Infrastructure.PaymentGateway;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;

    public PaymentsController(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    /// <summary>
    /// Procesa un pago con tarjeta
    /// </summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ErrorResponse
            {
                Message = "Datos de pago inválidos",
                Errors = errors
            });
        }

        var result = await _paymentGateway.ProcessPaymentAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse
            {
                Message = result.Message,
                Errors = new List<string> { result.ErrorCode }
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Procesa un reembolso
    /// </summary>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRefund(
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentGateway.ProcessRefundAsync(request.TransactionId, request.Amount);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse
            {
                Message = result.Message,
                Errors = new List<string> { result.ErrorCode }
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el estado de una transacción
    /// </summary>
    [HttpGet("transaction/{transactionId}")]
    [ProducesResponseType(typeof(PaymentTransaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentGateway.GetTransactionAsync(transactionId);

        if (transaction == null)
            return NotFound(new { message = "Transacción no encontrada" });

        return Ok(transaction);
    }

    /// <summary>
    /// Obtiene todas las transacciones de un usuario
    /// </summary>
    [HttpGet("user/{userId}/transactions")]
    [ProducesResponseType(typeof(List<PaymentTransaction>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserTransactions(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _paymentGateway.GetUserTransactionsAsync(userId);
        return Ok(transactions);
    }

    /// <summary>
    /// Obtiene el balance de un usuario
    /// </summary>
    [HttpGet("user/{userId}/balance")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserBalance(
        int userId,
        [FromQuery] string currency = "USD",
        CancellationToken cancellationToken = default)
    {
        var balance = await _paymentGateway.GetUserBalanceAsync(userId, currency);
        
        return Ok(new BalanceResponse
        {
            UserId = userId,
            Balance = balance,
            Currency = currency
        });
    }

    /// <summary>
    /// Valida una tarjeta sin procesar pago
    /// </summary>
    [HttpPost("validate-card")]
    [ProducesResponseType(typeof(CardValidationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCard(
        [FromBody] CardValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var isValid = await _paymentGateway.ValidateCardAsync(
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Cvv);

        return Ok(new CardValidationResponse
        {
            IsValid = isValid,
            Message = isValid ? "Tarjeta válida" : "Tarjeta inválida"
        });
    }
}

public class RefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BalanceResponse
{
    public int UserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class CardValidationRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Cvv { get; set; } = string.Empty;
}

public class CardValidationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

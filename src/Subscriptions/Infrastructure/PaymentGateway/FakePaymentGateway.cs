using System.ComponentModel.DataAnnotations;

namespace Energix.Subscriptions.Infrastructure.PaymentGateway;

/// <summary>
/// Gateway de pago simulado para desarrollo y pruebas
/// En producción, reemplazar con Stripe, PayPal, etc.
/// </summary>
public class FakePaymentGateway : IPaymentGateway
{
    private static readonly Dictionary<string, decimal> _balances = new();
    private static readonly List<PaymentTransaction> _transactions = new();

    /// <summary>
    /// Procesa un pago con tarjeta
    /// </summary>
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        await Task.Delay(500); // Simular latencia de red

        // Validar número de tarjeta (simulado)
        if (string.IsNullOrWhiteSpace(request.CardNumber))
        {
            return PaymentResult.Failed("Número de tarjeta inválido");
        }

        // Validación defensiva: evitar month=0 que lanza ArgumentOutOfRange
        if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
        {
            return PaymentResult.Failed("Mes de expiración inválido");
        }

        // Validación defensiva: rango de año razonable
        if (request.ExpiryYear < 1900 || request.ExpiryYear > DateTime.Now.Year + 50)
        {
            return PaymentResult.Failed("Año de expiración inválido");
        }

        // Simular rechazo si el número termina en 0000
        if (request.CardNumber.EndsWith("0000"))
        {
            return PaymentResult.Failed("Pago rechazado por el banco");
        }

        // Simular tarjeta expirada
        var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, 
            DateTime.DaysInMonth(request.ExpiryYear, request.ExpiryMonth));
        
        if (expiryDate < DateTime.Now)
        {
            return PaymentResult.Failed("Tarjeta expirada");
        }

        // Simular fondos insuficientes si el monto es mayor a 10000
        if (request.Amount > 10000)
        {
            return PaymentResult.Failed("Fondos insuficientes");
        }

        // Generar ID de transacción
        var transactionId = Guid.NewGuid().ToString();

        // Registrar transacción
        var transaction = new PaymentTransaction
        {
            TransactionId = transactionId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            CardLast4 = GetLast4Digits(request.CardNumber),
            Status = "Approved",
            ProcessedAt = DateTime.UtcNow,
            Description = request.Description
        };

        _transactions.Add(transaction);

        // Simular actualización de balance
        var key = $"{request.UserId}_{request.Currency}";
        if (_balances.ContainsKey(key))
            _balances[key] += request.Amount;
        else
            _balances[key] = request.Amount;

        return PaymentResult.Success(transactionId, "Pago procesado exitosamente");
    }

    /// <summary>
    /// Procesa un reembolso
    /// </summary>
    public async Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal amount)
    {
        await Task.Delay(300);

        var transaction = _transactions.FirstOrDefault(t => t.TransactionId == transactionId);
        
        if (transaction == null)
        {
            return PaymentResult.Failed("Transacción no encontrada");
        }

        if (transaction.Status == "Refunded")
        {
            return PaymentResult.Failed("Transacción ya fue reembolsada");
        }

        if (amount > transaction.Amount)
        {
            return PaymentResult.Failed("El monto del reembolso excede el monto original");
        }

        var refundId = Guid.NewGuid().ToString();

        var refundTransaction = new PaymentTransaction
        {
            TransactionId = refundId,
            UserId = transaction.UserId,
            Amount = -amount,
            Currency = transaction.Currency,
            CardLast4 = transaction.CardLast4,
            Status = "Refunded",
            ProcessedAt = DateTime.UtcNow,
            Description = $"Reembolso de {transactionId}"
        };

        _transactions.Add(refundTransaction);
        transaction.Status = "Refunded";

        // Actualizar balance
        var key = $"{transaction.UserId}_{transaction.Currency}";
        if (_balances.ContainsKey(key))
            _balances[key] -= amount;

        return PaymentResult.Success(refundId, "Reembolso procesado exitosamente");
    }

    /// <summary>
    /// Verifica el estado de una transacción
    /// </summary>
    public async Task<PaymentTransaction?> GetTransactionAsync(string transactionId)
    {
        await Task.Delay(100);
        return _transactions.FirstOrDefault(t => t.TransactionId == transactionId);
    }

    /// <summary>
    /// Obtiene todas las transacciones de un usuario
    /// </summary>
    public async Task<List<PaymentTransaction>> GetUserTransactionsAsync(Guid userId)
    {
        await Task.Delay(200);
        return _transactions.Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ProcessedAt)
            .ToList();
    }

    /// <summary>
    /// Obtiene el balance de un usuario
    /// </summary>
    public Task<decimal> GetUserBalanceAsync(Guid userId, string currency = "USD")
    {
        var key = $"{userId}_{currency}";
        return Task.FromResult(_balances.GetValueOrDefault(key, 0));
    }

    /// <summary>
    /// Valida una tarjeta sin procesar pago
    /// </summary>
    public async Task<bool> ValidateCardAsync(string cardNumber, int expiryMonth, int expiryYear, string cvv)
    {
        await Task.Delay(200);

        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 13)
            return false;

        if (expiryMonth < 1 || expiryMonth > 12)
            return false;

        if (expiryYear < DateTime.Now.Year)
            return false;

        if (string.IsNullOrWhiteSpace(cvv) || cvv.Length < 3)
            return false;

        var expiryDate = new DateTime(expiryYear, expiryMonth, 
            DateTime.DaysInMonth(expiryYear, expiryMonth));

        return expiryDate >= DateTime.Now;
    }

    /// <summary>
    /// Limpia todas las transacciones (solo para testing)
    /// </summary>
    public static void ClearTransactions()
    {
        _transactions.Clear();
        _balances.Clear();
    }

    private static string GetLast4Digits(string cardNumber)
    {
        var digits = cardNumber.Replace(" ", "").Replace("-", "");
        return digits.Length >= 4 ? digits.Substring(digits.Length - 4) : digits;
    }
}

/// <summary>
/// Interfaz del gateway de pagos
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal amount);
    Task<PaymentTransaction?> GetTransactionAsync(string transactionId);
    Task<List<PaymentTransaction>> GetUserTransactionsAsync(Guid userId);
    Task<decimal> GetUserBalanceAsync(Guid userId, string currency = "USD");
    Task<bool> ValidateCardAsync(string cardNumber, int expiryMonth, int expiryYear, string cvv);
}

/// <summary>
/// Request para procesar un pago
/// </summary>
public class PaymentRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "ExpiryMonth debe estar entre 1 y 12")]
    public int ExpiryMonth { get; set; }

    [Range(1900, 9999, ErrorMessage = "ExpiryYear no es válido")]
    public int ExpiryYear { get; set; }

    public string Cvv { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de un pago
/// </summary>
public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;

    public static PaymentResult Success(string transactionId, string message)
    {
        return new PaymentResult
        {
            IsSuccess = true,
            TransactionId = transactionId,
            Message = message
        };
    }

    public static PaymentResult Failed(string message, string errorCode = "PAYMENT_FAILED")
    {
        return new PaymentResult
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Transacción de pago
/// </summary>
public class PaymentTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CardLast4 { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Approved, Failed, Refunded, Pending
    public DateTime ProcessedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}

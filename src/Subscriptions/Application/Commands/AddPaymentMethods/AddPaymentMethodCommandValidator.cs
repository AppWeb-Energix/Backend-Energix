using System.Text.RegularExpressions;

namespace Energix.Subscriptions.Application.Commands.AddPaymentMethods;

public class AddPaymentMethodCommandValidator
{
    public List<string> Validate(AddPaymentMethodCommand command)
    {
        var errors = new List<string>();

        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (string.IsNullOrWhiteSpace(command.CardNumber))
            errors.Add("El número de tarjeta es requerido");
        else if (!IsValidCreditCard(command.CardNumber))
            errors.Add("El número de tarjeta no es válido");

        if (string.IsNullOrWhiteSpace(command.CardHolderName))
            errors.Add("El nombre del titular es requerido");
        else if (command.CardHolderName.Length > 100)
            errors.Add("El nombre no puede exceder 100 caracteres");

        if (command.ExpiryMonth < 1 || command.ExpiryMonth > 12)
            errors.Add("El mes debe estar entre 1 y 12");

        if (command.ExpiryYear < DateTime.Now.Year)
            errors.Add("La tarjeta no puede estar expirada");

        if (string.IsNullOrWhiteSpace(command.Cvv))
            errors.Add("El CVV es requerido");
        else if (!Regex.IsMatch(command.Cvv, @"^\d{3,4}$"))
            errors.Add("El CVV debe tener 3 o 4 dígitos");

        if (string.IsNullOrWhiteSpace(command.CardBrand))
            errors.Add("La marca de la tarjeta es requerida");
        else if (!new[] { "Visa", "Mastercard", "AmericanExpress", "Discover" }.Contains(command.CardBrand))
            errors.Add("Marca de tarjeta no válida");

        if (IsExpired(command.ExpiryMonth, command.ExpiryYear))
            errors.Add("La tarjeta ha expirado");

        return errors;
    }

    private bool IsValidCreditCard(string cardNumber)
    {
        var digits = cardNumber.Replace(" ", "").Replace("-", "");
        if (!Regex.IsMatch(digits, @"^\d{13,19}$"))
            return false;

        int sum = 0;
        bool alternate = false;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(digits[i].ToString());
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return (sum % 10) == 0;
    }

    private bool IsExpired(int month, int year)
    {
        var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        return expiryDate < DateTime.Now;
    }
}


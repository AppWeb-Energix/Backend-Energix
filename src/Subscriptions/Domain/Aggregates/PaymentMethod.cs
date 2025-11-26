﻿using Energix.Subscriptions.Domain.ValueObjects;

namespace Energix.Subscriptions.Domain.Aggregates;

public class PaymentMethod
{
    public Guid Id { get; private set; }
    public int UserId { get; private set; }
    public MaskedCardNumber MaskedCardNumber { get; private set; }
    public CardBrand CardBrand { get; private set; }
    public string CardHolderName { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PaymentMethod() { }

    private PaymentMethod(
        int userId,
        MaskedCardNumber maskedCardNumber,
        CardBrand cardBrand,
        string cardHolderName,
        DateTime expiryDate,
        bool isDefault)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        MaskedCardNumber = maskedCardNumber;
        CardBrand = cardBrand;
        CardHolderName = cardHolderName;
        ExpiryDate = expiryDate;
        IsDefault = isDefault;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static PaymentMethod Create(
        int userId,
        MaskedCardNumber maskedCardNumber,
        CardBrand cardBrand,
        string cardHolderName,
        DateTime expiryDate,
        bool isDefault = false)
    {
        if (userId <= 0)
            throw new ArgumentException("El UserId debe ser mayor que cero");
        
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException("El nombre del titular es requerido");

        if (expiryDate < DateTime.Now)
            throw new ArgumentException("La tarjeta está expirada");

        return new PaymentMethod(userId, maskedCardNumber, cardBrand, cardHolderName, expiryDate, isDefault);
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsNonDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired() => ExpiryDate < DateTime.Now;
}


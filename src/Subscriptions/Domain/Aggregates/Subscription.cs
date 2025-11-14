﻿using Energix.Subscriptions.Domain.ValueObjects;

namespace Energix.Subscriptions.Domain.Aggregates;

public class Subscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PlanType { get; private set; }
    public Money Price { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private List<PaymentMethod> _paymentMethods = new();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    private Subscription() { }

    private Subscription(Guid userId, string planType, Money price)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PlanType = planType;
        Price = price;
        StartDate = DateTime.UtcNow;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        _paymentMethods = new List<PaymentMethod>();
    }

    public static Subscription Create(Guid userId, string planType, Money price)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El UserId no puede estar vacío");
        
        if (string.IsNullOrWhiteSpace(planType))
            throw new ArgumentException("El tipo de plan es requerido");

        return new Subscription(userId, planType, price);
    }

    public void AddPaymentMethod(PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        if (paymentMethod.UserId != UserId)
            throw new InvalidOperationException("El método de pago no pertenece a este usuario");

        _paymentMethods.Add(paymentMethod);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePaymentMethod(Guid paymentMethodId)
    {
        var method = _paymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
        if (method != null)
        {
            method.Deactivate();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ChangePlan(string newPlanType, Money newPrice)
    {
        if (string.IsNullOrWhiteSpace(newPlanType))
            throw new ArgumentException("El nuevo tipo de plan es requerido");

        PlanType = newPlanType;
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RenewSubscription(DateTime newEndDate)
    {
        if (newEndDate <= DateTime.UtcNow)
            throw new ArgumentException("La nueva fecha de fin debe ser en el futuro");

        EndDate = newEndDate;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


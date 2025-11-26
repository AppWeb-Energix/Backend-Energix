using Energix.Subscriptions.Domain.Enums;
using Energix.Subscriptions.Domain.Schemas;
using Energix.Subscriptions.Domain.ValueObjects;

namespace Energix.Subscriptions.Domain.Aggregates;

public class Subscription
{
    public Guid Id { get; private set; }
    public int UserId { get; private set; }
    public PlanType PlanType { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public Money Price { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public bool IsActive { get; private set; }
    public bool AutoRenew { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private List<PaymentMethod> _paymentMethods = new();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    private Subscription() { }

    private Subscription(
        int userId, 
        PlanType planType, 
        BillingPeriod billingPeriod,
        Money price)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PlanType = planType;
        BillingPeriod = billingPeriod;
        Price = price;
        StartDate = DateTime.UtcNow;
        IsActive = true;
        AutoRenew = true;
        CreatedAt = DateTime.UtcNow;
        _paymentMethods = new List<PaymentMethod>();
        CalculateNextBillingDate();
    }

    public static Subscription Create(
        int userId, 
        PlanType planType, 
        BillingPeriod billingPeriod = BillingPeriod.Monthly)
    {
        if (userId <= 0)
            throw new ArgumentException("El UserId debe ser mayor que cero");

        var planSchema = PlanSchema.GetPlanSchema(planType);
        var price = planSchema.GetPrice(billingPeriod);

        return new Subscription(userId, planType, billingPeriod, price);
    }

    private void CalculateNextBillingDate()
    {
        NextBillingDate = BillingPeriod switch
        {
            BillingPeriod.Monthly => StartDate.AddMonths(1),
            BillingPeriod.Yearly => StartDate.AddYears(1),
            _ => StartDate.AddMonths(1)
        };
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

    public void ChangePlan(PlanType newPlanType, BillingPeriod? newBillingPeriod = null)
    {
        if (PlanType == newPlanType && (newBillingPeriod == null || BillingPeriod == newBillingPeriod))
            throw new InvalidOperationException("El plan seleccionado es el mismo que el actual");

        if (!PlanSchema.CanChangePlan(PlanType, newPlanType))
            throw new InvalidOperationException($"No se puede cambiar del plan {PlanType} al plan {newPlanType}");

        PlanType = newPlanType;
        
        if (newBillingPeriod.HasValue)
            BillingPeriod = newBillingPeriod.Value;

        var planSchema = PlanSchema.GetPlanSchema(newPlanType);
        Price = planSchema.GetPrice(BillingPeriod);
        
        UpdatedAt = DateTime.UtcNow;
        CalculateNextBillingDate();
    }

    public void ChangeBillingPeriod(BillingPeriod newBillingPeriod)
    {
        if (BillingPeriod == newBillingPeriod)
            throw new InvalidOperationException("El periodo de facturación es el mismo");

        BillingPeriod = newBillingPeriod;
        
        var planSchema = PlanSchema.GetPlanSchema(PlanType);
        Price = planSchema.GetPrice(BillingPeriod);
        
        UpdatedAt = DateTime.UtcNow;
        CalculateNextBillingDate();
    }

    public void Cancel()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RenewSubscription()
    {
        if (!IsActive)
        {
            IsActive = true;
            StartDate = DateTime.UtcNow;
        }

        CalculateNextBillingDate();
        EndDate = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAutoRenew(bool autoRenew)
    {
        AutoRenew = autoRenew;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsUpgrade(PlanType targetPlan)
    {
        return PlanSchema.IsUpgrade(PlanType, targetPlan);
    }

    public bool IsDowngrade(PlanType targetPlan)
    {
        return PlanSchema.IsDowngrade(PlanType, targetPlan);
    }

    public PlanSchema GetPlanSchema()
    {
        return PlanSchema.GetPlanSchema(PlanType);
    }

    public bool CanAddDevice(int currentDeviceCount)
    {
        var schema = GetPlanSchema();
        return currentDeviceCount < schema.MaxDevices;
    }

    public int GetRemainingDeviceSlots(int currentDeviceCount)
    {
        var schema = GetPlanSchema();
        if (schema.MaxDevices == int.MaxValue)
            return int.MaxValue;
        
        return Math.Max(0, schema.MaxDevices - currentDeviceCount);
    }
}


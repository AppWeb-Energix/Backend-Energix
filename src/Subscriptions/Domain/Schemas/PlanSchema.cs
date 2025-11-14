using Energix.Subscriptions.Domain.Enums;
using Energix.Subscriptions.Domain.ValueObjects;

namespace Energix.Subscriptions.Domain.Schemas;

/// <summary>
/// Schema que define la configuración de un plan de suscripción
/// </summary>
public class PlanSchema
{
    public PlanType PlanType { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string Description { get; private set; }
    public Money MonthlyPrice { get; private set; }
    public Money YearlyPrice { get; private set; }
    public int MaxDevices { get; private set; }
    public int HistoryDays { get; private set; }
    public bool HasUnlimitedHistory { get; private set; }
    public bool HasConsumptionForecast { get; private set; }
    public bool HasSmartAlerts { get; private set; }
    public bool HasPersonalizedRecommendations { get; private set; }
    public bool HasReportExport { get; private set; }
    public bool AllowsManualDeviceAddition { get; private set; }
    public List<PlanFeature> Features { get; private set; }
    public List<string> IncludedFeatures { get; private set; }

    private PlanSchema(
        PlanType planType,
        string name,
        string displayName,
        string description,
        Money monthlyPrice,
        Money yearlyPrice,
        int maxDevices,
        int historyDays,
        bool hasUnlimitedHistory,
        bool hasConsumptionForecast,
        bool hasSmartAlerts,
        bool hasPersonalizedRecommendations,
        bool hasReportExport,
        bool allowsManualDeviceAddition)
    {
        PlanType = planType;
        Name = name;
        DisplayName = displayName;
        Description = description;
        MonthlyPrice = monthlyPrice;
        YearlyPrice = yearlyPrice;
        MaxDevices = maxDevices;
        HistoryDays = historyDays;
        HasUnlimitedHistory = hasUnlimitedHistory;
        HasConsumptionForecast = hasConsumptionForecast;
        HasSmartAlerts = hasSmartAlerts;
        HasPersonalizedRecommendations = hasPersonalizedRecommendations;
        HasReportExport = hasReportExport;
        AllowsManualDeviceAddition = allowsManualDeviceAddition;
        Features = new List<PlanFeature>();
        IncludedFeatures = new List<string>();
    }

    /// <summary>
    /// Obtiene el schema del plan Basic
    /// </summary>
    public static PlanSchema BasicPlan()
    {
        var schema = new PlanSchema(
            planType: PlanType.Basic,
            name: "Basic",
            displayName: "Basic Plan",
            description: "Increase your device limit",
            monthlyPrice: Money.Create(9.99m, "USD"),
            yearlyPrice: Money.Create(99.99m, "USD"),
            maxDevices: 1,
            historyDays: 7,
            hasUnlimitedHistory: false,
            hasConsumptionForecast: false,
            hasSmartAlerts: false,
            hasPersonalizedRecommendations: false,
            hasReportExport: false,
            allowsManualDeviceAddition: true
        );

        schema.Features = new List<PlanFeature>
        {
            PlanFeature.Create("devices", "Devices: 1", true),
            PlanFeature.Create("history", "History: 7 days", true),
            PlanFeature.Create("consumption_history", "Consumption history (last 7 days)", true),
            PlanFeature.Create("basic_alerts", "Basic alerts (monthly limit)", true),
            PlanFeature.Create("manual_devices", "Add devices manually", true)
        };

        schema.IncludedFeatures = new List<string>
        {
            "Consumption history (last 7 days)",
            "Basic alerts (monthly limit)",
            "Add devices manually"
        };

        return schema;
    }

    /// <summary>
    /// Obtiene el schema del plan Student
    /// </summary>
    public static PlanSchema StudentPlan()
    {
        var schema = new PlanSchema(
            planType: PlanType.Student,
            name: "Student",
            displayName: "Student Plan",
            description: "Everything in Basic Plan",
            monthlyPrice: Money.Create(14.99m, "USD"),
            yearlyPrice: Money.Create(149.99m, "USD"),
            maxDevices: 2,
            historyDays: 90,
            hasUnlimitedHistory: false,
            hasConsumptionForecast: false,
            hasSmartAlerts: true,
            hasPersonalizedRecommendations: true,
            hasReportExport: false,
            allowsManualDeviceAddition: true
        );

        schema.Features = new List<PlanFeature>
        {
            PlanFeature.Create("everything_basic", "Everything in Basic Plan", true),
            PlanFeature.Create("devices", "Devices: 2", true),
            PlanFeature.Create("history", "History: 90 days", true),
            PlanFeature.Create("extended_history", "Extended history (last 3 months)", true),
            PlanFeature.Create("recommendations", "Personalized recommendations", true),
            PlanFeature.Create("smart_alerts", "Smart alerts (unusual consumption, lights left on)", true),
            PlanFeature.Create("manual_devices", "Add up to 2 devices", true)
        };

        schema.IncludedFeatures = new List<string>
        {
            "Everything in Basic Plan",
            "Extended history (last 3 months)",
            "Personalized recommendations",
            "Smart alerts (unusual consumption, lights left on)",
            "Add up to 2 devices"
        };

        return schema;
    }

    /// <summary>
    /// Obtiene el schema del plan Family Premium
    /// </summary>
    public static PlanSchema FamilyPremiumPlan()
    {
        var schema = new PlanSchema(
            planType: PlanType.FamilyPremium,
            name: "FamilyPremium",
            displayName: "Family Plan (Premium)",
            description: "Everything in Student Plan",
            monthlyPrice: Money.Create(24.99m, "USD"),
            yearlyPrice: Money.Create(249.99m, "USD"),
            maxDevices: int.MaxValue, // Ilimitados
            historyDays: int.MaxValue, // Ilimitado
            hasUnlimitedHistory: true,
            hasConsumptionForecast: true,
            hasSmartAlerts: true,
            hasPersonalizedRecommendations: true,
            hasReportExport: true,
            allowsManualDeviceAddition: true
        );

        schema.Features = new List<PlanFeature>
        {
            PlanFeature.Create("everything_student", "Everything in Student Plan", true),
            PlanFeature.Create("devices", "Devices: ∞", true),
            PlanFeature.Create("history", "History: ∞", true),
            PlanFeature.Create("unlimited_history", "Complete history without limits", true),
            PlanFeature.Create("forecast", "Consumption forecast and estimated savings", true),
            PlanFeature.Create("report_export", "Report export in PDF and Excel", true)
        };

        schema.IncludedFeatures = new List<string>
        {
            "Everything in Student Plan",
            "Complete history without limits",
            "Consumption forecast and estimated savings",
            "Report export in PDF and Excel"
        };

        return schema;
    }

    /// <summary>
    /// Obtiene todos los planes disponibles
    /// </summary>
    public static List<PlanSchema> GetAllPlans()
    {
        return new List<PlanSchema>
        {
            BasicPlan(),
            StudentPlan(),
            FamilyPremiumPlan()
        };
    }

    /// <summary>
    /// Obtiene el schema de un plan específico
    /// </summary>
    public static PlanSchema GetPlanSchema(PlanType planType)
    {
        return planType switch
        {
            PlanType.Basic => BasicPlan(),
            PlanType.Student => StudentPlan(),
            PlanType.FamilyPremium => FamilyPremiumPlan(),
            _ => throw new ArgumentException($"Plan type {planType} no soportado", nameof(planType))
        };
    }

    /// <summary>
    /// Obtiene el schema de un plan por su nombre
    /// </summary>
    public static PlanSchema? GetPlanSchemaByName(string planName)
    {
        if (string.IsNullOrWhiteSpace(planName))
            return null;

        return planName.ToLowerInvariant() switch
        {
            "basic" => BasicPlan(),
            "student" => StudentPlan(),
            "familypremium" or "family" or "premium" => FamilyPremiumPlan(),
            _ => null
        };
    }

    /// <summary>
    /// Verifica si se puede cambiar de un plan a otro
    /// </summary>
    public static bool CanChangePlan(PlanType currentPlan, PlanType newPlan)
    {
        // Se puede cambiar a cualquier plan, incluyendo downgrade
        return currentPlan != newPlan;
    }

    /// <summary>
    /// Determina si el cambio es un upgrade
    /// </summary>
    public static bool IsUpgrade(PlanType currentPlan, PlanType newPlan)
    {
        return (int)newPlan > (int)currentPlan;
    }

    /// <summary>
    /// Determina si el cambio es un downgrade
    /// </summary>
    public static bool IsDowngrade(PlanType currentPlan, PlanType newPlan)
    {
        return (int)newPlan < (int)currentPlan;
    }

    /// <summary>
    /// Calcula el precio según el periodo de facturación
    /// </summary>
    public Money GetPrice(BillingPeriod billingPeriod)
    {
        return billingPeriod switch
        {
            BillingPeriod.Monthly => MonthlyPrice,
            BillingPeriod.Yearly => YearlyPrice,
            _ => MonthlyPrice
        };
    }

    /// <summary>
    /// Calcula el ahorro anual
    /// </summary>
    public decimal CalculateYearlySavings()
    {
        var monthlyYearlyCost = MonthlyPrice.Amount * 12;
        return monthlyYearlyCost - YearlyPrice.Amount;
    }

    /// <summary>
    /// Porcentaje de descuento en plan anual
    /// </summary>
    public decimal YearlyDiscountPercentage()
    {
        var monthlyYearlyCost = MonthlyPrice.Amount * 12;
        if (monthlyYearlyCost == 0) return 0;
        
        return Math.Round((1 - (YearlyPrice.Amount / monthlyYearlyCost)) * 100, 2);
    }
}


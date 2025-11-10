namespace Energix.API.DeviceManagement.Domain.Model.ValueObjects;

/// <summary>
/// Energy consumption metrics (for handheld devices only)
/// </summary>
public class DeviceMetrics : IEquatable<DeviceMetrics>
{
    /// <summary>
    /// Monthly consumption in kWh
    /// </summary>
    public decimal Monthly { get; private set; }
    
    /// <summary>
    /// Estimated monthly cost in local currency
    /// </summary>
    public decimal EstimatedCost { get; private set; }
    
    /// <summary>
    /// Rate per kWh
    /// </summary>
    public decimal? Tariff { get; private set; }
    
    /// <summary>
    /// Average daily consumption in kWh (calculated automatically)
    /// </summary>
    public decimal DailyAvg { get; private set; }
    
    protected DeviceMetrics()
    {
    }

    /// <summary>
    /// Main constructor
    /// </summary>
    public DeviceMetrics(decimal monthly, decimal estimatedCost, decimal? tariff = null)
    {
        if (monthly < 0)
            throw new ArgumentException("El consumo mensual no puede ser negativo", nameof(monthly));
        
        if (estimatedCost < 0)
            throw new ArgumentException("El costo estimado no puede ser negativo", nameof(estimatedCost));
        
        if (tariff.HasValue && tariff.Value < 0)
            throw new ArgumentException("La tarifa no puede ser negativa", nameof(tariff));

        Monthly = monthly;
        EstimatedCost = estimatedCost;
        Tariff = tariff;
        DailyAvg = CalculateDailyAverage(monthly);
    }

    /// <summary>
    /// Calculate the daily average based on monthly consumption
    /// </summary>
    private static decimal CalculateDailyAverage(decimal monthly)
    {
        return monthly / 30m;
    }

    /// <summary>
    /// Create new metrics with updated monthly consumption
    /// </summary>
    public DeviceMetrics WithMonthly(decimal newMonthly)
    {
        return new DeviceMetrics(newMonthly, EstimatedCost, Tariff);
    }

    /// <summary>
    /// Create new metrics with updated estimated cost.
    /// </summary>
    public DeviceMetrics WithEstimatedCost(decimal newCost)
    {
        return new DeviceMetrics(Monthly, newCost, Tariff);
    }

    /// <summary>
    /// Create new metrics with updated rates
    /// </summary>
    public DeviceMetrics WithTariff(decimal? newTariff)
    {
        return new DeviceMetrics(Monthly, EstimatedCost, newTariff);
    }
    
    public bool Equals(DeviceMetrics? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Monthly == other.Monthly 
            && EstimatedCost == other.EstimatedCost 
            && Tariff == other.Tariff;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as DeviceMetrics);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Monthly, EstimatedCost, Tariff);
    }

    public static bool operator ==(DeviceMetrics? left, DeviceMetrics? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DeviceMetrics? left, DeviceMetrics? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"Monthly: {Monthly}kWh, Cost: ${EstimatedCost}, Daily: {DailyAvg:F2}kWh";
    }
}
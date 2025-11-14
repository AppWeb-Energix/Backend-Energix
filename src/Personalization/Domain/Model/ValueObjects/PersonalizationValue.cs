﻿namespace Energix.API.Personalization.Domain.ValueObjects;

/// <summary>
/// Value object for personalization settings
/// </summary>
public record PersonalizationValue
{
    public bool Current { get; init; }
    public bool Cost { get; init; }
    public bool Monthly { get; init; }
    public bool Hourly { get; init; }
    public bool Device { get; init; }

    public PersonalizationValue(
        bool current = true,
        bool cost = true,
        bool monthly = true,
        bool hourly = true,
        bool device = true)
    {
        Current = current;
        Cost = cost;
        Monthly = monthly;
        Hourly = hourly;
        Device = device;
    }

    public static PersonalizationValue CreateKpiDefault() => new(
        current: true,
        cost: true,
        monthly: true,
        hourly: false,
        device: false
    );

    public static PersonalizationValue CreateChartDefault() => new(
        current: false,
        cost: false,
        monthly: false,
        hourly: true,
        device: true
    );
}


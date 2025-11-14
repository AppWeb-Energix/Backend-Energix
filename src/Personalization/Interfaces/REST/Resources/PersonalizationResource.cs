﻿namespace Energix.API.Personalization.Interfaces.REST.Resources;

/// <summary>
/// Resource to represent personalization settings in responses
/// </summary>
public record PersonalizationResource(
    int Id,
    int UserId,
    bool KpiCurrent,
    bool KpiCost,
    bool KpiMonthly,
    bool ChartHourly,
    bool ChartMonthly,
    bool ChartDevice,
    DateTime CreatedAt,
    DateTime UpdatedAt
);




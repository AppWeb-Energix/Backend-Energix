﻿namespace Energix.Subscriptions.Application.Commands.RenewPlan;

/// <summary>
/// Comando para renovar una suscripción expirada o cancelada
/// </summary>
public class RenewPlanCommand
{
    public Guid UserId { get; set; }
}



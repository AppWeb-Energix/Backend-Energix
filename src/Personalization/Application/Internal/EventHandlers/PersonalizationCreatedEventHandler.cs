﻿using Energix.API.Personalization.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Energix.API.Personalization.Application.Internal.EventHandlers;

/// <summary>
/// Event handler for PersonalizationCreatedEvent
/// </summary>
public class PersonalizationCreatedEventHandler
{
    private readonly ILogger<PersonalizationCreatedEventHandler> _logger;

    public PersonalizationCreatedEventHandler(ILogger<PersonalizationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handle the personalization created event
    /// </summary>
    public async Task HandleAsync(PersonalizationCreatedEvent @event)
    {
        _logger.LogInformation(
            "Personalization created for User {UserId} with ID {PersonalizationId} at {CreatedAt}",
            @event.UserId,
            @event.PersonalizationId,
            @event.CreatedAt);

        // Here you can add additional logic such as:
        // - Send notification to user
        // - Update analytics
        // - Trigger other domain events
        // - etc.

        await Task.CompletedTask;
    }
}


﻿namespace Energix.API.Personalization.Domain.Events;

/// <summary>
/// Domain event fired when a personalization is created
/// </summary>
public record PersonalizationCreatedEvent
{
    public int PersonalizationId { get; init; }
    public int UserId { get; init; }
    public DateTime CreatedAt { get; init; }

    public PersonalizationCreatedEvent(int personalizationId, int userId, DateTime createdAt)
    {
        PersonalizationId = personalizationId;
        UserId = userId;
        CreatedAt = createdAt;
    }
}


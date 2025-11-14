namespace Energix.API.Personalization.Domain.Model.Queries;

/// <summary>
/// Query to get personalization by ID
/// </summary>
public record GetPersonalizationByIdQuery
{
    public int PersonalizationId { get; init; }

    public GetPersonalizationByIdQuery(int personalizationId)
    {
        PersonalizationId = personalizationId;
    }
}


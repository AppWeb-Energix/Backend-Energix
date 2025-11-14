namespace Energix.API.Personalization.Domain.Model.Queries;

/// <summary>
/// Query to get personalization by user ID
/// </summary>
public record GetPersonalizationByUserIdQuery
{
    public int UserId { get; init; }

    public GetPersonalizationByUserIdQuery(int userId)
    {
        UserId = userId;
    }
}

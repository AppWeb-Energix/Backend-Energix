using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Interfaces.REST.Resources;

namespace Energix.API.Personalization.Interfaces.REST.Transform;

public static class PersonalizationResourceFromEntityAssembler
{
    public static PersonalizationResource ToResourceFromEntity(PersonalizationAggregate entity)
    {
        return new PersonalizationResource(
            entity.Id,
            entity.UserId,
            entity.KpiCurrent,
            entity.KpiCost,
            entity.KpiMonthly,
            entity.ChartHourly,
            entity.ChartMonthly,
            entity.ChartDevice,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}


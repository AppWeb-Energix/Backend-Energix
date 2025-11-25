namespace Energix.Subscriptions.Domain.Schemas;

/// <summary>
/// Característica o funcionalidad de un plan
/// </summary>
public class PlanFeature
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsAvailable { get; private set; }

    private PlanFeature(string name, string description, bool isAvailable = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la característica es requerido", nameof(name));
        
        Name = name;
        Description = description ?? string.Empty;
        IsAvailable = isAvailable;
    }

    public static PlanFeature Create(string name, string description, bool isAvailable = true)
        => new(name, description, isAvailable);

    public override string ToString() => Name;
}


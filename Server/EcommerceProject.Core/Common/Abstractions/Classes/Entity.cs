using EcommerceProject.Core.Common.Abstractions.Interfaces;

namespace EcommerceProject.Core.Common.Abstractions.Classes;

public abstract class Entity<TId> : IEntity<TId>
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public TId Id { get; init; }

    protected Entity(TId id)
    {
        Id = id;
    }
    
    protected Entity(){}
}

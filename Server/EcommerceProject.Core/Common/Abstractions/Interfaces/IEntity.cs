namespace EcommerceProject.Core.Common.Abstractions.Interfaces;

public interface IEntity<TId> : IEntity
{
    public TId Id { get; init; }
}

public interface IEntity
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}

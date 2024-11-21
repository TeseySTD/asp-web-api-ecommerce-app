using System;

namespace EcommerceProject.API.Contracts;

public record ProductResponse(Guid Id, string Title, string Description, decimal Price);
public record ProductRequest(string Title, string Description, decimal Price);

﻿namespace Users.Infrastructure.Authorization;

public static class Policies
{
    public const string RequireAdminPolicy = "Admin";
    public const string RequireSellerPolicy = "Seller";
    public const string RequireDefaultPolicy = "Default";
}
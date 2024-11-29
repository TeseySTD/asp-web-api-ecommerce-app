﻿using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceProject.Persistence.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ConfigureUserTable(builder);
    }

    private void ConfigureUserTable(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(
                x => x.Value,
                value  => new UserId(value));

        builder.Property(u => u.Name)
            .HasConversion(
                n => n.Value,
                value => UserName.Of(value))
            .HasMaxLength(UserName.MaxNameLength);

        builder.Property(u => u.Email)
            .HasConversion(
                e => e.Value,
                value => Email.Of(value));
        
        builder.Property(u => u.Password)
            .HasConversion(
                p => p.Value,
                value => Password.Of(value));
        
        builder.Property(u => u.PhoneNumber)
            .HasConversion(
                p => p.Value,
                value => PhoneNumber.Of(value));
        
        builder.Property(u => u.Role)
            .HasConversion(
                r => r.ToString(),
                value => Enum.Parse<User.UserRole>(value));
    }
}
using EcommerceProject.Core.Models.Users;
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
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value  => UserId.Create(value).Value);

        builder.Property(u => u.Name)
            .HasConversion(
                n => n.Value,
                value => UserName.Create(value).Value)
            .HasMaxLength(UserName.MaxNameLength);

        builder.Property(u => u.Email)
            .HasConversion(
                e => e.Value,
                value => Email.Create(value).Value);
        
        builder.Property(u => u.HashedPassword)
            .HasConversion(
                p => p.Value,
                value => HashedPassword.Create(value).Value);
        
        builder.Property(u => u.PhoneNumber)
            .HasConversion(
                p => p.Value,
                value => PhoneNumber.Create(value).Value);
        
        builder.Property(u => u.Role)
            .HasConversion(
                r => r.ToString(),
                value => Enum.Parse<User.UserRole>(value));
    }
}
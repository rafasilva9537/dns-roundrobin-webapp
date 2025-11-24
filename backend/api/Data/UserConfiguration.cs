using api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.UserName).IsRequired();
        builder.Property(u => u.NormalizedUserName).IsRequired();
        builder.Property(u => u.Email).IsRequired();
        builder.Property(u => u.NormalizedEmail).IsRequired();
        builder.Property(u => u.Description).HasMaxLength(300);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using tm.Domain.Entities;

namespace tm.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.PackageName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.Price)
            .HasColumnType("decimal(18,2)");
            
        builder.Property(s => s.Status)
            .HasConversion<int>();
            
        builder.HasOne(s => s.Project)
            .WithOne(p => p.Subscription)
            .HasForeignKey<Subscription>(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


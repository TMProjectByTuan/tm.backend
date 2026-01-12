using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using tm.Domain.Entities;

namespace tm.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.InvitedEmail)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(i => i.Status)
            .HasConversion<int>();
            
        builder.HasIndex(i => new { i.ProjectId, i.InvitedEmail, i.Status })
            .HasFilter("[Status] = 1"); // Only index pending invitations
            
        builder.HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(i => i.InvitedByUser)
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(i => i.AcceptedByUser)
            .WithMany()
            .HasForeignKey(i => i.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


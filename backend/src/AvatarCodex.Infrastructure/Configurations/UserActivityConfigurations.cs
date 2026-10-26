using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

// Dados do usuário: CASCADE quando o usuário é removido, RESTRICT para o conteúdo (docs/database.md §1).

public class UserCharacterDiscoveryConfiguration : IEntityTypeConfiguration<UserCharacterDiscovery>
{
    public void Configure(EntityTypeBuilder<UserCharacterDiscovery> builder)
    {
        builder.HasKey(d => new { d.UserId, d.CharacterId });
        builder.HasIndex(d => new { d.UserId, d.DiscoveredAt }).IsDescending(false, true);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Character).WithMany().HasForeignKey(d => d.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserNationDiscoveryConfiguration : IEntityTypeConfiguration<UserNationDiscovery>
{
    public void Configure(EntityTypeBuilder<UserNationDiscovery> builder)
    {
        builder.HasKey(d => new { d.UserId, d.NationId });
        builder.HasIndex(d => new { d.UserId, d.DiscoveredAt }).IsDescending(false, true);

        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Nation).WithMany().HasForeignKey(d => d.NationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserFavoriteCharacterConfiguration : IEntityTypeConfiguration<UserFavoriteCharacter>
{
    public void Configure(EntityTypeBuilder<UserFavoriteCharacter> builder)
    {
        builder.HasKey(f => new { f.UserId, f.CharacterId });
        builder.HasIndex(f => new { f.UserId, f.CreatedAt }).IsDescending(false, true);

        builder.HasOne<User>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Character).WithMany().HasForeignKey(f => f.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExperienceTransactionConfiguration : IEntityTypeConfiguration<ExperienceTransaction>
{
    public void Configure(EntityTypeBuilder<ExperienceTransaction> builder)
    {
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.Description).HasMaxLength(200);

        // RN-05: a mesma recompensa nunca é concedida duas vezes, nem com requisições simultâneas.
        builder.HasIndex(t => new { t.UserId, t.Type, t.SourceId }).IsUnique();

        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_experience_transactions_amount_positive", "amount > 0"));
    }
}

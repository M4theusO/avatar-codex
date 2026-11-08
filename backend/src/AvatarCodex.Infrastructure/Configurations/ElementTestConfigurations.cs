using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class ElementTestQuestionConfiguration : IEntityTypeConfiguration<ElementTestQuestion>
{
    public void Configure(EntityTypeBuilder<ElementTestQuestion> builder)
    {
        builder.Property(q => q.Text).HasMaxLength(300);
        builder.HasIndex(q => q.Order).IsUnique();

        builder.HasMany(q => q.Options).WithOne().HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ElementTestOptionConfiguration : IEntityTypeConfiguration<ElementTestOption>
{
    public void Configure(EntityTypeBuilder<ElementTestOption> builder)
    {
        builder.Property(o => o.Text).HasMaxLength(300);
        builder.HasIndex(o => new { o.QuestionId, o.Order }).IsUnique();

        // Uma opção por elemento em cada pergunta: nenhum elemento sai em vantagem.
        builder.HasIndex(o => new { o.QuestionId, o.ElementId }).IsUnique();
        builder.HasOne(o => o.Element).WithMany().HasForeignKey(o => o.ElementId).OnDelete(DeleteBehavior.Restrict);
    }
}

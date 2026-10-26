using System.Linq.Expressions;
using AvatarCodex.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

internal static class ImageInfoConfiguration
{
    /// <summary>Mapeia um <see cref="ImageInfo"/> como colunas da própria tabela (ex. image_url, image_credit).</summary>
    public static void OwnsImage<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, ImageInfo?>> navigation) where TEntity : class
    {
        builder.OwnsOne(navigation, image =>
        {
            image.Property(i => i.Url).HasMaxLength(500);
            image.Property(i => i.SourceUrl).HasMaxLength(500);
            image.Property(i => i.Credit).HasMaxLength(200);
        });
    }
}

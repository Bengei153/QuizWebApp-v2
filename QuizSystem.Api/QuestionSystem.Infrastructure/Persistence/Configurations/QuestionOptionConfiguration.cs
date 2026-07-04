using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Persistence.Configurations;

public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.ImageUrl)
            .HasMaxLength(2000);

        builder.Property(o => o.ImagePublicId)
            .HasMaxLength(300);

        // Soft-deleted options should never surface in normal queries — matches
        // the filter already applied to Question and Folder.
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoteService.Domain.Entities;

namespace NoteService.Infrastructure.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(e => e.NoteId);

        builder.Property(e => e.NoteId)
            .ValueGeneratedOnAdd();
        
        builder.HasIndex(e => e.UserId);
    }
}
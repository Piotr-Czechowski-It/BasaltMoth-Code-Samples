using Microsoft.EntityFrameworkCore;

namespace BasaltMoth.CodeSamples.Quotes;

public sealed class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuoteNumberSequence>(entity =>
        {
            entity.ToTable("QuoteNumberSequences");
            entity.HasKey(sequence => sequence.CompanyId);
            entity.Property(sequence => sequence.LastNumber).IsRequired();
        });
    }
}

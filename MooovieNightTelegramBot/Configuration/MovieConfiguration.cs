using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using MooovieNightTelegramBot.Model;

namespace MooovieNightTelegramBot.Configuration
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder
                .ToTable("Movies")
                .HasKey(t => t.Id);
            builder
                .Property(p => p.Id)
                .UseHiLo("MovieSeq");
            builder
                .Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            builder
                .Property(p => p.AlternativeName)
                .HasMaxLength(100)
                .IsRequired();
            builder
                .Property(p => p.Description);
            builder
                .Property(p => p.Country);
        }
    }
}

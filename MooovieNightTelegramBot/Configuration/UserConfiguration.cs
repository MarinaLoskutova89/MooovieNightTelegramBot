using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using MooovieNightTelegramBot.Model;

namespace MooovieNightTelegramBot.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .ToTable("Users")
                .HasKey(t => t.TelegramUserId);
            builder
                .Property(p => p.TelegramUserId)
                .UseHiLo("UserSeq")
                .IsRequired();
            builder
                .Property(p => p.FirstName)
                .HasMaxLength(100);
            builder
                .Property(p => p.LastName)
                .HasMaxLength(100);
        }
    }
}

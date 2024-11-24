using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MooovieNightTelegramBot.Configuration;
using MooovieNightTelegramBot.Model;

namespace MooovieNightTelegramBot
{
    public class Context : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }

        public Context(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(_configuration["ConnectionStrings:MovieDBLocalConnection"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new MovieConfiguration());
        }
    }
}

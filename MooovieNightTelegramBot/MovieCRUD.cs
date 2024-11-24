using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MooovieNightTelegramBot.Model;

namespace MooovieNightTelegramBot
{
    public class MovieCRUD
    {
        private IConfiguration _configuration;

        public MovieCRUD(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public async Task CreateMovie(Movie movie)
        {
            using (Context context = new(_configuration))
            {

                await context.Movies.AddAsync(movie);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Movie> GetMovieByName(string movieName)
        {
            using (Context context = new(_configuration))
            {
                Movie? movie = await context.Movies
                    .Where(r => r.Name == movieName)
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync();

                return movie;
            }
        }
    }
}

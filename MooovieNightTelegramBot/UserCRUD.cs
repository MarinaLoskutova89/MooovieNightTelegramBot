using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MooovieNightTelegramBot.Model;

namespace MooovieNightTelegramBot
{
    public class UserCRUD
    {
        private IConfiguration _configuration;

        public UserCRUD(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task CreateUser(User user)
        {
            using (Context context = new(_configuration))
            { 
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
        }

        public async Task<User> UpdateUser(Movie movie, User user)
        {
            using (Context context = new(_configuration))
            {
                User? updatedUser = await context.Users
                    .Where(userId => userId.TelegramUserId == user.TelegramUserId)
                    .Include(movie => movie.Movies)
                    .FirstOrDefaultAsync();

                updatedUser?.Movies.Add(movie);
                await context.SaveChangesAsync();
                return updatedUser;
            }
        }

        public async Task<User> FindUser(long telegramUserId)
        {
            using (Context context = new(_configuration))
            {
                User? avaliableUser = await context.Users
                .Where(r => r.TelegramUserId == telegramUserId)
                .Include(r => r.Movies)
                .FirstOrDefaultAsync();

                return avaliableUser;
            }
        }

        public async Task<User> DeleteMovie(int movieId, long telegramUserId)
        {
            using (Context context = new(_configuration))
            {
                User? user = await context.Users
                .Where(r => r.TelegramUserId == telegramUserId)
                .Include(r => r.Movies)
                .FirstOrDefaultAsync();

                Movie? deletedMovie = user?.Movies.Where(r => r.Id == movieId).FirstOrDefault();
                user?.Movies.Remove(deletedMovie);

                await context.SaveChangesAsync();

                return user;
            }
        }

        public async Task<bool> UserAlreadySawThisMovie(Movie movie)
        {
            using (Context context = new(_configuration))
            {
                User? user = await context.Users
                    .Where(r => r.Movies.Contains(movie))
                    .FirstOrDefaultAsync();

                if (user != null) return true;
                else return false;
            }
        }
    }
}

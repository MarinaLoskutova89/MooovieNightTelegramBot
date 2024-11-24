

namespace MooovieNightTelegramBot.Model
{
    public class User
    {
        public long TelegramUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual List<Movie> Movies { get; set; } = new List<Movie>();
    }
}

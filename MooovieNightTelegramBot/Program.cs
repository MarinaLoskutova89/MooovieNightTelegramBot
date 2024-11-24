using Microsoft.Extensions.Configuration;
using MooovieNightTelegramBot.Controllers;

namespace MooovieNightTelegramBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                        .Build();

            HomeController controller = new HomeController(configuration);
            await controller.TelegramHandler();
        }
    }
}
 
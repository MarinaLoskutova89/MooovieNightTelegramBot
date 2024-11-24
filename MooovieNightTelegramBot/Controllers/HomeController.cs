using System.Text;
using MooovieNightTelegramBot.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace MooovieNightTelegramBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        private Movie _movie = new();
        private Model.User _user = new();

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly ReplyKeyboardMarkup RkmStartMenu = new(
            new[]
            {
                new KeyboardButton[] { "Подобрать фильм", "Отложенные фильмы" }
            })
        {
            ResizeKeyboard = true
        };
        private readonly ReplyKeyboardMarkup AnotherRkmStartMenu = new(
            new[]
            {
                new KeyboardButton[] { "Подобрать фильм" }
            })
        {
            ResizeKeyboard = true
        };


        public async Task TelegramHandler()
        {
            using CancellationTokenSource cts = new();
            TelegramBotClient bot = new(_configuration["Telegram:Bot_token"], cancellationToken: cts.Token);
            Telegram.Bot.Types.User? me = await bot.GetMe();
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            cts.Cancel();
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception);
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await BotOnMessageReceived(botClient, update.Message);
                        break;
                    case UpdateType.CallbackQuery:
                        await BotOnCallbackQueryReceived(botClient, update.CallbackQuery);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task BotOnMessageReceived(ITelegramBotClient bot, Message message)
        {
            UserCRUD userCRUD = new(_configuration);

            if (message?.Text != null)
            {
                Console.WriteLine($"ChatId = {message.Chat.Id} => {message.Text}");

                switch (message.Text)
                {
                    case "/start":
                        await CheckUsers(message);
                        ReplyKeyboardMarkup rmk = (_user.Movies == null || _user.Movies.Count == 0) ? AnotherRkmStartMenu : RkmStartMenu;
                        await bot.SendMessage(message.Chat, "Добро пожаловать! \nВыберите действие!", replyMarkup: rmk);
                        break;
                    case "Подобрать фильм":
                    case "Другой фильм":
                        await SuggestMovie(bot, message);
                        break;
                    case "Отложенные фильмы":
                        List<InlineKeyboardButton[]> buttonsList = new();
                        foreach (Movie? movie in _user.Movies)
                        {
                            InlineKeyboardButton[] buttons = [$"{movie.Name}"];
                            buttonsList.Add(buttons);
                        }
                        InlineKeyboardMarkup IkmVarMenu = new(buttonsList);

                        await bot.SendMessage(message.Chat, $"Список отложенных фильмов:", replyMarkup: IkmVarMenu);
                        break;
                    default:
                        await OnUnknownMessage(message, bot);
                        break;
                }
            }
        }

        private async Task BotOnCallbackQueryReceived(ITelegramBotClient bot, CallbackQuery callbackQuery)
        {
            MovieCRUD movieCRUD = new(_configuration);
            UserCRUD userCRUD = new(_configuration);
            
            ReplyKeyboardMarkup rkmAfterSavedMenu = _user.Movies != null && _user.Movies.Count > 0 
                ? new(new[]
                {
                    new KeyboardButton[] { "Другой фильм", "Отложенные фильмы" }
                })
                {
                    ResizeKeyboard = true
                } 
                : new(new[]
                {
                    new KeyboardButton[] { "Другой фильм" }
                })
                {
                    ResizeKeyboard = true
                }; ;

            if (callbackQuery.Data != null)
            {
                Console.WriteLine($"ChatId = {callbackQuery.Message!.Chat.Id} => {callbackQuery.Data}");

                switch (callbackQuery.Data)
                {
                    case "postpone":
                        await movieCRUD.CreateMovie(_movie);
                        _user = await userCRUD.UpdateUser(_movie, _user);
                        await bot.SendMessage(callbackQuery.Message!.Chat, "Фильм добавлен в список. \nВыберите действие!", replyMarkup: rkmAfterSavedMenu);
                        break;
                    case "watch":
                        await bot.SendMessage(callbackQuery.Message!.Chat, "Приятного просмотра!. \nВыберите действие!", replyMarkup: rkmAfterSavedMenu);
                        break;
                    case "watch_now":
                        _user = await userCRUD.DeleteMovie(_movie.Id, _user.TelegramUserId);
                        await bot.SendMessage(callbackQuery.Message!.Chat, "Приятного просмотра!. \nВыберите действие!", replyMarkup: rkmAfterSavedMenu);
                        break;
                    default:
                        _movie = await movieCRUD.GetMovieByName(callbackQuery.Data);
                        StringBuilder result = new();
                        result.Append($"Название: {_movie.Name}");
                        if (!string.IsNullOrEmpty(_movie.Country)) result.Append($"\nСтрана изготовитель: {_movie.Country}");
                        if (!string.IsNullOrEmpty(_movie.Description)) result.Append($"\nОписание: {_movie.Description}");

                        InlineKeyboardMarkup ikmPostponeMenu = new(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Посмотреть", "watch_now")
                            }
                        });
                        await bot.SendMessage(callbackQuery.Message!.Chat, result.ToString(), replyMarkup: ikmPostponeMenu);
                        break;
                }
            }
        }

        public async Task OnUnknownMessage(Message msg, ITelegramBotClient bot)
        {
            Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
            await bot.SendMessage(msg.Chat, "Неизвестная команда", replyMarkup: RkmStartMenu);
        }

        public async Task SuggestMovie(ITelegramBotClient bot, Message message)
        {
            var flag = false;
            StringBuilder result = new();
            KinoService ks = new(_configuration);
            UserCRUD userCRUD = new(_configuration);

            do
            {
                _movie = await ks.GetRandomMovie("");
                bool sawThisMovie = await userCRUD.UserAlreadySawThisMovie(_movie);
                if (!sawThisMovie)
                {
                    if (!string.IsNullOrEmpty(_movie.Name) && !string.IsNullOrEmpty(_movie.AlternativeName))
                    {
                        result.Append($"Название: {_movie.Name}");
                        if (!string.IsNullOrEmpty(_movie.Country)) result.Append(Environment.NewLine + $"Страна изготовитель: {_movie.Country}");
                        if (!string.IsNullOrEmpty(_movie.Description)) result.Append(Environment.NewLine + $"Описание: {_movie.Description}");
                        flag = true;
                    }
                }
            } while (!flag);
            
            InlineKeyboardMarkup IkmVarMenu = new(new[]
            {
                new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отложить на потом", "postpone"),
                        InlineKeyboardButton.WithCallbackData("Буду смотреть", "watch")
                    }
            });
            
            await bot.SendMessage(message!.Chat, result.ToString(), replyMarkup: IkmVarMenu);
        }

        public async Task CheckUsers(Message? message) 
        {
            UserCRUD userCRUD = new(_configuration);
            Model.User user = await userCRUD.FindUser(message.From.Id);

            if (user == null)
            {
                user = new()
                {
                    TelegramUserId = message.From.Id,
                    FirstName = message.From.FirstName,
                    LastName = message.From.LastName
                }; 

                await userCRUD.CreateUser(user);
            }

            _user = user;
        }
    }
}

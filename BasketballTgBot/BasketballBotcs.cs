using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using BasketballTgBot.BasketballClients;
using System.Diagnostics.Metrics;

namespace BasketballTgBot
{
    public class BasketballBotcs
    {
        TelegramBotClient botClient = new TelegramBotClient("7354906142:AAGM44H0eXiH-L5MVvTnFzay_RJF6MXAkTM");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start() // метод, що запускає бота
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} розпочав працювати");
            Console.ReadKey();
        }
        // Метод, що попереджає 
        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот API\n{apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
        }
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Вітаю! Оберіть команду:/keyboard");
                return;
            }
            else
            if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new
               (
                 new[]
                 {
                      new KeyboardButton[] {"Надати інформацію про баскетбольну команду🏀 по ID:"},
                      new KeyboardButton[] {"Надати інформацію про баскетбольну команду🏀 по назві:"},
                      new KeyboardButton[] {"Надати інформацію про баскетбольну лігу🏀 по назві:"},
                     new KeyboardButton[] { "Додати улюблену команду🏀", "Видалити улюблену команду🏀" },
                    new KeyboardButton[] { "Змінити улюблену команду🏀", "Переглянути улюблену команду🏀" }
                 }
               )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Оберіть пункт меню:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            if (message.Text == "Надати інформацію про баскетбольну команду🏀 по ID:") // знаходження команди по ID
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть ID команди:");
            }
            else if (int.TryParse(message.Text, out int id))
            {
                var teamInfo = await GetTeamById(id);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: teamInfo, cancellationToken: cancellationToken);
            }
            if (message.Text == "Надати інформацію про баскетбольну команду🏀 по назві:")  // Знаходження команди по назві
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть назву баскетбольної команди у форматі (Назва команди: команда)");
            }
            else if (message.Text.StartsWith("Назва команди: "))
            {
                var teamName = message.Text.Substring("Назва команди: ".Length);
                Console.WriteLine($"Searching for team with name: {teamName}");
                teamName = teamName.Replace(" ", "%20");
                var teamInfo = await GetTeamByName(teamName, message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: teamInfo, cancellationToken: cancellationToken);
            }
            if (message.Text == "Надати інформацію про баскетбольну лігу🏀 по назві:")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть назву баскетбольної ліги у форматі (Назва ліги: ліга)");
            }
            else if (message.Text.StartsWith("Назва ліги: "))
            {
                var leagueName = message.Text.Substring("Назва ліги: ".Length);
                Console.WriteLine($"Searching for league with name: {leagueName}");
                leagueName = leagueName.Replace(" ", "%20");
                var teamInfo = await GetLeagueByName(leagueName);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: teamInfo, cancellationToken: cancellationToken);
            }
            if (message.Text == "Додати улюблену команду🏀")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Щоб додати улюблену команду потрібно ввести назву команди (Улюблена команда⛹‍♂️: команда)");
                return;
            }
            else if (message.Text.StartsWith("Улюблена команда⛹‍♂️: "))
            {
                var teamName = message.Text.Substring("Улюблена команда⛹‍♂️: ".Length);

                var response = await InsertFavouriteTeamAsync(teamName, message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: response, cancellationToken: cancellationToken);      
            }
            if (message.Text == "Видалити улюблену команду🏀")
            {
                var response = await DeleteFavouriteTeamAsync(message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: response, cancellationToken: cancellationToken);
            }
            if (message.Text == "Змінити улюблену команду🏀")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть нову назву улюбленої команди у форматі (Змінити баскетбольну команду⛹‍♂️: команда)");
            }
            else if (message.Text.StartsWith("Змінити баскетбольну команду⛹‍♂️: "))
            {
                var teamName = message.Text.Substring("Змінити баскетбольну команду⛹‍♂️: ".Length);
                var response = await ChangeFavouriteTeamAsync(teamName, message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: response, cancellationToken: cancellationToken);
            }
            if (message.Text == "Переглянути улюблену команду🏀")
            {
                var response = await GetFavouriteTeamAsync(message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, text: response, cancellationToken: cancellationToken);
            }
        }
        private async Task<string> GetTeamById(int id) // метод для знаходження команди за ID
        {
            try
            {
                var basketballClient = new BasketballTgBot.BasketballClients.BasketballClients();
                var teamInfo = await basketballClient.GetTeamById(id);
                if (teamInfo == null)
                {
                    return "Команда не знайдена. Перевірте правильність введеного ID.";
                }
                return teamInfo;
            }
            catch (Exception ex)
            {
                return $"Виникла помилка під час отримання інформації про команду: {ex.Message}";
            }
        }
        private async Task<string> GetTeamByName(string name, long UserId) // метод для знаходження команди за ім'ям
        {
            try
            {
                var basketballClient = new BasketballTgBot.BasketballClients.BasketballClients();
                var teamInfo = await basketballClient.GetTeamByName(name, UserId);
                if (teamInfo == null)
                {
                    return "Команда не знайдена. Перевірте правильність введеної назви.";
                }
                return teamInfo;
            }
            catch (Exception ex)
            {
                return $"Виникла помилка під час отримання інформації про команду: {ex.Message}";
            }
        }
        private async Task<string> GetLeagueByName(string name) // метод для знаходження ліги за ім'ям
        {
            try
            {
                var basketballClient = new BasketballTgBot.BasketballClients.BasketballClients();
                var leagueInfo = await basketballClient.GetLeagueByName(name);
                if (leagueInfo == null)
                {
                    return "Ліга не знайдена. Перевірте правильність введеної назви.";
                }
                return leagueInfo;
            }
            catch (Exception ex)
            {
                return $"Виникла помилка під час отримання інформації про лігу: {ex.Message}";
            }
        }
        // Інші методи, що повязані з мазою даних
        private async Task<string> InsertFavouriteTeamAsync(string NameOfTeam, long IdOfTeam)
        {
            try
            {
                var basketballClient = new BasketballClients.BasketballClients();
                await basketballClient.InsertFavouriteTeamAsync(NameOfTeam, IdOfTeam);
                return "Улюблена команда успішно додана!";
            }
            catch (Exception ex)
            {
                return $"Виникла помилка під час додавання улюбленої команди: {ex.Message}";
            }
        }
        private async Task<string> DeleteFavouriteTeamAsync(long IdOfTeam)
        {
            try
            {
                var basketballClient = new BasketballClients.BasketballClients();
                await basketballClient.DeleteFavouriteTeamAsync(IdOfTeam);
                return "Улюблена команда успішно видалена!";
            }
            catch (Exception ex)
            {
                return $"Виникла помилка під час видалення улюбленої команди: {ex.Message}";
            }
        }
        private async Task<string> ChangeFavouriteTeamAsync(string NameOfTeam, long IdOfTeam)
        {
            try
            {
                var basketballClient = new BasketballClients.BasketballClients();
                await basketballClient.ChangeFavouriteTeamAsync(NameOfTeam, IdOfTeam);
                return "Улюблена команда успішно змінена!";
            }
            catch (Exception ex)
            {
                return $"Виникла помилка під час зміни улюбленої команди: {ex.Message}";
            }
        }
        private async Task<string> GetFavouriteTeamAsync(long IdOfTeam)
        {
            try
            {
                var basketballClient = new BasketballClients.BasketballClients();
                return await basketballClient.GetFavouriteTeamAsync(IdOfTeam);
            }
            catch (Exception ex)
            {
                return $"Помилка під час отримання інформації про улюблену команду: {ex.Message}";
            }
        }
    }
}
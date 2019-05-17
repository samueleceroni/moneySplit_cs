using System;
using System.Collections.Generic;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using TelegramBot.User;
using TelegramBot.Logger;

namespace TelegramBot.Bot
{
    public class Bot
    {
        private static TelegramBotClient botClient;
        private Telegram.Bot.Types.User botUser;
        private Dictionary<int, State> userStates;

        public string Token { get; set; }

        public Bot(string token)
        {
            botClient = new TelegramBotClient(token);
            botUser = botClient.GetMeAsync().Result;
            botClient.OnMessage += OnMessageReceived;
            SingletonLogger.Istance.TraceInformation($"Bot Started: ${botUser.Id}");
        }

        private async void OnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var chat = message.Chat;
            var user = message.From;

            if (message == null || message.Type != MessageType.Text)
            {
                SingletonLogger.Istance.TraceInformation($"Message from ${user.Id} in chat ${chat.Id} does not contain text");
                return;
            }

            var state = GetUserState(user.Id);
            var parser = Parser.Parser.BuildParser(state, message.Text, chat);

            if (parser.IsFailure)
            {
                SingletonLogger.Istance.TraceInformation($"Message from ${user.Id} in chat ${chat.Id}: parse failed");
                return;
            }

            var query = parser.Value.Parse();

            if (query.IsFailure)
            {
                SingletonLogger.Istance.TraceInformation($"Message from ${user.Id} in chat ${chat.Id}: query error");
                return;
            }

            SingletonLogger.Istance.TraceInformation($"Message from ${user.Id} in chat ${chat.Id}: OK");

            //QUERY AL DB
            //TODO: Formatter
            var result = "";

            await botClient.SendTextMessageAsync(
                    chat.Id,
                    result
                );
        }

        private State GetUserState(int userId)
        {
            if (userStates.ContainsKey(userId))
                return userStates[userId];
            userStates.Add(userId, new State());
            return userStates[userId];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Query;
using TelegramBot.User;
using CSharpFunctionalExtensions;
using Telegram.Bot.Types;

namespace TelegramBot.Parser
{
    public class Parser : IEquatable<Parser>
    {
        public static readonly string ParseErrorMessage = "An error occurred";
        public static readonly string TextIsEmptyMessage = "Text is empty";
        public static readonly string ChatIsNullMessage = "Chat is null";
        public static readonly string ChatMemberIsNullMessage = "Chat member is null";
        public static readonly string StateIsNullMessage = "State is null";

        private static readonly string NewTransactionCommand = "/new_transaction";
        private static readonly string ShowDetailCommand = "/show_detail";
        private static readonly string BalanceCommand = "/balance";
        private static readonly string NewListCommand = "/new_list";
        private static readonly string DeleteCommand = "/delete";
        private static readonly string ResetCommand = "/reset";
        private static readonly string ShowCommand = "/show";
        private static readonly string AllCommand = "/all";
        private static readonly string StartCommand = "/start";
        private static readonly string HelpCommand = "/help";

        /// <summary>
        /// Message's text
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Context Id
        /// </summary>
        public Chat Chat { get; private set; }
        /// <summary>
        /// User's id
        /// </summary>
        public ChatMember ChatMember { get; private set; }
        /// <summary>
        /// User's State
        /// </summary>
        public State State { get; private set; }

        public Parser(State state, string text, Chat chat, ChatMember chatMember)
        {
            Text = text;
            Chat = chat;
            ChatMember = chatMember;
            State = state;
        }

        /// <summary>
        /// Builds a parser and returns a result with the outcome 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="text"></param>
        /// <param name="chat"></param>
        /// <param name="chatMember"></param>
        /// <returns>Result.Ok if parameters are good, Result.Fail otherwise</returns>
        static public Result<Parser> BuildParser(State state, string text, Chat chat, ChatMember chatMember)
                => Result.Ok<Parser>(new Parser(state, text, chat, chatMember))
                         .Ensure(query => !String.IsNullOrEmpty(query.Text), TextIsEmptyMessage)
                         .Ensure(query => query.Chat != null, ChatIsNullMessage)
                         .Ensure(query => query.ChatMember != null, ChatMemberIsNullMessage)
                         .Ensure(query => query.State != null, StateIsNullMessage);

        /// <summary>
        /// Takes the first word and tries to match it with bot's commands and returns a result with the query object
        /// </summary>
        /// <returns>Result.Ok if the command and the syntax are correct, Result.Fail otherwise</returns>
        public Result<QueryObject> Parse()
        {
            string command = Text.Split(' ')[0];
            string arguments = Text.Substring(Math.Min(Text.Length, Text.IndexOf(command) + command.Length + 1));
            AbstractQueryParser parser;

            if(command == NewTransactionCommand)
                    parser = new NewTransactionParser(State, arguments, Chat, ChatMember);
            else if(command == ShowDetailCommand)
                    parser = new ShowDetailParser(State, arguments, Chat, ChatMember);
            else if(command == BalanceCommand)
                    parser = new BalanceParser(State, arguments, Chat, ChatMember);
            else if(command == NewListCommand)
                    parser = new NewListParser(State, arguments, Chat, ChatMember);
            else if(command == DeleteCommand)
                parser = new DeleteParser(State, arguments, Chat, ChatMember);
            else if(command == ResetCommand)
                    parser = new ResetParser(State, arguments, Chat, ChatMember);
            else if(command == ShowCommand)
                parser = new ShowParser(State, arguments, Chat, ChatMember);
            else if(command == AllCommand)
                parser = new AllParser(State, arguments, Chat, ChatMember);
            else
                return Result.Fail<QueryObject>(ParseErrorMessage);    
            
            return parser.GetQueryObject();
        }

        public bool Equals(Parser other)
        {
            return other != null &&
                   Chat == other.Chat &&
                   ChatMember == other.ChatMember &&
                   Text == other.Text &&
                   State.Equals(other.State);
        }
    }
}

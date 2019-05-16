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
    public class Parser
    {
        public readonly static string ParseErrorMessage = "An error occurred";
        public readonly static string TextIsEmptyMessage = "Text is empty";

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

        private Parser(State state, string text, Chat chat, ChatMember chatMember)
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
                         .Ensure(query => query.Chat != null, TextIsEmptyMessage)
                         .Ensure(query => query.ChatMember != null, TextIsEmptyMessage)
                         .Ensure(query => query.State != null, TextIsEmptyMessage);

        /// <summary>
        /// Takes the first word and tries to match it with bot's commands and returns a result with the query object
        /// </summary>
        /// <returns>Result.Ok if the command and the syntax are correct, Result.Fail otherwise</returns>
        public Result<QueryObject> Parse()
        {
            AbstractQueryParser parser;
            switch(Text.Split(' ')[0])
            {
                case "/new_transaction":
                    parser = new NewTransactionParser(State, Text, Chat, ChatMember);
                    break;
                case "/show_detail":
                    parser = new ShowDetailParser(State, Text, Chat, ChatMember);
                    break;
                case "/balance":
                    parser = new BalanceParser(State, Text, Chat, ChatMember);
                    break;
                case "/new_list":
                    parser = new NewListParser(State, Text, Chat, ChatMember);
                    break;
                case "/delete":
                    parser = new DeleteParser(State, Text, Chat, ChatMember);
                    break;
                case "/reset":
                    parser = new ResetParser(State, Text, Chat, ChatMember);
                    break;
                case "/show":
                    parser = new ShowParser(State, Text, Chat, ChatMember);
                    break;
                case "/all":
                    parser = new AllParser(State, Text, Chat, ChatMember);
                    break;
                case "/start":
                case "/help":
                default:
                    return Result.Fail<QueryObject>(ParseErrorMessage);    
            }
            return parser.GetQueryObject();
        }
    }
}

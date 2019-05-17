﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.Query
{
    public class QueryObject : IEquatable<QueryObject>
    {
        /// <summary>
        /// Query
        /// </summary>
        public QueryType Type { get; private set; }
        /// <summary>
        /// List used for the query
        /// </summary>
        public string ListName { get; private set; }
        /// <summary>
        /// Query's amount 
        /// </summary>
        public double Value { get; private set; }
        /// <summary>
        /// Query's description
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Query's tags
        /// </summary>
        public List<string> Tags { get; private set; }
        /// <summary>
        /// User/Group's chat Id
        /// </summary>
        public Chat Chat { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chat"></param>
        /// <param name="type"></param>
        /// <param name="listName"></param>
        /// <param name="value"></param>
        /// <param name="description"></param>
        /// <param name="tags"></param>
        public QueryObject(Chat chat, QueryType type, string listName, double value, string description, List<string> tags)
        {
            Type = type;
            ListName = listName;
            Value = value;
            Description = description;
            Tags = tags;
            Chat = chat;
        }

        public bool Equals(QueryObject other)
        {
            return other != null &&
                   Type == other.Type &&
                   Chat == other.Chat &&
                   ListName == other.ListName &&
                   Value == other.Value &&
                   Description == other.Description &&
                   Tags.SequenceEqual(other.Tags);
        }
    }
}

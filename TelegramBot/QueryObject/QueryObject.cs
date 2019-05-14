using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.QueryObject
{
    class QueryObject
    {
        public QueryType Type { get; private set; }
        public String ListName { get;  private set; }
        public double Value { get; private set; }
        public string Description { get; private set; }
        public List<string> Tags { get; private set; }

        private QueryObject(QueryType type, string listName, double value, string description, List<string> tags)
        {
            Type = type;
            ListName = listName;
            Value = value;
            Description = description;
            Tags = tags;
        }

        public static Result<QueryObject> BuildQueryObject(QueryType type, string listName, double value, string description, List<string> tags)
        {
            if (type == QueryType.None)
                return Result.Fail<QueryObject>("Type can't be set to None");
            if (string.IsNullOrEmpty(listName))
                return Result.Fail<QueryObject>("List name is empty");

            return Result.Ok<QueryObject>(
                    new QueryObject(
                            type,
                            listName, 
                            value, 
                            description, 
                            tags    
                   ));
        }
    }
}

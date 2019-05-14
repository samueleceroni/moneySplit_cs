using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace TelegramBot.QueryObject
{
    class QueryObjectBuilder
    {
        public QueryType Type { get; set; }
        public string ListName { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }

        public QueryObjectBuilder()
        {
            Tags = new List<string>();
            Type = QueryType.None;
        }

        public Result<QueryObject> Build()
        {
            return QueryObject.BuildQueryObject(
                    Type,
                    ListName,
                    Value,
                    Description,
                    Tags
                );
        }
    }
}

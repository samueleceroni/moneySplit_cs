using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController
{
    public class GeneralTransactionDouble
    {
        public int TransactionId { get => GT.TransactionId; }

        public double Amount { get => ((double)GT.Amount) / 100; }

        public string Description { get => GT.Description; }

        public int TransType { get => GT.TransType; }

        public System.Nullable<System.DateTime> Date { get => GT.Date; }

        public System.Nullable<int> DayRecurrence { get => GT.DayRecurrence; }

        public System.Nullable<int> MonthRecurrence { get => GT.MonthRecurrence; }

        public System.Nullable<System.DateTime> Time { get => GT.Time; }

        public System.Nullable<System.DateTime> StartDate { get => GT.StartDate; }

        public System.Nullable<System.DateTime> EndDate { get => GT.EndDate; }

        public int ListId { get => GT.ListId; }

        public System.Nullable<int> UserAuthor { get => GT.UserAuthor; }

        private GeneralTransaction GT { get; }

        public GeneralTransactionDouble(GeneralTransaction GT)
        {
            this.GT = GT;
        }
    }
}

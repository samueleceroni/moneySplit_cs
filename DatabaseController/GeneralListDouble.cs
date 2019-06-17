using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController
{
    public class GeneralListDouble
    {
        public int ListId { get => GL.ListId; }

        public int Vers { get => GL.Vers; }

        public string Name { get => GL.Name; }

        public int ContextId { get => GL.ContextId; }

        public string Iban { get => GL.Iban; }

        public string CF_owner { get => GL.CF_owner; }

        public int ListType { get => GL.ListType; }

        public double TotalAmount { get => ((double)GL.TotalAmount)/100; }

        private GeneralList GL { get; }

        public GeneralListDouble(GeneralList GL)
        {
            this.GL = GL;
        }
    }
}

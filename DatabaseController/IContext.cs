using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController
{
    interface IContext
    {
        void GetAllList();
        void NewList();
        void NewBankAccount();
    }
}

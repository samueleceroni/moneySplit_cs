using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DatabaseController;
using CSharpFunctionalExtensions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //var res = DatabaseController.DatabaseController.RegisterNewUser("chiaramongola", "1234567");
            var res1 = DatabaseController.DatabaseController.GroupAddUser("12345678", "1234567", true);
            //if (res.IsFailure)
            //{
            //    Console.WriteLine(res.Error);
            //}
            if (res1.IsFailure)
            {
                Console.WriteLine(res1.Error);
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoIt;
using System.Threading;
using System.Configuration;
using System.Configuration.Assemblies;
namespace TraderBoi
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Proven's trading bot";
            Console.BackgroundColor = System.ConsoleColor.DarkBlue;
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " Proven's trading bot starting now");
            Console.BackgroundColor = System.ConsoleColor.Black;
            Thread.Sleep(2000);
            if (AutoItX.WinActive("Path of") == 0)
                AutoItX.WinActivate("Path of");

            Logic.MainLogic();
            Console.ReadLine();
        }
    }
}

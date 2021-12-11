using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AutoIt;

namespace TraderBoi
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Proven's trading bot";
            Console.BackgroundColor = System.ConsoleColor.DarkBlue;
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " Proven's trading bot starting now");

            AutoItX.AutoItSetOption("MouseCoordMode", 0);
            Currency.InitialiseCurrencyList();

            Logic.ReLaunchGame();
            Console.BackgroundColor = System.ConsoleColor.Black;
            Thread.Sleep(2000);
            Logic.MainLogic();
            Console.ReadLine();
        }
    }
}

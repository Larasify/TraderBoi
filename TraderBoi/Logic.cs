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
    class Logic
    {
        public static List<Player> potential = new List<Player>();
        public static List<Player> invited = new List<Player>();

        public static string PoELogPath = ConfigurationManager.AppSettings["PoEPath"]+"logs/client.txt";
        public static string PoEExePath = ConfigurationManager.AppSettings["PoEPath"] + "PathOfExile.exe";

        public static int LastStockWritten = 6;
        public static double ChaosChanges = 0;
        public static List<int> startingstock = new List<int>();

        public static DateTime lastrelaunch = DateTime.Now;

        public static void MainLogic()
        {
            lastrelaunch = DateTime.Now;
            InitialiseStartingStock();
            while (true)
            {
                Thread.Sleep(300);
                Maintenance.CheckPotential(potential);
                if(Maintenance.CheckInvited(invited) == 1)
                {
                    Maintenance.InviteToParty(potential, invited);
                }
                Trade.CheckForTrade(invited);
                WriteCurrentStockChanges();
                if (1800 < (DateTime.Now - lastrelaunch).TotalSeconds)
                {
                    ReLaunchGame();
                    lastrelaunch = DateTime.Now;
                }
            }
        }

        public static void InitialiseStartingStock()
        {
            foreach(Currency c in Currency.CurrencyList)
            {
                startingstock.Add(c.InStock);
            }
        }

        public static void WriteCurrentStockChanges()
        {
            if (int.Parse(ConfigurationManager.AppSettings["CurrencyChangesWrittenToConsole"])<= LastStockWritten && invited.Count==0)
            {
                ChaosChanges = 0;
                for (int i = 0; i < Currency.CurrencyList.Count; i++)
                {
                    int CurrentStock = Currency.CurrencyList[i].InStock;
                    int StartingStock = startingstock[i];

                    if (StartingStock < CurrentStock)
                    {
                        Console.BackgroundColor = System.ConsoleColor.DarkGreen;
                        Console.Write(Currency.CurrencyList[i].CurrencyName+":"+CurrentStock+":+"+(CurrentStock-StartingStock));

                        foreach (SiteRate r in SiteSide.rates)
                        {
                            if (r.MyCurrencyName == Currency.CurrencyList[i].CurrencyName && r.TheirCurrencyName == "chaos")
                            {
                                ChaosChanges = ChaosChanges + ((CurrentStock - StartingStock)/r.Rate);
                            }
                        }

                        Console.Write("|");
                    }
                    if (CurrentStock < StartingStock)
                    {
                        Console.BackgroundColor = System.ConsoleColor.DarkRed;
                        Console.Write(Currency.CurrencyList[i].CurrencyName + ":" + CurrentStock + ":" + (CurrentStock - StartingStock));
                        foreach (SiteRate r in SiteSide.rates)
                        {
                            if (r.TheirCurrencyName == Currency.CurrencyList[i].CurrencyName && r.MyCurrencyName == "chaos")
                            {
                                ChaosChanges = ChaosChanges + ((CurrentStock - StartingStock) * r.Rate);
                            }
                        }
                        Console.Write("|");
                    }
                    if (Currency.CurrencyList[i].CurrencyName == "chaos")
                        ChaosChanges = ChaosChanges + (CurrentStock - StartingStock);
                }
                Console.BackgroundColor = System.ConsoleColor.DarkGreen;
                Console.Write("Profit:" + ChaosChanges);
                LastStockWritten = 0;
                Console.Write("\n");
                Console.BackgroundColor = System.ConsoleColor.Black;
            }
        }

        public static void ReaderThread()
        {
            using (FileStream fileStream = File.Open(PoELogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Console.BackgroundColor = System.ConsoleColor.DarkBlue;
                Console.WriteLine("Starting reader Thread");
                Console.BackgroundColor = System.ConsoleColor.Black;
                fileStream.Seek(0, SeekOrigin.End);
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (true)
                    {
                        Thread.Sleep(500);
                        string line = streamReader.ReadLine();
                        if (AutoItX.WinExists("Path of") == 0)
                        {
                            Console.BackgroundColor = System.ConsoleColor.DarkBlue;
                            Console.WriteLine("Closing reader thread because the game is closed");
                            Console.BackgroundColor = System.ConsoleColor.Black;
                            return;
                        }
                        if (line != null)
                        {
                            if (line.Contains("Hi, I'd like to buy your ") && line.Contains(" for my ") && line.Contains(" in "))
                            {
                                Regex regex = new Regex("@", RegexOptions.IgnoreCase);
                                MatchCollection matches = regex.Matches(line);
                                int fromcount = matches.Count;
                                if (1 < fromcount)
                                {
                                    continue;
                                }
                                String[] FirstSplit = line.Split(new string[] { "Hi, I'd like to buy your " },StringSplitOptions.None);
                                String[] SecondSplit = FirstSplit[1].Split(new string[] { " for my " }, StringSplitOptions.None);
                                String[] ThirdSplit = SecondSplit[1].Split(new string[] { " in " }, StringSplitOptions.None);
                                String newline = Regex.Replace(line, "<[^>]*>", "");
                                String TheirName = newline.Substring(newline.LastIndexOf("@From ") + 6, newline.LastIndexOf(": H") - (newline.LastIndexOf("@From ") + 6));

                                if (line.Contains("<") && line.Contains(">"))
                                {
                                    TheirName = newline.Substring(newline.LastIndexOf("@From ") + 7, newline.LastIndexOf(": H") - (newline.LastIndexOf("@From ") + 7));
                                }

                                int MyCurrencyAmount;
                                if (int.TryParse(SecondSplit[0].Split(new string[] { " " }, StringSplitOptions.None)[0], out int n)){
                                    MyCurrencyAmount = int.Parse(SecondSplit[0].Split(new string[] { " " }, StringSplitOptions.None)[0]);
                                }
                                else
                                {
                                    continue;
                                }

                                int TheirCurrencyAmount;
                                if (int.TryParse(ThirdSplit[0].Split(new string[] { " " }, StringSplitOptions.None)[0], out int m))
                                {
                                    TheirCurrencyAmount = int.Parse(ThirdSplit[0].Split(new string[] { " " }, StringSplitOptions.None)[0]);
                                }
                                else
                                {
                                    continue;
                                }
                                /*
                                String MyCurrencyName = SecondSplit[0].Split(new string[] { " " }, StringSplitOptions.None)[1];
                                String TheirCurrencyName = ThirdSplit[0].Split(new string[] { " " }, StringSplitOptions.None)[1];*/

                                string mystring = "your " + MyCurrencyAmount+" ";
                                string theirstring = "for my " + TheirCurrencyAmount+" ";

                                String MyCurrencyName = newline.Substring(newline.LastIndexOf(mystring)+mystring.Length, newline.LastIndexOf(" for my") - (newline.LastIndexOf(mystring)+mystring.Length));
                                String TheirCurrencyName = newline.Substring(newline.LastIndexOf(theirstring) + theirstring.Length, newline.LastIndexOf(" in") - (newline.LastIndexOf(theirstring) + theirstring.Length));

                                potential.Add(new Player(TheirName, TheirCurrencyName, MyCurrencyName, TheirCurrencyAmount, MyCurrencyAmount, DateTime.Now));
                            }

                            if (line.Contains("has joined the area.")){
                                String JoinerName = line.Substring(line.LastIndexOf(" : ")+3, line.LastIndexOf(" has joined the area.")- (line.LastIndexOf(" : ")+3));
                                foreach(Player p in invited){
                                    if(p.Name == JoinerName)
                                    {
                                        p.ZoneJoinTime = DateTime.Now;
                                        p.InZone = true;
                                    }
                                }
                            }

                            if (line.Contains("has left the area."))
                            {
                                String JoinerName = line.Substring(line.LastIndexOf(" : ") + 3, line.LastIndexOf(" has left the area.") - (line.LastIndexOf(" : ") + 3));
                                foreach (Player p in invited)
                                {
                                    if (p.Name == JoinerName)
                                    {
                                        p.ZoneJoinTime = DateTime.MinValue;
                                        p.InZone = false;
                                    }
                                }
                            }
                            if (line.Contains("Trade cancelled."))
                            {
                                if (0 < Trade.DeclinedIncomingTradeCount)
                                    Trade.DeclinedIncomingTradeCount--;
                                else
                                    Trade.TradeHasBeenCanceled = true;
                            }
                            if (line.Contains("Relaunch"))
                            {
                                ReLaunchGame();
                                Console.BackgroundColor = System.ConsoleColor.DarkBlue;
                                Console.WriteLine("Closing reader thread because the game is closed");
                                Console.BackgroundColor = System.ConsoleColor.Black;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static void ReLaunchGame()
        {
            if (AutoItX.WinExists("Path of") == 1) { 
                potential.Clear();
                invited.Clear();
                AutoItX.WinClose("Path of");
                Thread.Sleep(90000);
            }
            Thread siter = new Thread(new ThreadStart(SiteSide.Getsite));
            siter.Start();
            AutoItX.Run(PoEExePath, "");
            AutoItX.WinWait("Path of", "", 10);
            if (AutoItX.WinActive("Path of") == 0)
                AutoItX.WinActivate("Path of");
            Thread.Sleep(5000);
            while (ImageSearcher.UseImageSearch("img/login.png", "15", "0x00ff00") == null)
            {
                Thread.Sleep(500);
            }
            AutoItX.MouseMove(int.Parse(ConfigurationManager.AppSettings["LoginX"]), int.Parse(ConfigurationManager.AppSettings["LoginY"]), 2);
            AutoItX.MouseClick("LEFT");
            while (ImageSearcher.UseImageSearch("img/characterselect.png", "15", "0x00ff00") == null)
            {
                Thread.Sleep(500);
            }
            AutoItX.MouseMove(int.Parse(ConfigurationManager.AppSettings["PlayX"]), int.Parse(ConfigurationManager.AppSettings["PlayY"]), 2);
            AutoItX.MouseClick("LEFT");
            while (ImageSearcher.UseImageSearch("img/ingame.png", "15", "0x00ff00") == null)
            {
                Thread.Sleep(500);
            }
            AutoItX.Send("{ENTER}");
            AutoItX.Send("/hideout");
            AutoItX.Send("{ENTER}");
            Thread.Sleep(5000);
            while (ImageSearcher.UseImageSearch("img/ingame.png", "15", "0x00ff00") == null)
            {
                Thread.Sleep(500);
            }
            AutoItX.MouseMove(int.Parse(ConfigurationManager.AppSettings["InitialStashX"]), int.Parse(ConfigurationManager.AppSettings["InitialStashY"]), 2);
            AutoItX.MouseClick("LEFT");
            Thread.Sleep(3000);
            Maintenance.GetStock();
            Thread reader = new Thread(new ThreadStart(ReaderThread));
            reader.Start();
        }
    }
}

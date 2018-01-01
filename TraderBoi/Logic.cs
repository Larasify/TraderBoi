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

        public static string PoELogPath = ConfigurationManager.AppSettings["PoELogPath"];

        public static int LastStockWritten = 6;
        public static double ChaosChanges = 0;
        public static List<int> startingstock = new List<int>();

        public static void MainLogic()
        {
            //INITIALISATIONS
            Currency.InitialiseCurrencyList();
            Maintenance.GetStock();
            InitialiseStartingStock();

            Thread reader = new Thread(new ThreadStart(ReaderThread));
            reader.Start();
            Thread siter = new Thread(new ThreadStart(SiteSide.Getsite));
            siter.Start();

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

            }
        }

        private static void InitialiseStartingStock()
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
                Console.WriteLine("Starting reader Thread");
                fileStream.Seek(0, SeekOrigin.End);
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (true)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                        string line = streamReader.ReadLine();
                        if (line != null)
                        {
                            if (line.Contains("Hi, I'd like to buy your ") && line.Contains(" for my ") && line.Contains(" in "))
                            {
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
                                Trade.TradeHasBeenCanceled = true;
                            }
                        }
                    }
                }
            }
        }
    }
}

using AutoIt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraderBoi.Objects;
using static System.Configuration.ConfigurationManager;

namespace TraderBoi
{
    class Maintenance
    {
        public static int StashX = int.Parse(AppSettings["StashX"]);
        public static int StashY = int.Parse(AppSettings["StashY"]);
        public static int InventoryStartX1 = int.Parse(AppSettings["InventoryStartX1"]);
        public static int InventoryStartY1 = int.Parse(AppSettings["InventoryStartY1"]);
        public static int InventoryStartX2 = int.Parse(AppSettings["InventoryStartX2"]);
        public static int InventoryStartY2 = int.Parse(AppSettings["InventoryStartY2"]);
        public static int InventoryStep = int.Parse(AppSettings["InventoryStep"]);
        public static int partylimit = int.Parse(AppSettings["partylimit"]);
        
        public static Boolean DumpingInventory = false;

        public static void CheckPotential(List<Player> potential)
        {
            foreach(Player p in potential.ToList())
            {
                //If they have been waiting for too long
                foreach(Player i in Logic.invited)
                {
                    if(i.Name == p.Name)
                    {
                        Console.BackgroundColor = System.ConsoleColor.DarkRed;
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " removed from potential list because they are already invited.");
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        potential.Remove(p);
                        continue;
                    }
                }
                if (90 < (DateTime.Now - p.MessageTime).TotalSeconds)
                {
                    Console.BackgroundColor = System.ConsoleColor.DarkRed;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " "+p.Name+" removed from potential list because they have waited too long(90s).");
                    Console.BackgroundColor = System.ConsoleColor.Black;
                    potential.Remove(p);
                    continue;
                }

                foreach (SiteRate r in SiteSide.rates)
                {
                    if(r.MyCurrencyName == p.MyCurrencyName && r.TheirCurrencyName == p.TheirCurrencyName && r.Rate < (double) ((double) p.MyCurrencyAmount / (double) p.TheirCurrencyAmount))
                    {
                        Console.BackgroundColor = System.ConsoleColor.DarkRed;
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " removed from potential list because they have altered the deal rate in a way that is bad for us.");
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        potential.Remove(p);
                        continue;
                    }
                }
                foreach(Currency c in Currency.CurrencyList)
                {
                    if(c.CurrencyName == p.TheirCurrencyName && c.Limit < c.InStock + p.TheirCurrencyAmount)
                    {
                        Console.BackgroundColor = System.ConsoleColor.DarkRed;
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " removed from potential list because we are at our "+ c.CurrencyName+" limit");
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        potential.Remove(p);
                        continue;
                    }
                    if(c.CurrencyName == p.MyCurrencyName && c.InStock < p.MyCurrencyAmount)
                    {
                        Console.BackgroundColor = System.ConsoleColor.DarkRed;
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " removed from potential list because we are out of stock on " + c.CurrencyName);
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        potential.Remove(p);
                        continue;
                    }
                }
                foreach (SiteRate r in SiteSide.rates)
                {
                    foreach (SiteRate m in SiteSide.rates)
                    {
                        if (r.MyCurrencyName == p.MyCurrencyName && r.TheirCurrencyName == p.TheirCurrencyName && m.MyCurrencyName == p.TheirCurrencyName && m.TheirCurrencyName == p.MyCurrencyName)
                        {
                            p.Profit = (1-(m.Rate*r.Rate))*p.TheirCurrencyAmount;
                        }
                    }
                }
                }

        }

        public static int CheckInvited(List<Player> invited)
        {
            foreach (Player p in invited.ToList())
            {
                if (!FreeToUseChat())
                {
                    return 0;
                }
                if (p.TradeCompleted == true)
                {
                    Console.BackgroundColor = System.ConsoleColor.DarkMagenta;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " kicked from party because we completed a trade with them.");
                    Console.BackgroundColor = System.ConsoleColor.Black;
                    ClipboardManager.SetContents("/kick " + p.Name);
                    AutoItX.Send("{ENTER}");
                    AutoItX.Send("{CTRLDOWN}v{CTRLUP}");
                    AutoItX.Send("{ENTER}");
                    //I increase my currency when i complete a trade with them
                    foreach (Currency c in Currency.CurrencyList)
                    {
                        if (c.CurrencyName == p.TheirCurrencyName)
                        {
                            c.InStock = c.InStock + p.TheirCurrencyAmount;
                        }
                    }
                    Logic.LastStockWritten++;
                    invited.Remove(p);
                    continue;
                }
                if (90 < (DateTime.Now - p.InviteTime).TotalSeconds && p.InZone==false)
                {
                    Console.BackgroundColor = System.ConsoleColor.DarkRed;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " kicked from party because we have invited them too long ago and they haven't joined the zone(90s).");
                    Console.BackgroundColor = System.ConsoleColor.Black;
                    ClipboardManager.SetContents("/kick " + p.Name);
                    AutoItX.Send("{ENTER}");
                    AutoItX.Send("{CTRLDOWN}v{CTRLUP}");
                    AutoItX.Send("{ENTER}");
                    //I increase my currency back from when i invited them because the trade failed
                    foreach (Currency c in Currency.CurrencyList)
                    {
                        if (c.CurrencyName == p.MyCurrencyName)
                        {
                            c.InStock = c.InStock + p.MyCurrencyAmount;
                        }
                    }

                    invited.Remove(p);
                    continue;
                }
            }
            if (invited.Count < partylimit)
                return 1;
            else return 0;
        }

        public static void InviteToParty(List<Player> potential, List<Player> invited)
        {
            while (invited.Count < partylimit )
            {
                if (!FreeToUseChat())
                {
                    return;
                }
                if (potential.Count == 0)
                {
                    return;
                }
                int i = 0;
                for(int j = 0; j < potential.Count; j++)
                {
                    if (potential[i].Profit <= potential[j].Profit)
                    {
                        i = j;
                    }
                }
                if(potential[i].Profit == 0)
                {
                    return;
                }
                while(2 < (DateTime.Now - potential[i].MessageTime).TotalSeconds)
                {
                    Thread.Sleep(2000);
                }
                ClipboardManager.SetContents("/invite " + potential[i].Name);
                AutoItX.Send("{ENTER}");
                AutoItX.Send("{CTRLDOWN}v{CTRLUP}");
                AutoItX.Send("{ENTER}");

                Console.BackgroundColor = System.ConsoleColor.Blue;
                Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + potential[i].Name + " invited to the party.");
                Console.BackgroundColor = System.ConsoleColor.Black;
                //I decrease my currency when i invite them
                foreach (Currency c in Currency.CurrencyList)
                {
                    if (c.CurrencyName == potential[i].MyCurrencyName)
                    {
                        c.InStock = c.InStock - potential[i].MyCurrencyAmount;
                    }
                }

                potential[i].InviteTime = DateTime.Now;
                invited.Add(potential[i]);
                potential.RemoveAt(i);
            }
        }

        public static void GetStock()
        {
            Console.BackgroundColor = System.ConsoleColor.DarkGreen;
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " Getting Stock");
            Console.BackgroundColor = System.ConsoleColor.Black;
            Random rnd = new Random();
            AutoItX.Send("{SPACE}");
            Thread.Sleep(rnd.Next(280,350));
            AutoItX.AutoItSetOption("MouseCoordMode", 0);
            AutoItX.MouseMove(StashX, StashY, 2);
            AutoItX.MouseClick("LEFT");
            Thread.Sleep(rnd.Next(280, 350));
            foreach (Currency c in Currency.CurrencyList)
            {
                AutoItX.MouseMove(c.StashX, c.StashY, 5);
                ClipboardManager.SetEmpty();
                AutoItX.Send("{CTRLDOWN}c{CTRLUP}");
                ItemStack a = GetCurrencyFromClipboard();
                if (a!=null)
                {
                    c.InStock = a.ItemAmount;
                }
                Thread.Sleep(rnd.Next(100, 120));
            }
            AutoItX.Send("{SPACE}");
 
        }

        public static ItemStack GetCurrencyFromClipboard()
        {
            String[] AllLines = ClipboardManager.GetContents().Split(new [] { Environment.NewLine }, StringSplitOptions.None);
            if (AllLines.Length < 3)
            {
                return null;
            }
            return new ItemStack(AllLines[1], int.Parse(AllLines[3].Substring(AllLines[3].LastIndexOf("e: ") + 3, AllLines[3].LastIndexOf("/") - (AllLines[3].LastIndexOf("e: ") + 3))));
        }

        public static void DumpInventory()
        {
            DumpingInventory = true;
            Random rnd = new Random();
            AutoItX.Send("{SPACE}");
            Thread.Sleep(rnd.Next(280, 350));
            AutoItX.AutoItSetOption("MouseCoordMode", 0);
            AutoItX.MouseMove(StashX, StashY, 2);
            AutoItX.MouseClick("LEFT");
            Thread.Sleep(rnd.Next(280, 350));
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Rectangle b = AutoItX.WinGetPos("[ACTIVE]");

                    int xCoord1 = InventoryStartX1 + (i * InventoryStep) + b.Left + 7;
                    int yCoord1 = InventoryStartY1 + (j * InventoryStep) + b.Top;
                    int xCoord2 = InventoryStartX2 + (i * InventoryStep) + b.Left + 7;
                    int yCoord2 = InventoryStartY2 + (j * InventoryStep) + b.Top;

                    string image = ("img/empty.png");
                    string[] results = ImageSearcher.UseImageSearchArea(xCoord1, yCoord1, xCoord2, yCoord2, image, "15", "0x00ff00");
                    if (results == null)
                    {
                        AutoItX.MouseMove(InventoryStartX1 + (i * InventoryStep) + 15, InventoryStartY1 + (j * InventoryStep) + 15, 2);
                        AutoItX.Send("{CTRLDOWN}");
                        AutoItX.MouseClick("LEFT");
                        AutoItX.Send("{CTRLUP}");
                    }
                }
            }
            AutoItX.Send("{SPACE}");
            DumpingInventory = false;
        }

        public static Boolean FreeToUseChat()
        {
            if (Trade.IsTradeSent() || Trade.GetCurrencyWorking || DumpingInventory)
                return false;
            else
                return true;
        }
    }
}

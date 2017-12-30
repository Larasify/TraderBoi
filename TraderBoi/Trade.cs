using AutoIt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraderBoi.Objects;
using static System.Configuration.ConfigurationManager;

namespace TraderBoi
{
    class Trade
    {
        public static int StashX = int.Parse(AppSettings["StashX"]);
        public static int StashY = int.Parse(AppSettings["StashY"]);

        public static int AcceptButtonX = int.Parse(AppSettings["AcceptButtonX"]);
        public static int AcceptButtonY = int.Parse(AppSettings["AcceptButtonY"]);

        public static int InventoryStartX1 = int.Parse(AppSettings["InventoryStartX1"]);
        public static int InventoryStartY1 = int.Parse(AppSettings["InventoryStartY1"]);
        public static int InventoryStartX2 = int.Parse(AppSettings["InventoryStartX2"]);
        public static int InventoryStartY2 = int.Parse(AppSettings["InventoryStartY2"]);
        public static int InventoryStep = int.Parse(AppSettings["InventoryStep"]);
        public static int TradeInventoryStartX1 = int.Parse(AppSettings["TradeInventoryStartX1"]);
        public static int TradeInventoryStartY1 = int.Parse(AppSettings["TradeInventoryStartY1"]);
        public static int TradeInventoryStartX2 = int.Parse(AppSettings["TradeInventoryStartX2"]);
        public static int TradeInventoryStartY2 = int.Parse(AppSettings["TradeInventoryStartY2"]);

        public static int CancelButtonX1 = int.Parse(AppSettings["CancelButtonX1"]);
        public static int CancelButtonY1 = int.Parse(AppSettings["CancelButtonY1"]);
        public static int CancelButtonX2 = int.Parse(AppSettings["CancelButtonX2"]);
        public static int CancelButtonY2 = int.Parse(AppSettings["CancelButtonY2"]);

        public static Boolean TradeHasBeenCanceled = false;

        public static Boolean TradeThreadWorking = false;

        public static Boolean GetCurrencyWorking = false;

        public static DateTime lasttradesent = DateTime.Now;

        public static void CheckForTrade(List<Player> invited)
        {
            foreach (Player p in invited)
            {
                if (p.InZone && !p.TradeCompleted &&  TradeThreadWorking == false)
                {
                    TradeThreadWorking = true;

                    var t = new Thread(() => TradeThread(p));
                    t.Start();
                }
            }
        }

        private static void TradeThread(Player p)
        {
            GetMyCurrencyFromStash(p.MyCurrencyName, p.MyCurrencyAmount);
            GetCurrencyWorking = false;
            lasttradesent = DateTime.Now;
            while (true)
            {
                if(p.InZone == false)
                {
                    Console.BackgroundColor = System.ConsoleColor.DarkRed;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " kicked from party they came to zone and left without trading.");
                    Console.BackgroundColor = System.ConsoleColor.Black;
                    Maintenance.DumpInventory();
                    p.InZone = false;
                    TradeThreadWorking = false;
                    return;
                }
                if (140 < (DateTime.Now - p.InviteTime).TotalSeconds && !IsTradeSent() && !InTradeWith())
                {
                    Console.BackgroundColor = System.ConsoleColor.DarkRed;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " " + p.Name + " kicked from party because they havent completed a trade for too long (140s).");
                    Console.BackgroundColor = System.ConsoleColor.Black;
                    Maintenance.DumpInventory();
                    p.InZone = false;
                    TradeThreadWorking = false;
                    return;
                }
                if (InTradeWith())
                {
                    if (CompleteTrade(p))
                    {
                        Maintenance.DumpInventory();
                        //Stock Changes were here
                        p.TradeCompleted = true;
                        TradeThreadWorking = false;
                        return;
                    }
                }

                if (!IsTradeSent() && 5 < (DateTime.Now - lasttradesent).TotalSeconds)
                {
                    lasttradesent = DateTime.Now;
                    AutoItX.Send("{SPACE}");
                    CancelAllIncomingTrades();
                    ClipboardManager.SetContents("/tradewith " + p.Name);
                    AutoItX.Send("{ENTER}");
                    AutoItX.Send("{CTRLDOWN}v{CTRLUP}");
                    AutoItX.Send("{ENTER}");
                    Thread.Sleep(500);
                }

                Thread.Sleep(1000);
            }
        }

        public static Boolean CompleteTrade(Player p)
        {
            TradeHasBeenCanceled = false;
            String TheirIngameName = null;
            foreach(Currency c in Currency.CurrencyList)
            {
                if(p.TheirCurrencyName == c.CurrencyName)
                {
                    TheirIngameName = c.IngameName;
                }
            }
            // PUT MY SHIT
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 5; j++)
                {

                    Rectangle b = AutoItX.WinGetPos("[ACTIVE]");
                    Random rnd = new Random();
                    int xCoord1 = InventoryStartX1 + (i * InventoryStep) + b.Left + 7;
                    int yCoord1 = InventoryStartY1 + (j * InventoryStep) + b.Top;
                    int xCoord2 = InventoryStartX2 + (i * InventoryStep) + b.Left + 7;
                    int yCoord2 = InventoryStartY2 + (j * InventoryStep) + b.Top;

                    string image = ("img/empty.png");
                    string[] results = ImageSearcher.UseImageSearchArea(xCoord1, yCoord1, xCoord2, yCoord2, image, "15", "0x00ff00");
                    if (results == null && InTradeWith())
                    {
                        if (TradeAltered() || !InTradeWith())
                        {
                            AutoItX.Send("{SPACE}");
                            return false;
                        }
                        Thread.Sleep(rnd.Next(35, 85));
                        AutoItX.MouseMove(InventoryStartX1 + (i * InventoryStep) + 15, InventoryStartY1 + (j * InventoryStep) + 15, 2);
                        AutoItX.Send("{CTRLDOWN}");
                        AutoItX.MouseClick("LEFT");
                        AutoItX.Send("{CTRLUP}");
                    }
                }
            }
            //CHECK FOR THEIR SHIT
            int AmountTheyPut = 0;
            while (AmountTheyPut < p.TheirCurrencyAmount)
            {
                    if (TradeAltered() || !InTradeWith())
                    {
                        AutoItX.Send("{SPACE}");
                        return false;
                    }
                AmountTheyPut = 0;
                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Rectangle b = AutoItX.WinGetPos("[ACTIVE]");

                        int xCoord1 = TradeInventoryStartX1 + (i * InventoryStep) + b.Left + 7;
                        int yCoord1 = TradeInventoryStartY1 + (j * InventoryStep) + b.Top;
                        int xCoord2 = TradeInventoryStartX2 + (i * InventoryStep) + b.Left + 7;
                        int yCoord2 = TradeInventoryStartY2 + (j * InventoryStep) + b.Top;

                        string image = ("img/empty.png");
                        string[] results = ImageSearcher.UseImageSearchArea(xCoord1, yCoord1, xCoord2, yCoord2, image, "15", "0x00ff00");
                        if (results == null && InTradeWith())
                        {
                            AutoItX.MouseMove(TradeInventoryStartX1 + (i * InventoryStep) + 15, TradeInventoryStartY1 + (j * InventoryStep) + 15, 2);
                            ClipboardManager.SetEmpty();
                            AutoItX.Send("{CTRLDOWN}c{CTRLUP}");
                            ItemStack a = Maintenance.GetCurrencyFromClipboard();
                            if (a != null)
                            {
                                if (a.ItemName == TheirIngameName)
                                {
                                    AmountTheyPut = AmountTheyPut + a.ItemAmount;
                                }
                            }
                        }
                    }
                }
            }
            AutoItX.MouseMove(AcceptButtonX, AcceptButtonY, 2);
            if (TradeAltered() || !InTradeWith())
            {
                AutoItX.Send("{SPACE}");
                return false;
            }
            AutoItX.MouseClick("LEFT");
            while(true)
            {
                if (TradeAltered())
                {
                    lasttradesent = DateTime.Now;
                    AutoItX.Send("{SPACE}");
                    return false;
                }
                if (!InTradeWith() && TradeHasBeenCanceled == false)
                {
                    AutoItX.Send("{SPACE}");
                    return true;
                }
                else if (!InTradeWith() && TradeHasBeenCanceled == true)
                {
                    AutoItX.Send("{SPACE}");
                    return false;
                }
                Thread.Sleep(1500);
            }
        }

        public static void GetMyCurrencyFromStash(string myCurrencyName, int myCurrencyAmount)
        {
            GetCurrencyWorking = true;
            Console.BackgroundColor = System.ConsoleColor.DarkMagenta;
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " Trade - Getting my currency out of stash.");
            Console.BackgroundColor = System.ConsoleColor.Black;

            Random rnd = new Random();
            AutoItX.Send("{SPACE}");
            Thread.Sleep(rnd.Next(280, 350));
            AutoItX.AutoItSetOption("MouseCoordMode", 0);
            AutoItX.MouseMove(StashX, StashY, 2);
            AutoItX.MouseClick("LEFT");
            Thread.Sleep(rnd.Next(280, 350));
            foreach (Currency c in Currency.CurrencyList)
            {
                if (c.CurrencyName == myCurrencyName)
                {
                    AutoItX.MouseMove(c.StashX, c.StashY, 2);
                    int CurrencyNeededLeft = myCurrencyAmount;
                    while (c.StackLimit <= CurrencyNeededLeft)
                    {
                        AutoItX.Send("{CTRLDOWN}");
                        AutoItX.MouseClick("LEFT");
                        AutoItX.Send("{CTRLUP}");
                        CurrencyNeededLeft = CurrencyNeededLeft - c.StackLimit;
                        Thread.Sleep(rnd.Next(150, 210));
                    }
                    if (CurrencyNeededLeft == 0)
                    {
                        AutoItX.Send("{SPACE}");
                        return;
                    }
                    AutoItX.Send("{SHIFTDOWN}");
                    AutoItX.MouseClick("LEFT");
                    AutoItX.Send("{SHIFTUP}");
                    AutoItX.Send(CurrencyNeededLeft.ToString());
                    AutoItX.Send("{ENTER}");

                    for (int i = 0; i < 12; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            /*
                            SetProcessDPIAware();
                            Rectangle b;
                            GetWindowRect(GetForegroundWindow(), out b);

                            OUtside
                                    [DllImport("user32.dll")]
                            private static extern bool SetProcessDPIAware();
                            [DllImport("user32.dll")]
                            static extern IntPtr GetForegroundWindow();
                            [DllImport("user32.dll")]
                            [return: MarshalAs(UnmanagedType.Bool)]
                             static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);
                            */

                            Rectangle b =  AutoItX.WinGetPos("[ACTIVE]");

                            int xCoord1 = InventoryStartX1 + (i * InventoryStep) + b.Left + 7;
                            int yCoord1 = InventoryStartY1 + (j * InventoryStep) + b.Top;
                            int xCoord2 = InventoryStartX2 + (i * InventoryStep) + b.Left + 7;
                            int yCoord2 = InventoryStartY2 + (j * InventoryStep) + b.Top;

                            string image = ("img/empty.png");
                            string[] results = ImageSearcher.UseImageSearchArea(xCoord1,yCoord1,xCoord2,yCoord2, image, "15", "0x00ff00");
                            if (results != null)
                            {
                               
                                AutoItX.MouseMove(InventoryStartX1 + (i * InventoryStep)+15, InventoryStartY1 + (j * InventoryStep)+15, 4);
                                AutoItX.MouseClick("LEFT");
                                Thread.Sleep(500);
                                AutoItX.Send("{SPACE}");
                                return;

                            }
                        }
                    }

                }
            }
        }

        public static void CancelAllIncomingTrades()
        {
            Rectangle b = AutoItX.WinGetPos("[ACTIVE]");
            string image = ("img/decline.png");
            string[] results = ImageSearcher.UseImageSearch(image, "15", "0x00ff00");
            while(results != null)
            {
                AutoItX.MouseMove(int.Parse(results[1])-b.Left, int.Parse(results[2])-b.Top);
                AutoItX.MouseClick("LEFT");
                Thread.Sleep(500);
                results = ImageSearcher.UseImageSearch(image, "15", "0x00ff00");
            }
        }

        public static Boolean IsTradeSent()
        {
            string image = ("img/senttrade.png");
            string[] results = ImageSearcher.UseImageSearch(image, "15", "0x00ff00");
            if (results == null)
                return false;
            else
                return true;
        }

        private static Boolean InTradeWith()
        {
            string image = ("img/intrade.png");
            Rectangle b = AutoItX.WinGetPos("[ACTIVE]");
            int xCoord1 = CancelButtonX1 + b.Left + 7;
            int yCoord1 = CancelButtonY1 + b.Top;
            int xCoord2 = CancelButtonX2 + b.Left + 7;
            int yCoord2 = CancelButtonY2 + b.Top;

            string[] results = ImageSearcher.UseImageSearchArea(xCoord1, yCoord1, xCoord2, yCoord2, image, "15", "0x00ff00");
            if (results == null)
                return false;
            else
                return true;
        }

        private static Boolean TradeAltered()
        {
            string image = ("img/tradealtered.png");
            Rectangle b = AutoItX.WinGetPos("[ACTIVE]");
            int xCoord1 = 34 + b.Left + 7;
            int yCoord1 = 148 + b.Top;
            int xCoord2 = 525 + b.Left + 7;
            int yCoord2 = 384 + b.Top;

            string[] results = ImageSearcher.UseImageSearchArea(xCoord1, yCoord1, xCoord2, yCoord2, image, "15", "0x00ff00");
            if (results == null)
                return false;
            else
                return true;
        }
        

    }
}

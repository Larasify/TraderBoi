using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Configuration.ConfigurationManager;
using System.Numerics;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace TraderBoi
{
    class SiteRate
    {
        public String MyCurrencyName;
        public String TheirCurrencyName;
        public int MyCurrencyAmount;
        public int TheirCurrencyAmount;
        public double Rate;

        public SiteRate(string myCurrencyName, string theirCurrencyName, int myCurrencyAmount, int theirCurrencyAmount, double rate)
        {
            MyCurrencyName = myCurrencyName;
            TheirCurrencyName = theirCurrencyName;
            MyCurrencyAmount = myCurrencyAmount;
            TheirCurrencyAmount = theirCurrencyAmount;
            Rate = rate;
        }
    }

    class SiteSide
    {
        private static string leaguename = AppSettings["leaguename"];
        public static List<SiteRate> rates = new List<SiteRate>();
        private static int working_chrome_Threads;
        public static void Getsite()
        {
            rates.Clear();
            Console.WriteLine("-------------------------------Changing Rates-----------------------------------------");
            GetActiveTrades();
            PushToSite();
            FixSextants();
            Console.WriteLine("-------------------------------Finished Changing Rates-----------------------------------------");
        }
            
        private static void FixSextants()
        {
            foreach(SiteRate c in rates)
            {
                if (c.MyCurrencyName == "apprentice")
                    c.MyCurrencyName = "apprentice sextant";
                if (c.MyCurrencyName == "journeyman")
                    c.MyCurrencyName = "journeyman sextant";
                if (c.MyCurrencyName == "master")
                    c.MyCurrencyName = "master sextant";
                if (c.TheirCurrencyName == "apprentice")
                    c.TheirCurrencyName = "apprentice sextant";
                if (c.TheirCurrencyName == "journeyman")
                    c.TheirCurrencyName = "journeyman sextant";
                if (c.TheirCurrencyName == "master")
                    c.TheirCurrencyName = "master sextant";
            }
        }
        private static void PushToSite()
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArgument("--headless");
            IWebDriver driver = new ChromeDriver(option);
            driver.Navigate().GoToUrl("http://currency.poe.trade/shop");
            Cookie apikeycookie = new Cookie("apikey", AppSettings["apikey"]);
            driver.Manage().Cookies.AddCookie(apikeycookie);
            driver.Navigate().GoToUrl("http://currency.poe.trade/shop?league=" + leaguename);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            Console.WriteLine(driver.Title);
            foreach (SiteRate c in rates)
            {
                if (c.MyCurrencyName == "gcp")
                    c.MyCurrencyName = "gemcutter";
                if (c.MyCurrencyName == "chrome")
                    c.MyCurrencyName = "chromatic";
                if (c.TheirCurrencyName == "gcp")
                    c.TheirCurrencyName = "gemcutter";
                if (c.TheirCurrencyName == "chrome")
                    c.TheirCurrencyName = "chromatic";
            }
            for (int i = 1; i < rates.Count + 1; i++)
            {
                String MySideName = (String)executor.ExecuteScript("return document.getElementsByClassName('row offer')[" + i + "].childNodes[3].childNodes[2].firstChild.text");
                String TheirSideName = (String)executor.ExecuteScript("return document.getElementsByClassName('row offer')[" + i + "].childNodes[9].childNodes[2].firstChild.text");

                for (int j = 0; j < rates.Count; j++)
                {
                    if (MySideName.ToLower().Contains(rates[j].MyCurrencyName.ToLower()) && TheirSideName.ToLower().Contains(rates[j].TheirCurrencyName.ToLower()))
                    {
                        executor.ExecuteScript("return document.getElementsByClassName('row offer')[" + i + "].childNodes[5].childNodes[1].value = " + rates[j].MyCurrencyAmount);
                        executor.ExecuteScript("return document.getElementsByClassName('row offer')[" + i + "].childNodes[7].childNodes[1].value = " + rates[j].TheirCurrencyAmount);
                    }
                }
            }
            foreach (SiteRate c in rates)
            {
                if (c.MyCurrencyName == "gemcutter")
                    c.MyCurrencyName = "gcp";
                if (c.MyCurrencyName == "chromatic")
                    c.MyCurrencyName = "chrome";
                if (c.TheirCurrencyName == "gemcutter")
                    c.TheirCurrencyName = "gcp";
                if (c.TheirCurrencyName == "chromatic")
                    c.TheirCurrencyName = "chrome";
            }
            executor.ExecuteScript("document.getElementsByClassName('search button')[0].click()");
            driver.Quit();
        }

        public static void GetActiveTrades()
        {
            foreach(Currency c in Currency.CurrencyList)
            {
                foreach (Currency p in Currency.CurrencyList)
                {
                    if (AppSettings[c.CurrencyName +"+" +p.CurrencyName] == "True")
                    {
                        while (2 < working_chrome_Threads)
                        {
                            Thread.Sleep(500);
                        }
                        var t = new Thread(() => FindSuitableTradeRate(c, p));
                        t.Start();
                        working_chrome_Threads++;
                    }
                }
            }
            while (working_chrome_Threads != 0)
            {
                Thread.Sleep(1000);
            }
        }

        private static void FindSuitableTradeRate(Currency c, Currency p)
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArgument("--headless");
            var driver1 = new ChromeDriver(option);
            var driver2 = new ChromeDriver(option);
            IJavaScriptExecutor executor1 = (IJavaScriptExecutor)driver1;
            IJavaScriptExecutor executor2 = (IJavaScriptExecutor)driver2;
            String FirstLink = "http://currency.poe.trade/search?league=" + leaguename + "&online=x&want=" + c.PoETradeCode + "&have=" + p.PoETradeCode;
            String SecondLink = "http://currency.poe.trade/search?league=" + leaguename + "&online=x&want=" + p.PoETradeCode + "&have=" + c.PoETradeCode;
            driver1.Navigate().GoToUrl(FirstLink);
            driver2.Navigate().GoToUrl(SecondLink);

            int i = int.Parse(AppSettings["MarketStanding"]); ; //CHANGE THIS TO CHANGE WHERE YOUR SHIT APPEARS
            int j = int.Parse(AppSettings["MarketStanding"]); ;
            while (true)
            {
                String[] Anan = executor1.ExecuteScript("return document.getElementsByClassName('displayoffer')["+i+"].childNodes[1].childNodes[3].childNodes[1].childNodes[1].childNodes[3].childNodes[5].innerHTML").ToString().Split(new string[] { " ⇐ " }, StringSplitOptions.None);
                String FirstMyCurrencyName = executor1.ExecuteScript("return document.getElementsByClassName('displayoffer')[" + i + "].childNodes[1].childNodes[3].childNodes[1].childNodes[1].childNodes[1].innerHTML").ToString();
                String FirstTheirCurrencyName = executor1.ExecuteScript("return document.getElementsByClassName('displayoffer')[" + i + "].childNodes[1].childNodes[3].childNodes[1].childNodes[1].childNodes[5].innerHTML").ToString();
                FirstMyCurrencyName = Regex.Replace(FirstMyCurrencyName, "\\s+", "");
                FirstTheirCurrencyName = Regex.Replace(FirstTheirCurrencyName, "\\s+", "");
                if (Anan[0].Contains(".") || Anan[1].Contains("."))
                {
                    if (i == j)
                        i++;
                    if (i < j)
                        i++;
                    if (j < i)
                        j++;
                    continue;
                }
                int FirstMyCurrencyAmount = int.Parse(Anan[0]);
                int FirstTheirCurrencyAmount = int.Parse(Anan[1]);

                int ebob = (int) BigInteger.GreatestCommonDivisor(FirstMyCurrencyAmount, FirstTheirCurrencyAmount);

                double FirstRate = (double)(FirstMyCurrencyAmount / ebob) / (FirstTheirCurrencyAmount / ebob);


                SiteRate firstrate = new SiteRate(FirstMyCurrencyName, FirstTheirCurrencyName, FirstMyCurrencyAmount / ebob, FirstTheirCurrencyAmount / ebob, FirstRate);

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                String[] Anan2 = executor2.ExecuteScript("return document.getElementsByClassName('displayoffer')[" + j + "].childNodes[1].childNodes[3].childNodes[1].childNodes[1].childNodes[3].childNodes[5].innerHTML").ToString().Split(new string[] { " ⇐ " }, StringSplitOptions.None);
                String SecondMyCurrencyName = executor2.ExecuteScript("return document.getElementsByClassName('displayoffer')[" + j + "].childNodes[1].childNodes[3].childNodes[1].childNodes[1].childNodes[1].innerHTML").ToString();
                String SecondTheirCurrencyName = executor2.ExecuteScript("return document.getElementsByClassName('displayoffer')[" + j + "].childNodes[1].childNodes[3].childNodes[1].childNodes[1].childNodes[5].innerHTML").ToString();
                SecondMyCurrencyName = Regex.Replace(SecondMyCurrencyName, "\\s+", "");
                SecondTheirCurrencyName = Regex.Replace(SecondTheirCurrencyName, "\\s+", "");
                if (Anan2[0].Contains(".") || Anan2[1].Contains("."))
                {
                    if (i == j)
                        i++;
                    if (i < j)
                        i++;
                    if (j < i)
                        j++;
                    continue;
                }
                int SecondMyCurrencyAmount = int.Parse(Anan2[0]);
                int SecondTheirCurrencyAmount = int.Parse(Anan2[1]);

                int ebob2 = (int)BigInteger.GreatestCommonDivisor(SecondMyCurrencyAmount, SecondTheirCurrencyAmount);

                double SecondRate = (double)(SecondMyCurrencyAmount / ebob2) / (SecondTheirCurrencyAmount / ebob2);

                SiteRate secondrate = new SiteRate(SecondMyCurrencyName, SecondTheirCurrencyName, SecondMyCurrencyAmount / ebob2, SecondTheirCurrencyAmount / ebob2, SecondRate);
                //////////////////////////////////////////////////////////////////////////////////////////////////
                //2 BASAMAK
                if (FirstRate * SecondRate < 1 && firstrate.MyCurrencyAmount < 100 && firstrate.TheirCurrencyAmount < 100 && secondrate.MyCurrencyAmount < 100 && secondrate.TheirCurrencyAmount < 100)
                {
                    rates.Add(firstrate);
                    rates.Add(secondrate);
                    Console.BackgroundColor = System.ConsoleColor.DarkGreen;
                    Console.WriteLine(FirstMyCurrencyName + " " + FirstMyCurrencyAmount / ebob + " " + FirstTheirCurrencyName + " " + FirstTheirCurrencyAmount / ebob + " " + FirstRate);
                    Console.WriteLine(SecondMyCurrencyName + " " + SecondMyCurrencyAmount / ebob2 + " " + SecondTheirCurrencyName + " " + SecondTheirCurrencyAmount / ebob2 + " " + SecondRate);
                    Console.BackgroundColor = System.ConsoleColor.Black;
                    driver1.Quit();
                    driver2.Quit();
                    working_chrome_Threads--;
                    return;
                }
                else
                {
                    if (i == j)
                        i++;
                    if (i < j)
                        i++;
                    if (j < i)
                        j++;
                }
            }

        }

    }
}

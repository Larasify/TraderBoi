using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Configuration.ConfigurationManager;
namespace TraderBoi
{
    class Currency
    {
        public string CurrencyName;
        public int StackLimit;
        public String IngameName;
        public int PoETradeCode;
        public int StashX;
        public int StashY;
        public int Limit;
        public int InStock = 0;
        public static List<Currency> CurrencyList = new List<Currency>();
        public static Dictionary<string, double> Rates = new Dictionary<string, double>();

        public Currency(string currencyName, int stackLimit, string ıngameName, int poETradeCode)
        {
            CurrencyName = currencyName;
            StackLimit = stackLimit;
            IngameName = ıngameName;
            PoETradeCode = poETradeCode;
        }

        public static void InitialiseCurrencyList() {

            Console.BackgroundColor = System.ConsoleColor.DarkBlue;
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " Initialising Currency List");
            Console.BackgroundColor = System.ConsoleColor.Black;
            CurrencyList.Add(new Currency("alteration", 20, "Orb of Alteration",1)); //0
            CurrencyList.Add(new Currency("fusing", 20, "Orb of Fusing",2));
            CurrencyList.Add(new Currency("alchemy", 10, "Orb of Alchemy",3));
            CurrencyList.Add(new Currency("chaos", 10, "Chaos Orb",4));
            CurrencyList.Add(new Currency("gcp", 20, "Gemcutter's Prism",5));
            CurrencyList.Add(new Currency("exalted", 10, "Exalted Orb",6)); //5
            CurrencyList.Add(new Currency("chrome", 20, "Chromatic Orb",7));
            CurrencyList.Add(new Currency("jeweller", 20, "Jeweller's Orb",8));
            CurrencyList.Add(new Currency("chance", 10, "Orb of Chance",9));
            CurrencyList.Add(new Currency("chisel", 20, "Cartographer's Chisel",10));
            CurrencyList.Add(new Currency("scouring", 30, "Orb of Scouring",11)); //10
            CurrencyList.Add(new Currency("blessed", 20, "Blessed Orb",12));
            CurrencyList.Add(new Currency("regret", 40, "Orb of Regret",13));
            CurrencyList.Add(new Currency("regal", 10, "Regal Orb",14));
            CurrencyList.Add(new Currency("divine", 10, "Divine Orb",15));
            CurrencyList.Add(new Currency("vaal", 10, "Vaal Orb",16)); //15
            CurrencyList.Add(new Currency("bauble", 20, "Glassblower's Bauble",20));
            CurrencyList.Add(new Currency("silver", 30, "Silver Coin",35));
            CurrencyList.Add(new Currency("wisdom", 40, "Scroll of Wisdom",17)); //18
            CurrencyList.Add(new Currency("apprentice sextant", 10, "Apprentice Cartographer's Sextant",45));
            CurrencyList.Add(new Currency("journeyman sextant", 10, "Journeyman Cartographer's Sextant",46));
            CurrencyList.Add(new Currency("master sextant", 10, "Master Cartographer's Sextant",47));
            CurrencyList.Add(new Currency("Orb of Annulment", 20, "Orb of Annulment",513)); //22

            foreach(Currency c in CurrencyList)
            {
                c.StashX = int.Parse(AppSettings[c.CurrencyName + "X"]);
                c.StashY = int.Parse(AppSettings[c.CurrencyName + "Y"]);
                if(int.TryParse(AppSettings[c.CurrencyName + "Limit"],out int n))
                    c.Limit = int.Parse(AppSettings[c.CurrencyName + "Limit"]);
            }
        }

    }
}

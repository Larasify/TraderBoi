using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderBoi
{
    class Player
    {
        public String Name;
        public String TheirCurrencyName;
        public String MyCurrencyName;
        public int TheirCurrencyAmount;
        public int MyCurrencyAmount;
        public DateTime MessageTime;
        public DateTime InviteTime;
        public DateTime ZoneJoinTime;
        public Boolean InZone;
        public Boolean TradeCompleted;
        public int AmountOfTimesTradeCancelled = 0;
        public double Profit=0;

        public Player(string name, string theirCurrencyName, string myCurrencyName, int theirCurrencyAmount, int myCurrencyAmount, DateTime messageTime)
        {
            Name = name;
            TheirCurrencyName = theirCurrencyName;
            MyCurrencyName = myCurrencyName;
            TheirCurrencyAmount = theirCurrencyAmount;
            MyCurrencyAmount = myCurrencyAmount;
            MessageTime = messageTime;
            InZone = false;
        }
    }
}

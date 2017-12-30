using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderBoi.Objects
{
    class ItemStack
    {
        public String ItemName;
        public int ItemAmount;

        public ItemStack(String ıtemName, int ıtemAmount)
        {
            ItemName = ıtemName;
            ItemAmount = ıtemAmount;
        }
    }
}

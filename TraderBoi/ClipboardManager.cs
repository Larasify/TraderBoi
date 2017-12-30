using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraderBoi
{
    class ClipboardManager
    {
        public static Boolean ClipboardInUse;
        public static string GetContents()
        {
            while (ClipboardInUse == true)
            {
                Thread.Sleep(10);
            }
            ClipboardInUse = true;
            string ctext = null;
            Thread thread = new Thread(() => ctext = Clipboard.GetText(TextDataFormat.Text));
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            ClipboardInUse = false;
            thread.Join();

            return ctext;
        }
        public static void SetContents(string clipcontents)
        {
            while (ClipboardInUse == true)
            {
                Thread.Sleep(10);
            }
            ClipboardInUse = true;
            Thread thread = new Thread(() => Clipboard.SetText(clipcontents));
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
            ClipboardInUse = false;
        }
        public static void SetEmpty()
        {
            while (ClipboardInUse == true)
            {
                Thread.Sleep(10);
            }
            ClipboardInUse = true;
            Thread thread = new Thread(() => Clipboard.Clear());
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
            ClipboardInUse = false;
        }
    }
}

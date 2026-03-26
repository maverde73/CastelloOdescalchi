using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thera.Biglietteria.Boca;

namespace Presentation.LocalPrinter
{
    
    public class DummyPrinter: IBocaPrinter
    {
        public DummyPrinter(string portname)
        {
            
        }

        public bool Print(string s)
        {
            Console.WriteLine(s);
            return true;
        }

        public bool Open()
        {
            return true;
        }

        public void Close()
        {
            return;
        }

        public void Dispose()
        {
            return;
        }

        public bool isOpen
        {
            get { return true; }
        }

        public bool WaitStatus(DateTime date)
        {
            return true;
        }

        public bool WaitStatus(DateTime date, PrinterStatus st)
        {
            return true;
        }

        public bool WaitStatus(DateTime date, PrinterStatus st, int timeout)
        {
            return true;
        }

        public static string[] GetPrinterPorts()
        {
            return new string[] { "PortDummy" };
        }
    }
}

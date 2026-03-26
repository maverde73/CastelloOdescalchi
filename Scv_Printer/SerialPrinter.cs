using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using Thera.Biglietteria.Cassa.Commons.Utils;
using System.Configuration;

namespace Thera.Biglietteria.Cassa
{
    public enum PrinterStatus
    {
        ACK=0x06,
        NO_PAPER=0x10,
        NO_ERROR=0x11,
        NACK=0x15,
        OPEN_COVER=0x18,
        COMMAND_ERROR=0x19,
        NOTCH_ERROR=0x20,
        OVERHEAT_ERROR=0x21,
        VOLTAGE_ERROR=0x22,
        CUTTER_ERROR=0x23,
        APPLICATION_ERROR=0xFF
    }
    public class SerialPrinter :IDisposable
    {
        public delegate void PrinterStatusEventHandler(PrinterStatusEventArgs e);
        public event PrinterStatusEventHandler OnStatusChange;
        private const int BUFFER_SIZE = 50;

        private SerialPort sp = new SerialPort();
        #region IDisposable Members

        public void Dispose()
        {
            sp.Close();
        }

        #endregion
        public SerialPrinter()
           // : this("")
        {
            string[] ports = GetPrinterPorts(true);
            if (ports.Length > 0)
            {
                sp.PortName = ports[0];
            }
            else
            {
                throw new Exception("Stampante non trovata!");
            }
        }
        public SerialPrinter(string PortName)
        {
            sp.PortName = PortName;
        }

        #region Properties

        public string PortName { get; set; }

        private Utilities.PrinterType _PrinterType;
        public  Utilities.PrinterType PrinterType
        {
            get
            {
                return _PrinterType;
            }
            set
            {
                _PrinterType = value;
            }
        }

        //**Kube
        private object m_DataToPrint = null;
        public object DataToPrint
        {
            get
            {
                return m_DataToPrint;
            }
            set
            {
                m_DataToPrint = value;
            }
        }
        //**fine kube****


        #endregion

        #region Methods
        private SerialDataReceivedEventHandler serialDataReceivedEventHandler;

        public bool Close()
        {
            bool Close = true;
            try
            {
                sp.Close();
                sp.DataReceived -= serialDataReceivedEventHandler;
                sp.Dispose();
            }
            catch (Exception ex)
            {
                Close = false;
            }
            return Close;
        }
        
        public bool Open()
        {
            bool Open = true;
            try
            {
                //sp.DataBits = 8;
                //sp.Handshake = Handshake.RequestToSendXOnXOff;
                //sp.Parity = Parity.None;
                //sp.BaudRate = 115200;
                
                //sp.StopBits = StopBits.One;
                serialDataReceivedEventHandler = new SerialDataReceivedEventHandler(sp_DataReceived);
                sp.DataReceived += serialDataReceivedEventHandler;
                ConfigurePort(sp);
                sp.Open();

                //sp.RtsEnable = true;
                //sp.DtrEnable = true;
            }
            catch (Exception ex)
            {
                Open = false;
            }
            return Open;
        }

        public byte[] Read(int timeout)
        {
            byte[] ret = new byte[BUFFER_SIZE];
            int len = 0;
            DateTime start = DateTime.Now;
            while (sp.BytesToRead == 0 && start.AddMilliseconds(timeout) >= DateTime.Now)
            {
                Thread.Sleep(10);
            }
            if (sp.BytesToRead == 0)
                return null;
            while (sp.BytesToRead > 0)
            {
                int oldLen = len;
                len += sp.BytesToRead;
                while (len > ret.Length)
                {
                    Array.Resize<byte>(ref ret, ret.Length + BUFFER_SIZE);
                }
                sp.Read(ret, oldLen, len - oldLen);
                Thread.Sleep(10);
            }
            Array.Resize<byte>(ref ret, len);
            return ret;
        }
        public byte[] Read()
        {
            return Read(200);
        }
        public bool isOpen
        {
            get
            {
                if (sp != null)
                {
                    return sp.IsOpen;
                }
                else
                {
                    return false;
                }
            }
        }
        public PrinterStatus GetStatus()
        {
            return GetStatus(0);
        }
        public PrinterStatus GetStatus(int type)
        {
            while (ClearBuffer())
            {
                Thread.Sleep(50);
            }
            if (type > 0)
            {
                Print("<S" + type.ToString() + ">");
                //sp.Write("<S1>");
            }
            try
            {
                byte[] buff = Read(500);
                return (PrinterStatus)buff[buff.Length - 1];
            }
            catch (Exception ex)
            {
                return PrinterStatus.APPLICATION_ERROR;
            }
        }
        public bool ClearBuffer()
        {
            string s = sp.ReadExisting();
            return (s != "");
        }
        public void EnablePrinterFullStatus()
        {
            //byte[] buff = new byte[] { 0x1c, (byte)'<', (byte)'S', (byte)'V', (byte)'E', (byte)'L', (byte)'>', (byte)'\r', (byte)'\n' };
            //sp.Write(buff,0,buff.Length);
            sp.Write("<AFSBF>\r\n");
            sp.Write("<BEEP1>");
        }

        public void DisablePrinterFullStatus()
        {
            sp.Write("<AFSB0>\r\n");
        }

       

        public void Print(byte[] b)
        {
            sp.Write(b, 0, b.Length);
        }
        /*public string ParseTags(string s)
        {
            string ret="";
            string startTag="<EPOS>";
            string endTag=startTag[0] + "/" + startTag.Substring(1);
            string tmp = "";
            int start = s.IndexOf(startTag);
            int end = s.IndexOf(endTag);
            ret += s.Substring(0, start);
            ret += startTag;
            if (start >= 0 && end > start)
            {
                tmp = s.Substring(start + startTag.Length, (end + endTag.Length) - (start + startTag.Length));
                tmp = tmp.Replace(" ", "");
                for (int i = 0;i<tmp.Length ; i += 2)
                {

                }
            }
            else
            {
                return s;
            }
        }*/
        public void Print(string s)
        {
            // a partire dalla stringa da stampare creo l'array di byte per poter stampare anche caratteri particolari quali '€'
            byte[] b = new byte[s.Length * 3];
            int offset = 0;
            int startSize = s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                if (((int)s[i]) > 0x7f)
                {
                    byte[] uniChar = getUnicodeChar(s[i]);
                    Array.Copy(uniChar, 0, b, i + offset, uniChar.Length);
                    offset += uniChar.Length - 1;
                }
                else
                {
                    b[i + offset] = Convert.ToByte(s[i]);
                }
            }
            //chiamo il metodo per la stampa del biglietto
            if (b.Length != offset + startSize)
            {
                Array.Resize<byte>(ref b, offset + startSize);
            }
            Print(b);
        }

        
        /// <summary>
        /// devo considerare un metodo generico a cui passo i valori del biglietto da stampare: in base al tipo di stampante utilizzo il file di testo o il file XML
        /// </summary>
        public void Print(bool cut, string TicketDescription, float Price, string BarCode, int pax, DateTime date, string protocol, string serial)
        {
            Utilities.PrinterType printertype = (Utilities.PrinterType)Enum.Parse(typeof(Utilities.PrinterType), ConfigurationManager.AppSettings["PrinterType"], false);
            switch (printertype)
            {
                case Utilities.PrinterType.KPM300H:
                    Print(TicketPrintString(TicketDescription, String.Format("{0:0.00}",Price * pax), BarCode, pax, date, protocol, serial));
                    Print((cut ? "<P>" : "<Q>"));
                    break;
                case Utilities.PrinterType.KubeLottery:
                    PrintXML(cut, TicketDescription, Price, BarCode, pax, date);
                    break;
            }
        }

        #region Kube
        //***Kube
        //metodo da eliminare
        public void PrintKube()
        {
            // Load templates
            TemplateManager templateManager = new TemplateManager();
            templateManager.Init(@"xml\InternationalChars.xml");
            templateManager.LoadTemplate(@"xml\PrintTemplate.xml", "EN", "PrintTemplate");
            PrintXML(templateManager, "EN", "PrintTemplate");
            //sp.Write(b, 0, b.Length);
        }
        //fine metodo da eliminare
        public void PrintXML(bool cut, string TicketDescription, float Price, string BarCode, int pax, DateTime date)
        {
            //devo inizializzare l'oggetto 
            TicketToPrintXML tpx = new TicketToPrintXML();
            tpx.Pax = pax.ToString();
            tpx.TicketType = TicketDescription;
            tpx.Date = DateTime.Now;
            tpx.BarCode = BarCode;
            tpx.Price = String.Format("{0:0.00}", Price);
            m_DataToPrint = tpx;
            // Load templates
            TemplateManager templateManager = new TemplateManager();
            templateManager.Init(@"xml\InternationalChars.xml");
            templateManager.LoadTemplate(@"xml\PrintTemplate.xml", "EN", "PrintTemplate");
            PrintXML(templateManager, "EN", "PrintTemplate");
        }

        public void PrintXML(TemplateManager templateManager, string languageKey, string templateKey)
        {
            PrintXML(templateManager, languageKey, templateKey, 0);
        }

        public void PrintXML(TemplateManager templateManager, string languageKey, string templateKey, int msPrintWait)
        {
            if (m_DataToPrint == null)
            {
                throw new Exception("Nothing to print!");
            }
            // Generate command list for the template and transforme command list in byte sequence
            byte[] buffer = templateManager.GenerateRawData(languageKey, templateKey, m_DataToPrint);
            // Sending buffer to printer
           
                // Send via RS232
                SerialPort serial = null;
                try
                {
                  
                    // Check for paper and print
                    //if (getPaperStatus(serial) != PrinterPaperStatusType.NoPaper)
                    //{
                    //    // Send via RS232
                    //    serial.Write(buffer, 0, buffer.Length);
                    //    int a = serial.BytesToRead;
                    //    // Wait for print
                    //    System.Threading.Thread.Sleep(msPrintWait);
                    //    // Query printer
                    //    if (getPrintStatus(serial) == PrinterStatusType.OK)
                    //    {
                    //        // Set printed flag
                    //        m_IsPrinted = true;
                    //    }
                    //}
                        sp.Write(buffer, 0, buffer.Length);
                        int a = sp.BytesToRead;
                        // Wait for print
                        System.Threading.Thread.Sleep(msPrintWait);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex);
                    throw ex;
                }
                finally
                {
                    if (serial != null)
                    {
                        serial.Close();
                        serial.Dispose();
                        serial = null;
                    }
                }
            
        }

        //*** Kube
        #endregion 


        private string TicketPrintString(string TicketDescription, string Price, string BarCode, int pax, DateTime date, string protocol, string serial)
        {
            string TicketString = "";
            //TicketString = File.ReadAllText("PrintTemplate.txt", Encoding.UTF8);
            TicketString = File.ReadAllText("PrintTemplate.txt", Encoding.UTF8);
			TicketString = TicketString.Replace("#", TicketDescription).Replace("@", Price).Replace("&", BarCode);
			TicketString = TicketString.Replace("TicketNotRefudable", "Biglietto non rimborsabile - Ticket not refundable");
			TicketString = TicketString.Replace("Protocol", "Protocollo: " + protocol);
			TicketString = TicketString.Replace("Serial", "Seriale: " + serial);
			TicketString = TicketString.Replace("<DATE>", date.ToString("dd/MM/yyyy"));
            TicketString = TicketString.Replace("<TIME>", date.ToString("HH\\:mm\\:ss"));
            TicketString = TicketString.Replace("§", pax.ToString());

            return TicketString;
        }

        public byte[] getUnicodeChar(char c)
        {
            byte[] b = new byte[4];
            int cu, bu;
            bool cmp;
            Encoding.UTF8.GetEncoder().Convert(new char[] { c }, 0, 1, b, 0, 4, false, out cu, out bu, out cmp);
            byte[] ret = new byte[bu];
            for (int i = 0; i < bu; i++)
            {
                ret[i] = b[i];
            }
            return ret;
        }

        #endregion

        public bool isOnline { get; set; }
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {            
            try
            {
                SerialPort sp = (sender as SerialPort);
                
                Utilities.PrinterType printertype = (Utilities.PrinterType)Enum.Parse(typeof(Utilities.PrinterType), ConfigurationManager.AppSettings["PrinterType"], false);
                
                if (e.EventType == SerialData.Chars)
                {

                    switch (printertype)
                    {
                        case Utilities.PrinterType.KPM300H:
                            string s = sp.ReadExisting();
                            Thread.Sleep(50);
                            while (sp.BytesToRead > 0)
                            {
                                s += sp.ReadExisting();
                                Thread.Sleep(20);
                            }
                            Debug.WriteLine(s);
                            if (s != null)
                            {
                                foreach (char c in s)
                                {
                                    byte bfi = (byte)c;
                                    Debug.WriteLine(bfi.ToString("X"));
                                }
                            }
                            string[] stati = s.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < stati.Length; i++)
                            {
                                if (stati[i].IndexOf("SB") == 0)
                                {
                                    if (stati[i].IndexOf(",") != -1)
                                    {

                                        CheckStatus(stati[i]);
                                        isOnline = true;
                                    }
                                }

                            }
                            break;
                        case Utilities.PrinterType.KubeLottery:
                            DateTime startT = DateTime.Now;
                            byte[] buffer = new byte[0];
                            int len = 0;
                            Thread.Sleep(50);
                            while (sp.BytesToRead == 0 && startT.AddMilliseconds(2000) >= DateTime.Now)
                            {
                                Thread.Sleep(50);
                            }
                            while (sp.BytesToRead > 0)
                            {
                                Array.Resize<byte>(ref buffer, len + sp.BytesToRead);
                                sp.Read(buffer, len, sp.BytesToRead);
                                len += sp.BytesToRead;
                                Thread.Sleep(50);
                            }
                            try
                            {
                                //devo testare l'array buffer
                                if (buffer.Length > 0)
                                {
                                    CheckStatus(buffer);
                                    isOnline = true;
                                }
                            }
                            catch
                            {
                            }
                            break;

                    }


                    //string s = sp.ReadExisting();
                    //Thread.Sleep(50);
                    //while (sp.BytesToRead > 0)
                    //{
                    //    s += sp.ReadExisting();
                    //    Thread.Sleep(20);
                    //}
                    //Debug.WriteLine(s);
                    //if (s != null)
                    //{
                    //    foreach (char c in s)
                    //    {
                    //        byte bfi = (byte)c;
                    //        Debug.WriteLine(bfi.ToString("X"));
                    //    }
                    //}
                    //string[] stati = s.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                    //for (int i = 0; i < stati.Length; i++)
                    //{
                    //    if (stati[i].IndexOf("SB") == 0)
                    //    {
                    //        if (stati[i].IndexOf(",") != -1)
                    //        {

                    //            CheckStatus(stati[i]);
                    //            isOnline = true;
                    //        }
                    //    }

                    //}
                }
                #region OldCode
                //if (e.EventType == SerialData.Chars)
                //{

                //    string s = sp.ReadExisting();
                //    Thread.Sleep(50);
                //    while (sp.BytesToRead > 0)
                //    {
                //        s += sp.ReadExisting();
                //        Thread.Sleep(20);
                //    }
                //    Debug.WriteLine(s);
                //    if (s != null)
                //    {
                //        foreach (char c in s)
                //        {
                //            byte bfi = (byte)c;
                //            Debug.WriteLine(bfi.ToString("X"));
                //        }
                //    }
                //    string[] stati = s.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                //    for (int i = 0; i < stati.Length; i++)
                //    {
                //        if (stati[i].IndexOf("SB") == 0)
                //        {
                //            if (stati[i].IndexOf(",") != -1)
                //            {

                //                CheckStatus(stati[i]);
                //                isOnline = true;
                //            }
                //        }

                //    }
                //}
                #endregion 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                File.AppendAllText("printerr.log", DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss"));
                File.AppendAllText("printerr.log", ex.Message);
                File.AppendAllText("printerr.log", ex.StackTrace);
            }
        }
        private byte[] WaitReadBuffer(int timeout)
        {
            DateTime timetowait = DateTime.Now.AddMilliseconds(timeout);
            byte[] ret=new byte[0];
            while (sp.BytesToRead == 0 && DateTime.Now < timetowait)
            {
                Thread.Sleep(50);
            }
            int len=0;
            while (sp.BytesToRead > 0)
            {
                int cLen=len + sp.BytesToRead;
                Array.Resize(ref ret, cLen);
                sp.Read(ret, len, cLen);
                len = cLen;
                Thread.Sleep(50);
            }
            return ret;
        }
        private string WaitReadString(int timeout)
        {
            DateTime timetowait = DateTime.Now.AddMilliseconds(timeout);
            string ret ="";
            while (sp.BytesToRead == 0 && DateTime.Now < timetowait)
            {
                Thread.Sleep(50);
            }
            string tmp = "AAA";
            while (tmp!=ret)
            {
                tmp = ret;
                ret += sp.ReadExisting();
            }
            return ret;
        }
        public Thera.Biglietteria.Cassa.Utilities.PrinterState GetState()
        {
            Thera.Biglietteria.Cassa.Utilities.PrinterState ret = new Utilities.PrinterState();
            Print("<SBF>");
            string sStatus = WaitReadString(1000);
            string[] stati = sStatus.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < stati.Length; i++)
            {
                if (stati[i].IndexOf("SB") == 0)
                {
                    if (stati[i].IndexOf(",") != -1)
                    {

                        ret = ParseStatus(stati[i]);
                        
                    }
                }
            }
            return ret;
        }
        private Thera.Biglietteria.Cassa.Utilities.PrinterState ParseStatus(string stato)
        {
            try
            {
                string[] s = stato.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                byte b = System.Byte.Parse(s[0].Substring(2, 1), System.Globalization.NumberStyles.AllowHexSpecifier);
                byte[] val = new byte[4];
                s[1] = s[1].Substring(0, s[1].IndexOf('>')).PadLeft(8, '0');
                for (int i = 0; i < 8; i += 2)
                {
                    val[i / 2] = System.Byte.Parse(s[1].Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                }

                byte[] B = GetBytes(b);
                PrinterStatusEventArgs e = new PrinterStatusEventArgs();
                
                if (B != null)
                {
                    for (int i = 0; i < B.Length; i++)
                    {
                        switch (B[i])
                        {
                            case 1:
                                e.IsGeneralState = true;
                                e.Status.General = val[i];
                                break;
                            case 2:
                                e.IsUserState = true;
                                e.Status.User = val[i];
                                break;
                            case 4:
                                e.IsRecoverableError = true;
                                e.Status.Recoverable = val[i];
                                break;
                            case 8:
                                e.IsUnrecoverableError = true;
                                e.Status.Unrecoverable = val[i];
                                break;
                            default:
                                break;
                        }

                    }
                }
                return e.Status;
                //OnStatusChange(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }


        private void CheckStatus(byte[] buffer)
        {
            try
            {
                // al momento gestisco gli stati Carta Assente e Coperchio Aperto
                PrinterStatusEventArgs e = new PrinterStatusEventArgs();
                if (buffer[0] != 0x10)
                {
                    throw new Exception("Error in read pinter status");
                }
                if (buffer[1] != 0x0F)
                {
                    throw new Exception("Error in read pinter status");
                }
                //Il primo bit del terzo byte è quello che stabilisce se la carta è presente o meno
                /*
                  0x00  carta Presente
                  0x01  carta Assente
                 */
                bool flag = (buffer[2] & 0x01) == 0x01;
                if (flag)
                {
                    // non c'è carta
                    e.IsGeneralState = true;
                    e.Status.General = 197;   // utlizzo gli stati definiti per la KPM300H e non quello effettivo
                }
                //Il primo ed il secondo bit del quarto byte statbilisco se il coperchio è aperto
                /*
                0x00  coperchio abbassato
                0x03  coperchio alzato 
                */
                flag = (buffer[3] & 0x03) == 0x03;
                if (flag)
                {
                    // coperchio alzato
                    e.IsUserState = true;
                    e.Status.User = 3;  // utlizzo gli stati definiti per la KPM300H e non quello effettivo
                }

                OnStatusChange(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }







        private void CheckStatus(string stato)
        {
            try
            {
                string[] s = stato.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                byte b = System.Byte.Parse(s[0].Substring(2, 1), System.Globalization.NumberStyles.AllowHexSpecifier);
                byte[] val = new byte[4];
                s[1] = s[1].Substring(0, s[1].IndexOf('>')).PadLeft(8, '0');
                for (int i = 0; i < 8; i += 2)
                {
                    val[i / 2] = System.Byte.Parse(s[1].Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                }

                byte[] B = GetBytes(b);
                PrinterStatusEventArgs e = new PrinterStatusEventArgs();
                if (B != null)
                {
                    for (int i = 0; i < B.Length; i++)
                    {
                        switch (B[i])
                        {
                            case 1:
                                e.IsGeneralState = true;
                                e.Status.General = val[i];
                                break;
                            case 2:
                                e.IsUserState = true;
                                e.Status.User = val[i];
                                break;
                            case 4:
                                e.IsRecoverableError = true;
                                e.Status.Recoverable = val[i];
                                break;
                            case 8:
                                e.IsUnrecoverableError = true;
                                e.Status.Unrecoverable = val[i];
                                break;
                            default:
                                break;
                        }

                    }
                }
                OnStatusChange(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
        private byte[] GetBytes(byte b)
        {
            byte[] ret = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                byte mask = 1;
                while (((b & mask) == 0) && (mask != 16))
                {
                    mask *= 2;
                }
                if (mask >= 16)
                    return null;
                else
                    ret[i] = mask;
                b ^= mask;
            }
            return ret;
        }
        private void PrintStatus(byte b, byte val)
        {
            switch (b)
            {
                case 1:
                    PrintStatus<Thera.Biglietteria.Cassa.Utilities.GeneralState>(val);
                    break;
                case 2:
                    PrintStatus<Thera.Biglietteria.Cassa.Utilities.UserState>(val);
                    break;
                case 4:
                    PrintStatus<Thera.Biglietteria.Cassa.Utilities.RecoverableError>(val);
                    break;
                case 8:
                    PrintStatus<Thera.Biglietteria.Cassa.Utilities.UnrecoverableError>(val);
                    break;
            }

        }
        private Hashtable maskOld = new Hashtable();
        private void PrintStatus<T>(byte n)
        {
            byte old = 0;
            if (maskOld.Contains(typeof(T)))
            {
                old = (byte)maskOld[typeof(T)];
            }

            string[] names = Enum.GetNames(typeof(T));
            //byte[] values =(byte[]) Enum.GetValues(typeof(T));
            byte mask = 1;
            int i = 0;
            while (mask <= 255)
            {
                if (/*(n & mask) > 0 &&*/ ((n & mask) != (n & old)))
                {
                    System.Diagnostics.Debug.WriteLine(names[i] + "\t:" + (n & mask).ToString());

                }
                i++;
                mask *= 2;
            }
            maskOld[typeof(T)] = n;
        }

        /***************************************
     * 
     * 
     * *************************************/

        public static string[] GetPrinterPorts()
        {
            return GetPrinterPorts(false);
        }
        public static bool CheckPort(string port)
        {

            //Utilities.PrinterType printertype = ;
            //if(ConfigurationManager.AppSettings["PrinterType"]!=null)
            Utilities.PrinterType printertype = (Utilities.PrinterType)Enum.Parse(typeof(Utilities.PrinterType), ConfigurationManager.AppSettings["PrinterType"], false);

            Debug.WriteLine("Testing " + port);
            using (SerialPort sp = new SerialPort(port))
            {
                Debug.WriteLine("Opening port " + port);
                try
                {

                    //ConfigurePort(sp);
                    ConfigurePort(sp);
                    sp.Open();
                    sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived2);

                    if (sp.IsOpen)
                    {


                        Debug.WriteLine("Porta " + port + " aperta correttamente");
                    }
                    else
                    {
                        Debug.WriteLine("Apertura porta " + port + " fallita");
                        return false;
                    }
                    Debug.WriteLine("Test presenza stampante su porta " + port);


                   // Thera.Biglietteria.Cassa.Utilities.PrinterType _PrinterType = ;

                    PrinterQuery(sp, printertype);
                    //****************KUBE*********************
                    //byte[] b = new byte[] { 0x10, 0x04, 0x2 };
                    //sp.Write(b, 0, b.Length);
                    //***************Fine Kube****************


                    //Inizio Vaticano********* 
                    //sp.Write("<BEEP100>");
                    ////Write(sp, ((char)0x1c) + "<CE><STSMOQ><CMD>< >");
                    //byte[] buff = new byte[] { 0x1c, (byte)'<', (byte)'S', (byte)'V', (byte)'E', (byte)'L', (byte)'>', (byte)'\r', (byte)'\n' };
                    //sp.Write(buff, 0, buff.Length);
                    ////Write(sp, "<T>\r\n");
                    //Write(sp, "<SBF>");
                    //**************Fine Vaticano************


                    Debug.WriteLine("Attesa risposta stampante su porta " + port);

                    if (Read(sp, 2000))
                    //if (ReadKube(sp, 2000))
                    {

                        Debug.WriteLine((">>>>>>>>>>>Stampante trovata su porta " + port).ToUpper());
                        sp.Close();
                        return true;
                        /*ret += port + ",";
                        if (findfirst)
                            break;*/
                        
                    }
                    else
                    {

                        Debug.WriteLine("Stampante NON trovata su porta " + port);
                        return false;
                    }
                    
                }
                catch (Exception ex)
                {

                    Debug.WriteLine("Operazione fallita su porta " + port);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    return false;
                }
                finally
                {

                    Debug.WriteLine("<------------------------------>");

                }

            }
        }
        public static string[] GetPrinterPorts(bool findfirst)
        {
            string[] allPorts = SerialPort.GetPortNames();
            Array.Sort<string>(allPorts);
            string ret = "";
            foreach (string port in allPorts)
            {
                if (CheckPort(port))
                {
                    ret += port + ",";
                }
            }
            return ret.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        static void sp_DataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(10);
        }
        static void Write(SerialPort sp, string s)
        {
            byte[] b = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                b[i] = Convert.ToByte(s[i]);
            }
            sp.Write(b, 0, b.Length);
        }

        static void PrinterQuery(SerialPort sp, Utilities.PrinterType printertype)
        {
            switch (printertype)
            {
                case Utilities.PrinterType.KPM300H:
                    KPM300HQuery(sp);
                    break;
                case Utilities.PrinterType.KubeLottery:
                    KubeLotteryQuery(sp,true);
                    break;
                default:
                    break;
            }
        }

        static void KubeLotteryQuery(SerialPort sp, bool findprinter)
        {
            byte[] b;
            if(findprinter)
               b = new byte[] { 0x10, 0x04, 0x2 };
            else
               b = new byte[] { 0x10, 0x04, 0x14 };
            sp.Write(b, 0, b.Length);
        }

        static void KPM300HQuery(SerialPort sp)
        {
            sp.Write("<BEEP100>");
            //Write(sp, ((char)0x1c) + "<CE><STSMOQ><CMD>< >");
            byte[] buff = new byte[] { 0x1c, (byte)'<', (byte)'S', (byte)'V', (byte)'E', (byte)'L', (byte)'>', (byte)'\r', (byte)'\n' };
            sp.Write(buff, 0, buff.Length);
            //Write(sp, "<T>\r\n");
            Write(sp, "<SBF>");
        }




        static bool Read(SerialPort sp, double timeout)
        {
            DateTime startT = DateTime.Now;
            byte[] buffer = new byte[0];
            byte[] retValues = new byte[] { 0x10, 0x11, 0x18, 0x19, 0x20, 0x21, 0x22, 0x23 };
            int len = 0;
            Thread.Sleep(50);
            while (sp.BytesToRead == 0 && startT.AddMilliseconds(timeout) >= DateTime.Now)
            {
                Thread.Sleep(50);
            }
            while (sp.BytesToRead > 0)
            {
                Array.Resize<byte>(ref buffer, len + sp.BytesToRead);
                sp.Read(buffer, len, sp.BytesToRead);
                len += sp.BytesToRead;
                Thread.Sleep(50);
            }
            try
            {
                //StopWaiting();
                Debug.WriteLine("DATA LEN:\t" + buffer.Length.ToString());
                //for (int i = 0; i < buffer.Length; i++)
                //{
                //    Debug.Write(Convert.ToString(buffer[i], 16).PadLeft(2, '0'));
                //}
                Debug.WriteLine("DATA----->");
                for (int i = 0; i < buffer.Length; i++)
                {
                    Debug.Write((char)buffer[i]);
                }

                //********oldCode***********
                ////byte b = Array.Find<byte>(retValues, a => a == buffer[0]);
                ////if (buffer[0] == 0x3c && buffer[1] == 0x4c && buffer[2] == 0x48 && buffer[3] == 0x54)
                //if (buffer[0] == (byte)'<' && buffer[1] == (byte)'S' && buffer[2] == (byte)'B' && buffer[3] == (byte)'F')
                //    return true;
                //else
                //    return false;
                //******Fine oldCode***************
                return PrinterResponse(buffer);
            }
            catch
            {
                return false;
            }


        }

        static bool PrinterResponse(byte[] buffer)
        {
            bool response = true;
            Utilities.PrinterType  printertype = (Utilities.PrinterType)Enum.Parse(typeof(Utilities.PrinterType), ConfigurationManager.AppSettings["PrinterType"], false);
            switch(printertype)
            {
                case Utilities.PrinterType.KPM300H:
                    response = KPM300HPrinterResponse( buffer);
                    break;
                case Utilities.PrinterType.KubeLottery:
                    response = KubePrinterResponse( buffer);
                    break;
            }
            return response;
        }

        static bool KubePrinterResponse( byte[] buffer)
        {
            //bool response = true;
            ////da rivedere
            //if (buffer.Length == 0)
            //{
            //    response = false;
            //}
            //if ((buffer[0] & 0x20) == 0x20)
            //{
            //    //stampa interrotta per fine carta
            //    response = false;
            //}
            //else if ((buffer[0] & 0x40) == 0x40)
            //{
            //    //Errore
            //    response = false;
            //}
            //return response;

            //considero solo il caso in cui il buffer di risposta sia pieno indipendentemente da come è valorizzato e dallo stato corrispondente
            if (buffer.Length == 0)
                return  false;
            else
                return true;
        }

        static bool KPM300HPrinterResponse(byte[] buffer)
        {
            if (buffer[0] == (byte)'<' && buffer[1] == (byte)'S' && buffer[2] == (byte)'B' && buffer[3] == (byte)'F')
                return true;
            else
                return false;
        }
        //***Kube8(si può eliminare)
        static bool ReadKube(SerialPort sp, double timeout)
        {
            int n = 0;
            bool status = true;
            while ((n = sp.BytesToRead) != 1)
            {
                System.Threading.Thread.Sleep(50);
            }
            byte[] b = new byte[n];
            sp.Read(b, 0, n);
            if (b.Length == 0)
            {
                status = true;
            }
             if ((b[0] & 0x20) == 0x20)
            {
                status = false;
            }
            else if ((b[0] & 0x40) == 0x40)
            {
                status = false;
            }
             return status;
        }
        //**Kube

        public void QueryPrinterStatus()
        {
            Utilities.PrinterType printertype = (Utilities.PrinterType)Enum.Parse(typeof(Utilities.PrinterType), ConfigurationManager.AppSettings["PrinterType"], false);
            switch (printertype)
            {
                case Utilities.PrinterType.KPM300H:
                    Print("<SBF>");
                    break;
                case Utilities.PrinterType.KubeLottery:
                    KubeLotteryQuery(sp,false);
                    break;
            }
        }

        public void QueryPaperStatus(bool ForwardStep)
        {
            Utilities.PrinterType printertype = (Utilities.PrinterType)Enum.Parse(typeof(Utilities.PrinterType), ConfigurationManager.AppSettings["PrinterType"], false);
            switch (printertype)
            {
                case Utilities.PrinterType.KPM300H:
                    if (ForwardStep)
                    {
                        Print("<CB>");
                        Print("<MM1000>");
                    }
                    else
                        Print("<CB>");
                    break;
                case Utilities.PrinterType.KubeLottery:
                    Print(new byte[] { 0x1B, 0x40 });
                    //0x1B, 0x40
                    //KubeLotteryQuery(sp, false);
                    break;
            }
        }



        //static void ConfigurePort(SerialPort sp, Thera.Biglietteria.Cassa.Utilities.PrinterType PrinterType)
        //{
        //    switch (PrinterType)
        //    {
        //        case Utilities.PrinterType.KPM300H:
        //            sp.DataBits = 8;
        //            sp.Handshake = Handshake.RequestToSend;
        //            sp.Parity = Parity.None;
        //            sp.BaudRate = 115200;
        //            sp.RtsEnable = true;
        //            sp.DtrEnable = true;
        //            sp.StopBits = StopBits.One;
        //            sp.WriteTimeout = 2000;
        //            break;
        //        case Utilities.PrinterType.KubeLottery:
        //            sp.BaudRate = 19200;
        //            sp.Parity = Parity.None;
        //            sp.DataBits = 8;
        //            sp.StopBits = StopBits.One;
        //            break;
        //        default:
        //            sp.DataBits = 8;
        //            sp.Handshake = Handshake.RequestToSend;
        //            sp.Parity = Parity.None;
        //            sp.BaudRate = 115200;
        //            sp.RtsEnable = true;
        //            sp.DtrEnable = true;
        //            sp.StopBits = StopBits.One;
        //            sp.WriteTimeout = 2000;
        //            break;
        //    }

        //}



        static void ConfigurePort(SerialPort sp)
        {
            if (ConfigurationManager.AppSettings["DataBits"] != "")
                sp.DataBits = Convert.ToInt32(ConfigurationManager.AppSettings["DataBits"]);
            if (ConfigurationManager.AppSettings["Handshake"] != "")
                sp.Handshake = (System.IO.Ports.Handshake)Enum.Parse(typeof(System.IO.Ports.Handshake), ConfigurationManager.AppSettings["Handshake"], false);
            if (ConfigurationManager.AppSettings["Parity"] != "")
                sp.Parity = (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), ConfigurationManager.AppSettings["Parity"], false);
            if (ConfigurationManager.AppSettings["BaudRate"] != "")
                sp.BaudRate = Convert.ToInt32(ConfigurationManager.AppSettings["BaudRate"]);
            if (ConfigurationManager.AppSettings["RtsEnable"] != "")
                sp.RtsEnable = ConfigurationManager.AppSettings["RtsEnable"]== "1" ? true : false;
            if (ConfigurationManager.AppSettings["DtrEnable"] != "")
                sp.DtrEnable = ConfigurationManager.AppSettings["DtrEnable"] == "1" ? true : false;
            if (ConfigurationManager.AppSettings["StopBits"] != "")
                sp.StopBits = (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), ConfigurationManager.AppSettings["StopBits"], false);
            if (ConfigurationManager.AppSettings["WriteTimeout"] != "")
                sp.WriteTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["WriteTimeout"]);
            
            
            //***Vaticano***
            //sp.DataBits = 8;
            //sp.Handshake = Handshake.RequestToSend;
            //sp.Parity = Parity.None;
            //sp.BaudRate = 115200;
            //sp.RtsEnable = true;
            //sp.DtrEnable = true;
            //sp.StopBits = StopBits.One;
            //sp.WriteTimeout = 2000;
            //*****fine vaticano*****


            //***inizio Kube

            //sp.BaudRate = 19200;
            //sp.Parity = Parity.None;
            //sp.DataBits = 8;
            //sp.StopBits = StopBits.One;
            //*****fine kube****

        }




       
    }

    public class PrinterStatusEventArgs: EventArgs
    {
        public Utilities.PrinterState Status = new Utilities.PrinterState();        
        public bool IsUserState { get; set; }
        public bool IsGeneralState { get; set; }
        public bool IsRecoverableError { get; set; }
        public bool IsUnrecoverableError { get; set; }


        //public BiglietteriaCupola.Utilities.TipoBiglietti UserState { get; set; }
    }

    #region Kube
    //***************Kube

    public enum PrinterPaperStatusType
    {
        Paper = 0, NoPaper = 1, PoorPaper = 2
    }

    public enum PrinterStatusType
    {
        OK = 0, PrintBreak = 1, Error = 2
    }

    public class CommandPrinter
    {
        private byte[] m_Command = null;
        private byte[] m_Params = null;
        private bool m_NeedParam = false;

        public CommandPrinter()
        {
        }

        protected bool NeedParam
        {
            get
            {
                return m_NeedParam;
            }
            set
            {
                m_NeedParam = value;
            }
        }

        protected byte[] Command
        {
            get
            {
                return m_Command;
            }
            set
            {
                m_Command = value;
            }
        }

        protected byte[] Params
        {
            get
            {
                return m_Params;
            }
            set
            {
                m_Params = value;
            }
        }

        public int SizeInByte
        {
            get
            {
                if (m_NeedParam && m_Params == null)
                {
                    throw new Exception("Missed params");
                }
                return m_Command.Length + ((m_Params == null) ? 0 : m_Params.Length);
            }
        }

        public byte[] GetRawData()
        {
            byte[] ret = new byte[SizeInByte];
            Array.Copy(m_Command, 0, ret, 0, m_Command.Length);
            if (m_Params != null)
            {
                Array.Copy(m_Params, 0, ret, m_Command.Length, m_Params.Length);
            }
            return ret;
        }
    }

    class CommandPrintAndFeed : CommandPrinter
    {
        private static byte[] m_CommandPrintAndFeed = new byte[] { 0x1b, 0x64 };

        public CommandPrintAndFeed()
            : base()
        {
            Command = m_CommandPrintAndFeed;
            NeedParam = true;
        }

        public void SetLineNumber(int n)
        {
            byte b = (byte)n;
            Params = new byte[] { b };
        }
    }

    class CommandSetLeftMargin : CommandPrinter
    {
        private static byte[] m_CommandSetLeftMargin = new byte[] { 0x1d, 0x4c };

        public CommandSetLeftMargin()
            : base()
        {
            Command = m_CommandSetLeftMargin;
            NeedParam = true;
        }

        public void SetMargin(int n)
        {
            byte low = (byte)(n & 0x00FF);
            byte high = (byte)((n & 0xFF00) >> 8);
            Params = new byte[] { low, high };
        }
    }

    enum CustomPrinterFontType
    {
        FontLarge = 0, FontSmall = 1
    }

    class CommandSetFont : CommandPrinter
    {
        private static byte[] m_CommandSetFont = new byte[] { 0x1b, 0x4d };

        public CommandSetFont()
            : base()
        {
            Command = m_CommandSetFont;
            NeedParam = true;
        }

        public void SetFont(CustomPrinterFontType font)
        {
            byte b = (byte)font;
            Params = new byte[] { b };
        }
    }

    class CommandPrintString : CommandPrinter
    {
        private bool m_AppendNewLine = false;
        private CommandNewLine m_NewLine = new CommandNewLine();

        public CommandPrintString(string text, bool bNewline)
            : base()
        {
            NeedParam = false;
            m_AppendNewLine = bNewline;
            SetText(text);
        }

        public CommandPrintString(string text)
            : this(text, false)
        {
        }

        public bool NewLine
        {
            get
            {
                return m_AppendNewLine;
            }
            set
            {
                m_AppendNewLine = value;
            }
        }

        public static byte[] GetBytes(string s, bool convert)
        {
            //if (convert)
            //{
            //    s = StringConverterHelper.Convert(s);
            //}
            char[] c = s.ToCharArray();
            byte[] b = new byte[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                b[i] = (byte)c[i];
            }
            return b;
        }

        public void SetText(string text)
        {
            Command = GetBytes(text, true);
            if (m_AppendNewLine)
            {
                byte[] raw = new byte[SizeInByte + m_NewLine.SizeInByte];
                Array.Copy(GetRawData(), 0, raw, 0, SizeInByte);
                Array.Copy(m_NewLine.GetRawData(), 0, raw, SizeInByte, m_NewLine.SizeInByte);
                Command = raw;
            }
        }

        public void SetText(string text, bool bNewline)
        {
            m_AppendNewLine = bNewline;
            SetText(text);
        }
    }

    //class CommandPrintLabel : CommandPrintString
    //{
    //    public CommandPrintLabel(LocalizeLabelMapper htLabels, string labelKey, bool bNewLine)
    //        : base("")
    //    {
    //        NewLine = bNewLine;
    //        try
    //        {
    //            string s = htLabels[labelKey];
    //            SetText(s);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Error.WriteLine(ex);
    //            SetText("Label not found");
    //        }
    //    }
    //}

    //class CommandPrintMoney : CommandPrintString
    //{
    //    public CommandPrintMoney(LocalizeLabelMapper htLabels, decimal v, bool bShowCurrency, bool bNewLine)
    //        : base("")
    //    {
    //        string s = getFormatString(htLabels, v, bShowCurrency);
    //        NewLine = bNewLine;
    //        SetText(s);
    //    }

    //    public static string getFormatString(LocalizeLabelMapper htLabels, decimal v, bool bShowCurrency)
    //    {
    //        string s = string.Empty;
    //        try
    //        {
    //            s = (bShowCurrency) ? " " + htLabels["Money"] : string.Empty;
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.Error.WriteLine(ex);
    //            s = "Label not found";
    //        }
    //        return (v.ToString("#,##0.00") + s);
    //    }
    //}

    class CommandNewLine : CommandPrinter
    {
        private static byte[] m_CommandNewLine = new byte[] { 0x0D, 0x0A };

        public CommandNewLine()
            : base()
        {
            Command = m_CommandNewLine;
        }
    }

    public enum UnderlineType
    {
        Null = 0, Single = 1, Double = 2
    }

    class CommandSetUnderline : CommandPrinter
    {
        private static byte[] m_CommandSetUnderline = new byte[] { 0x1B, 0x2D };

        public CommandSetUnderline(UnderlineType line)
            : base()
        {
            SetUnderline(line);
        }

        public void SetUnderline(UnderlineType line)
        {
            Command = new byte[] { m_CommandSetUnderline[0], m_CommandSetUnderline[1], (byte)line };
        }
    }

    public enum HAlignType
    {
        Left = 0, Center = 1, Rigth = 2
    }

    class CommandSetHAlign : CommandPrinter
    {
        private static byte[] m_CommandSetHAlign = new byte[] { 0x1B, 0x61 };

        public CommandSetHAlign(HAlignType align)
            : base()
        {
            SetAlign(align);
        }

        public void SetAlign(HAlignType align)
        {
            Command = new byte[] { m_CommandSetHAlign[0], m_CommandSetHAlign[1], (byte)align };
        }
    }

    class CommandSetPosition : CommandPrinter
    {
        private static byte[] m_CommandSetPosition = new byte[] { 0x1B, 0x24 };

        public CommandSetPosition(int pos)
            : base()
        {
            SetPosition(pos);
        }

        public void SetPosition(int pos)
        {
            Command = m_CommandSetPosition;
            byte low = (byte)(pos & 0x00FF);
            byte high = (byte)((pos & 0xFF00) >> 8);
            Params = new byte[] { low, high };
        }
    }

    class CommandDefineXYUnit : CommandPrinter
    {
        private static byte[] m_CommandDefineXYUnit = new byte[] { 0x1D, 0x50 };

        public CommandDefineXYUnit()
            : this(0, 0)
        {
        }

        public CommandDefineXYUnit(byte x, byte y)
            : base()
        {
            SetUnit(x, y);
        }

        public void SetUnit(byte x, byte y)
        {
            Command = m_CommandDefineXYUnit;
            Params = new byte[] { x, y };
        }
    }

    class CommandDefineDefualtInterline : CommandPrinter
    {
        private static byte[] m_CommandDefineDefualtInterline = new byte[] { 0x1B, 0x32 };

        public CommandDefineDefualtInterline()
            : base()
        {
            Command = m_CommandDefineDefualtInterline;
        }
    }

    class CommandDefineInterline : CommandPrinter
    {
        private static byte[] m_CommandDefineInterline = new byte[] { 0x1B, 0x33 };

        public CommandDefineInterline()
            : this(64)
        {
        }

        public CommandDefineInterline(byte n)
            : base()
        {
            SetInterline(n);
        }

        public void SetInterline(byte n)
        {
            Command = m_CommandDefineInterline;
            Params = new byte[] { n };
        }
    }

    class CommandPrintAndUnitFeed : CommandPrinter
    {
        private static byte[] m_CommandPrintAndUnitFeed = new byte[] { 0x1B, 0x4A };

        public CommandPrintAndUnitFeed(byte n)
            : base()
        {
            SetFeed(n);
        }

        public void SetFeed(byte n)
        {
            Command = m_CommandPrintAndUnitFeed;
            Params = new byte[] { n };
        }
    }

    class CommandSetCharSize : CommandPrinter
    {
        private static byte[] m_CommandSetCharSize = new byte[] { 0x1D, 0x21 };

        public CommandSetCharSize(byte x, byte y)
            : base()
        {
            SetSize(x, y);
        }

        public void SetSize(byte x, byte y)
        {
            if (x < 1 || x > 8 || y < 1 || y > 8)
            {
                throw new Exception("Invalid char size");
            }
            byte v = (byte)(((x - 1) << 4) | (y - 1));
            Command = m_CommandSetCharSize;
            Params = new byte[] { v };
        }
    }

    class CommandSetTabs : CommandPrinter
    {
        private static byte[] m_CommandSetTabs = new byte[] { 0x1B, 0x44 };

        public CommandSetTabs(byte[] tabs)
            : base()
        {
            SetTabs(tabs);
        }

        public void SetTabs(byte[] tabs)
        {
            Command = m_CommandSetTabs;
            Params = new byte[tabs.Length + 1];
            Array.Copy(tabs, 0, Params, 0, tabs.Length);
            Params[Params.Length - 1] = 0x00;
        }
    }

    class CommandSetPrintMode : CommandPrinter
    {
        private static byte[] m_CommandSetPrintMode = new byte[] { 0x1B, 0x21 };

        public CommandSetPrintMode()
            : this(0)
        {
        }

        public CommandSetPrintMode(byte x)
            : base()
        {
            SetMode(x);
        }

        public void SetMode(byte x)
        {
            Command = m_CommandSetPrintMode;
            Params = new byte[] { x };
        }
    }

    class CommandSetBoldMode : CommandPrinter
    {
        private static byte[] m_CommandSetBoldMode = new byte[] { 0x1B, 0x45 };

        public CommandSetBoldMode()
            : this(0)
        {
        }

        public CommandSetBoldMode(byte x)
            : base()
        {
            SetMode(x);
        }

        public void SetMode(byte x)
        {
            Command = m_CommandSetBoldMode;
            Params = new byte[] { x };
        }
    }

    public enum BarCodeTextPositionType
    {
        Null = 0, Upper = 1, Down = 2, UpperAndDown = 3
    }

    public enum BarCodeType
    {
        UPC_A = 0, UPC_E = 1, EAN13 = 2, EAN8 = 3, CODE39 = 4, ITF = 5, CODABAR = 6, CODE93 = 7, CODE128 = 8, CODE32 = 20
    }

    class CommandPrintBarcode : CommandPrinter
    {
        private static byte[] m_CommandBarCodeSetFont = new byte[] { 0x1D, 0x66 };
        private static byte[] m_CommandBarCodeSetTextPosition = new byte[] { 0x1D, 0x48 };
        private static byte[] m_CommandBarCodeSetHeight = new byte[] { 0x1D, 0x68 };
        private static byte[] m_CommandBarCodePrint = new byte[] { 0x1D, 0x6B };

        public CommandPrintBarcode(CustomPrinterFontType font, BarCodeTextPositionType position, BarCodeType type, byte[] barcode)
            : base()
        {
            SetAndPrintBarCode(font, position, 162, type, barcode);
        }

        public CommandPrintBarcode(CustomPrinterFontType font, BarCodeTextPositionType position, byte height, BarCodeType type, byte[] barcode)
            : base()
        {
            SetAndPrintBarCode(font, position, height, type, barcode);
        }

        private void SetAndPrintBarCode(CustomPrinterFontType font, BarCodeTextPositionType position, byte height, BarCodeType type, byte[] barcode)
        {
            byte[] buf1 = new byte[] { m_CommandBarCodeSetFont[0], m_CommandBarCodeSetFont[1], (byte)font };
            byte[] buf2 = new byte[] { m_CommandBarCodeSetTextPosition[0], m_CommandBarCodeSetTextPosition[1], (byte)position };
            byte[] buf3 = new byte[] { m_CommandBarCodeSetHeight[0], m_CommandBarCodeSetHeight[1], height };
            Command = new byte[buf1.Length + buf2.Length + buf3.Length + m_CommandBarCodePrint.Length + barcode.Length + 2];
            int idx = 0;
            Array.Copy(buf1, 0, Command, idx, buf1.Length);
            idx += buf1.Length;
            Array.Copy(buf2, 0, Command, idx, buf2.Length);
            idx += buf2.Length;
            Array.Copy(buf3, 0, Command, idx, buf3.Length);
            idx += buf3.Length;
            Array.Copy(m_CommandBarCodePrint, 0, Command, idx, m_CommandBarCodePrint.Length);
            idx += m_CommandBarCodePrint.Length;
            Command[idx++] = (byte)type;
            Array.Copy(barcode, 0, Command, idx, barcode.Length);
            idx += barcode.Length;
            Command[idx] = 0x00;
        }
    }

    class CommandCutPaper : CommandPrinter
    {
        private static byte[] m_CommandCutPaper = new byte[] { 0x1B, 0x69 };

        public CommandCutPaper()
            : base()
        {
            Command = m_CommandCutPaper;
        }
    }

    class CommandAlignPaperToCut : CommandPrinter
    {
        private static byte[] m_CommandAlignPaperToCut = new byte[] { 0x1D, 0xF8 };

        public CommandAlignPaperToCut()
            : base()
        {
            Command = m_CommandAlignPaperToCut;
        }
    }

    class CommandPrintSmallImage : CommandPrinter
    {
        private static byte[] m_CommandPrintImage = new byte[] { 0x1B, 0x2A };

        public CommandPrintSmallImage(byte[] image)
            : base()
        {
            NeedParam = true;
            Command = m_CommandPrintImage;
            SetImage(image);
        }

        public CommandPrintSmallImage()
            : this(null)
        {
        }

        public void SetImage(byte[] image)
        {
            Params = image;
        }
    }

    class CommandDefineImage : CommandPrinter
    {
        private static byte[] m_CommandDefineImage = new byte[] { 0x1D, 0x2A };

        public CommandDefineImage(byte[] imageWithSize)
            : base()
        {
            Command = m_CommandDefineImage;
            Params = imageWithSize;
        }
    }

    public enum PrintModeType
    {
        Normal = 0, DoubleWidth = 1, DoubleHeight = 2, DoubleDouble = 3
    }

    class CommandPrintImageDefined : CommandPrinter
    {
        private static byte[] m_CommandPrintImageDefined = new byte[] { 0x1D, 0x2F };

        public CommandPrintImageDefined(PrintModeType type)
            : base()
        {
            Command = m_CommandPrintImageDefined;
            Params = new byte[] { (byte)type };
        }
    }

    class CommandResetImageDefined : CommandPrinter
    {
        private static byte[] m_CommandResetImageDefined = new byte[] { 0x1D, 0x71 };

        public CommandResetImageDefined()
            : base()
        {
            Command = m_CommandResetImageDefined;
        }
    }

    class CommandReset : CommandPrinter
    {
        private static byte[] m_CommandReset = new byte[] { 0x1B, 0x40 };

        public CommandReset()
            : base()
        {
            Command = m_CommandReset;
        }
    }

    class CommandDrawLine : CommandPrintSmallImage
    {
        public CommandDrawLine(int w)
            : base()
        {
            Bitmap bitmap = new Bitmap(w, 8);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.White, 0, 0, w, 8);
            graphics.DrawLine(Pens.Black, 0, 4, w, 4);
            graphics.Dispose();
            Params = ImageRasterHelper.ConvertShortBitmap(bitmap, true, false);
        }
    }

    class CommandDefineCustomChar : CommandPrinter
    {
        private static byte[] m_CommandDefineCustomChar = new byte[] { 0x1B, 0x26, 0x03 };

        public CommandDefineCustomChar(byte startCharCode, byte endCharCode, byte[] charDefinition)
            : base()
        {
            NeedParam = true;
            Command = m_CommandDefineCustomChar;
            Params = new byte[charDefinition.Length + 2];
            Params[0] = startCharCode;
            Params[1] = endCharCode;
            Array.Copy(charDefinition, 0, Params, 2, charDefinition.Length);
        }
    }

    class CommandSetCustomChar : CommandPrinter
    {
        private static byte[] m_CommandSetCustomChar = new byte[] { 0x1B, 0x25 };

        public CommandSetCustomChar(bool flag)
            : base()
        {
            NeedParam = true;
            Command = m_CommandSetCustomChar;
            Params = new byte[] { (byte)(flag ? 0x01 : 0x00) };
        }
    }

    class CommandPrintEuro : CommandPrinter
    {
        private static byte[] m_CommandPrintEuro = new byte[] { 0x1B, 0x74, 0x13, 0xD5 };

        public CommandPrintEuro()
            : base()
        {
            Command = m_CommandPrintEuro;
        }
    }

    class CommandPrintImageNV : CommandPrinter
    {
        private static byte[] m_CommandPrintImageNV = new byte[] { 0x1C, 0x70 };

        public CommandPrintImageNV()
            : this(0, 0)
        {
        }

        public CommandPrintImageNV(byte x, byte y)
            : base()
        {
            SetUnit(x, y);
        }

        public void SetUnit(byte x, byte y)
        {
            NeedParam = true;
            Command = m_CommandPrintImageNV;
            Params = new byte[] { x, y };
        }
    }

    class CommandPrintLogo : CommandPrinter
    {
        private static byte[] m_CommandPrintLogo = new byte[] { 0x1B, 0xFA };

        public CommandPrintLogo()
            : base()
        {
            Command = m_CommandPrintLogo;
            Params = new byte[] {0x02,0x01,0x74,0x00,0x7C  };
        }

       
    }

    public class CommandPrinterList : List<CommandPrinter>
    {
    }
    //********
    #endregion 


}

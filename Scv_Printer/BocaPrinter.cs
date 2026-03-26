using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;


namespace Thera.Biglietteria.Boca
{
    public interface IBocaPrinter : IDisposable
    {
        bool Print(string s);
        bool Open();
        void Close();

        bool isOpen { get; }

        bool WaitStatus(DateTime date);

        bool WaitStatus(DateTime date, PrinterStatus st);

        bool WaitStatus(DateTime date, PrinterStatus st, int timeout);
    }

    public class PrinterQueue : List<PrinterStatusEvent>
    {


        public void Push(PrinterStatusEvent obj)
        {
            base.Add(obj);
        }
        public PrinterStatusEvent Pop()
        {
            PrinterStatusEvent ret = this[0].Clone();
            this.RemoveAt(0);
            return ret;
        }

        public bool Contains(PrinterStatusEvent obj)
        {
            return base.Contains(obj);
        }
        public bool ExistsEvent(DateTime start, PrinterStatus st)
        {
            List<PrinterStatusEvent> ret = GetEvents(start);
            foreach (var item in ret)
            {
                if (item.Status == st)
                {
                    return true;
                }
            }
            return false;
        }
        public List<PrinterStatusEvent> GetEvents(DateTime start)
        {
            List<PrinterStatusEvent> ret = new List<PrinterStatusEvent>();

            foreach (var item in this)
            {
                if (item.Time >= start)
                {
                    ret.Add(item);
                }
            }
            return ret;
        }
    }
    public enum PrinterStatus
    {
        NONE = 0x00,
        REJECT_BIN_WARNING = 0x01,
        REJECT_BIN_ERROR = 0x02,
        PAPER_JAM_PATH_1 = 0x03,
        PAPER_JAM_PATH_2 = 0x04,
        TEST_BUTTON_TICKET_ACK = 0x05,
        TICKET_ACK = 0x06,
        WRONG_FILE_IDENTIFIER_DURING_UPDATE = 0x07,
        INVALID_CHECKSUM = 0x08,
        VALID_CHECKSUM = 0x09,
        OUT_OF_PAPER_PATH_1 = 0x0A,
        OUT_OF_PAPER_PATH_2 = 0x0B,
        PAPER_LOADED_PATH_1 = 0x0C,
        PAPER_LOADED_PATH_2 = 0x0D,
        ESCROW_JAM = 0x0E,
        LOW_PAPER = 0x0F,
        OUT_OF_PAPER = 0x10,
        X_ON = 0x11,
        POWER_ON = 0x12,
        X_OFF = 0x13,
        BAD_FLASH_MEMORY = 0x14,
        NAK = 0x15,// (illegal print command) 
        RIBBON_LOW = 0x16,
        RIBBON_OUT = 0x17,
        PAPER_JAM = 0x18,
        ILLEGAL_DATA = 0x19,
        POWERUP_PROBLEM = 0x1A,
        DOWNLOADING_ERROR = 0x1B,
        CUTTER_JAM = 0x1C,
        STUCK_TICKET_CUTJAM_PATH1 = 0x1D,
        CUTJAM_PATH2 = 0x1F
    }
    public class PrinterConfig
    {
        public int DataBits = 8;
        public Handshake Handshake = Handshake.RequestToSend;
        public Parity Parity = Parity.None;
        public int BaudRate = 9600;
        //public int BaudRate = 115200;
        public bool RtsEnable = true;
        public bool DtrEnable = true;
        public StopBits StopBits = StopBits.One;
    }
    public class PrinterStatusEvent
    {
        public PrinterStatus Status = PrinterStatus.NONE;
        public string LastStream = "";
        public DateTime Time = DateTime.MinValue;
        public PrinterStatusEvent Clone()
        {
            return new PrinterStatusEvent() { Status = Status, LastStream = LastStream, Time = Time };
        }
    }
    public class BocaPrinter : IBocaPrinter
    {
        private volatile SerialPort sp = new SerialPort();
        PrinterQueue _messages = new PrinterQueue();

        public PrinterQueue Messages
        {
            get { return _messages; }
        }
        public BocaPrinter(string portname)
        {
            sp.PortName = portname;

            ConfigurePort(sp);
        }



        public bool Print(string s)
        {
            try
            {
                byte[] b = new byte[s.Length * 3];
                int offset = 0;
                int startSize = s.Length;
                for (int i = 0; i < s.Length; i++)
                {
                    //new byte[]{0x1b,0x74,0x13,0xd5}
                    if (s[i] == '€')
                    {
                        byte[] uniChar = new byte[] { 0x13, 0xd5 };
                        Array.Copy(uniChar, 0, b, i + offset, uniChar.Length);
                        offset += uniChar.Length - 1;
                    }
                    else if (((int)s[i]) > 0x7f)
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
                if (b.Length != offset + startSize)
                {
                    Array.Resize<byte>(ref b, offset + startSize);
                }

                return Print(b);
            }
            catch (Exception ex)
            {
                LogExc(GetRecursiveInnerException(ex));
                return false;
            }
        }
        public bool Print(byte[] buffer)
        {
            try
            {
                //giorgio
                if (!sp.IsOpen)
                    sp.Open();
                //giorgio fine

                sp.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception ex)
            {
                LogExc(GetRecursiveInnerException(ex));
                return false;
            }
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
        private void UpdateStatus()
        {
            Thread.Sleep(100);
            byte[] buff = ReadData(sp);
            DateTime time = DateTime.Now;
            string sBuff = Encoding.ASCII.GetString(buff);
            foreach (byte b in buff)
            {
                PrinterStatusEvent pse = new PrinterStatusEvent();
                pse.Time = time;
                var values = Enum.GetValues(typeof(PrinterStatus)).GetEnumerator();
                while (values.MoveNext())
                {
                    if (Convert.ToByte(values.Current) == b)
                    {
                        pse.Status = (PrinterStatus)b;
                        break;
                    }
                }
                pse.LastStream = sBuff;
                _messages.Push(pse);
            }
        }

        //void sp_DataReceivedBoca(object sender, SerialDataReceivedEventArgs e)
        //{
        //    SerialPort sp = (SerialPort)sender;
        //    byte[] buff = ReadData(sp);
        //    DateTime time = DateTime.Now;
        //    string sBuff=Encoding.ASCII.GetString(buff);
        //    foreach (byte b in buff)
        //    {
        //        PrinterStatusEvent pse = new PrinterStatusEvent();
        //        pse.Time = time;
        //        var values = Enum.GetValues(typeof(PrinterStatus)).GetEnumerator();
        //        while (values.MoveNext())
        //        {
        //            if (Convert.ToByte(values.Current) == b)
        //            {
        //                pse.Status = (PrinterStatus)b;
        //                break;
        //            }
        //        }                
        //        pse.LastStream = sBuff;
        //        _messages.Push(pse);
        //    }
        //}
        public bool isOpen
        {
            get
            {
                try
                {
                    return sp.IsOpen;
                }
                catch(Exception ex)
                {
                    LogExc(GetRecursiveInnerException(ex));
                    return false;
                }
            }
        }
        public bool Open()
        {
            try
            {
                sp.Open();
                //flavio
                //GC.SuppressFinalize(sp.BaseStream);
                //fine flavio
                return true;
            }
            catch(Exception ex) 
            {
                LogExc(GetRecursiveInnerException(ex));
                return false; 
            }
        }
        public void Close()
        {
            try
            {
                sp.Close();
                //flavio
                //GC.ReRegisterForFinalize(sp.BaseStream);
                //fine flavio
            }
            catch(Exception ex) 
            {
                LogExc(GetRecursiveInnerException(ex));
            }



        }
        static byte[] ReadData(SerialPort sp)
        {
            Thread.Sleep(200);
            byte[] buff = new byte[0];
            while (sp.BytesToRead > 0)
            {
                int btr = sp.BytesToRead;
                Array.Resize(ref buff, buff.Length + btr);
                sp.Read(buff, buff.Length - btr, btr);
                Thread.Sleep(25);
            }
            return buff;
        }
        public bool WaitStatus(DateTime date)
        {
            UpdateStatus();

            var ps = this.Messages.GetEvents(date);

            while (ps.Count == 0 && DateTime.Now.AddMilliseconds(200) < date)
            {
                ps = _messages.GetEvents(date);
            }
            return ps.Count > 0;
        }
        public bool WaitStatus(DateTime date, PrinterStatus st)
        {
            return WaitStatus(date, st, 0);
        }
        public bool WaitStatus(DateTime date, PrinterStatus st, int timeout)
        {

            do
            {
                UpdateStatus();

                var ps = this.Messages.GetEvents(date);
                while (ps.Count == 0 && DateTime.Now.AddMilliseconds(200) < date)
                {
                    ps = _messages.GetEvents(date);
                }
                foreach (var s in ps)
                {
                    Debug.WriteLine(s.Status.ToString());
                    if (s.Status == PrinterStatus.TICKET_ACK)
                    {
                        return true;
                    }
                }
            } while (date.AddMilliseconds(timeout) > DateTime.Now);
            return false;
        }
        //public string[] GetPrinterPorts()
        public static string[] GetPrinterPorts()
        {
            string[] portnames = SerialPort.GetPortNames();

            string[] ret = new string[0];
            Hashtable results = new Hashtable();
            Thread[] threads = new Thread[portnames.Length];
            for (int i = 0; i < portnames.Length; i++)
            {
                results.Add(portnames[i], new CheckPortParams { PortName = portnames[i], isBocaPrinter = false });

                //threads[i] = new Thread(CheckPortInt);
                threads[i] = new Thread(CheckPort);
                threads[i].Start(results[portnames[i]]);
            }
            for (int i = 0; i < portnames.Length; i++)
            {
                threads[i].Join();
            }
            IDictionaryEnumerator dic = results.GetEnumerator();
            while (dic.MoveNext())
            {
                CheckPortParams p = (CheckPortParams)dic.Value;
                if (p.isBocaPrinter)
                {
                    Array.Resize(ref ret, ret.Length + 1);
                    ret[ret.Length - 1] = p.PortName;
                }
            }
            return ret;
        }
        public static bool CheckPort(string portname)
        {
            CheckPortParams p = new CheckPortParams { PortName = portname, isBocaPrinter = false };
            BocaPrinter.CheckPort(p);
            //CheckPort(p);
            return p.isBocaPrinter;
        }
        private static void CheckPort(object pars)
        {
            CheckPortParams p = (CheckPortParams)pars;
            using (SerialPort port = new SerialPort(p.PortName))
            {
                ConfigurePort(port);

                try
                {
                    if (port.IsOpen)
                    {
                        port.Close();
                        ////flavio
                        //GC.ReRegisterForFinalize(port.BaseStream);
                        ////fine flavio

                    }
                    port.Open();
                    //flavio
                    //GC.SuppressFinalize(port.BaseStream);
                    //fine flavio
                    //ERRORE TIMEOUT
                    port.Write("<S2>");
                    Thread.Sleep(100);
                    byte[] buff = ReadData(port);
                    p.isBocaPrinter = Encoding.ASCII.GetString(buff).IndexOf("PROM") >= 8;
                    Debug.WriteLine(p.PortName + ":" + p.isBocaPrinter.ToString());
                    try
                    {
                        port.Close();
                        //port.Dispose();
                        ////flavio
                        //GC.ReRegisterForFinalize(port.BaseStream);
                        ////fine flavio
                    }
                    catch(Exception exint) 
                    {
                        LogExc(GetRecursiveInnerException(exint));
                    }
                }
                catch
                {
                    p.isBocaPrinter = false;
                    return;
                }
            }
        }

        private class CheckPortParams
        {
            public bool isBocaPrinter;
            public string PortName;
        }


        static void ConfigurePort(SerialPort sp)
        {
            try
            {
                PrinterConfig pc = new PrinterConfig();
                BocaPrinter.ConfigurePort(sp, new PrinterConfig());
                sp.WriteTimeout = 10000;
            }
            catch (Exception ex)
            {
                LogExc(GetRecursiveInnerException(ex));
            }
            //catch (ObjectDisposedException obex)
            //{
            //}
        }
        static void ConfigurePort(SerialPort sp, PrinterConfig conf)
        {
            try
            {
                sp.DataBits = conf.DataBits;
                sp.BaudRate = conf.BaudRate;
                sp.DtrEnable = conf.DtrEnable;
                sp.Handshake = conf.Handshake;
                sp.Parity = conf.Parity;
                sp.RtsEnable = conf.RtsEnable;
                sp.StopBits = conf.StopBits;
                sp.WriteTimeout = 2000;
            }
            catch (Exception ex)
            {
                LogExc(GetRecursiveInnerException(ex));
            }
        }


        #region Error Logging


        private static Exception GetRecursiveInnerException(Exception exc)
        {
            if (exc.InnerException != null)
                return GetRecursiveInnerException(exc.InnerException);
            else
                return exc;
        }

        static object c = new object();
        private static void LogExc(Exception ex, string message = "")
        {
            lock (c)
            {
                var logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log.txt";
                var oldLogFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\SCV_Log_old.txt";

                try
                {
                    long len = 0;
                    using (FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.Read))
                        len = fs.Length;
                    if (File.Exists(logFile) && new FileInfo(logFile).Length > 1048576)
                    {
                        File.Copy(logFile, oldLogFile, true);
                        File.Delete(logFile);
                    }
                }
                catch { }
                try
                {
                    using (FileStream fs = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Format("{0}{1}{2}{3}Message:{4}{5}source:{6}{7}Stack Trace:{8}{9}TargetSite:{10}{11}"
                                                                                 , "---------------------------------------------------------"
                                                                                 , System.Environment.NewLine
                                                                                 , DateTime.Now.ToString()
                                                                                 , System.Environment.NewLine
                                                                                 , ex.Message
                                                                                 , System.Environment.NewLine
                                                                                 , ex.Source
                                                                                 , System.Environment.NewLine
                                                                                 , ex.StackTrace
                                                                                 , System.Environment.NewLine
                                                                                 , ex.StackTrace
                                                                                 , System.Environment.NewLine));


                    }
                }
                catch { }
            }
        }
        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                sp.Close();
                ////flavio
                //GC.ReRegisterForFinalize(sp.BaseStream);
                ////fine flavio

            }
            catch(Exception ex) 
            {
                LogExc(GetRecursiveInnerException(ex));
            }



        }

        #endregion
    }



}
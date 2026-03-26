using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Data;
using System.Configuration;
using System.Threading;

namespace REA_DCP_Server_Tool
{
    public class UDP_server
    {
        public delegate void CommunInfo(IPAddress ip, string rcvdata, string snddata);

        private int lport;

        private int rport;

        private UdpClient udpClient;

        private IPEndPoint LocalPoint;

        static Dictionary<string, DateTime> readersEPList = new Dictionary<string, DateTime>();

        static Dictionary<string, DateTime> turnstilsEPList = new Dictionary<string, DateTime>();

        static Dictionary<string, int> turnstileMessageId = new Dictionary<string, int>();

        static string bcode = "";

        public CommunInfo e_comminfo;

        public int act;

        public int cnt;

        private static DateTime dtPrev = new DateTime();

        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        private static int lastid = 1;

        bool differentMessageId = false;

        public UDP_server(IPAddress LocalIP, int localport, int remoteport)
        {
            readersEPList.Clear();
            turnstilsEPList.Clear();
            this.cnt = 1;
            this.lport = localport;
            this.rport = remoteport;
            this.LocalPoint = new IPEndPoint(LocalIP, this.lport);
            dtPrev = DateTime.Now;
            try
            {
                this.udpClient = new UdpClient(this.LocalPoint);
                this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                MessageBox.Show(exception.Message);
            }
        }

        public void Close()
        {
            if (this.udpClient != null)
            {
                this.udpClient.Close();
                //this.Dispose();
            }
        }

        public void Dispose()
        {
            ((IDisposable)this.udpClient).Dispose();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            XmlDocument xmlDocument;
            XmlDeclaration xmlDeclaration;
            XmlElement documentElement;
            XmlElement xmlElement;
            XmlElement xmlElement1;
            XmlElement str;
            StringWriter stringWriter;
            XmlTextWriter xmlTextWriter;
            string str1;
            string barcode = "";

            Common.TicketState ticketState;

            bool pass = false;

            int pax = 0;
            int versoEntrata = 0;

            if (this.udpClient != null)
            {
                try
                {
                    barcode = "";
                    IPEndPoint pEndPoint = null;
                    pEndPoint = null;
                    byte[] numArray = this.udpClient.EndReceive(ar, ref pEndPoint);

                    if (string.IsNullOrEmpty(readersEPList.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                    {
                        readersEPList.Add(pEndPoint.Address.ToString(), DateTime.Now);
                    }

                    if (string.IsNullOrEmpty(turnstilsEPList.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                    {
                        turnstilsEPList.Add(pEndPoint.Address.ToString(), DateTime.Now);
                    }



                    if (numArray != null && pEndPoint.Address.ToString() != this.LocalPoint.Address.ToString())
                    {
                        pEndPoint.Port = this.rport;
                        string str2 = Encoding.UTF8.GetString(numArray);
                        XmlDocument xmlDocument1 = new XmlDocument();

                        try
                        {
                            if (ConfigurationManager.AppSettings["versoentrata"] != null)
                                versoEntrata = Convert.ToInt32(ConfigurationManager.AppSettings["versoentrata"]);

                            xmlDocument1.LoadXml(str2);
                            XmlNode xmlNodes = xmlDocument1.SelectSingleNode("cmf");
                            if (xmlNodes != null)
                            {
                                string value = "1";
                                string value1 = "1";
                                string value2 = "1.0";
                                for (int i = 0; i < xmlNodes.Attributes.Count; i++)
                                {
                                    string name = xmlNodes.Attributes[i].Name;
                                    string str3 = name;
                                    if (name != null)
                                    {
                                        if (str3 == "id")
                                        {
                                            value = xmlNodes.Attributes[i].Value;
                                        }
                                        else
                                        {
                                            if (str3 == "addr")
                                            {
                                                value1 = xmlNodes.Attributes[i].Value;
                                            }
                                            else
                                            {
                                                if (str3 == "protocol")
                                                {
                                                    value2 = xmlNodes.Attributes[i].Value;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!differentMessageId)
                                {
                                    lastid = Convert.ToInt32(value);
                                    lastid++;
                                }
                                else
                                {
                                    int currentMessageId = Convert.ToInt32(value);
                                    if (string.IsNullOrEmpty(turnstileMessageId.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                                    {
                                        turnstileMessageId.Add(pEndPoint.Address.ToString(), currentMessageId++);
                                    }
                                    else
                                    {
                                        turnstileMessageId[pEndPoint.Address.ToString()] = currentMessageId++;
                                    }
                                }

                                XmlNodeList childNodes = xmlNodes.ChildNodes;
                                for (int j = 0; j < childNodes.Count; j++)
                                {
                                    string name1 = childNodes[j].Name;
                                    string str4 = name1;
                                    if (name1 != null)
                                    {
                                        if (str4 == "test")
                                        {
                                            xmlDocument = new XmlDocument();
                                            xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                            documentElement = xmlDocument.DocumentElement;
                                            xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                            xmlElement = xmlDocument.CreateElement("cmf");
                                            xmlDocument.AppendChild(xmlElement);
                                            xmlElement.SetAttribute("id", value);
                                            xmlElement.SetAttribute("addr", value1);
                                            xmlElement.SetAttribute("protocol", value2);
                                            xmlElement1 = xmlDocument.CreateElement("return");
                                            xmlElement.AppendChild(xmlElement1);
                                            str = xmlDocument.CreateElement("time");
                                            DateTime now = DateTime.Now;
                                            str.InnerText = now.ToString("yyyyMMddHHmmss");
                                            xmlElement1.AppendChild(str);
                                            stringWriter = new StringWriter();
                                            xmlTextWriter = new XmlTextWriter(stringWriter);
                                            xmlDocument.WriteTo(xmlTextWriter);
                                            str1 = stringWriter.ToString();
                                            this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                            xmlTextWriter.Close();
                                            if (this.e_comminfo != null)
                                            {
                                                this.e_comminfo(pEndPoint.Address, str2, str1);
                                            }
                                        }
                                        else
                                        {
                                            if (str4 == "entry")
                                            {
                                                // readersEPList.Add(pEndPoint.Address, DateTime.Now);
                                                string textToShow = "";

                                                DateTime dt_Prev_Current_Turn = Convert.ToDateTime(readersEPList[pEndPoint.Address.ToString()]);
                                                //if (DateTime.Now.Subtract(dtPrev).TotalMilliseconds > 3000)
                                                if (DateTime.Now.Subtract(dt_Prev_Current_Turn).TotalMilliseconds > 3000)
                                                {

                                                    #region Lettura Barcode
                                                    DataSet ds = new DataSet();
                                                    StringReader SR = new StringReader(str2);
                                                    ds.ReadXml(SR);
                                                    if (ds.Tables.Count > 1)
                                                    {
                                                        var dt = ds.Tables[1];
                                                        if (dt.Columns.Contains("chip"))
                                                        {
                                                            //lettura barcode
                                                            if (!Convert.IsDBNull(dt.Rows[0]["chip"]))
                                                            {
                                                                bcode = null;
                                                                barcode = Convert.ToString(dt.Rows[0]["chip"]);
                                                                bcode = barcode;
                                                                Ticket_Checker ticket_Checker = null;
                                                                ticket_Checker = new Ticket_Checker();
                                                                ticketState = new Common.TicketState();
                                                                //ticketState.State = Common.TicketStateEnum.Valido;

                                                                ticketState = ticket_Checker.CheckTicket(barcode, true);
                                                                //Thread.Sleep(3000);
                                                                switch (ticketState.State)
                                                                {
                                                                    case Common.TicketStateEnum.Valido:
                                                                        textToShow = "Biglietto valido";
                                                                        pax = ticketState.Pax;
                                                                        this.cnt = pax;
                                                                        this.act = versoEntrata;
                                                                        break;

                                                                    case Common.TicketStateEnum.Validato:
                                                                        textToShow = "Biglietto già vidimato";
                                                                        this.act = 2;
                                                                        break;

                                                                    case Common.TicketStateEnum.Eccedente:
                                                                        textToShow = "Biglietto già vidimato";
                                                                        this.act = 2;
                                                                        break;


                                                                    case Common.TicketStateEnum.Scaduto:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto scaduto";
                                                                        break;

                                                                    case Common.TicketStateEnum.Invalido:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto non valido";
                                                                        break;

                                                                    default:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto non valido";
                                                                        break;
                                                                }
                                                            }

                                                        }
                                                    }
                                                    #endregion

                                                    xmlDocument = new XmlDocument();
                                                    xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                                    documentElement = xmlDocument.DocumentElement;
                                                    xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                                    xmlElement = xmlDocument.CreateElement("cmf");
                                                    xmlDocument.AppendChild(xmlElement);
                                                    xmlElement.SetAttribute("id", value);
                                                    xmlElement.SetAttribute("addr", value1);
                                                    xmlElement.SetAttribute("protocol", value2);
                                                    xmlElement1 = xmlDocument.CreateElement("return");
                                                    xmlElement.AppendChild(xmlElement1);
                                                    str = xmlDocument.CreateElement("act");
                                                    XmlElement xmlElement2 = xmlDocument.CreateElement("cnt");
                                                    XmlElement xmlElement3 = xmlDocument.CreateElement("txt");


                                                    //if (this.act != 0)
                                                    //{
                                                    //    if (this.act != 1)
                                                    //    {
                                                    //        str.InnerText = "x";
                                                    //        xmlElement3.InnerText = "Access denied";
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        str.InnerText = "r";
                                                    //        xmlElement3.InnerText = "Please enter";
                                                    //    }
                                                    //}
                                                    //else
                                                    //{
                                                    //    str.InnerText = "l";
                                                    //    xmlElement3.InnerText = "Please enter";
                                                    //}

                                                    if (this.act != 0)
                                                    {
                                                        if (this.act != 1)
                                                        {
                                                            str.InnerText = "x";
                                                            xmlElement3.InnerText = textToShow;
                                                        }
                                                        else
                                                        {
                                                            str.InnerText = "r";
                                                            xmlElement3.InnerText = textToShow;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        str.InnerText = "l";
                                                        xmlElement3.InnerText = textToShow;
                                                    }

                                                    xmlElement2.InnerText = this.cnt.ToString();
                                                    xmlElement1.AppendChild(str);
                                                    xmlElement1.AppendChild(xmlElement2);
                                                    xmlElement1.AppendChild(xmlElement3);
                                                    stringWriter = new StringWriter();
                                                    xmlTextWriter = new XmlTextWriter(stringWriter);
                                                    xmlDocument.WriteTo(xmlTextWriter);
                                                    str1 = stringWriter.ToString();
                                                    this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                                    xmlTextWriter.Close();

                                                    if (this.e_comminfo != null)
                                                    {
                                                        //dtPrev = DateTime.Now;
                                                        readersEPList[pEndPoint.Address.ToString()] = DateTime.Now;
                                                        this.e_comminfo(pEndPoint.Address, str2, str1);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (str4 == "pass")
                                                {
                                                    string returnValue = "true";

                                                    //DateTime dt_Prev_Current_Turn = Convert.ToDateTime(turnstilsEPList[pEndPoint.Address.ToString()]);
                                                    //if (DateTime.Now.Subtract(dt_Prev_Current_Turn).TotalMilliseconds > 2000)
                                                    //{
                                                    pass = true;
                                                    /*
                                                    try
                                                    {
                                                                
                                                        #region Lettura Barcode
                                                        DataSet ds = new DataSet();
                                                        StringReader SR = new StringReader(str2);
                                                        ds.ReadXml(SR);
                                                        if (ds.Tables.Count > 1)
                                                        {
                                                            var dt = ds.Tables[1];
                                                            //if (dt.Columns.Contains("chip"))
                                                            if (bcode != null)
                                                            {
                                                                //lettura barcode
                                                                //if (!Convert.IsDBNull(dt.Rows[0]["chip"]))
                                                                //{
                                                                //barcode = Convert.ToString(dt.Rows[0]["chip"]);
                                                                barcode = bcode;
                                                                Ticket_Checker ticket_Checker = null;
                                                                ticket_Checker = new Ticket_Checker();
                                                                ticketState = new Common.TicketState();
                                                                //ticketState.State = Common.TicketStateEnum.Valido;

                                                                ticketState = ticket_Checker.CheckTicket(barcode, false);
                                                                //Thread.Sleep(3000);
                                                                switch (ticketState.State)
                                                                {
                                                                    case Common.TicketStateEnum.Valido:
                                                                        returnValue = "true";
                                                                        break;

                                                                    case Common.TicketStateEnum.Validato:
                                                                        returnValue = "false";
                                                                        break;

                                                                    case Common.TicketStateEnum.Eccedente:
                                                                        returnValue = "false";
                                                                        break;


                                                                    case Common.TicketStateEnum.Scaduto:
                                                                        returnValue = "false";
                                                                        break;

                                                                    case Common.TicketStateEnum.Invalido:
                                                                        returnValue = "false";
                                                                        break;

                                                                    default:
                                                                        returnValue = "false";
                                                                        break;
                                                                }
                                                                //}

                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                    }
                                                    */

                                                    xmlDocument = new XmlDocument();
                                                    xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                                    documentElement = xmlDocument.DocumentElement;
                                                    xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                                    xmlElement = xmlDocument.CreateElement("cmf");
                                                    xmlDocument.AppendChild(xmlElement);
                                                    xmlElement.SetAttribute("id", value);
                                                    xmlElement.SetAttribute("addr", value1);
                                                    xmlElement.SetAttribute("protocol", value2);

                                                    xmlElement1 = xmlDocument.CreateElement("return");
                                                    xmlElement1.InnerText = returnValue;
                                                    xmlElement.AppendChild(xmlElement1);
                                                    stringWriter = new StringWriter();
                                                    xmlTextWriter = new XmlTextWriter(stringWriter);
                                                    xmlDocument.WriteTo(xmlTextWriter);
                                                    str1 = stringWriter.ToString();
                                                    this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                                    xmlTextWriter.Close();
                                                    if (this.e_comminfo != null)
                                                    {
                                                        turnstilsEPList[pEndPoint.Address.ToString()] = DateTime.Now;
                                                        this.e_comminfo(pEndPoint.Address, str2, str1);
                                                    }

                                                    //if (pass)
                                                    //{
                                                    //    foreach (var item in readersEPList)
                                                    //    {
                                                    //        OpenTurnstile(item.Key);
                                                    //    }
                                                    //}

                                                    //}
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);




                }
                catch
                {
                    this.udpClient.Close();
                    this.udpClient = null;
                }
                return;
            }
            else
            {
                return;
            }
        }

        private void ReceiveCallback_1(IAsyncResult ar)
        {
            XmlDocument xmlDocument;
            XmlDeclaration xmlDeclaration;
            XmlElement documentElement;
            XmlElement xmlElement;
            XmlElement xmlElement1;
            XmlElement str;
            StringWriter stringWriter;
            XmlTextWriter xmlTextWriter;
            string str1;
            string barcode = "";

            Common.TicketState ticketState;

            int pax = 0;
            int versoEntrata = 0;

            if (this.udpClient != null)
            {
                try
                {
                    barcode = "";
                    IPEndPoint pEndPoint = null;
                    pEndPoint = null;
                    byte[] numArray = this.udpClient.EndReceive(ar, ref pEndPoint);

                    if (string.IsNullOrEmpty(readersEPList.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                    {
                        readersEPList.Add(pEndPoint.Address.ToString(), DateTime.Now);
                    }

                    if (string.IsNullOrEmpty(turnstilsEPList.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                    {
                        turnstilsEPList.Add(pEndPoint.Address.ToString(), DateTime.Now);
                    }



                    if (numArray != null && pEndPoint.Address.ToString() != this.LocalPoint.Address.ToString())
                    {
                        pEndPoint.Port = this.rport;
                        string str2 = Encoding.UTF8.GetString(numArray);
                        XmlDocument xmlDocument1 = new XmlDocument();

                        try
                        {
                            if (ConfigurationManager.AppSettings["versoentrata"] != null)
                                versoEntrata = Convert.ToInt32(ConfigurationManager.AppSettings["versoentrata"]);

                            xmlDocument1.LoadXml(str2);
                            XmlNode xmlNodes = xmlDocument1.SelectSingleNode("cmf");
                            if (xmlNodes != null)
                            {
                                string value = "1";
                                string value1 = "1";
                                string value2 = "1.0";
                                for (int i = 0; i < xmlNodes.Attributes.Count; i++)
                                {
                                    string name = xmlNodes.Attributes[i].Name;
                                    string str3 = name;
                                    if (name != null)
                                    {
                                        if (str3 == "id")
                                        {
                                            value = xmlNodes.Attributes[i].Value;
                                        }
                                        else
                                        {
                                            if (str3 == "addr")
                                            {
                                                value1 = xmlNodes.Attributes[i].Value;
                                            }
                                            else
                                            {
                                                if (str3 == "protocol")
                                                {
                                                    value2 = xmlNodes.Attributes[i].Value;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!differentMessageId)
                                {
                                    lastid = Convert.ToInt32(value);
                                    lastid++;
                                }
                                else
                                {
                                    int currentMessageId = Convert.ToInt32(value);
                                    if (string.IsNullOrEmpty(turnstileMessageId.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                                    {
                                        turnstileMessageId.Add(pEndPoint.Address.ToString(), currentMessageId++);
                                    }
                                    else
                                    {
                                        turnstileMessageId[pEndPoint.Address.ToString()] = currentMessageId++;
                                    }
                                }

                                XmlNodeList childNodes = xmlNodes.ChildNodes;
                                for (int j = 0; j < childNodes.Count; j++)
                                {
                                    string name1 = childNodes[j].Name;
                                    string str4 = name1;
                                    if (name1 != null)
                                    {
                                        if (str4 == "test")
                                        {
                                            xmlDocument = new XmlDocument();
                                            xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                            documentElement = xmlDocument.DocumentElement;
                                            xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                            xmlElement = xmlDocument.CreateElement("cmf");
                                            xmlDocument.AppendChild(xmlElement);
                                            xmlElement.SetAttribute("id", value);
                                            xmlElement.SetAttribute("addr", value1);
                                            xmlElement.SetAttribute("protocol", value2);
                                            xmlElement1 = xmlDocument.CreateElement("return");
                                            xmlElement.AppendChild(xmlElement1);
                                            str = xmlDocument.CreateElement("time");
                                            DateTime now = DateTime.Now;
                                            str.InnerText = now.ToString("yyyyMMddHHmmss");
                                            xmlElement1.AppendChild(str);
                                            stringWriter = new StringWriter();
                                            xmlTextWriter = new XmlTextWriter(stringWriter);
                                            xmlDocument.WriteTo(xmlTextWriter);
                                            str1 = stringWriter.ToString();
                                            this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                            xmlTextWriter.Close();
                                            if (this.e_comminfo != null)
                                            {
                                                this.e_comminfo(pEndPoint.Address, str2, str1);
                                            }
                                        }
                                        else
                                        {
                                            if (str4 == "entry")
                                            {
                                                // readersEPList.Add(pEndPoint.Address, DateTime.Now);
                                                string textToShow = "";
                                                DateTime dt_Prev_Current_Turn = Convert.ToDateTime(readersEPList[pEndPoint.Address.ToString()]);
                                                //if (DateTime.Now.Subtract(dtPrev).TotalMilliseconds > 3000)
                                                if (DateTime.Now.Subtract(dt_Prev_Current_Turn).TotalMilliseconds > 3000)
                                                {

                                                    #region Lettura Barcode
                                                    DataSet ds = new DataSet();
                                                    StringReader SR = new StringReader(str2);
                                                    ds.ReadXml(SR);
                                                    if (ds.Tables.Count > 1)
                                                    {
                                                        var dt = ds.Tables[1];
                                                        if (dt.Columns.Contains("chip"))
                                                        {
                                                            //lettura barcode
                                                            if (!Convert.IsDBNull(dt.Rows[0]["chip"]))
                                                            {
                                                                barcode = Convert.ToString(dt.Rows[0]["chip"]);
                                                                bcode = barcode;
                                                                Ticket_Checker ticket_Checker = null;
                                                                ticket_Checker = new Ticket_Checker();
                                                                ticketState = new Common.TicketState();
                                                                //ticketState.State = Common.TicketStateEnum.Valido;

                                                                ticketState = ticket_Checker.CheckTicket(barcode, false);
                                                                //Thread.Sleep(3000);
                                                                switch (ticketState.State)
                                                                {
                                                                    case Common.TicketStateEnum.Valido:
                                                                        textToShow = "Biglietto valido";
                                                                        pax = ticketState.Pax;
                                                                        this.cnt = pax;
                                                                        this.act = versoEntrata;
                                                                        break;

                                                                    case Common.TicketStateEnum.Validato:
                                                                        textToShow = "Biglietto già vidimato";
                                                                        this.act = 2;
                                                                        break;

                                                                    case Common.TicketStateEnum.Eccedente:
                                                                        textToShow = "Biglietto già vidimato";
                                                                        this.act = 2;
                                                                        break;


                                                                    case Common.TicketStateEnum.Scaduto:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto scaduto";
                                                                        break;

                                                                    case Common.TicketStateEnum.Invalido:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto non valido";
                                                                        break;

                                                                    default:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto non valido";
                                                                        break;
                                                                }
                                                            }

                                                        }
                                                    }
                                                    #endregion

                                                    xmlDocument = new XmlDocument();
                                                    xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                                    documentElement = xmlDocument.DocumentElement;
                                                    xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                                    xmlElement = xmlDocument.CreateElement("cmf");
                                                    xmlDocument.AppendChild(xmlElement);
                                                    xmlElement.SetAttribute("id", value);
                                                    xmlElement.SetAttribute("addr", value1);
                                                    xmlElement.SetAttribute("protocol", value2);
                                                    xmlElement1 = xmlDocument.CreateElement("return");
                                                    xmlElement.AppendChild(xmlElement1);
                                                    str = xmlDocument.CreateElement("act");
                                                    XmlElement xmlElement2 = xmlDocument.CreateElement("cnt");
                                                    XmlElement xmlElement3 = xmlDocument.CreateElement("txt");


                                                    //if (this.act != 0)
                                                    //{
                                                    //    if (this.act != 1)
                                                    //    {
                                                    //        str.InnerText = "x";
                                                    //        xmlElement3.InnerText = "Access denied";
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        str.InnerText = "r";
                                                    //        xmlElement3.InnerText = "Please enter";
                                                    //    }
                                                    //}
                                                    //else
                                                    //{
                                                    //    str.InnerText = "l";
                                                    //    xmlElement3.InnerText = "Please enter";
                                                    //}

                                                    if (this.act != 0)
                                                    {
                                                        if (this.act != 1)
                                                        {
                                                            str.InnerText = "x";
                                                            xmlElement3.InnerText = textToShow;
                                                        }
                                                        else
                                                        {
                                                            str.InnerText = "r";
                                                            xmlElement3.InnerText = textToShow;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        str.InnerText = "l";
                                                        xmlElement3.InnerText = textToShow;
                                                    }

                                                    xmlElement2.InnerText = this.cnt.ToString();
                                                    xmlElement1.AppendChild(str);
                                                    xmlElement1.AppendChild(xmlElement2);
                                                    xmlElement1.AppendChild(xmlElement3);
                                                    stringWriter = new StringWriter();
                                                    xmlTextWriter = new XmlTextWriter(stringWriter);
                                                    xmlDocument.WriteTo(xmlTextWriter);
                                                    str1 = stringWriter.ToString();
                                                    this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                                    xmlTextWriter.Close();

                                                    if (this.e_comminfo != null)
                                                    {
                                                        //dtPrev = DateTime.Now;
                                                        readersEPList[pEndPoint.Address.ToString()] = DateTime.Now;
                                                        this.e_comminfo(pEndPoint.Address, str2, str1);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (str4 == "pass")
                                                {
                                                    string returnValue = "true";

                                                    DateTime dt_Prev_Current_Turn = Convert.ToDateTime(turnstilsEPList[pEndPoint.Address.ToString()]);

                                                    if (DateTime.Now.Subtract(dt_Prev_Current_Turn).TotalMilliseconds > 3000)
                                                    {
                                                        try
                                                        {
                                                            #region Lettura Barcode
                                                            DataSet ds = new DataSet();
                                                            StringReader SR = new StringReader(str2);
                                                            ds.ReadXml(SR);
                                                            if (ds.Tables.Count > 1)
                                                            {
                                                                var dt = ds.Tables[1];
                                                                if (dt.Columns.Contains("chip"))
                                                                {
                                                                    //lettura barcode
                                                                    if (!Convert.IsDBNull(dt.Rows[0]["chip"]))
                                                                    {
                                                                        barcode = Convert.ToString(dt.Rows[0]["chip"]);

                                                                        Ticket_Checker ticket_Checker = null;
                                                                        ticket_Checker = new Ticket_Checker();
                                                                        ticketState = new Common.TicketState();
                                                                        //ticketState.State = Common.TicketStateEnum.Valido;

                                                                        ticketState = ticket_Checker.CheckTicket(barcode, false);
                                                                        //Thread.Sleep(3000);
                                                                        switch (ticketState.State)
                                                                        {
                                                                            case Common.TicketStateEnum.Valido:
                                                                                returnValue = "true";
                                                                                break;

                                                                            case Common.TicketStateEnum.Validato:
                                                                                returnValue = "false";
                                                                                break;

                                                                            case Common.TicketStateEnum.Eccedente:
                                                                                returnValue = "false";
                                                                                break;


                                                                            case Common.TicketStateEnum.Scaduto:
                                                                                returnValue = "false";
                                                                                break;

                                                                            case Common.TicketStateEnum.Invalido:
                                                                                returnValue = "false";
                                                                                break;

                                                                            default:
                                                                                returnValue = "false";
                                                                                break;
                                                                        }
                                                                    }

                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                        }


                                                        xmlDocument = new XmlDocument();
                                                        xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                                        documentElement = xmlDocument.DocumentElement;
                                                        xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                                        xmlElement = xmlDocument.CreateElement("cmf");
                                                        xmlDocument.AppendChild(xmlElement);
                                                        xmlElement.SetAttribute("id", value);
                                                        xmlElement.SetAttribute("addr", value1);
                                                        xmlElement.SetAttribute("protocol", value2);

                                                        xmlElement1 = xmlDocument.CreateElement("return");
                                                        xmlElement1.InnerText = returnValue;
                                                        xmlElement.AppendChild(xmlElement1);
                                                        stringWriter = new StringWriter();
                                                        xmlTextWriter = new XmlTextWriter(stringWriter);
                                                        xmlDocument.WriteTo(xmlTextWriter);
                                                        str1 = stringWriter.ToString();
                                                        this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                                        xmlTextWriter.Close();
                                                        if (this.e_comminfo != null)
                                                        {
                                                            //turnstilsEPList[pEndPoint.Address.ToString()] = DateTime.Now;
                                                            this.e_comminfo(pEndPoint.Address, str2, str1);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback_1), null);




                }
                catch
                {
                    this.udpClient.Close();
                    this.udpClient = null;
                }
                return;
            }
            else
            {
                return;
            }
        }

        private void ReceiveCallback_2(IAsyncResult ar)
        {
            XmlDocument xmlDocument;
            XmlDeclaration xmlDeclaration;
            XmlElement documentElement;
            XmlElement xmlElement;
            XmlElement xmlElement1;
            XmlElement str;
            StringWriter stringWriter;
            XmlTextWriter xmlTextWriter;
            string str1;
            string barcode = "";

            Common.TicketState ticketState;

            int pax = 0;
            int versoEntrata = 0;

            if (this.udpClient != null)
            {
                try
                {
                    barcode = "";
                    IPEndPoint pEndPoint = null;
                    pEndPoint = null;
                    byte[] numArray = this.udpClient.EndReceive(ar, ref pEndPoint);

                    if (string.IsNullOrEmpty(readersEPList.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                    {
                        readersEPList.Add(pEndPoint.Address.ToString(), DateTime.Now);
                    }

                    if (string.IsNullOrEmpty(turnstilsEPList.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                    {
                        turnstilsEPList.Add(pEndPoint.Address.ToString(), DateTime.Now);
                    }



                    if (numArray != null && pEndPoint.Address.ToString() != this.LocalPoint.Address.ToString())
                    {
                        pEndPoint.Port = this.rport;
                        string str2 = Encoding.UTF8.GetString(numArray);
                        XmlDocument xmlDocument1 = new XmlDocument();

                        try
                        {
                            if (ConfigurationManager.AppSettings["versoentrata"] != null)
                                versoEntrata = Convert.ToInt32(ConfigurationManager.AppSettings["versoentrata"]);

                            xmlDocument1.LoadXml(str2);
                            XmlNode xmlNodes = xmlDocument1.SelectSingleNode("cmf");
                            if (xmlNodes != null)
                            {
                                string value = "1";
                                string value1 = "1";
                                string value2 = "1.0";
                                for (int i = 0; i < xmlNodes.Attributes.Count; i++)
                                {
                                    string name = xmlNodes.Attributes[i].Name;
                                    string str3 = name;
                                    if (name != null)
                                    {
                                        if (str3 == "id")
                                        {
                                            value = xmlNodes.Attributes[i].Value;
                                        }
                                        else
                                        {
                                            if (str3 == "addr")
                                            {
                                                value1 = xmlNodes.Attributes[i].Value;
                                            }
                                            else
                                            {
                                                if (str3 == "protocol")
                                                {
                                                    value2 = xmlNodes.Attributes[i].Value;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!differentMessageId)
                                {
                                    lastid = Convert.ToInt32(value);
                                    lastid++;
                                }
                                else
                                {
                                    int currentMessageId = Convert.ToInt32(value);
                                    if (string.IsNullOrEmpty(turnstileMessageId.FirstOrDefault(e => e.Key == pEndPoint.Address.ToString()).Key))
                                    {
                                        turnstileMessageId.Add(pEndPoint.Address.ToString(), currentMessageId++);
                                    }
                                    else
                                    {
                                        turnstileMessageId[pEndPoint.Address.ToString()] = currentMessageId++;
                                    }
                                }

                                XmlNodeList childNodes = xmlNodes.ChildNodes;
                                for (int j = 0; j < childNodes.Count; j++)
                                {
                                    string name1 = childNodes[j].Name;
                                    string str4 = name1;
                                    if (name1 != null)
                                    {
                                        if (str4 == "test")
                                        {
                                            xmlDocument = new XmlDocument();
                                            xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                            documentElement = xmlDocument.DocumentElement;
                                            xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                            xmlElement = xmlDocument.CreateElement("cmf");
                                            xmlDocument.AppendChild(xmlElement);
                                            xmlElement.SetAttribute("id", value);
                                            xmlElement.SetAttribute("addr", value1);
                                            xmlElement.SetAttribute("protocol", value2);
                                            xmlElement1 = xmlDocument.CreateElement("return");
                                            xmlElement.AppendChild(xmlElement1);
                                            str = xmlDocument.CreateElement("time");
                                            DateTime now = DateTime.Now;
                                            str.InnerText = now.ToString("yyyyMMddHHmmss");
                                            xmlElement1.AppendChild(str);
                                            stringWriter = new StringWriter();
                                            xmlTextWriter = new XmlTextWriter(stringWriter);
                                            xmlDocument.WriteTo(xmlTextWriter);
                                            str1 = stringWriter.ToString();
                                            this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                            xmlTextWriter.Close();
                                            if (this.e_comminfo != null)
                                            {
                                                this.e_comminfo(pEndPoint.Address, str2, str1);
                                            }
                                        }
                                        else
                                        {
                                            if (str4 == "entry")
                                            {
                                                // readersEPList.Add(pEndPoint.Address, DateTime.Now);
                                                string textToShow = "";

                                                DateTime dt_Prev_Current_Turn = Convert.ToDateTime(readersEPList[pEndPoint.Address.ToString()]);
                                                //if (DateTime.Now.Subtract(dtPrev).TotalMilliseconds > 3000)
                                                if (DateTime.Now.Subtract(dt_Prev_Current_Turn).TotalMilliseconds > 3000)
                                                {

                                                    #region Lettura Barcode
                                                    DataSet ds = new DataSet();
                                                    StringReader SR = new StringReader(str2);
                                                    ds.ReadXml(SR);
                                                    if (ds.Tables.Count > 1)
                                                    {
                                                        var dt = ds.Tables[1];
                                                        if (dt.Columns.Contains("chip"))
                                                        {
                                                            //lettura barcode
                                                            if (!Convert.IsDBNull(dt.Rows[0]["chip"]))
                                                            {
                                                                bcode = null;
                                                                barcode = Convert.ToString(dt.Rows[0]["chip"]);
                                                                bcode = barcode;
                                                                Ticket_Checker ticket_Checker = null;
                                                                ticket_Checker = new Ticket_Checker();
                                                                ticketState = new Common.TicketState();
                                                                //ticketState.State = Common.TicketStateEnum.Valido;

                                                                ticketState = ticket_Checker.CheckTicket(barcode, false);
                                                                //Thread.Sleep(3000);
                                                                switch (ticketState.State)
                                                                {
                                                                    case Common.TicketStateEnum.Valido:
                                                                        textToShow = "Biglietto valido";
                                                                        pax = ticketState.Pax;
                                                                        this.cnt = pax;
                                                                        this.act = versoEntrata;
                                                                        break;

                                                                    case Common.TicketStateEnum.Validato:
                                                                        textToShow = "Biglietto già vidimato";
                                                                        this.act = 2;
                                                                        break;

                                                                    case Common.TicketStateEnum.Eccedente:
                                                                        textToShow = "Biglietto già vidimato";
                                                                        this.act = 2;
                                                                        break;


                                                                    case Common.TicketStateEnum.Scaduto:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto scaduto";
                                                                        break;

                                                                    case Common.TicketStateEnum.Invalido:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto non valido";
                                                                        break;

                                                                    default:
                                                                        this.act = 2;
                                                                        textToShow = "Biglietto non valido";
                                                                        break;
                                                                }
                                                            }

                                                        }
                                                    }
                                                    #endregion

                                                    xmlDocument = new XmlDocument();
                                                    xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                                    documentElement = xmlDocument.DocumentElement;
                                                    xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                                    xmlElement = xmlDocument.CreateElement("cmf");
                                                    xmlDocument.AppendChild(xmlElement);
                                                    xmlElement.SetAttribute("id", value);
                                                    xmlElement.SetAttribute("addr", value1);
                                                    xmlElement.SetAttribute("protocol", value2);
                                                    xmlElement1 = xmlDocument.CreateElement("return");
                                                    xmlElement.AppendChild(xmlElement1);
                                                    str = xmlDocument.CreateElement("act");
                                                    XmlElement xmlElement2 = xmlDocument.CreateElement("cnt");
                                                    XmlElement xmlElement3 = xmlDocument.CreateElement("txt");


                                                    //if (this.act != 0)
                                                    //{
                                                    //    if (this.act != 1)
                                                    //    {
                                                    //        str.InnerText = "x";
                                                    //        xmlElement3.InnerText = "Access denied";
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        str.InnerText = "r";
                                                    //        xmlElement3.InnerText = "Please enter";
                                                    //    }
                                                    //}
                                                    //else
                                                    //{
                                                    //    str.InnerText = "l";
                                                    //    xmlElement3.InnerText = "Please enter";
                                                    //}

                                                    if (this.act != 0)
                                                    {
                                                        if (this.act != 1)
                                                        {
                                                            str.InnerText = "x";
                                                            xmlElement3.InnerText = textToShow;
                                                        }
                                                        else
                                                        {
                                                            str.InnerText = "r";
                                                            xmlElement3.InnerText = textToShow;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        str.InnerText = "l";
                                                        xmlElement3.InnerText = textToShow;
                                                    }

                                                    xmlElement2.InnerText = this.cnt.ToString();
                                                    xmlElement1.AppendChild(str);
                                                    xmlElement1.AppendChild(xmlElement2);
                                                    xmlElement1.AppendChild(xmlElement3);
                                                    stringWriter = new StringWriter();
                                                    xmlTextWriter = new XmlTextWriter(stringWriter);
                                                    xmlDocument.WriteTo(xmlTextWriter);
                                                    str1 = stringWriter.ToString();
                                                    this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                                    xmlTextWriter.Close();

                                                    if (this.e_comminfo != null)
                                                    {
                                                        //dtPrev = DateTime.Now;
                                                        readersEPList[pEndPoint.Address.ToString()] = DateTime.Now;
                                                        this.e_comminfo(pEndPoint.Address, str2, str1);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (str4 == "pass")
                                                {
                                                    string returnValue = "true";

                                                    DateTime dt_Prev_Current_Turn = Convert.ToDateTime(turnstilsEPList[pEndPoint.Address.ToString()]);

                                                    if (DateTime.Now.Subtract(dt_Prev_Current_Turn).TotalMilliseconds > 2000)
                                                    {

                                                        #region Lettura Barcode
                                                        DataSet ds = new DataSet();
                                                        StringReader SR = new StringReader(str2);
                                                        ds.ReadXml(SR);
                                                        string textToShow = "";


                                                        //lettura barcode
                                                        if (bcode != null)
                                                        {

                                                            barcode = Convert.ToString(bcode);
                                                            bcode = barcode;
                                                            Ticket_Checker ticket_Checker = null;
                                                            ticket_Checker = new Ticket_Checker();
                                                            ticketState = new Common.TicketState();

                                                            textToShow = "Biglietto valido";
                                                            pax = ticketState.Pax;
                                                            this.cnt = 1;
                                                            this.act = versoEntrata;


                                                            /*
                                                            ticketState = ticket_Checker.CheckTicket(barcode, false);
                                                               
                                                            switch (ticketState.State)
                                                            {
                                                                case Common.TicketStateEnum.Valido:
                                                                    textToShow = "Biglietto valido";
                                                                    pax = ticketState.Pax;
                                                                    this.cnt = pax;
                                                                    this.act = versoEntrata;
                                                                    break;

                                                                case Common.TicketStateEnum.Validato:
                                                                    textToShow = "Biglietto già vidimato";
                                                                    this.act = 2;
                                                                    break;

                                                                case Common.TicketStateEnum.Eccedente:
                                                                    textToShow = "Biglietto già vidimato";
                                                                    this.act = 2;
                                                                    break;


                                                                case Common.TicketStateEnum.Scaduto:
                                                                    this.act = 2;
                                                                    textToShow = "Biglietto scaduto";
                                                                    break;

                                                                case Common.TicketStateEnum.Invalido:
                                                                    this.act = 2;
                                                                    textToShow = "Biglietto non valido";
                                                                    break;

                                                                default:
                                                                    this.act = 2;
                                                                    textToShow = "Biglietto non valido";
                                                                    break;
                                                            } 
                                                            */


                                                        }



                                                        #endregion

                                                        xmlDocument = new XmlDocument();
                                                        xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                                                        documentElement = xmlDocument.DocumentElement;
                                                        xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                                                        xmlElement = xmlDocument.CreateElement("cmf");
                                                        xmlDocument.AppendChild(xmlElement);
                                                        xmlElement.SetAttribute("id", value);
                                                        xmlElement.SetAttribute("addr", value1);
                                                        xmlElement.SetAttribute("protocol", value2);
                                                        xmlElement1 = xmlDocument.CreateElement("return");
                                                        xmlElement.AppendChild(xmlElement1);
                                                        str = xmlDocument.CreateElement("act");
                                                        XmlElement xmlElement2 = xmlDocument.CreateElement("cnt");
                                                        XmlElement xmlElement3 = xmlDocument.CreateElement("txt");


                                                        //if (this.act != 0)
                                                        //{
                                                        //    if (this.act != 1)
                                                        //    {
                                                        //        str.InnerText = "x";
                                                        //        xmlElement3.InnerText = "Access denied";
                                                        //    }
                                                        //    else
                                                        //    {
                                                        //        str.InnerText = "r";
                                                        //        xmlElement3.InnerText = "Please enter";
                                                        //    }
                                                        //}
                                                        //else
                                                        //{
                                                        //    str.InnerText = "l";
                                                        //    xmlElement3.InnerText = "Please enter";
                                                        //}

                                                        if (this.act != 0)
                                                        {
                                                            if (this.act != 1)
                                                            {
                                                                str.InnerText = "x";
                                                                xmlElement3.InnerText = textToShow;
                                                            }
                                                            else
                                                            {
                                                                str.InnerText = "r";
                                                                xmlElement3.InnerText = textToShow;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            str.InnerText = "l";
                                                            xmlElement3.InnerText = textToShow;
                                                        }

                                                        xmlElement2.InnerText = this.cnt.ToString();
                                                        xmlElement1.AppendChild(str);
                                                        xmlElement1.AppendChild(xmlElement2);
                                                        xmlElement1.AppendChild(xmlElement3);
                                                        stringWriter = new StringWriter();
                                                        xmlTextWriter = new XmlTextWriter(stringWriter);
                                                        xmlDocument.WriteTo(xmlTextWriter);
                                                        str1 = stringWriter.ToString();
                                                        this.Send(Encoding.UTF8.GetBytes(str1), pEndPoint);
                                                        xmlTextWriter.Close();

                                                        if (this.e_comminfo != null)
                                                        {
                                                            //dtPrev = DateTime.Now;
                                                            readersEPList[pEndPoint.Address.ToString()] = DateTime.Now;
                                                            this.e_comminfo(pEndPoint.Address, str2, str1);
                                                        }


                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback_2), null);




                }
                catch
                {
                    this.udpClient.Close();
                    this.udpClient = null;
                }
                return;
            }
            else
            {
                return;
            }
        }

        public bool Send(byte[] packet, IPEndPoint RP)
        {
            if (this.udpClient != null)
            {
                int num = 0;
                try
                {
                    num = this.udpClient.Send(packet, (int)packet.Length, RP);
                }
                catch
                {
                }
                if (num != (int)packet.Length)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        //public void SetPanicState(string state)
        //{

        //    if (this.udpClient != null)
        //    {
        //        try
        //        {
        //            //differentMessageId
        //            string value = "";

        //            value = lastid.ToString();

        //            string value1 = "1";
        //            string value2 = "1.0";
        //            XmlDocument xmlDocument = new XmlDocument();
        //            var xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
        //            var documentElement = xmlDocument.DocumentElement;
        //            xmlDocument.InsertBefore(xmlDeclaration, documentElement);
        //            var xmlElement = xmlDocument.CreateElement("cmf");
        //            xmlDocument.AppendChild(xmlElement);
        //            xmlElement.SetAttribute("id", value);
        //            xmlElement.SetAttribute("addr", value1);
        //            xmlElement.SetAttribute("protocol", value2);
        //            var xmlElement1 = xmlDocument.CreateElement("panic");
        //            xmlElement.AppendChild(xmlElement1);
        //            var stateEl = xmlDocument.CreateElement("state");

        //            stateEl.InnerText = state;

        //            xmlElement1.AppendChild(stateEl);
        //            if (state == "on")
        //            {
        //                XmlElement xmlElement3 = xmlDocument.CreateElement("txt");
        //                xmlElement3.InnerText = "Leave the area, please";
        //                xmlElement1.AppendChild(xmlElement3);
        //            }

        //            var stringWriter = new StringWriter();
        //            var xmlTextWriter = new XmlTextWriter(stringWriter);
        //            xmlDocument.WriteTo(xmlTextWriter);
        //            string str1 = stringWriter.ToString();
        //            var packet = Encoding.UTF8.GetBytes(str1);

        //            //var num = this.udpClient.Send(packet, (int)packet.Length, "192.168.0.101", 1001);
        //            lastid++;
        //            this.udpClient.BeginSend(packet, (int)packet.Length, "192.168.0.101", 1001, new AsyncCallback(SendCallback), null);

        //            xmlTextWriter.Close();
        //            //this.udpClient.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //    else
        //    {
        //        this.udpClient = new UdpClient(this.LocalPoint);
        //        SetPanicState(state);
        //    }



        //}

        public void SetPanicState(string state)
        {
            foreach (var item in readersEPList)
            {
                SetPanicState(item.Key, state);
            }
        }

        private void SetPanicState(string turnstileIP, string state)
        {
            if (this.udpClient != null)
            {
                try
                {
                    string value = "";

                    if (!differentMessageId)
                        value = lastid.ToString();
                    else
                        value = turnstileMessageId[turnstileIP].ToString();

                    string value1 = "1";
                    string value2 = "1.0";
                    XmlDocument xmlDocument = new XmlDocument();
                    var xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                    var documentElement = xmlDocument.DocumentElement;
                    xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                    var xmlElement = xmlDocument.CreateElement("cmf");
                    xmlDocument.AppendChild(xmlElement);
                    xmlElement.SetAttribute("id", value);
                    xmlElement.SetAttribute("addr", value1);
                    xmlElement.SetAttribute("protocol", value2);
                    var xmlElement1 = xmlDocument.CreateElement("panic");
                    xmlElement.AppendChild(xmlElement1);
                    var stateEl = xmlDocument.CreateElement("state");

                    stateEl.InnerText = state;

                    xmlElement1.AppendChild(stateEl);
                    if (state == "on")
                    {
                        XmlElement xmlElement3 = xmlDocument.CreateElement("txt");
                        //xmlElement3.InnerText = "Leave the area, please";
                        xmlElement3.InnerText = "Tornelli sbloccati";
                        xmlElement1.AppendChild(xmlElement3);
                    }

                    var stringWriter = new StringWriter();
                    var xmlTextWriter = new XmlTextWriter(stringWriter);
                    xmlDocument.WriteTo(xmlTextWriter);
                    string str1 = stringWriter.ToString();
                    var packet = Encoding.UTF8.GetBytes(str1);

                    //var num = this.udpClient.Send(packet, (int)packet.Length, "192.168.0.101", 1001);
                    if (!differentMessageId)
                        lastid++;
                    else
                        turnstileMessageId[turnstileIP] = turnstileMessageId[turnstileIP]++;

                    this.udpClient.BeginSend(packet, (int)packet.Length, turnstileIP, this.rport, new AsyncCallback(SendCallback), null);

                    xmlTextWriter.Close();
                    //this.udpClient.Close();
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                this.udpClient = new UdpClient(this.LocalPoint);
                SetPanicState(state);
            }



        }


        private void OpenTurnstile(string turnstileIP)
        {

            int versoEntrata = 0;

            if (ConfigurationManager.AppSettings["versoentrata"] != null)
                versoEntrata = Convert.ToInt32(ConfigurationManager.AppSettings["versoentrata"]);

            if (this.udpClient != null)
            {
                try
                {
                    string value = "";

                    if (!differentMessageId)
                        value = lastid.ToString();
                    else
                        value = turnstileMessageId[turnstileIP].ToString();

                    string value1 = "1";
                    string value2 = "1.0";
                    XmlDocument xmlDocument = new XmlDocument();
                    var xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                    var documentElement = xmlDocument.DocumentElement;
                    xmlDocument.InsertBefore(xmlDeclaration, documentElement);
                    var xmlElement = xmlDocument.CreateElement("cmf");
                    xmlDocument.AppendChild(xmlElement);
                    xmlElement.SetAttribute("id", value);
                    xmlElement.SetAttribute("addr", value1);
                    xmlElement.SetAttribute("protocol", value2);
                    var xmlElement1 = xmlDocument.CreateElement("open");
                    xmlElement.AppendChild(xmlElement1);
                    var actEl = xmlDocument.CreateElement("act");

                    if (versoEntrata != 0)
                    {
                        if (versoEntrata != 1)
                        {
                            actEl.InnerText = "x";

                        }
                        else
                        {
                            actEl.InnerText = "r";

                        }
                    }
                    else
                    {
                        actEl.InnerText = "l";

                    }

                    xmlElement1.AppendChild(actEl);

                    var cntEl = xmlDocument.CreateElement("cnt");
                    cntEl.InnerText = "1";
                    xmlElement1.AppendChild(cntEl);

                    var txtEl = xmlDocument.CreateElement("txt");
                    txtEl.InnerText = "Go in please";
                    xmlElement1.AppendChild(txtEl);


                    var stringWriter = new StringWriter();
                    var xmlTextWriter = new XmlTextWriter(stringWriter);
                    xmlDocument.WriteTo(xmlTextWriter);
                    string str1 = stringWriter.ToString();
                    var packet = Encoding.UTF8.GetBytes(str1);

                    //var num = this.udpClient.Send(packet, (int)packet.Length, "192.168.0.101", 1001);
                    if (!differentMessageId)
                        lastid++;
                    else
                        turnstileMessageId[turnstileIP] = turnstileMessageId[turnstileIP]++;

                    this.udpClient.BeginSend(packet, (int)packet.Length, turnstileIP, this.rport, new AsyncCallback(SendCallback), null);

                    xmlTextWriter.Close();
                    //this.udpClient.Close();
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                //this.udpClient = new UdpClient(this.LocalPoint);
                //OpenTurnstile(turnstileIP);
            }



        }


        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                //Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                //int bytesSent = client.EndSend(ar);
                int bytesSent = this.udpClient.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}

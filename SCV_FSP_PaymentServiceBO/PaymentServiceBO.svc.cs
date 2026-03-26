using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using Scv_Dal;
using Scv_Entities;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using Scv_Model;
using Scv_Model.Common;
using System.Collections.ObjectModel;
using Microsoft.Reporting.WinForms;
using System.Runtime.Serialization.Json;
using System.Net;
using System.ServiceModel.Web;

namespace SCV_FSP_PaymentServiceBO
{

    public class PaymentServiceBO : IPaymentServiceBO
    {
        string frontofficeserviceurl = "";
        WebClient webClient = null;
        Pagamento_Dal pagamento_Dal = null;
        public PaymentServiceBO()
        {
            frontofficeserviceurl = ConfigurationManager.AppSettings["frontofficeserviceurl"];
            webClient = new WebClient();

        }

        #region BACKOFFICE VS FRONTOFFICE

        public string createPaymentOrder(string name,
                                         string surname,
                                         string reservationNumber,
                                         string email,
                                         int amount,
                                         string visitDate,
                                         string expirationDate,
                                         string guestFullName,
                                         ReservationRequestDetailList[] reservationRequestDetailList)
        {
            string orderNumber = "";

            try
            {

                PaymentOrderData paymentOrderData = new PaymentOrderData
                                                    {
                                                        name = name,
                                                        surname = surname,
                                                        reservationNumber = reservationNumber,
                                                        email = email,
                                                        amount = amount,
                                                        visitDate = visitDate,
                                                        expirationDate = expirationDate,
                                                        guestFullName = guestFullName
                                                    };

                paymentOrderData.reservationRequestDetailList = reservationRequestDetailList;

                string dataToPost = JsonSerializer.Serialize<PaymentOrderData>(paymentOrderData);
                webClient.Headers["Content-type"] = "application/json";
                webClient.Encoding = Encoding.UTF8;

                var retvalue = webClient.UploadString(string.Format("{0}/createPaymentOrder.service", ConfigurationManager.AppSettings["frontofficeserviceurl"]), "POST", dataToPost);

                if (retvalue.IndexOf("KO") != -1)
                {
                    LogMessage(string.Format("createPaymentOrder() protocollo:{0} message frontOffice ws: {1}", reservationNumber, retvalue));
                    return retvalue;
                }

                if (ConfigurationManager.AppSettings["frontofficeserviceurl"].IndexOf("SCV_FSP_PaymentServiceFO") != -1)
                {
                    CreatePaymentOrderResultStruct obj = JsonSerializer.Deserialize<CreatePaymentOrderResultStruct>(retvalue);
                    orderNumber = obj.createPaymentOrderResult;
                }
                else
                    orderNumber = retvalue;
            }
            catch (Exception ex)
            {
                LogExc(ex, string.Format("createPaymentOrder() protocollo:{0}", reservationNumber));
                orderNumber = "KO Eccezione";
            }

            return orderNumber;
        }

        public string abortPaymentOrder(string orderNumber)
        {
            string result = "OK";
            try
            {
                WebClient webclient = new WebClient();
                string serviceURL = string.Format("{0}/abortPaymentOrder/{1}", ConfigurationManager.AppSettings["frontofficeserviceurl"], orderNumber);

                var res = webclient.DownloadString(serviceURL);
                
                if (!string.IsNullOrEmpty(res))
                {
                    if (res.IndexOf("KO") != -1)
                    {
                        LogMessage(string.Format("abortPaymentOrder() orderNumber:{0} message frontOffice ws: {1}", orderNumber, res));
                        result = res;
                    }
                }
            }
            catch (Exception ex)
            {
                LogExc(ex, string.Format("abortPaymentOrder() orderNumber:{0}", orderNumber));
                result = "KO - Eccezione.";
            }

            return result;
        }

        public PaymentOrder findPaymentOrder(string orderNumber)
        {
            PaymentOrder paymentOrder = new PaymentOrder();

            try
            {
                WebClient webclient = new WebClient();
                string serviceURL = string.Format("{0}/findPaymentOrder/{1}", ConfigurationManager.AppSettings["frontofficeserviceurl"], orderNumber);
                byte[] data = webclient.DownloadData(serviceURL);
                Stream stream = new MemoryStream(data);
                DataContractJsonSerializer obj = new DataContractJsonSerializer(typeof(PaymentOrder));
                paymentOrder = obj.ReadObject(stream) as PaymentOrder;

                bool noPaymentInfo = false;
                if (paymentOrder.status == "PAID")
                {
                    if (paymentOrder.paymentInfo == null)
                        noPaymentInfo = true;

                    if (noPaymentInfo)
                    {
                        LogMessage(string.Format("findPaymentOrder() L'ordine di pagamento con numero {0} presenta lo status 'PAID' ma per esso non sono state fornite 'paymentInfo'", orderNumber));
                        paymentOrder.status = "nopaymentinfo";
                    }
                }

            }
            catch (Exception ex)
            {
                LogExc(ex, string.Format("findPaymentOrder() orderNumber:{0}", orderNumber));
            }
     
            return paymentOrder;
        }

        #endregion

        #region FRONTOFFICE VS BACKOFFICE

        public void notifyPaymentConfirmation(string orderNumber, string idTransaction, string authorization, string amount, string paymentDate, string currency, string resultCode)
        {
            bool logErrorMess = false;

            try
            {
                pagamento_Dal = new Pagamento_Dal();
                string message = "";

                if (!pagamento_Dal.SetPaymentNotificationData(orderNumber,  
                                                              idTransaction,  
                                                              authorization,  
                                                              amount, 
                                                              Convert.ToDateTime(paymentDate),  
                                                              currency,  
                                                              resultCode,
                                                              out message))
                {
                    logErrorMess = true;
                    throw new Exception(message);
                }

            }
            catch (Exception ex)
            {
                if (logErrorMess)
                    LogMessage("notifyPaymentConfirmation() " + ex.Message);
                else
                    LogExc(ex, string.Format("notifyPaymentConfirmation() orderNumber:{0}", (!string.IsNullOrEmpty(orderNumber)) ? orderNumber : "null"));

                throw new WebFaultException<string>(ex.Message, HttpStatusCode.NotAcceptable);
            }


        }

        public void notifyOrderCancellation(string orderNumber)
        {
            bool logErrorMess = false;

            try
            {
                pagamento_Dal = new Pagamento_Dal();
                string message = "";
                if (!pagamento_Dal.NotifyOrderCancel(orderNumber, out message))
                {
                    logErrorMess = true;
                    throw new Exception(message);
                }
            }
            catch (Exception ex)
            {
                if (logErrorMess)
                    LogMessage("notifyPaymentCancellation() " + ex.Message);
                else
                    LogExc(ex, string.Format("notifyPaymentCancellation() orderNumber:{0}", (!string.IsNullOrEmpty(orderNumber)) ? orderNumber : "null"));

                throw new WebFaultException<string>(ex.Message, HttpStatusCode.NotAcceptable);
            }
        }

        #endregion

        #region LOGGING

        static object c = new object();
        private static void LogExc(Exception ex, string message = "")
        {
            string logfilepath = "";
            string logfilename = "";
            string logfilenamebackup = "";

            logfilepath = ConfigurationManager.AppSettings["logfilepath"];
            logfilename = ConfigurationManager.AppSettings["logfilename"];
            logfilenamebackup = ConfigurationManager.AppSettings["logfilenamebackup"];

            lock (c)
            {
                var logFile = string.Format("{0}{1}", logfilepath, logfilename);
                var oldLogFile = string.Format("{0}{1}", logfilepath, logfilenamebackup);

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
                        sw.WriteLine(CreateExceptionString(ex, message));
                    }
                }
                catch { }
            }
        }

        private static void LogMessage(string message = "")
        {
            string logfilepath = "";
            string logfilename = "";
            string logfilenamebackup = "";

            logfilepath = ConfigurationManager.AppSettings["logfilepath"];
            logfilename = ConfigurationManager.AppSettings["logfilename"];
            logfilenamebackup = ConfigurationManager.AppSettings["logfilenamebackup"];

            lock (c)
            {
                var logFile = string.Format("{0}{1}", logfilepath, logfilename);
                var oldLogFile = string.Format("{0}{1}", logfilepath, logfilenamebackup);

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
                        sw.WriteLine(CreateMessage(message));
                    }
                }
                catch { }
            }
        }

        public static string CreateExceptionString(Exception e, string otherMessage = "")
        {
            StringBuilder sb = new StringBuilder();
            CreateExceptionString(sb, e, string.Empty, otherMessage);

            return sb.ToString();
        }

        public static string CreateMessage(string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(System.Environment.NewLine);
            sb.Append(DateTime.Now.ToString());
            sb.Append(System.Environment.NewLine);
            sb.Append(message);
            sb.Append(System.Environment.NewLine);

            sb.AppendLine("-----------------------------------------");
            sb.Append(System.Environment.NewLine);

            return sb.ToString();
        }

        private static void CreateExceptionString(StringBuilder sb, Exception e, string indent, string otherMessage = "")
        {
            if (indent == null)
            {
                indent = string.Empty;
            }
            else if (indent.Length > 0)
            {
                sb.AppendFormat("{0}Inner ", indent);
            }
            sb.Append(System.Environment.NewLine);
            sb.Append(DateTime.Now.ToString());
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("Exception Found:{0}Type: {1}", indent, e.GetType().FullName));
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("{0}Message: {1}", indent, e.Message));
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("{0}Source: {1}", indent, e.Source));
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("{0}Stacktrace: {1}", indent, e.StackTrace));
            sb.Append(System.Environment.NewLine);
            if (otherMessage != "")
            {
                sb.Append(string.Format("{0}otherMessage: {1}", indent, otherMessage));
                sb.Append(System.Environment.NewLine);
            }

            if (e.InnerException != null)
            {

                CreateExceptionString(sb, e.InnerException, indent + "  ");
                sb.Append(System.Environment.NewLine);

            }

            sb.AppendLine("-----------------------------------------");
            sb.Append(System.Environment.NewLine);
        }
        #endregion

    }
}

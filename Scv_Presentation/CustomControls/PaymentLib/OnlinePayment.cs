using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using Telerik.Windows.Controls.GridView;
using Scv_Entities;
using Scv_Model;
using Telerik.Windows.Controls;
using Scv_Dal;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using Scv_Model.Common;
using SCV_FSP_PaymentServiceBO;

namespace Presentation.CustomControls.PaymentLib
{
    public class OnlinePayment
    {
        Parametri_Dal dalParameters = new Parametri_Dal();

        public bool SendOnLinePaymentRequest(Pagamento pagamento , int idUser, out string message, out OnLinePaymentLog onLinePaymentLog, out string _numeroOrdine)
        {
            message = "";

            _numeroOrdine = "";

            onLinePaymentLog = null;

            try
            {

                ObservableCollection<V_VisiteProgrammate> list = new ObservableCollection<V_VisiteProgrammate>();

                list = new ObservableCollection<V_VisiteProgrammate>(new VisitaProgrammata_Dal().GetVListByIdPrenotazione(pagamento.Id_Prenotazione));

                V_Prenotazione prenotazione = new Prenotazione_Dal().Get_V_Item(pagamento.Id_Prenotazione);

                PaymentOrderData paymentOrderData = new PaymentOrderData();

                if (string.IsNullOrEmpty(prenotazione.Email))
                {
                    message = "Non è stato fornito l'indirizzo email del richiedente, non è consentito procedere all'inoltro dell'ordine di pagamento online.";
                    return false;
                }

                string emailRecipient = "";

                if (dalParameters.GetItem("sendOLPaymentReqToDummy").Valore == "1")
                    emailRecipient = dalParameters.GetItem("olp_email_dummy").Valore;
                else
                    emailRecipient = prenotazione.Email;

                paymentOrderData.email = emailRecipient;

                decimal? importoDec = (pagamento.Importo_Interi + pagamento.Importo_Ridotti);

                if (importoDec > 0)
                {
                    //TUTTI GLI IMPORTI PER L'ORDINE DI PAGAMENTO ONLINE DEVONO ESSERE DEFINITI IN CENTESIMI DI EURO
                    paymentOrderData.amount = Convert.ToInt32(importoDec * 100);

                    int confirmDays = 0;

                    if (DateTime.Now.Date < list[0].Dt_Visita.Date)
                    {
                        var dateDiff = list[0].Dt_Visita.Date.Subtract(DateTime.Now.Date);

                        //1
                        if (dateDiff.Days < 7)
                            int.TryParse(dalParameters.GetItem("data_visita_minore_7").Valore, out confirmDays);
                        //5
                        if (dateDiff.Days >= 7 && dateDiff.Days <= 30)
                            int.TryParse(dalParameters.GetItem("data_visita_fino_30").Valore, out confirmDays);
                        //10
                        if (dateDiff.Days > 30)
                            int.TryParse(dalParameters.GetItem("data_visita_oltre_30").Valore, out confirmDays);

                        paymentOrderData.expirationDate = DateTime.Now.Date.AddDays(confirmDays).ToString("dd/MM/yyyy");
                    }

                    paymentOrderData.surname = Convert.ToString(prenotazione.Cognome);
                    if (!string.IsNullOrEmpty(prenotazione.Nome))
                        paymentOrderData.name = Convert.ToString(prenotazione.Nome);
                    else
                        paymentOrderData.name = "";
                    paymentOrderData.guestFullName = prenotazione.Responsabile;
                    paymentOrderData.reservationNumber = prenotazione.NProtocollo.ToString();
                    paymentOrderData.visitDate = list[0].Dt_Visita.Date.ToString("dd/MM/yyyy");

                    if (list.Count > 0)
                    {
                        paymentOrderData.reservationRequestDetailList = new ReservationRequestDetailList[list.Count];

                        for (int i = 0; i < list.Count; i++)
                        {
                            paymentOrderData.reservationRequestDetailList[i] = new ReservationRequestDetailList
                            {
                                visitHour = list[i].Ora_Visita,
                                language = list[i].LinguaVisita,
                                fullTicketNumber = list[i].Nr_Interi != null ? Convert.ToInt32(list[i].Nr_Interi) : 0,
                                reducedTicketNumber = list[i].Nr_Ridotti != null ? Convert.ToInt32(list[i].Nr_Ridotti) : 0
                            };
                        }
                    }

                    string dataToPost = JsonSerializer.Serialize<PaymentOrderData>(paymentOrderData);

                    WebClient webClient = new WebClient();
                    webClient.Headers["Content-type"] = "application/json";
                    webClient.Encoding = Encoding.UTF8;
                    string numeroOrdine = webClient.UploadString(string.Format("{0}/createPaymentOrder.service", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"]), "POST", dataToPost);

                    CreatePaymentOrderResultStruct createPaymentOrderResultStruct = JsonSerializer.Deserialize<CreatePaymentOrderResultStruct>(numeroOrdine);
                    numeroOrdine = createPaymentOrderResultStruct.createPaymentOrderResult;

                    if (!string.IsNullOrEmpty(numeroOrdine))
                    {
                        if (numeroOrdine.IndexOf("KO") != -1)
                        {
                            message = string.Format("La richiesta di inoltro dell'ordine di pagamento online non è andata a buon fine. Messaggio ws: {0}", numeroOrdine);
                            return false;
                        }
                        else
                        {
                            //SUCCESSO
                            _numeroOrdine = numeroOrdine;
                            //todo effettuare assegnazioni fuori
                            //pagamento.Dt_Pagamento = null;
                            //pagamento.NumeroOrdine = numeroOrdine;
                            //pagamento.StatusOrdine = "CREATED";

                            onLinePaymentLog = new OnLinePaymentLog
                            {
                                PrenotationNumber = (int)prenotazione.NProtocollo,
                                OrderNumber = numeroOrdine,
                                Success = true,
                                User_ID = idUser,
                                EntryType_Code = "RP",
                                EntryDate = DateTime.Now
                            };
                        }
                    }
                    else
                    {
                        message = "La richiesta di inoltro dell'ordine di pagamento non ha restituito il numero dell'ordine.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        public bool CancelOnLinePaymentRequest(Pagamento pagamento, int idUser, out string message, out OnLinePaymentLog onLinePaymentLog)
        {
            message = "";

            onLinePaymentLog = null;

            try
            {
                WebClient webclient = new WebClient();

                if (string.IsNullOrEmpty(pagamento.NumeroOrdine))
                {
                    message = "Non risulta nessun ordine di pagamento online";
                    return false;
                }

                string serviceURL = string.Format("{0}/abortPaymentOrder/{1}", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"], pagamento.NumeroOrdine);

                string result = webclient.DownloadString(serviceURL);

                if (result.IndexOf("KO") != -1)
                {
                    message = string.Format("La richiesta di annullamento dell'ordine di pagamento online non è andata a buon fine. Messaggio ws: {0}", result);
                    return false;
                }

                onLinePaymentLog = new OnLinePaymentLog
                {
                    PaymentID = pagamento.Id_Pagamento,
                    OrderNumber = pagamento.NumeroOrdine,
                    User_ID = idUser,
                    EntryType_Code = "RA",
                    EntryDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        public void TestPaymentConfirmation()
        {
            PaymentConfirmationData paymentConfirmationData = new PaymentConfirmationData
            {
                orderNumber = "FSP1386366665861",
                idTransaction = "trans001",
                authorization = "auth001",
                paymentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                currency = "978",
                resultCode = "00",
                amount = "2500"
            };

            string dataToPost = JsonSerializer.Serialize<PaymentConfirmationData>(paymentConfirmationData);

            WebClient webClient = new WebClient();
            webClient.Headers["Content-type"] = "application/json";
            webClient.Encoding = Encoding.UTF8;
            string numeroOrdine = webClient.UploadString(string.Format("{0}/notifyPaymentConfirmation", ConfigurationManager.AppSettings["onlinepaymentwrapperserviceurl"]), "POST", dataToPost);
        }
    }
}

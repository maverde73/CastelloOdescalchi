using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace SCV_FSP_PaymentServiceBO
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPaymentServiceBO" in both code and config file together.
    [ServiceContract]
    public interface IPaymentServiceBO
    {
        #region BACKOFFICE VS FRONTOFFICE

        [OperationContract]
        [WebInvoke(UriTemplate = "/createPaymentOrder.service",
                   BodyStyle = WebMessageBodyStyle.Wrapped,
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   Method = "POST")]
        string createPaymentOrder(string name,
                                  string surname,
                                  string reservationNumber,
                                  string email,
                                  int amount,
                                  string visitDate,
                                  string expirationDate,
                                  string guestFullName,
                                  ReservationRequestDetailList[] reservationRequestDetailList);



        [OperationContract]
        [WebGet(UriTemplate = "/abortPaymentOrder/{orderNumber}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string abortPaymentOrder(string orderNumber);


        [OperationContract]
        [WebGet(UriTemplate = "/findPaymentOrder/{orderNumber}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        PaymentOrder findPaymentOrder(string orderNumber);

        #endregion

        #region FRONTOFFICE VS BACKOFFICE

        [OperationContract]
        [WebInvoke(UriTemplate = "/notifyPaymentConfirmation",
           BodyStyle = WebMessageBodyStyle.Wrapped,
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           Method = "POST")]
        void notifyPaymentConfirmation(string orderNumber, string idTransaction, string authorization, string amount, string paymentDate, string currency, string resultCode);

        [OperationContract]
        [WebGet(UriTemplate = "/notifyOrderCancellation/{orderNumber}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        void notifyOrderCancellation(string orderNumber);

        #endregion
    }
}

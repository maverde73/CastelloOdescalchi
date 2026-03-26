using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SCV_FSP_PaymentServiceBO
{
    #region BACKOFFICE VS FRONTOFFICE

    #region createPaymentOrder
    [DataContract]
    public class CreatePaymentOrderResultStruct
    {
        [DataMember]
        public string createPaymentOrderResult { get; set; }
    }

    [DataContract]
    public class PaymentOrderData
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string surname { get; set; }
        [DataMember]
        public string reservationNumber { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public int amount { get; set; }
        [DataMember]
        public string visitDate { get; set; }
        [DataMember]
        public string expirationDate { get; set; }
        [DataMember]
        public ReservationRequestDetailList[] reservationRequestDetailList { get; set; }
        [DataMember]
        public string guestFullName { get; set; }
    }

    [DataContract]
    public class ReservationRequestDetailList
    {
        [DataMember]
        public string visitHour { get; set; }
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public int fullTicketNumber { get; set; }
        [DataMember]
        public int reducedTicketNumber { get; set; }
    }
    #endregion

    #region findPaymentOrder
    [DataContract]
    public class PaymentOrder
    {
        [DataMember]
        public string orderNumber { get; set; }
        [DataMember]
        public string rifOrderNumber { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string surname { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public int amount { get; set; }
        [DataMember]
        public string currency { get; set; }
        [DataMember]
        public string note { get; set; }
        [DataMember]
        public PaymentInfo[] paymentInfo { get; set; }
        [DataMember]
        public PaymentOrderDetail[] paymentOrderDetails { get; set; }
        [DataMember]
        public string formattedAmount { get; set; }
        [DataMember]
        public string creationDate { get; set; }
        [DataMember]
        public string visitDate { get; set; }
        [DataMember]
        public string expirationDate { get; set; }
    }

    [DataContract]
    public class PaymentInfo
    {
                [DataMember]
        public string orderNumber { get; set; }
                [DataMember]
        public string idTransaction { get; set; }
                [DataMember]
        public string authorization { get; set; }
                [DataMember]
        public int amount { get; set; }
                [DataMember]
        public string currency { get; set; }
                [DataMember]
        public string resultCode { get; set; }
                [DataMember]
        public string paymentDate { get; set; }
    }

    [DataContract]
    public class PaymentOrderDetail
    {
        [DataMember]
        public string visitHour { get; set; }
        [DataMember]
        public int fullTicketNumber { get; set; }
        [DataMember]
        public int reducedTicketNumber { get; set; }
        [DataMember]
        public string language { get; set; }
    }
    #endregion

    #endregion

    #region FRONTOFFICE VS BACKOFFICE
    [DataContract]
    public class PaymentConfirmationData
    {
        [DataMember]
        public string orderNumber { get; set; }
        [DataMember]
        public string idTransaction { get; set; }
        [DataMember]
        public string authorization { get; set; }
        [DataMember]
        public string amount { get; set; }
        [DataMember]
        public string paymentDate { get; set; }
        [DataMember]
        public string currency { get; set; }
        [DataMember]
        public string resultCode { get; set; }
    }

    #endregion
}
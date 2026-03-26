using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
    public class EmailPreviewItem : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion// Events




        #region Private Fields

        private string recipientsLabel = string.Empty;

        private string recipients = string.Empty;

        private string recipientsNames = string.Empty;

        private string attachments = string.Empty;

        private string subjectLabel = string.Empty;

        private string subject = string.Empty;

        private string body = string.Empty;

        private bool canSendOnLinePaymentOrder = false;

        //ONLINEPAYMENT
        private int prenotationID = 0;

        #endregion// Private Fields



        #region Public Properties

        public string RecipientsLabel
        {
            get { return recipientsLabel; }
            set { recipientsLabel = value; OnPropertiesChanged(this, "RecipientsLabel"); }
        }

        public string Recipients
        {
            get { return recipients; }
            set
            {
                recipients = value;
                OnPropertiesChanged(this, "Recipients");
            }
        }

        public string RecipientsNames
        {
            get { return recipientsNames; }
            set { recipientsNames = value; OnPropertiesChanged(this, "RecipientsNames"); }
        }

        public string Attachments
        {
            get { return attachments; }
            set { attachments = value; OnPropertiesChanged(this, "Attachments"); }
        }

        public string SubjectLabel
        {
            get { return subjectLabel; }
            set { subjectLabel = value; OnPropertiesChanged(this, "SubjectLabel"); }
        }

        public string Subject
        {
            get { return subject; }
            set { subject = value; OnPropertiesChanged(this, "Subject"); }


        }

        public string Body
        {
            get { return body; }
            set { body = value; OnPropertiesChanged(this, "Body"); }
        }

        public bool UserHasEmail
        {
            get { return !string.IsNullOrEmpty(recipients); }
        }

        //ONLINEPAYMENT
        public bool CanSendOnLinePaymentOrder
        {
            get { return canSendOnLinePaymentOrder; }
            set { canSendOnLinePaymentOrder = value; OnPropertiesChanged(this, "CanSendOnLinePaymentOrder"); }
        }

        //ONLINEPAYMENT
        public int PrenotationID
        {
            get { return prenotationID; }
            set { prenotationID = value; OnPropertiesChanged(this, "PrenotationID"); }
        }

        #endregion// Public Properties



        #region Constructors

        public EmailPreviewItem() { }

        public EmailPreviewItem(string recipientsLabel, string recipients, string recipientsNames, string subjectLabel, string subject, string attachments, string body)
        {
            this.RecipientsLabel = recipientsLabel;
            this.Recipients = recipients;
            this.RecipientsNames = recipientsNames;
            this.SubjectLabel = subjectLabel;
            this.Subject = subject;
            this.Attachments = attachments;
            this.Body = body;
        }

        #endregion// Constructors



        #region Events Handlers

        private void OnPropertiesChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion// Events Handlers
    }
}

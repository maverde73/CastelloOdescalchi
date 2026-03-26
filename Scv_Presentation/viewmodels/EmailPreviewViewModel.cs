using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Scv_Model;

namespace Presentation
{
    class EmailPreviewViewModel : INotifyPropertyChanged
    {
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion// Public Events




        #region Private Fields

        private EmailPreviewItem objEmail = null;

        #endregion// Private Fields




        #region Public Properties

        public EmailPreviewItem ObjEmail
        {
            get
            {
                if (objEmail == null)
                    objEmail = new EmailPreviewItem();
                return objEmail;
            }
            set { objEmail = value; OnPropertyChanged(this, "ObjEmail"); }
        }

        int prenotationID = 0;
        public int PrenotationID
        {
            get
            {

                return prenotationID;
            }
            set { prenotationID = value; OnPropertyChanged(this, "PrenotationID"); }
        }

        #endregion// Public Properties




        #region Conctructors

        public EmailPreviewViewModel(string recipientsLabel, string recipients, string recipientsNames, string subjectLabel, string subject, string attachments, string body)
        {
            ObjEmail = new EmailPreviewItem(recipientsLabel, recipients, recipientsNames, subjectLabel, subject, attachments, body);
        }

        public EmailPreviewViewModel(EmailPreviewItem item)
        {
            ObjEmail = item;
            PrenotationID = item.PrenotationID;
        }

        #endregion// Constructors




        #region Event Handling

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion// Event Handling


    }
}

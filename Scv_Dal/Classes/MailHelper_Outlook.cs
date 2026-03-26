using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Configuration;
 
namespace Scv_Entities
{

    public class MailHelper_Outlook
    {
        public Outlook.Application oApp = null;

        Outlook.Account account = null;


        public MailHelper_Outlook()
        {
            GetApplicationObject();
            //SetAccount();
        }


        public void CreateEmailItem(string subjectEmail, string toEmail, string bodyEmail, bool bodyHTML = true, bool display = false)
        {
            Outlook.MailItem oMsg = null;
            Outlook.Inspector oAddSig = null;

            oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
            //oAddSig = oMsg.GetInspector;

            oMsg.Subject = subjectEmail;
            oMsg.To = toEmail;
            if (bodyHTML)
                oMsg.HTMLBody = bodyEmail;
            else
                oMsg.Body = bodyEmail;
            
            oMsg.Importance = Outlook.OlImportance.olImportanceLow;
            //if(account != null)
            //    oMsg.SendUsingAccount = account;

            // Retrieve the account that has the specific SMTP address. 

            account = GetAccountForEmailAddress(oApp, Convert.ToString(ConfigurationManager.AppSettings["smtpaddress"]));
            // Use this account to send the e-mail. 
            oMsg.SendUsingAccount = account;

            oMsg.Display(true);
            
            //((Outlook._MailItem)oMsg).Send();
        }

        public static Outlook.Account GetAccountForEmailAddress(Outlook.Application application, string smtpAddress)
        {

            // Loop over the Accounts collection of the current Outlook session. 
            Outlook.Accounts accounts = application.Session.Accounts;
            foreach (Outlook.Account account in accounts)
            {
                // When the e-mail address matches, return the account. 
                if (account.SmtpAddress == smtpAddress)
                {
                    return account;
                }
            }
            throw new System.Exception(string.Format("No Account with SmtpAddress: {0} exists!", smtpAddress));
        } 

        Outlook.Application GetApplicationObject(bool checkIfOpened = true)
        {
            // Check whether there is an Outlook process running.
            bool displaySentMails = true;
            
            if (Process.GetProcessesByName("OUTLOOK").Count() > 0)
            {
                // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                oApp = Marshal.GetActiveObject("Outlook.Application") as Outlook.Application;

                var selectedFolder = oApp.ActiveExplorer().CurrentFolder;
                if (selectedFolder.Name == Convert.ToString(ConfigurationManager.AppSettings["sentemailfoldername"]))
                {
                    displaySentMails = false;
                }

            }
            else
            {
                // If not, create a new instance of Outlook and log on to the default profile.
                oApp = new Outlook.Application();
            }

            if (displaySentMails)
            {
                Outlook.NameSpace nameSpace = oApp.GetNamespace("MAPI");

                Outlook.MAPIFolder sentMails = nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderSentMail);

                

                sentMails.Display();
            }

            // Return the Outlook Application object.
            return oApp;
        }

        void SetAccount()
        {
            
            foreach (Outlook.Account _account in oApp.Session.Accounts)
            {
                if (_account.AccountType == Outlook.OlAccountType.olPop3)
                    account = _account;
                    
            }

           
        }


        public void KillProcess()
        {
            foreach (Process proc in Process.GetProcessesByName("OUTLOOK"))
            {
                proc.Kill();
            }
        }

        public void QuitApplication()
        {
            if (oApp != null)
            {
               //((Outlook._Application)oApp).Quit();
                oApp = null;
            }

        }
    }
}

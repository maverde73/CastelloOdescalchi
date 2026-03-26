using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GestioneVisite_MailService
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
            oAddSig = oMsg.GetInspector;

            oMsg.Subject = subjectEmail;
            oMsg.To = toEmail;
            if (bodyHTML)
                oMsg.HTMLBody = bodyEmail;
            else
                oMsg.Body = bodyEmail;

            //oMsg.Display(true);
            
            oMsg.Importance = Outlook.OlImportance.olImportanceLow;
            if(account != null)
                oMsg.SendUsingAccount = account;

            
            
            ((Outlook._MailItem)oMsg).Send();
        }

        Outlook.Application GetApplicationObject(bool checkIfOpened = true)
        {
            // Check whether there is an Outlook process running.
            /*
            if (Process.GetProcessesByName("OUTLOOK").Count() > 0)
            {
                // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                oApp = Marshal.GetActiveObject("Outlook.Application") as Outlook.Application;

            }
            else
            {
                // If not, create a new instance of Outlook and log on to the default profile.
                oApp = new Outlook.Application();
                //Outlook.NameSpace nameSpace = application.GetNamespace("MAPI");
                //nameSpace.Logon("museo@odescalchi.it", "castellano", false, true);
                //nameSpace = null;

            }
            */
            oApp = new Outlook.Application();

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

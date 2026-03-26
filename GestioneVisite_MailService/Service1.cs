using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Security;
using System.Collections.ObjectModel;
using System.Collections;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Scv_Entities;
using System.Data.Objects;
using Scv_Dal;

namespace GestioneVisite_MailService
{
    public partial class Service1 : ServiceBase
    {
        private static bool _shutdown = false;
        //private string _hostName;
        //private int _port;
        //private string _user, _password, _mailFrom;
        //private string _user_alt, _password_alt, _mailFrom_alt;

        SmtpInfo _smtpInfo = new SmtpInfo();

        SmtpInfo _smtpInfo_Altern = new SmtpInfo();

        private int _serviceTimeout, _mailCheckTimeout;

        //private int 
        public Service1()
        {
            InitializeComponent();
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;
        }


        #region Events
        protected override void OnStart(string[] args) { base.OnStart(args); Start(args); }
        public void Start(string[] args)
        {
            GetServiceSettings();
            QueueThread(() => SendPendingMails(), _mailCheckTimeout);
            QueueThread(() => SilentGetServiceSettings(), _serviceTimeout);
        }

        protected override void OnStop() { Stop(); base.OnStop(); }
        public new void Stop()
        {
            _shutdown = true;
            Thread.Sleep(30000);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        #endregion

        #region Methods
        private void GetServiceSettings()
        {
            try
            {
                using (IN_VIAEntities _context = new IN_VIAEntities())
                {

                    List<Parametri> settings = _context.Parametris.ToList();
                    _serviceTimeout = Convert.ToInt32(settings.SingleOrDefault(kx => kx.Chiave == "mailService_service_interval").Valore);
                    _mailCheckTimeout = Convert.ToInt32(settings.SingleOrDefault(kx => kx.Chiave == "mailService_mails_sendig_interval").Valore);

                    //host,enableSsl,port,defaultCredentials,userName,password,from

                    string[] mailService_smtp = settings.SingleOrDefault(kx => kx.Chiave == "mailService_smtp").Valore.Split(',');
                    _smtpInfo.Host = mailService_smtp[0];
                    _smtpInfo.EnableSsl = Convert.ToBoolean(mailService_smtp[1]);
                    _smtpInfo.Port = Convert.ToInt32(mailService_smtp[2]);
                    _smtpInfo.UseDefaultCredentials = Convert.ToBoolean(mailService_smtp[3]);
                    _smtpInfo.UserName = mailService_smtp[4];
                    _smtpInfo.Password = mailService_smtp[5];
                    _smtpInfo.From = mailService_smtp[6];

                    string[] mailService_smtp_altern = settings.SingleOrDefault(kx => kx.Chiave == "mailService_smtp_altern").Valore.Split(',');
                    _smtpInfo_Altern.Host = mailService_smtp_altern[0];
                    _smtpInfo_Altern.EnableSsl = Convert.ToBoolean(mailService_smtp_altern[1]);
                    _smtpInfo_Altern.Port = Convert.ToInt32(mailService_smtp_altern[2]);
                    _smtpInfo_Altern.UseDefaultCredentials = Convert.ToBoolean(mailService_smtp_altern[3]);
                    _smtpInfo_Altern.UserName = mailService_smtp_altern[4];
                    _smtpInfo_Altern.Password = mailService_smtp_altern[5];
                    _smtpInfo_Altern.From = mailService_smtp_altern[6];

                    //_user = settings.SingleOrDefault(kx => kx.Chiave == "mailService_username").Valore;
                    //_password = settings.SingleOrDefault(kx => kx.Chiave == "mailService_password").Valore;
                    //_mailFrom = settings.SingleOrDefault(kx => kx.Chiave == "mailService_default_from").Valore;

                    //_user_alt = settings.SingleOrDefault(kx => kx.Chiave == "mailService_altern_username").Valore;
                    //_password_alt = settings.SingleOrDefault(kx => kx.Chiave == "mailService_altern_password").Valore;
                    //_mailFrom_alt = settings.SingleOrDefault(kx => kx.Chiave == "mailService_altern_from").Valore;
                }

            }
            catch (Exception ex)
            {
                LogExc(ex);
            }

        }

        private void SilentGetServiceSettings()
        { try { GetServiceSettings(); } catch (Exception ex) { LogExc(ex); } }

        private void QueueThread(Action a, int timeout)
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                Thread.Sleep(5000);
                int count = 600 * timeout;
                while (!_shutdown)
                {
                    if (count == 600 * timeout)
                    {
                        count = 0;
                        a();
                    }
                    count++;
                    Thread.Sleep(100);
                }
            });
        }

        protected void SendPendingMails()
        {
            Parametri_Dal dalParameters = new Parametri_Dal();
            Parametri parEmailDummy = dalParameters.GetItem("email_dummy");
            Parametri parUSeEmailDummy = dalParameters.GetItem("sendToDummy");
            List<Attachment> emailAttachments = new List<Attachment>();
            SmtpInfo localSmtpInfo = null;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<PendingEmail> emailsToSend = _context.PendingEmails.ToList();
                string mailFrom, user, password;

                MailHelper_Outlook mailHelper_Outlook = null;

                for (int i = 0; i < emailsToSend.Count; i++)
                {
                    if (_shutdown)
                        break;
                    try
                    {
                        switch (emailsToSend[i].Sender)
                        {
                            case "petitionerFrom@email.it":
                                localSmtpInfo = _smtpInfo;
                                break;

                            case "guideFrom@email.it":
                                localSmtpInfo = _smtpInfo_Altern;
                                break;

                            default:
                                localSmtpInfo = _smtpInfo;
                                break;
                        }

                       
                        mailHelper_Outlook = new MailHelper_Outlook();

                        string recipient = emailsToSend[i].Recipient;

                        if (recipient == "petitionerFrom@email.it" || recipient == "guideFrom@email.it")
                        {
                            //SE IL RECIPIENT  E' UGUALE AL SENDER (FORWARD AL RECIPIENT)
                            //VIENE IMPOSTATO COME RECIPIENT LO STESSO INDIRIZZO DEL FROM
                            recipient = localSmtpInfo.From;
                        }

                        int attNumber = 0;
                        int emailID = emailsToSend[i].Id;
                        Stream stream = null;
                        emailAttachments.Clear();
                        foreach (PendingEmailsAttachment attachment in _context.PendingEmailsAttachments.Where(x => x.Id_PendingEmails == emailID).ToList())
                        {
                            stream = new MemoryStream(attachment.Attachment);
                            Attachment a = new Attachment(stream, "att" + (++attNumber).ToString() + ".jpg");
                            emailAttachments.Add(a);
                        }

                        /*
                        MailHelper.SendMailMessage(localSmtpInfo,
                                                   (parUSeEmailDummy.Valore == "1" ? parEmailDummy.Valore.ToString() : recipient),
                                                   emailsToSend[i].Bcc,
                                                   emailsToSend[i].Cc,
                                                   emailsToSend[i].Subject,
                                                   emailsToSend[i].Body,
                                                   emailAttachments);



                        _context.PendingEmails.DeleteObject(emailsToSend[i]);
                        _context.SaveChanges();
                         */

                        mailHelper_Outlook.CreateEmailItem(emailsToSend[i].Subject, parUSeEmailDummy.Valore == "1" ? parEmailDummy.Valore.ToString() : recipient, emailsToSend[i].Body);

                        _context.PendingEmails.DeleteObject(emailsToSend[i]);
                        _context.SaveChanges();
  
                    }
                    catch (Exception ex)
                    {
                        if (mailHelper_Outlook != null)
                        {
                            mailHelper_Outlook.KillProcess();
                        }

                        string message = string.Format("PendingEmail.Id : {0}, PendingEmail.Recipient : {1}, PendingEmail.DT_INS : {2} ", emailsToSend[i].Id.ToString(), (parUSeEmailDummy.Valore == "1" ? parEmailDummy.Valore.ToString() : emailsToSend[i].Recipient), emailsToSend[i].DT_INS.ToString());
                        LogExc(ex, message);
                    }

                    Thread.Sleep(100);
                }

            }



        }
        #endregion

        #region Error Logging

        static object c = new object();
        private static void LogExc(Exception ex, string message = "")
        {
            lock (c)
            {
                var logFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Scv_MailServiceLog.txt";
                var oldLogFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Scv_MailServiceLog_old.txt";
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
                        sw.WriteLine(string.Format("{0}-{1}{2}{3}{4}{5}{6}", DateTime.Now.ToString(), ex.ToString(), System.Environment.NewLine, ex.Message, System.Environment.NewLine, message, System.Environment.NewLine));
                    }
                }
                catch { }
            }
        }
        #endregion

    }
}

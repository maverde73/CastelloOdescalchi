using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Web.Mail;
using System.Net;
using System.IO;

namespace Scv_MailService
{
    public class SmtpInfo
    {
        public string Host { get; set; }
        public bool EnableSsl { get; set; }
        public int Port { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
    }

    public class MailHelper
    {


        public static void SendMailMessage(string senderUserName,
                                           string senderPassword,
                                           string from,
                                           string to,
                                           string bcc,
                                           string cc,
                                           string subject,
                                           string body,
										   List<Attachment> attachments,
                                           bool isBodyHtml = true)
										   
        {

            System.Net.Mail.MailMessage mMailMessage = new System.Net.Mail.MailMessage();

            mMailMessage.From = new MailAddress(from);

            mMailMessage.To.Add(new MailAddress(to));

            if ((bcc != null) && (bcc != string.Empty))
            {
                mMailMessage.Bcc.Add(new MailAddress(bcc));
            }

            if ((cc != null) && (cc != string.Empty))
            {
                mMailMessage.CC.Add(new MailAddress(cc));
            }

            mMailMessage.Subject = subject;

            mMailMessage.Body = body;

            mMailMessage.IsBodyHtml = isBodyHtml;

            mMailMessage.Priority = System.Net.Mail.MailPriority.Normal;

			foreach (Attachment a in attachments)
				mMailMessage.Attachments.Add(a);

            //impostazioni di mSmtpClient in App.config
            SmtpClient mSmtpClient = new SmtpClient();

            mSmtpClient.Credentials = new NetworkCredential(senderUserName, senderPassword);

            mSmtpClient.Send(mMailMessage);
        }

        public static void SendMailMessage(SmtpInfo objSmtpInfo,
                                           string to,
                                           string bcc,
                                           string cc,
                                           string subject,
                                           string body,
                                           List<Attachment> attachments,
                                           bool isBodyHtml = true)
                                   
        {

            System.Net.Mail.MailMessage mMailMessage = new System.Net.Mail.MailMessage();

            mMailMessage.From = new MailAddress(objSmtpInfo.From);

            mMailMessage.To.Add(new MailAddress(to));

            if ((bcc != null) && (bcc != string.Empty))
            {
                mMailMessage.Bcc.Add(new MailAddress(bcc));
            }

            if ((cc != null) && (cc != string.Empty))
            {
                mMailMessage.CC.Add(new MailAddress(cc));
            }

            mMailMessage.Subject = subject;

            mMailMessage.Body = body;

            mMailMessage.IsBodyHtml = isBodyHtml;

            mMailMessage.Priority = System.Net.Mail.MailPriority.Normal;

            foreach (Attachment a in attachments)
                mMailMessage.Attachments.Add(a);

            //impostazioni di mSmtpClient in App.config
            SmtpClient mSmtpClient = new SmtpClient(objSmtpInfo.Host,objSmtpInfo.Port);
            mSmtpClient.EnableSsl = objSmtpInfo.EnableSsl;
            mSmtpClient.UseDefaultCredentials = objSmtpInfo.UseDefaultCredentials;

            mSmtpClient.Credentials = new NetworkCredential(objSmtpInfo.UserName, objSmtpInfo.Password);

            mSmtpClient.Send(mMailMessage);
        }
    }
}

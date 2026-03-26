using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Scv_Dal;
using System.IO;
using Scv_Model;

namespace Scv_Entities
{
	public class Email_Dal
	{

        public int InsertOrUpdate(MailMessage obj, bool addAttachments, bool forwardToSender, DateSaveTarget saveDate, int? prenotationID, List<V_EvidenzeGiornaliere> ScheduledVisits = null, string guideName = "")
		{
			int id = 0;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						string exeFolder = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
						string imagesPath = string.Empty;

						PendingEmail o = new PendingEmail();

						o.Sender = obj.From.Address;
						o.Recipient = obj.To[0].Address;
						o.Subject = obj.Subject;
						o.Body = obj.Body;
						o.DT_INS = DateTime.Now;

						_context.PendingEmails.AddObject(o);

						_context.SaveChanges();

						//allegati
						if (addAttachments)
						{
							//for (int n = 0; n < 2; n++ )
							//{
							//    PendingEmailsAttachment a = new PendingEmailsAttachment();
							//    a.Id_PendingEmails = o.Id;
							//    a.Attachment = Helper_Dal.StoreImageData(Path.Combine(exeFolder, @"email_attachments\att_0" + (n+1).ToString() + ".jpg"));
							//    _context.PendingEmailsAttachments.AddObject(a);
							//    _context.SaveChanges();
							//}
						}

						if (forwardToSender)
						{
							o = new PendingEmail();

							string forwardRuleCode = new Parametri_Dal().GetItem("ForwardRuleCode").Valore.ToString();

							o.Sender = obj.From.Address;
							o.Recipient = o.Sender;
                            if(guideName == "")
                               o.Subject = forwardRuleCode + ": " + obj.Subject;
                            else
                               o.Subject = forwardRuleCode + ": " + " Guida " + guideName + " - notifica di invio email - oggetto email: " +  obj.Subject;
     					    o.Body = obj.Body;
							o.DT_INS = DateTime.Now;
							_context.PendingEmails.AddObject(o);
							_context.SaveChanges();
						}

						switch (saveDate)
						{
							case DateSaveTarget.Guide:
								if (ScheduledVisits != null) 
								{
									List<int> scheduledVisitsIDs = new List<int>();

									foreach (V_EvidenzeGiornaliere e in 
										_context.V_EvidenzeGiornaliere
											.OrderBy(x => x.Dt_Visita)
											.ThenBy(x => x.Ora_Visita)
											.ThenBy(x => x.Id_Lingua))
									{
										foreach (V_EvidenzeGiornaliere eg in ScheduledVisits.Where(x => x.Fl_AvvisaGuida == true))
											if (eg.Dt_Visita == e.Dt_Visita && eg.Ora_Visita == e.Ora_Visita && eg.Id_Lingua == e.Id_Lingua)
												scheduledVisitsIDs.Add(e.Id_VisitaProgrammata);

									}

									foreach (VisitaProgrammata v in _context.VisiteProgrammate.Where(x => scheduledVisitsIDs.Contains(x.Id_VisitaProgrammata)))
									{
										v.Dt_InvioAvviso = DateTime.Now.Date;
										_context.AttachUpdated(v);
										_context.SaveChanges();
									}
								}
								break;

							case DateSaveTarget.Visitor:
								if (prenotationID != null)
								{
									Prenotazione p = _context.Prenotazioni.FirstOrDefault(x => x.Id_Prenotazione == (int)prenotationID);
									if (p != null)
									{
										p.Dt_InvioMailRichiedente = DateTime.Now.Date;
										_context.AttachUpdated(p);
										_context.SaveChanges();
									}
								}
								break;
						}


                        //System.Diagnostics.Process.Start("mailto:me@example.com");

                        Parametri parEmailDummy = _context.Parametris.FirstOrDefault(px => px.Chiave == "email_dummy");
                        Parametri parUSeEmailDummy = _context.Parametris.FirstOrDefault(px => px.Chiave == "sendToDummy");
                        
                        MailHelper_Outlook mailHelper_Outlook = new MailHelper_Outlook();

                        o.Sender = obj.From.Address;
                        o.Recipient = obj.To[0].Address;
                        o.Subject = obj.Subject;
                        o.Body = obj.Body;

                        mailHelper_Outlook.CreateEmailItem(o.Subject, parUSeEmailDummy.Valore == "1" ? parEmailDummy.Valore.ToString() : o.Recipient, o.Body);

						//transaction.Commit();
                        //ripristinare Commit per ripristino inserimento in pendingemails
                        transaction.Rollback();

						id = o.Id;
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw e;
					}
					finally
					{
						_context.Connection.Close();
					}
				}
			}

			return id;
		}
	}
}

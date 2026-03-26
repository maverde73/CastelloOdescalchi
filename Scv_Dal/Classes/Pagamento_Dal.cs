using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using System.Collections.ObjectModel;
using Scv_Model.Classes;
using Scv_Model;
using System.Configuration;
using System.IO;


namespace Scv_Dal
{
    public class Pagamento_Dal
    {
        public Pagamento_Dal()
        {
            //_context = new SCV_DEVEntities();
            //_context.ContextOptions.LazyLoadingEnabled = false;
        }


		public List<Pagamento> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<Pagamento> p = _context.Pagamenti.Where(rx => rx.Id_Pagamento == id && rx.Fl_Annullato == false).ToList();
				return p;
			}
		}

        public Pagamento GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				Pagamento p = _context.Pagamenti.FirstOrDefault(rx => rx.Id_Pagamento == id && rx.Fl_Annullato == false);
				return p;
            }
        }

		public List<LK_TipoPagamento> GetList()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiPagamento.ToList();
			}
		}

        public Pagamento GetItemByIdPrenotazione(int idPrenotazione)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				Pagamento p = _context.Pagamenti.FirstOrDefault(rx => rx.Id_Prenotazione == idPrenotazione && rx.Fl_Annullato == false && rx.FL_PagamentoParziale == false);
				return p;
            }
        }

		public Pagamento GetItemByIdVisitaProgrammata(int idVisitaProgrammata)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
                return _context.Pagamenti.FirstOrDefault(rx => rx.Id_VisitaProgrammata == idVisitaProgrammata && rx.Fl_Annullato == false && rx.FL_PagamentoParziale == true);
			}
		}

        public List<Pagamento> GetListByIdPrenotazione(int idPrenotazione)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.Pagamenti.Where(rx => rx.Id_Prenotazione == idPrenotazione && rx.Fl_Annullato == false).ToList();
            }
        }

		public List<V_PagamentoAnnullato> GetV_PagamentoAnnullato(DateTime? startPaymentDate, DateTime? endPaymentDate)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_PagamentoAnnullato> ItemsList = null;
				try
				{
					ItemsList = _context.V_PagamentoAnnullato
						.Where(x =>
							startPaymentDate != null ? x.Dt_Pagamento >= startPaymentDate : x.Dt_Pagamento >= DateTime.MinValue
							&&
							endPaymentDate != null ? x.Dt_Pagamento <= endPaymentDate : x.Dt_Pagamento <= DateTime.MaxValue
							).ToList();
				}
				catch (Exception e)
				{
					throw (e);
				}
				return ItemsList.OrderBy(x => x.Dt_Pagamento).ToList();
			}
		}

		public int GetDeliveredTickets(int prenotationID, int? visitID)
		{
			int deliveredTickets = 0;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_VisiteProgrammate> visits = new List<V_VisiteProgrammate>();

				if(visitID == null)
					visits = _context.V_VisiteProgrammate.Where(x => x.Id_Prenotazione == prenotationID).ToList();
				else
					visits = _context.V_VisiteProgrammate.Where(x => x.Id_Prenotazione == prenotationID && x.Id_VisitaProgrammata == (int)visitID).ToList();

				foreach (V_VisiteProgrammate vp in visits)
				{
					deliveredTickets += vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0;
					deliveredTickets += vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0;
                    deliveredTickets += vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0;
                    deliveredTickets += vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0;
				}
			}

			return deliveredTickets;
		}

        public void InsertOrUpdate(List<Pagamento> payments, OnLinePaymentLog onLinePaymentLog = null, bool setRicevuta = false)
        {
            //ONLINEPAYMENT
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        foreach (Pagamento obj in payments)
                        {
                            obj.Dt_Update = DateTime.Now.Date;

                            if (obj.Id_Pagamento != 0)
                            {
                                if (setRicevuta)
                                    obj.Dt_Pagamento = DateTime.Now.Date;
                            }
                            else
                            {
                                if (obj.Dt_Pagamento != null)
                                    setRicevuta = true;
                            }



                            if (setRicevuta)
                            {
                                LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
                                LK_Progressivi pr = dalPR.GetSingleItem("R")[0];

                                if (pr != null)
                                {
                                    if (pr.Anno != obj.Dt_Pagamento.Value.Year)
                                    {
                                        pr.Anno = pr.Anno > 0 ? obj.Dt_Pagamento.Value.Year : 0;
                                        pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
                                    }
                                    else
                                        pr.Progr_UltimoUscito++;

                                    if (pr.Tipo != string.Empty)
                                        _context.AttachUpdated(pr);
                                    else
                                        _context.LK_Progressivi.AddObject(pr);
                                }

                                obj.Ricevuta = pr.Anno == 0 ? pr.Progr_UltimoUscito.ToString() : (pr.Anno.ToString() + "/") + pr.Progr_UltimoUscito.ToString();
                            }

                            if (obj.Id_Pagamento != 0)
                                _context.AttachUpdated(obj);
                            else
                                _context.Pagamenti.AddObject(obj);

                            _context.SaveChanges();

                            if (onLinePaymentLog != null)
                            {
                                onLinePaymentLog.PaymentID = obj.Id_Pagamento;
                                _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);
                            }

                        }

                        _context.SaveChanges();
                        transaction.Commit();
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
        }

		public void DeleteObject(Pagamento obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{

						//Cancello il pagamento
						Pagamento p = _context.Pagamenti.FirstOrDefault(x => x.Id_Pagamento == obj.Id_Pagamento);
						_context.Pagamenti.Attach(p);
						_context.Pagamenti.DeleteObject(p);

						_context.SaveChanges();

						transaction.Commit();
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
		}

        public void CancelPayment(Pagamento obj, OnLinePaymentLog onLinePaymentLog = null, bool deletePaymentOrder = false)
        {
            //ONLINEPAYMENT
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        if (onLinePaymentLog != null && obj.Id_TipoPagamento == 1 && obj.Dt_Pagamento == null)
                        {
                            //SI TRATTA DI UN ORDINE DI PAGAMENTO ONLINE.
                            //IN CASO DI ANNULLAMENTO, L'OPERAZIONE VIENE LOGGATA
                            //ED EVENTUALMENTE IL RECORD VIENE ELIMINATO.
                            var prenotationExtended = _context.V_Prenotazione.FirstOrDefault(px => px.Id_Prenotazione == obj.Id_Prenotazione);
                            onLinePaymentLog.PrenotationNumber = Convert.ToInt32(prenotationExtended.NProtocollo);
                            _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);
                            obj.StatusOrdine = "ABORTED";
                            obj.NoteOrdine = "ABORTED_BY_BACK_OFFICE";

                            if (deletePaymentOrder)
                            {
                                //_context.AttachUpdated(obj);
                                var paymentToDelete = _context.Pagamenti.FirstOrDefault(px => px.Id_Pagamento == obj.Id_Pagamento);
                                if(paymentToDelete != null)
                                    _context.Pagamenti.DeleteObject(paymentToDelete);
                            }
                        }
                        else if (obj.Id_TipoPagamento != 1)
                        {
                            obj.Fl_Annullato = true;
                            _context.AttachUpdated(obj);
                        }

                        _context.SaveChanges();
                        transaction.Commit();
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
        }

		public List<PaymentDataItem> GetPaymentData(DateTime searchDateStart, DateTime searchDateEnd, DateTime scanDateStart)
		{
			List<PaymentDataItem> data = new List<PaymentDataItem>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				List<V_Prenotazione> allPrenotations = null;
				List<V_Prenotazione> prenotations = new List<V_Prenotazione>();
				List<V_VisiteProgrammate> visits = null;
				PaymentDataItem pdi = null;
				PaymentDataDetail detail = null;

				allPrenotations = _context.V_Prenotazione.Where(x => 
					x.Dt_VisiteDA >= searchDateStart 
					&& 
					x.Dt_VisiteDA <= searchDateEnd
					//&&
					//_context.V_Pagamento.Where(y => y.Id_Prenotazione == x.Id_Prenotazione) == null
					).ToList();

				foreach (V_Prenotazione p in allPrenotations)
				{
					if (_context.V_Pagamento.Where(x => x.Id_Prenotazione == p.Id_Prenotazione) == null || _context.V_Pagamento.Where(x => x.Id_Prenotazione == p.Id_Prenotazione).Count() == 0)
						prenotations.Add(p);
				}

				foreach (V_Prenotazione vpr in prenotations)
				{
					visits = _context.V_VisiteProgrammate
						.Where(x => x.Id_Prenotazione == vpr.Id_Prenotazione)
						.Where(x => x.Dt_Visita >= scanDateStart)
						.ToList();

					if (visits != null)
					{
						pdi = new PaymentDataItem();
						pdi.Protocollo = vpr.Protocollo;
						pdi.Richiedente = vpr.Richiedente_Nominativo;
						pdi.Email = vpr.Email;
						pdi.Responsabile = vpr.Responsabile;
						pdi.Visitatori = vpr.Tot_Visitatori.ToString();
						pdi.DataRisposta = vpr.Dt_InvioMailRichiedente != null ? vpr.Dt_InvioMailRichiedente.Value.Date.ToShortDateString() : string.Empty;

						foreach(V_VisiteProgrammate v in visits)
						{
							detail = new PaymentDataDetail();
							detail.Data = v.Dt_Visita.ToShortDateString();
							detail.Ora = v.Ora_Visita;
							detail.BigliettiInteri = v.Nr_Interi != null ? ((int)v.Nr_Interi).ToString() : string.Empty;
							detail.BigliettiRidotti = v.Nr_Ridotti != null ? ((int)v.Nr_Ridotti).ToString() : string.Empty;
                            detail.BigliettiScontati = v.Nr_Scontati != null ? ((int)v.Nr_Scontati).ToString() : string.Empty;
                            detail.BigliettiCumulativi = v.Nr_Cumulativi != null ? ((int)v.Nr_Cumulativi).ToString() : string.Empty;
							detail.Lingua = v.LinguaVisita;
							pdi.Details.Add(detail);
						}
					}

					data.Add(pdi);
				}

			}
			return data;
		}

		public List<string> GetCSVPaymentData(List<PaymentDataItem> items, string fieldSep, int maxVisitPerRecord)
		{
			List<string> csv = new List<string>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				string line = string.Empty;

				foreach (PaymentDataItem i in items)
				{
					line =
						i.Protocollo
						+ fieldSep
						+ i.Richiedente
						+ fieldSep
						+ i.Email
						+ fieldSep
						+ i.Responsabile
						+ fieldSep
						+ i.Visitatori
						+ fieldSep
						+ i.DataRisposta
						;

					int visitsInRecord = 1;

					foreach (PaymentDataDetail d in i.Details)
					{
						if (visitsInRecord > maxVisitPerRecord)
						{
							visitsInRecord = 1;
							csv.Add(line);
							line =
								i.Protocollo
								+ fieldSep
								+ i.Richiedente
								+ fieldSep
								+ i.Email
								+ fieldSep
								+ i.Responsabile
								+ fieldSep
								+ i.Visitatori
								+ fieldSep
								+ i.DataRisposta
								;
						}

						line +=
							fieldSep
							+ d.Data
							+ fieldSep
							+ d.Ora
							+ fieldSep
							+ d.BigliettiInteri
							+ fieldSep
							+ d.BigliettiRidotti
							+ fieldSep
							+ d.Lingua
							;
						visitsInRecord++;
					}
					csv.Add(line);						
				}
			}

			return csv;

		}

        //ONLINEPAYMENT
        public bool SetPaymentNotificationData(string orderNumber, string idTransaction, string authorization, string amount, DateTime paymentDate, string currency, string resultCode, out string message, bool sendEmail = true)
        {
            bool retval = false;
            message = "";

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        var onLinePaymentServiceUsr = _context.LK_Users.FirstOrDefault(ux => ux.Identificativo == "OLPSRV");

                        if (onLinePaymentServiceUsr == null)
                        {
                            message = "Non è stato aggiunto l'utente 'OLPSRV'";
                            return false;
                        }

                        Pagamento payment = _context.Pagamenti.FirstOrDefault(px => px.NumeroOrdine == orderNumber && px.Fl_Annullato == false);

                        if (payment == null)
                        {
                            message = string.Format("Non è stato trovato un ordine di pagamento con numeroOrdine={0}.", orderNumber);
                            return false;
                        }

                        var prenotation = _context.Prenotazioni.FirstOrDefault(px => px.Id_Prenotazione == payment.Id_Prenotazione);
                        var prenotationExtended = _context.V_Prenotazione.FirstOrDefault(px => px.Id_Prenotazione == payment.Id_Prenotazione);

                        OnLinePaymentLog onLinePaymentLog = new OnLinePaymentLog();
                        onLinePaymentLog.PaymentID = payment.Id_Pagamento;
                        onLinePaymentLog.PrenotationNumber = Convert.ToInt32(prenotationExtended.NProtocollo);
                        onLinePaymentLog.OrderNumber = orderNumber;
                        onLinePaymentLog.EntryType_Code = "CP";
                        onLinePaymentLog.EntryDate = DateTime.Now;
                        onLinePaymentLog.User_ID = onLinePaymentServiceUsr.Id_User;

                        if (payment.Dt_Pagamento != null)
                        {
                            message = string.Concat(message,
                                                    System.Environment.NewLine,
                                                    string.Format("L'ordine di pagamento con numeroOrdine={0} risulta già effettuato in data {1}.", orderNumber, Convert.ToDateTime(payment.Dt_Pagamento).ToString(@"dd/MM/yyyy HH:mm")));
                        }

                        if (payment.Id_TipoPagamento != 1)
                        {
                            message = string.Concat(message,
                                                    System.Environment.NewLine,
                                                    string.Format("ordine di pagamento con numeroOrdine={0} non è di tipo 'OnLine'.", orderNumber));
                        }

                        var importoCentesimi = Convert.ToInt32(payment.Importo * 100);
                        if (importoCentesimi > Convert.ToInt32(amount))
                        {
                            message = string.Concat(message,
                                                    System.Environment.NewLine,
                                                    string.Format("La notifica di pagamento per l'ordine di pagamento con numeroOrdine={0} presenta un importo inferiore rispetto all'importo previsto.", orderNumber));

                        }


                        if (message != "")
                        {
                            onLinePaymentLog.Success = false;
                            onLinePaymentLog.Notes = message.Replace(System.Environment.NewLine, message);
                            _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);
                            _context.SaveChanges();
                            transaction.Commit();
                            return false;
                        }


                        payment.Dt_Pagamento = paymentDate;
                        payment.CodiceEsito = resultCode;
                        payment.StatusOrdine = "PAID";
                        payment.Dt_Update = DateTime.Now.Date;
                        payment.AutorizzazioneOrdine = authorization;
                        payment.IdTransazione = idTransaction;

                        if (payment.Dt_Pagamento != null)
                        {
                            LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
                            LK_Progressivi pr = dalPR.GetSingleItem("R")[0];

                            if (pr != null)
                            {
                                if (pr.Anno != payment.Dt_Pagamento.Value.Year)
                                {
                                    pr.Anno = pr.Anno > 0 ? payment.Dt_Pagamento.Value.Year : 0;
                                    pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
                                }
                                else
                                    pr.Progr_UltimoUscito++;

                                if (pr.Tipo != string.Empty)
                                    _context.AttachUpdated(pr);
                                else
                                    _context.LK_Progressivi.AddObject(pr);
                            }

                            payment.Ricevuta = pr.Anno == 0 ? pr.Progr_UltimoUscito.ToString() : (pr.Anno.ToString() + "/") + pr.Progr_UltimoUscito.ToString();
                        }

                        prenotation.Id_TipoRisposta = 3;  //CAMBIAMENTO STATO A 'Visita confermata (con ricevuta)'

                        onLinePaymentLog.Success = true;
                        _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);

                        _context.SaveChanges();

                        if (sendEmail)
                        {
                            //INVIO EMAIL 
                            Mailer mailer = new Mailer();
                            MailCreationResult result = new MailCreationResult();

                            Richiedente petitioner = _context.Richiedenti.FirstOrDefault(rx => rx.Id_Richiedente == prenotation.Id_Richiedente);
                            bool forwardToSenderVisitor = Convert.ToBoolean(_context.Parametris.FirstOrDefault(px => px.Chiave == "forwardToSenderVisitor").Valore);
                            var idvisitePrenotate = _context.VisitePrenotate.Where(vpx => vpx.Id_Prenotazione == prenotation.Id_Prenotazione).Select(vpx => vpx.Id_VisitaPrenotata).ToList();
                            var visiteProgrammate = _context.VisiteProgrammate.Where(vpx => idvisitePrenotate.Contains(vpx.Id_VisitaPrenotata)).ToList();

                            string subject = "";
                            string body = "";

                            result = mailer.CreatePetitionerVisitNoticeSubject(prenotation, (visiteProgrammate != null && visiteProgrammate.Count > 0) ? visiteProgrammate[0] : null, _context);

                            if (result.success)
                                subject = result.Text;

                            result = mailer.CreatePetitionerVisitNoticeBody(prenotation, visiteProgrammate, true, _context);

                            if (result.success)
                                body = result.Text;

                            PendingEmail o = new PendingEmail();

                            o.Sender = "petitionerFrom@email.it";
                            o.Recipient = petitioner.Email;
                            o.Subject = subject;
                            o.Body = body;
                            o.DT_INS = DateTime.Now;

                            _context.PendingEmails.AddObject(o);
                            _context.SaveChanges();

                            if (forwardToSenderVisitor)
                            {
                                o = new PendingEmail();
                                string ForwardRuleCode = _context.Parametris.FirstOrDefault(px => px.Chiave == "ForwardRuleCode").Valore;

                                o.Sender = "petitionerFrom@email.it";
                                o.Recipient = o.Sender;
                                o.Subject = ForwardRuleCode + ": " + subject;
                                o.Body = body;
                                o.DT_INS = DateTime.Now;
                                _context.PendingEmails.AddObject(o);
                                _context.SaveChanges();
                            }

                            prenotation.Dt_InvioMailRichiedente = DateTime.Now.Date;
                            _context.SaveChanges();
                        }

                        transaction.Commit();
                        message = "OK";
                        retval = true;

                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        if (message == "")
                            message = e.StackTrace;
                        else
                            message = string.Concat(message, System.Environment.NewLine, e.StackTrace);

                        retval = false;
                    }
                    finally
                    {
                        _context.Connection.Close();
                    }

                    return retval;
                }
            }
        }

        public bool SetPaymentData(string status,
                                   string note,
                                   string orderNumber,
                                   string idTransaction,
                                   string authorization,
                                   int amount,
                                   DateTime paymentDate,
                                   string currency,
                                   string resultCode,
                                   out string message)
        {
            bool retval = false;
            message = "";

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        var onLinePaymentServiceUsr = _context.LK_Users.FirstOrDefault(ux => ux.Identificativo == "OLPSRV");

                        if (onLinePaymentServiceUsr == null)
                        {
                            message = "Non è stato aggiunto l'utente 'OLPSRV'";
                            return false;
                        }

                        Pagamento payment = _context.Pagamenti.FirstOrDefault(px => px.NumeroOrdine == orderNumber && px.Fl_Annullato == false);

                        if (payment == null)
                        {
                            message = string.Format("Non è stato trovato un ordine di pagamento con numeroOrdine={0}.", orderNumber);
                            return false;
                        }

                        if (payment.StatusOrdine != status)
                        {
                            var prenotation = _context.Prenotazioni.FirstOrDefault(px => px.Id_Prenotazione == payment.Id_Prenotazione);
                            var prenotationExtended = _context.V_Prenotazione.FirstOrDefault(px => px.Id_Prenotazione == payment.Id_Prenotazione);

                            OnLinePaymentLog onLinePaymentLog = null;
                            if (status == "PAID")
                            {

                                onLinePaymentLog = new OnLinePaymentLog();
                                onLinePaymentLog.PaymentID = payment.Id_Pagamento;
                                onLinePaymentLog.PrenotationNumber = Convert.ToInt32(prenotationExtended.NProtocollo);
                                onLinePaymentLog.OrderNumber = orderNumber;
                                onLinePaymentLog.EntryType_Code = "CP";
                                onLinePaymentLog.EntryDate = DateTime.Now;
                                onLinePaymentLog.User_ID = onLinePaymentServiceUsr.Id_User;

                                if (payment.Id_TipoPagamento != 1)
                                {
                                    message = string.Concat(message,
                                                            System.Environment.NewLine,
                                                            string.Format("ordine di pagamento con numeroOrdine={0} non è di tipo 'OnLine'.", orderNumber));
                                }

                                if (payment.Dt_Pagamento != null)
                                {
                                    message = string.Concat(message,
                                                            System.Environment.NewLine,
                                                            string.Format("L'ordine di pagamento con numeroOrdine={0} risulta già effettuato in data {1}.", orderNumber, Convert.ToDateTime(payment.Dt_Pagamento).ToString(@"dd/MM/yyyy HH:mm")));
                                }

                                var importoCentesimi = Convert.ToInt32(payment.Importo * 100);
                                if (importoCentesimi > Convert.ToInt32(amount))
                                {
                                    message = string.Concat(message,
                                                            System.Environment.NewLine,
                                                            string.Format("La notifica di pagamento per l'ordine di pagamento con numeroOrdine={0} presenta un importo inferiore rispetto all'importo previsto.", orderNumber));

                                }


                                if (message != "")
                                {
                                    onLinePaymentLog.Success = false;
                                    onLinePaymentLog.Notes = message.Replace(System.Environment.NewLine, message);
                                    _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);
                                    _context.SaveChanges();
                                    transaction.Commit();
                                    return false;
                                }

                                payment.Dt_Pagamento = paymentDate;
                                payment.CodiceEsito = resultCode;
                                payment.Dt_Update = DateTime.Now.Date;
                                payment.AutorizzazioneOrdine = authorization;
                                payment.IdTransazione = idTransaction;

                                if (payment.Dt_Pagamento != null)
                                {
                                    LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
                                    LK_Progressivi pr = dalPR.GetSingleItem("R")[0];

                                    if (pr != null)
                                    {
                                        if (pr.Anno != payment.Dt_Pagamento.Value.Year)
                                        {
                                            pr.Anno = pr.Anno > 0 ? payment.Dt_Pagamento.Value.Year : 0;
                                            pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
                                        }
                                        else
                                            pr.Progr_UltimoUscito++;

                                        if (pr.Tipo != string.Empty)
                                            _context.AttachUpdated(pr);
                                        else
                                            _context.LK_Progressivi.AddObject(pr);
                                    }

                                    payment.Ricevuta = pr.Anno == 0 ? pr.Progr_UltimoUscito.ToString() : (pr.Anno.ToString() + "/") + pr.Progr_UltimoUscito.ToString();
                                }

                                prenotation.Id_TipoRisposta = 3;  //CAMBIAMENTO STATO A 'Visita confermata (con ricevuta)'

                                onLinePaymentLog.Success = true;
                                _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);

                            }

                            payment.StatusOrdine = status;
                            payment.NoteOrdine = note;

                            if (status == "ABORTED")
                            {
                                payment.Fl_Annullato = true;

                                onLinePaymentLog = new OnLinePaymentLog();
                                onLinePaymentLog.PaymentID = payment.Id_Pagamento;
                                onLinePaymentLog.PrenotationNumber = Convert.ToInt32(prenotationExtended.NProtocollo);
                                onLinePaymentLog.OrderNumber = orderNumber;
                                onLinePaymentLog.EntryType_Code = "CA";
                                onLinePaymentLog.EntryDate = DateTime.Now;
                                onLinePaymentLog.User_ID = onLinePaymentServiceUsr.Id_User;

                                _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);
                            }

                            _context.SaveChanges();
                        }


                        transaction.Commit();
                        message = "OK";
                        retval = true;

                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        if (message == "")
                            message = e.StackTrace;
                        else
                            message = string.Concat(message, System.Environment.NewLine, e.StackTrace);

                        retval = false;
                    }
                    finally
                    {
                        _context.Connection.Close();
                    }

                    return retval;
                }
            }
        }

        public bool NotifyOrderCancel(string numeroOrdine, out string message)
        {
            bool retval = false;
            message = "";
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {

                        var onLinePaymentServiceUsr = _context.LK_Users.FirstOrDefault(ux => ux.Identificativo == "OLPSRV");

                        if (onLinePaymentServiceUsr == null)
                        {
                            message = "Non è stato aggiunto l'utente 'OLPSRV'";
                            return false;
                        }

                        Pagamento payment = _context.Pagamenti.FirstOrDefault(px => px.NumeroOrdine == numeroOrdine && px.Fl_Annullato == false);

                        if (payment == null)
                        {
                            message = string.Format("Non è stato trovato un ordine di pagamento con numeroOrdine={0}.", numeroOrdine);
                            return false;
                        }

                        var prenotation = _context.Prenotazioni.FirstOrDefault(px => px.Id_Prenotazione == payment.Id_Prenotazione);
                        var prenotationExtended = _context.V_Prenotazione.FirstOrDefault(px => px.Id_Prenotazione == payment.Id_Prenotazione);

                        OnLinePaymentLog onLinePaymentLog = new OnLinePaymentLog();
                        onLinePaymentLog.PaymentID = payment.Id_Pagamento;
                        onLinePaymentLog.PrenotationNumber = Convert.ToInt32(prenotationExtended.NProtocollo);
                        onLinePaymentLog.OrderNumber = numeroOrdine;
                        onLinePaymentLog.EntryType_Code = "CA";
                        onLinePaymentLog.EntryDate = DateTime.Now;
                        onLinePaymentLog.User_ID = onLinePaymentServiceUsr.Id_User;

                        if (payment.Id_TipoPagamento != 1)
                        {
                            message = string.Concat(message,
                                                    System.Environment.NewLine,
                                                    string.Format("L'ordine di pagamento con numeroOrdine={0} non è di tipo 'OnLine'.", numeroOrdine));

                        }

                        if (payment.Dt_Pagamento != null || payment.StatusOrdine == "PAID")
                        {
                            message = string.Concat(message,
                                                    System.Environment.NewLine,
                                                    string.Format("L'ordine di pagamento con numeroOrdine={0} risulta in status 'PAID', non può essere annullato dal FrontOffice.", numeroOrdine));

                        }


                        if (message != "")
                        {
                            onLinePaymentLog.Success = false;
                            onLinePaymentLog.Notes = message.Replace(System.Environment.NewLine, " ");
                            _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);
                            _context.SaveChanges();
                            transaction.Commit();
                            return false;
                        }

                        payment.Fl_Annullato = true;
                        payment.Dt_Update = DateTime.Now.Date;
                        payment.StatusOrdine = "ABORTED";
                        payment.NoteOrdine = "ABORTED_BY_FRONT_OFFICE";

                        onLinePaymentLog.Success = true;
                        _context.OnLinePaymentLogs.AddObject(onLinePaymentLog);

                        _context.SaveChanges();
                        transaction.Commit();

                        message = "OK";
                        retval = true;

                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        if (message == "")
                            message = e.StackTrace;
                        else
                            message = string.Concat(message, System.Environment.NewLine, e.StackTrace);

                        retval = false;
                    }
                    finally
                    {
                        _context.Connection.Close();
                    }

                    return retval;
                }
            }

        }

        public List<V_OnLinePaymentLog> LoadOnLinePaymentLogEntries(int paymentId)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_OnLinePaymentLog.Where(x => x.PaymentID == paymentId && x.Success).ToList();
            }
        }


        #region LOGGING

        static object c = new object();
        private static void LogExc(Exception ex, string message = "")
        {
            string logfilepath = "";
            string logfilename = "";
            string logfilenamebackup = "";

            logfilepath = ConfigurationManager.AppSettings["logfilepath"];
            logfilename = ConfigurationManager.AppSettings["logfilename"];
            logfilenamebackup = ConfigurationManager.AppSettings["logfilenamebackup"];

            lock (c)
            {
                var logFile = string.Format("{0}{1}", logfilepath, logfilename);
                var oldLogFile = string.Format("{0}{1}", logfilepath, logfilenamebackup);

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
                        sw.WriteLine(CreateExceptionString(ex, message));
                    }
                }
                catch { }
            }
        }

        private static void LogMessage(string message = "")
        {
            string logfilepath = "";
            string logfilename = "";
            string logfilenamebackup = "";

            logfilepath = ConfigurationManager.AppSettings["logfilepath"];
            logfilename = ConfigurationManager.AppSettings["logfilename"];
            logfilenamebackup = ConfigurationManager.AppSettings["logfilenamebackup"];

            lock (c)
            {
                var logFile = string.Format("{0}{1}", logfilepath, logfilename);
                var oldLogFile = string.Format("{0}{1}", logfilepath, logfilenamebackup);

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
                        sw.WriteLine(CreateMessage(message));
                    }
                }
                catch { }
            }
        }

        public static string CreateExceptionString(Exception e, string otherMessage = "")
        {
            StringBuilder sb = new StringBuilder();
            CreateExceptionString(sb, e, string.Empty, otherMessage);

            return sb.ToString();
        }

        public static string CreateMessage(string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(System.Environment.NewLine);
            sb.Append(DateTime.Now.ToString());
            sb.Append(System.Environment.NewLine);
            sb.Append(message);
            sb.Append(System.Environment.NewLine);

            sb.AppendLine("-----------------------------------------");
            sb.Append(System.Environment.NewLine);

            return sb.ToString();
        }

        private static void CreateExceptionString(StringBuilder sb, Exception e, string indent, string otherMessage = "")
        {
            if (indent == null)
            {
                indent = string.Empty;
            }
            else if (indent.Length > 0)
            {
                sb.AppendFormat("{0}Inner ", indent);
            }
            sb.Append(System.Environment.NewLine);
            sb.Append(DateTime.Now.ToString());
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("Exception Found:{0}Type: {1}", indent, e.GetType().FullName));
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("{0}Message: {1}", indent, e.Message));
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("{0}Source: {1}", indent, e.Source));
            sb.Append(System.Environment.NewLine);
            sb.Append(string.Format("{0}Stacktrace: {1}", indent, e.StackTrace));
            sb.Append(System.Environment.NewLine);
            if (otherMessage != "")
            {
                sb.Append(string.Format("{0}otherMessage: {1}", indent, otherMessage));
                sb.Append(System.Environment.NewLine);
            }

            if (e.InnerException != null)
            {

                CreateExceptionString(sb, e.InnerException, indent + "  ");
                sb.Append(System.Environment.NewLine);

            }

            sb.AppendLine("-----------------------------------------");
            sb.Append(System.Environment.NewLine);
        }
        #endregion
	}
}

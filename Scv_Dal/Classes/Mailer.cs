using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model;
using Scv_Entities;
using System.Net.Mail;
using System.Globalization;
using System.Configuration;

namespace Scv_Dal
{
	public class Mailer
	{
		//Composizione completa della mail delle guide
		public MailCreationResult CreateGuideVisitNotice(List<V_EvidenzeGiornaliere> listAll, int monthNumber, int yearNumber)
		{
			MailCreationResult result = new MailCreationResult();

			//DAL
			Guida_Dal dalGuide = new Guida_Dal();
			LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
			GuidaLingua_Dal dalLanguage = new GuidaLingua_Dal();
			VisitaProgrammata_Dal dalVisits = new VisitaProgrammata_Dal();
			Parametri_Dal dalParameters = new Parametri_Dal();

			//Vars
			Guida g = null;
			V_GuidaLingua guideLanguage = null;

			//Lingua fissa italiano
			guideLanguage = new V_GuidaLingua();
			guideLanguage.Id_Lingua = 1;
			guideLanguage.Simbolo = "it-IT";


			//Primary keys per raggruppamento
			List<V_EvidenzeGiornaliere> DateHourLanguagegroupList = new List<V_EvidenzeGiornaliere>();
			List<string> visitsIDs = new List<string>();
			List<int> GuidesIds = new List<int>();

			foreach (V_EvidenzeGiornaliere item in listAll
				.Where(x => x.Dt_Visita.Month == monthNumber && x.Dt_Visita.Year == yearNumber && x.Fl_AvvisaGuida == true)
				.OrderBy(x => x.Id_Guida)
				.ThenBy(x => x.Dt_Visita)
				)
			{
				//Ricava le evidenze giornaliere raggruppate per data, ora e lingua
				//e la lista guide presenti nel gruppo
				if (!visitsIDs.Contains(item.Dt_Visita.ToShortDateString() + item.Ora_Visita + item.Id_Lingua.ToString()))
				{
					visitsIDs.Add(item.Dt_Visita.ToShortDateString() + item.Ora_Visita + item.Id_Lingua.ToString());
					if (item.Fl_AvvisaGuida != null && (bool)item.Fl_AvvisaGuida)
					{
						DateHourLanguagegroupList.Add(item);

						if (item.Id_Guida != null && !GuidesIds.Contains((int)item.Id_Guida))
							GuidesIds.Add((int)item.Id_Guida);
					}
				}
			}

			//composizione testi tradotti
			List<TextPartFilterItem> texts = null;

			//Variabili temporanee per facilitare la leggibilità
			string tableCaption = string.Empty;
			string table = string.Empty;
			string emailBody = string.Empty;

			//parametri creazione MailMessage
            string body = string.Empty;

			//Informazioni parametrizzate
			string comunicazioniEmail, comunicazioniTelefono, comunicazioniFax, comunicazioniPrivato, comunicazioniResponsabile = string.Empty;
			comunicazioniEmail = dalParameters.GetItem("comunicazioni_guide_email").Valore;
			comunicazioniTelefono = dalParameters.GetItem("comunicazioni_guide_telefono").Valore;
			comunicazioniFax = dalParameters.GetItem("comunicazioni_guide_fax").Valore;
			comunicazioniPrivato = dalParameters.GetItem("comunicazioni_guide_telefono_privato").Valore;
			comunicazioniResponsabile = dalParameters.GetItem("comunicazioni_guide_responsabile_ufficio_scavi").Valore;

			//Itera fra le guide
			foreach (int guideID in GuidesIds)
			{
				result.success = true;

				g = dalGuide.GetItem(guideID);

				//Se la guida non esiste, si ferma tutto e viene segnalato l'errore
				if (g == null)
				{
					result.success = false;
					result.ErrorList += "\nLa guida assegnata non esiste";
				}

				if (result.success)
				{
					//Se la guida esiste ma non ha un'email si ferma tutto e viene segnalato l'errore
					if (string.IsNullOrEmpty(g.Email))
					{
						result.success = false;
						result.ErrorList += "\n- La guida " + g.Nominativo + " non è associata a nessuna email";
					}

					//Se non c'è nessun errore, si procede alla creazione del corpo della mail
					if (result.success)
					{

						//crea la lista delle visite della guida
						List<V_EvidenzeGiornaliere> guideVisits = new List<V_EvidenzeGiornaliere>();
						foreach (V_EvidenzeGiornaliere eg in DateHourLanguagegroupList)
							if (eg.Id_Guida == guideID)
								guideVisits.Add(eg);

						//Crea titolo tabella
						texts = new List<TextPartFilterItem>();
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "R", 0, 0));
						tableCaption = dalText.AddParts(texts) + "<br />";

						//crea la tabella delle visite per la guida corrente
						result = CreateGuideVisitsTable(guideID, guideLanguage, guideVisits);
						if (result.success)
							table = result.Text;

						//crea corpo email
						texts = new List<TextPartFilterItem>();
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "Q5", false, new List<string> { comunicazioniEmail }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "Q2", false, new List<string> { comunicazioniTelefono }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "Q3", false, new List<string> { comunicazioniFax }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "Q4", false, new List<string> { comunicazioniPrivato }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "Q1", false, new List<string> { comunicazioniResponsabile }, ";", VariablePosition.End, 2, 0));
						texts.Add(new TextPartFilterItem(guideLanguage.Id_Lingua, "Q6", 1, 0));
						emailBody = dalText.AddParts(texts);

						//Si imposta il body racchiudendolo in un div per il font e l'allineamento html
						result.Text = "<div style=\"font-family:Arial;font-size:16px;\">" + tableCaption + table + emailBody + "</div>";
					}
				}
			}

			if (!result.success)
				result.ErrorList = "Non è stato possibile inviare alcune email per i seguenti motivi:" + result.ErrorList;

			return result;
		}

		public MailCreationResult CreateGuideVisitsTable(int guideID, V_GuidaLingua guideLanguage, List<V_EvidenzeGiornaliere> visitList)
		{
			MailCreationResult result = new MailCreationResult();
			string table = string.Empty;
			Guida_Dal dalGuide = new Guida_Dal();
			LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
			GuidaLingua_Dal dalGuideLanguage = new GuidaLingua_Dal();
			LK_DescrizioneLingua_Dal dalLanguageTranslation = new LK_DescrizioneLingua_Dal();

			Guida guide = dalGuide.GetItem(guideID);
			string visitLanguage = string.Empty;

			foreach (V_EvidenzeGiornaliere item in visitList)
			{
				result.success = true;
				if (guide == null)
				{
					result.success = false;
					result.ErrorList += "\n- visita " + item.Dt_Visita.ToShortDateString() + " " + item.Ora_Visita + ": Non è stata assegnata la guida";
				}

				if (result.success)
				{

					table += "<br /><b>Data</b>: " + item.Dt_Visita.ToString("D", CultureInfo.CreateSpecificCulture(guideLanguage.Simbolo));
					table += " <b>Ora</b>: " + item.Ora_Visita;
					table += " <b>Lingua</b>: " + dalLanguageTranslation.GetItemByLanguageID(item.Id_Lingua, guideLanguage.Id_Lingua).Descrizione;
					table += " <b>Conferma</b>: " + (item.Fl_AccettaGuida == true ? "SI" : "NO");
				}
			}

			table += "<br>_________________________________________________<br />";

			result.Text = table;

			return result;
		}

		//Oggetto della mail per il richiedente
		public MailCreationResult CreatePetitionerVisitNoticeSubject(Prenotazione prenotation, VisitaProgrammata visit)
		{
			MailCreationResult result = new MailCreationResult();

			Pagamento_Dal dalPayment = new Pagamento_Dal();
			Richiedente_Dal dalPetitioner = new Richiedente_Dal();
			VisitaPrenotata_Dal dalPrenotatedVisit = new VisitaPrenotata_Dal();
			VisitaProgrammata_Dal dalScheduledVisit = new VisitaProgrammata_Dal();
			Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();
			LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
			LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
			List<TextPartFilterItem> texts = new List<TextPartFilterItem>();

			string reference = string.Empty;
			string receipt = string.Empty;
			string date = string.Empty;
			string hour = string.Empty;
			int visitorsNumber = 0;
			string visitor = string.Empty;
			int prenotationResponseLanguageID = 0;

			if (prenotation != null)
			{
				reference = ((int)dalPrenotation.Get_V_Item(prenotation.Id_Prenotazione).NProtocollo).ToString();

				List<V_VisiteProgrammate> visits = dalScheduledVisit.GetVListByIdPrenotazione(prenotation.Id_Prenotazione).ToList();
				if (visits != null)
				{
					foreach (V_VisiteProgrammate v in visits)
					{
						visitorsNumber += v.Nr_Interi != null ? (int)v.Nr_Interi : 0;
						visitorsNumber += v.Nr_Ridotti != null ? (int)v.Nr_Ridotti : 0;
						visitorsNumber += v.Nr_Omaggio != null ? (int)v.Nr_Omaggio : 0;
					}
				}

				LK_Lingua prenotationLanguage = dalLanguage.GetItem(prenotation.Id_LinguaRisposta);
				if (prenotationLanguage != null)
				{
					prenotationResponseLanguageID = prenotationLanguage.Id_Lingua;

					if (visit != null)
					{
						date = visit.Dt_Visita != null ? visit.Dt_Visita.ToString("D", CultureInfo.CreateSpecificCulture(prenotationLanguage.Simbolo)) : string.Empty;
						hour = !string.IsNullOrEmpty(visit.Ora_Visita) ? visit.Ora_Visita : string.Empty;

					}

					Richiedente petitioner = dalPetitioner.GetItem(prenotation.Id_Richiedente);
                    if (petitioner != null)
                    {
                        if(!string.IsNullOrEmpty(petitioner.Nome))
                            visitor = petitioner.Cognome + " " + petitioner.Nome;
                        else
                            visitor = petitioner.Cognome;
                    }

                    //Pagamento payment = dalPayment.GetItemByIdPrenotazione(prenotation.Id_Prenotazione);
                    //if (payment != null)
                    //    receipt = payment.Ricevuta;
                    List<Pagamento> payments = dalPayment.GetListByIdPrenotazione(prenotation.Id_Prenotazione);
                    if (payments != null)
                    {
                        foreach (Pagamento pym in payments)
                        {
                            if (receipt.Length > 0)
                                receipt += ", ";
                            receipt += pym.Ricevuta;
                        }
                    }
				}
			}

			if (string.IsNullOrEmpty(reference))
			{
				result.ErrorList += "\n - Manca il protocollo della prenotazione";
				result.success = false;
			}

			if (string.IsNullOrEmpty(visitor))
			{
				result.ErrorList += "\n - Manca il nome del richiedente";
				result.success = false;
			}

			if ((prenotation.Id_TipoRisposta == 2 || prenotation.Id_TipoRisposta == 3) && visitorsNumber < 1)
			{
				result.ErrorList += "\n - Non ci sono visitatori per la visita";
				result.success = false;
			}

			if ((prenotation.Id_TipoRisposta == 2 || prenotation.Id_TipoRisposta == 3) && (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(hour)))
			{
				result.ErrorList += "\n - Non è indicata la data e/o l'orario della visita";
				result.success = false;
			}

			if (result.success)
			{
				texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "010", true, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
				if (!string.IsNullOrEmpty(receipt))
					texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "020", true, new List<string> { receipt }, ";", VariablePosition.End, 0, 0));
				if (
						(
						prenotation.Id_TipoRisposta == 2
						||
						prenotation.Id_TipoRisposta == 3
						||
						prenotation.Id_TipoRisposta == 10
						)
						&& 
						!string.IsNullOrEmpty(date) 
						&& !string.IsNullOrEmpty(hour)
					)
					texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "030", true, new List<string> { date }, ";", VariablePosition.End, 0, 0));
				
				if (
						(
						prenotation.Id_TipoRisposta == 2
						||
						prenotation.Id_TipoRisposta == 3
						||
						prenotation.Id_TipoRisposta == 10
						)
						&& !string.IsNullOrEmpty(date)
						&& !string.IsNullOrEmpty(hour)
					)
					texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "040", true, new List<string> { visitorsNumber.ToString() }, ";", VariablePosition.End, 0, 0));
		
				result.Text = dalText.AddParts(texts);
				result.Text = result.Text.Trim();
			}

			return result;
		}

        public MailCreationResult CreatePetitionerVisitNoticeSubject(Prenotazione prenotation, VisitaProgrammata visit, IN_VIAEntities _context)
        {
            MailCreationResult result = new MailCreationResult();

            LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
            List<TextPartFilterItem> texts = new List<TextPartFilterItem>();

            string reference = string.Empty;
            string receipt = string.Empty;
            string date = string.Empty;
            string hour = string.Empty;
            int visitorsNumber = 0;
            string visitor = string.Empty;
            int prenotationResponseLanguageID = 0;

            if (prenotation != null)
            {
                reference = _context.V_Prenotazione.FirstOrDefault(rx => rx.Id_Prenotazione == prenotation.Id_Prenotazione).NProtocollo.ToString();
                var visits = _context.V_VisiteProgrammate.Where(vpx => vpx.Id_Prenotazione == prenotation.Id_Prenotazione).ToList();
				visits.ToList().ForEach(x => x.IsEmpty = false);
				visits.ToList().ForEach(x => x.IsErasable = true);

                //List<V_VisiteProgrammate> visits = dalScheduledVisit.GetVListByIdPrenotazione(prenotation.Id_Prenotazione).ToList();
                if (visits != null)
                {
                    foreach (V_VisiteProgrammate v in visits)
                    {
                        visitorsNumber += v.Nr_Interi != null ? (int)v.Nr_Interi : 0;
                        visitorsNumber += v.Nr_Ridotti != null ? (int)v.Nr_Ridotti : 0;
                        visitorsNumber += v.Nr_Omaggio != null ? (int)v.Nr_Omaggio : 0;
                    }
                }

                LK_Lingua prenotationLanguage = _context.LK_Lingue.FirstOrDefault(rx => rx.Id_Lingua == prenotation.Id_LinguaRisposta);
                
                if (prenotationLanguage != null)
                {
                    prenotationResponseLanguageID = prenotationLanguage.Id_Lingua;

                    if (visit != null)
                    {
                        date = visit.Dt_Visita != null ? visit.Dt_Visita.ToString("D", CultureInfo.CreateSpecificCulture(prenotationLanguage.Simbolo)) : string.Empty;
                        hour = !string.IsNullOrEmpty(visit.Ora_Visita) ? visit.Ora_Visita : string.Empty;

                    }

                    Richiedente petitioner = _context.Richiedenti.SingleOrDefault(rx => rx.Id_Richiedente == prenotation.Id_Richiedente);

                    if (petitioner != null)
                    {
                        if(!string.IsNullOrEmpty(petitioner.Nome))
                            visitor = petitioner.Cognome + " " + petitioner.Nome;
                        else
                            visitor = petitioner.Cognome;
                    }

                   
                    List<Pagamento> payments = _context.Pagamenti.Where(rx => rx.Id_Prenotazione == prenotation.Id_Prenotazione && rx.Fl_Annullato == false).ToList();
                    if (payments != null)
                    {
                        foreach (Pagamento pym in payments)
                        {
                            if (receipt.Length > 0)
                                receipt += ", ";
                            receipt += pym.Ricevuta;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(reference))
            {
                result.ErrorList += "\n - Manca il protocollo della prenotazione";
                result.success = false;
            }

            if (string.IsNullOrEmpty(visitor))
            {
                result.ErrorList += "\n - Manca il nome del richiedente";
                result.success = false;
            }

            if ((prenotation.Id_TipoRisposta == 2 || prenotation.Id_TipoRisposta == 3) && visitorsNumber < 1)
            {
                result.ErrorList += "\n - Non ci sono visitatori per la visita";
                result.success = false;
            }

            if ((prenotation.Id_TipoRisposta == 2 || prenotation.Id_TipoRisposta == 3) && (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(hour)))
            {
                result.ErrorList += "\n - Non è indicata la data e/o l'orario della visita";
                result.success = false;
            }

            if (result.success)
            {
                texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "010", true, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
                if (!string.IsNullOrEmpty(receipt))
                    texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "020", true, new List<string> { receipt }, ";", VariablePosition.End, 0, 0));
                if (
                        (
                        prenotation.Id_TipoRisposta == 2
                        ||
                        prenotation.Id_TipoRisposta == 3
                        ||
                        prenotation.Id_TipoRisposta == 10
                        )
                        &&
                        !string.IsNullOrEmpty(date)
                        && !string.IsNullOrEmpty(hour)
                    )
                    texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "030", true, new List<string> { date }, ";", VariablePosition.End, 0, 0));

                if (
                        (
                        prenotation.Id_TipoRisposta == 2
                        ||
                        prenotation.Id_TipoRisposta == 3
                        ||
                        prenotation.Id_TipoRisposta == 10
                        )
                        && !string.IsNullOrEmpty(date)
                        && !string.IsNullOrEmpty(hour)
                    )
                    texts.Add(new TextPartFilterItem(prenotationResponseLanguageID, "040", true, new List<string> { visitorsNumber.ToString() }, ";", VariablePosition.End, 0, 0));

                result.Text = dalText.AddParts(texts, _context);
                result.Text = result.Text.Trim();
            }

            return result;
        }

		//Corpo della mail per il richiedente
		public MailCreationResult CreatePetitionerVisitNoticeBody(Prenotazione prenotation, List<VisitaProgrammata> visits, bool confirmed)
		{
			MailCreationResult result = new MailCreationResult();
			List<TextPartFilterItem> texts = new List<TextPartFilterItem>();

			Pagamento_Dal dalPayment = new Pagamento_Dal();
			Richiedente_Dal dalPetitioner = new Richiedente_Dal();
			VisitaPrenotata_Dal dalPrenotatedVisit = new VisitaPrenotata_Dal();
			VisitaProgrammata_Dal dalScheduledVisit = new VisitaProgrammata_Dal();
			Prenotazione_Dal dalPrenotation = new Prenotazione_Dal();
			LK_Lingua_Dal dalLanguage = new LK_Lingua_Dal();
			LK_DescrizioneLingua_Dal dalLanguageTranslate = new LK_DescrizioneLingua_Dal();
			Parametri_Dal dalParameters = new Parametri_Dal();
			LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
			LK_User_Dal dalUser = new LK_User_Dal();

			string reference = string.Empty;
			string receipt = string.Empty;
			string prenotationDate1 = string.Empty;
			string prenotationDate2 = string.Empty;
			string visitDateShort = string.Empty;
			string visitDateLong = string.Empty;
			string visitHour = string.Empty;
			int visitorsNumber = 0;
			string visitor = string.Empty;
			decimal ticketFullPrice = 0;
			decimal ticketFullPriceTotal = 0;
			decimal ticketReducedPrice = 0;
			decimal ticketReducedPriceTotal = 0;
			string totalPayment = string.Empty;
			string paymentDate = string.Empty;
			string visitLang = string.Empty;
			int confirmDays = 0;
			string closingPart = string.Empty;
			int visitsLanguageCount = 0;
            bool enableOnlinePayment = false;

			TimeSpan dateDiff = new TimeSpan();

			LK_Lingua prenotationLanguage = null;
			LK_Lingua visitLanguage = null;
			LK_User user = null;
			Richiedente petitioner = null;
			Parametri p = null;
			p = dalParameters.GetItem("biglietto_intero");
			if (p != null)
				decimal.TryParse(p.Valore, out ticketFullPrice);

			p = dalParameters.GetItem("biglietto_ridotto");
			if (p != null)
				decimal.TryParse(p.Valore, out ticketReducedPrice);

            if (ConfigurationManager.AppSettings["enableonlinepayment"] != null)
                enableOnlinePayment = Convert.ToBoolean(ConfigurationManager.AppSettings["enableonlinepayment"]);

			if (prenotation != null)
			{
				//Si ricava il tsto relativo ai giorni di chiusura dall'anno della data di preferenza 
				//impostata nella richiesta di prenotazione (la data "DAL").
				closingPart = "CH" + prenotation.Dt_VisiteDA.Year.ToString();

				//Si ricava il protocollo
				reference = ((int)dalPrenotation.Get_V_Item(prenotation.Id_Prenotazione).NProtocollo).ToString();

				//Si ricavano le date di preferenza del richiedente
				prenotationDate1 = prenotation.Dt_VisiteDA != null ? prenotation.Dt_VisiteDA.ToShortDateString() : string.Empty;
				prenotationDate2 = prenotation.Dt_VisiteA != null ? prenotation.Dt_VisiteA.ToShortDateString() : string.Empty;

				//Si ricava il richiedente
				petitioner = dalPetitioner.GetItem(prenotation.Id_Richiedente);

				//Si ricava il visitatore responsabile
				visitor = prenotation.Responsabile;

				//Si ricava l'utente del Vaticano che effettua l'operazione
				user = dalUser.GetItem((int)prenotation.Id_User);

				//Si ricava la lingua di risposta della prenotazione
				prenotationLanguage = dalLanguage.GetItem(prenotation.Id_LinguaRisposta);

				List<Pagamento> payment = dalPayment.GetListByIdPrenotazione(prenotation.Id_Prenotazione);
				if (payment != null)
				{
					foreach (Pagamento pym in payment)
					{
						if (receipt.Length > 0)
							receipt += ", ";
						receipt += pym.Ricevuta;
					}
				}

				//Si ricavano le visite programmate per ricavarne successivamente il numero di lingue presenti nelle visite
				List<V_VisiteProgrammate> vp = dalScheduledVisit.GetVListByIdPrenotazione(prenotation.Id_Prenotazione).ToList();
				if (vp != null)
					visitsLanguageCount = vp.Select(x => x.Id_Lingua).Count();

			}

			if (visits != null && visits.Count > 0)
			{
				//Data della visita in formato lungo e corto
				visitDateShort = visits[0].Dt_Visita.ToShortDateString();
				visitDateLong = visits[0].Dt_Visita.ToString("D", CultureInfo.CreateSpecificCulture(prenotationLanguage.Simbolo));

				//Dato che tutte le visite si svolgono nello stesso giorno, è sufficiente
				//la prima per ricavare il giorno di qualsiasi visita
				dateDiff = visits[0].Dt_Visita.Date.Subtract(DateTime.Now.Date);
				if (dateDiff.Days < 7)
					int.TryParse(dalParameters.GetItem("data_visita_minore_7").Valore, out confirmDays);

				if (dateDiff.Days >= 7 && dateDiff.Days <= 30)
					int.TryParse(dalParameters.GetItem("data_visita_fino_30").Valore, out confirmDays);

				if (dateDiff.Days > 30)
					int.TryParse(dalParameters.GetItem("data_visita_oltre_30").Valore, out confirmDays);
			}

			if (petitioner == null)
			{
				result.ErrorList += "\n - Manca il richiedente";
				result.success = false;
			}

			if (user == null)
			{
				result.ErrorList += "\n - Manca l'utente che ha registrato la prenotazione";
				result.success = false;
			}

			if (string.IsNullOrEmpty(reference))
			{
				result.ErrorList += "\n - Manca il protocollo della prenotazione";
				result.success = false;
			}

			if (prenotationLanguage == null)
			{
				result.ErrorList += "\n - Manca la lingua di risposta della prenotazione";
				result.success = false;
			}

			if (
				((TipoRisposta)prenotation.Id_TipoRisposta == TipoRisposta.VisitaProgrammata || prenotation.Id_TipoRisposta == (int)TipoRisposta.ConfermaNoRicevuta) 
				&& string.IsNullOrEmpty(visitor))
			{
				result.ErrorList += "\n - Manca il nome del capogruppo";
				result.success = false;
			}

			if ((TipoRisposta)prenotation.Id_TipoRisposta == TipoRisposta.NecropoliChiusa && string.IsNullOrEmpty(prenotationDate1))
			{
				result.ErrorList += "\n - Manca la data iniziale della prenotazione";
				result.success = false;
			}

			if ((TipoRisposta)prenotation.Id_TipoRisposta == TipoRisposta.NecropoliChiusa && string.IsNullOrEmpty(prenotationDate2))
			{
				result.ErrorList += "\n - Manca la data finale della prenotazione";
				result.success = false;
			}

			if (result.success)
			{
				int spInteri = 0;
				int spRidotti = 0;
				int totNrInteri = 0;
				int totNrRidotti = 0;
				int totNrOmaggio = 0;

				switch ((TipoRisposta)prenotation.Id_TipoRisposta)
				{
					case TipoRisposta.RichiestaIncompleta:
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "M1", false, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "M2", 2, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "3", false, new List<string> { ticketFullPrice.ToString() }, ";", VariablePosition.End, 2, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));						
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> {  "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);
						else
						{
							result.success = false;
							result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
						}

						break;

					case TipoRisposta.VisitaProgrammata:
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "A", 0, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D1", false, new List<string> { "<b>" + reference + "</b>" }, ";", VariablePosition.End, 2, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D2", false, new List<string> { "<b>" + visitor + "</b>" }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D3", false, new List<string> { "<b>" + visitDateLong + "</b>" }, ";", VariablePosition.End, 1, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);

							//Dettagli visita
							result.Text += "<br /><br />";

							result.Text += "_______________________________________________________<br /><br />";

							foreach (VisitaProgrammata item in visits.OrderBy(x => x.Ora_Visita))
							{
								texts.Clear();

								VisitaPrenotata vpr = dalPrenotatedVisit.GetItem(item.Id_VisitaPrenotata);
								if (vpr != null)
								{
									visitLanguage = dalLanguage.GetItem(vpr.Id_Lingua);
									if (visitLanguage != null)
										visitLang = dalLanguageTranslate.GetItemByLanguageID(visitLanguage.Id_Lingua, prenotationLanguage.Id_Lingua).Descrizione;
								}

								visitHour = !string.IsNullOrEmpty(item.Ora_Visita) ? item.Ora_Visita : string.Empty;

								visitorsNumber = item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
								visitorsNumber += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
								visitorsNumber += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

								ticketFullPriceTotal += item.Nr_Interi != null ? (decimal)item.Nr_Interi * ticketFullPrice : 0;
								ticketReducedPriceTotal += item.Nr_Ridotti != null ? (decimal)item.Nr_Ridotti * ticketReducedPrice : 0;
								totNrInteri += item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
								totNrRidotti += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
								totNrOmaggio += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D4", false, new List<string> { "<b>" + visitHour + "</b> - " }, ";", VariablePosition.End, 0, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D5", false, new List<string> { "<b>" + visitorsNumber.ToString() + "</b> - " }, ";", VariablePosition.End, 0, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D6", false, new List<string> { "<b>" + visitLang + "</b>" }, ";", VariablePosition.End, 0, 0));

								if (texts.Count > 0)
									result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

								result.Text += "<br /><br />";
							}

							result.Text += "_______________________________________________________";

							texts.Clear();

							spInteri = 0;
							spRidotti = 0;

							if (totNrInteri > 0)
							{
								spInteri = 2;
								spRidotti = 1;
							}
							else
								if (totNrRidotti > 0)
									spRidotti = 2;

							if (totNrInteri > 0)
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D13", false, new List<string> { "<b>" + ticketFullPrice.ToString() + "</b>" }, ";", VariablePosition.End, spInteri, 0));

							if (totNrRidotti > 0)
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D14", false, new List<string> { "<b>" + ticketReducedPrice.ToString() + "</b>" }, ";", VariablePosition.End, spRidotti, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

							if (totNrInteri > 0 || totNrRidotti > 0)
								result.Text += "<br />_______________________________________________________<br />";

							texts.Clear();

							if (totNrInteri > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + totNrInteri.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D8", false, new List<string> { "<b>" + ticketFullPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
							}

							if (totNrRidotti > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D9", false, new List<string> { "<b>" + totNrRidotti.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D10", false, new List<string> { "<b>" + ticketReducedPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
							}

							if (totNrOmaggio > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D11", false, new List<string> { "<b>" + totNrOmaggio.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D12", 0, 0));
							}

							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

							if (totNrInteri > 0 && totNrRidotti > 0)
							{
								result.Text += "<br />_______________________________________________________";

								texts.Clear();

								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + (totNrInteri + totNrRidotti).ToString() + "</b>" }, ";", VariablePosition.End, 2, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D15", false, new List<string> { "<b>" + (ticketFullPriceTotal + ticketReducedPriceTotal).ToString("c") + "</b>" }, ";", VariablePosition.End, 0, 0));
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);
							}

							texts.Clear();

                            int onlinePaymentDaysAfterAllowed = 0;
                            int.TryParse(dalParameters.GetItem("onlinePaymentDaysAfterAllowed").Valore, out onlinePaymentDaysAfterAllowed);

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F1", false, new List<string> { confirmDays.ToString() }, ";", VariablePosition.End, 2, 0));

                            if (enableOnlinePayment)
                            {
                                if (dateDiff.Days <= onlinePaymentDaysAfterAllowed)
                                {
                                    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F2", false, new List<string> { DateTime.Now.Date.ToShortDateString()}, ";", VariablePosition.End, 0, 0));
                                    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F3", 0, 0));
                                }
                                else
                                    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F2", false, new List<string> { string.Concat(DateTime.Now.Date.ToShortDateString(), "</b>") }, ";", VariablePosition.End, 0, 0));

                            }
                            else
                            {
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F2", false, new List<string> { DateTime.Now.Date.ToShortDateString() }, ";", VariablePosition.End, 0, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F3", 0, 0));
                            }


							if (totNrInteri > 0 || totNrRidotti > 0)
							{
                                if (dateDiff.Days <= onlinePaymentDaysAfterAllowed)
									texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "G", 2, 0));
								else
								{
									texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "H", 2, 0));
									texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I1", false, new List<string> { confirmDays.ToString() }, ";", VariablePosition.End, 2, 0));
									texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I1b", false, new List<string> { DateTime.Now.Date.ToShortDateString() }, ";", VariablePosition.End, 0, 0));

                                    if (!enableOnlinePayment)
                                    {
                                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I2", false, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
                                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I3", false, new List<string> { visitorsNumber.ToString() }, ";", VariablePosition.End, 0, 0));
                                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I4", false, new List<string> { visitDateLong, visitHour }, " ", VariablePosition.End, 0, 0));
                                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I5", 0, 0));
                                    }
                                    else
                                    {
                                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I2b" , 0, 0));
                                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I5b", 0, 0));
                                    }
								}
							}

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "J", 2, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "C", 2, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + user.Nominativo + (!string.IsNullOrEmpty(user.Titolo) ? "<br />" + user.Titolo + " " : string.Empty) }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);
							else
							{
								result.success = false;
								result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
							}

						break;

					case TipoRisposta.ConfermaNoRicevuta:
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "A", 0, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D1", false, new List<string> { "<b>" + reference + "</b>" }, ";", VariablePosition.End, 2, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D2", false, new List<string> { "<b>" + visitor + "</b>" }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D3", false, new List<string> { "<b>" + visitDateLong + "</b>" }, ";", VariablePosition.End, 1, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);

							//Dettagli visita
							result.Text += "<br /><br />";

							result.Text += "_______________________________________________________<br /><br />";

							foreach (VisitaProgrammata item in visits.OrderBy(x => x.Ora_Visita))
							{
								texts.Clear();

								VisitaPrenotata vpr = dalPrenotatedVisit.GetItem(item.Id_VisitaPrenotata);
								if (vpr != null)
								{
									visitLanguage = dalLanguage.GetItem(vpr.Id_Lingua);
									if (visitLanguage != null)
										visitLang = dalLanguageTranslate.GetItemByLanguageID(visitLanguage.Id_Lingua, prenotationLanguage.Id_Lingua).Descrizione;
								}

								visitHour = !string.IsNullOrEmpty(item.Ora_Visita) ? item.Ora_Visita : string.Empty;

								visitorsNumber = item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
								visitorsNumber += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
								visitorsNumber += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

								ticketFullPriceTotal += item.Nr_Interi != null ? (decimal)item.Nr_Interi * ticketFullPrice : 0;
								ticketReducedPriceTotal += item.Nr_Ridotti != null ? (decimal)item.Nr_Ridotti * ticketReducedPrice : 0;
								totNrInteri += item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
								totNrRidotti += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
								totNrOmaggio += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D4", false, new List<string> { "<b>" + visitHour + "</b> - " }, ";", VariablePosition.End, 0, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D5", false, new List<string> { "<b>" + visitorsNumber.ToString() + "</b> - " }, ";", VariablePosition.End, 0, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D6", false, new List<string> { "<b>" + visitLang + "</b>" }, ";", VariablePosition.End, 0, 0));

								if (texts.Count > 0)
									result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

								result.Text += "<br /><br />";
							}

							result.Text += "_______________________________________________________";

							texts.Clear();

							spInteri = 0;
							spRidotti = 0;

							if (totNrInteri > 0)
							{
								spInteri = 2;
								spRidotti = 1;
							}
							else
								if (totNrRidotti > 0)
									spRidotti = 2;

							if (totNrInteri > 0)
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D13", false, new List<string> { "<b>" + ticketFullPrice.ToString() + "</b>" }, ";", VariablePosition.End, spInteri, 0));

							if (totNrRidotti > 0)
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D14", false, new List<string> { "<b>" + ticketReducedPrice.ToString() + "</b>" }, ";", VariablePosition.End, spRidotti, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

							if (totNrInteri > 0 || totNrRidotti > 0)
								result.Text += "<br />_______________________________________________________<br />";

							texts.Clear();

							if (totNrInteri > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + totNrInteri.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D8", false, new List<string> { "<b>" + ticketFullPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
							}

							if (totNrRidotti > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D9", false, new List<string> { "<b>" + totNrRidotti.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D10", false, new List<string> { "<b>" + ticketReducedPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
							}

							if (totNrOmaggio > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D11", false, new List<string> { "<b>" + totNrOmaggio.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D12", 0, 0));
							}

							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

							if (totNrInteri > 0 && totNrRidotti > 0)
							{
								result.Text += "<br />_______________________________________________________";

								texts.Clear();

								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + (totNrInteri + totNrRidotti).ToString() + "</b>" }, ";", VariablePosition.End, 2, 0));
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D15", false, new List<string> { "<b>" + (ticketFullPriceTotal + ticketReducedPriceTotal).ToString("c") + "</b>" }, ";", VariablePosition.End, 0, 0));
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);
							}

							texts.Clear();

							if (totNrInteri > 0 || totNrRidotti > 0)
							{
								texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "G", 2, 0));
							}

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "B", 2, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "J", 2, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "C", 2, 0));

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + user.Nominativo + (!string.IsNullOrEmpty(user.Titolo) ? "<br />" + user.Titolo + " " : string.Empty) }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);
							else
							{
								result.success = false;
								result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
							}

						break;

                    case TipoRisposta.ConfermaRicevuta:
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "A", 0, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D1", false, new List<string> { reference }, ";", VariablePosition.End, 2, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D2", false, new List<string> { visitor }, ";", VariablePosition.End, 0, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D3", false, new List<string> { visitDateLong }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "020", false, new List<string> { receipt }, ";", VariablePosition.End, 1, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);

						//Dettagli visita
						result.Text += "<br /><br />";

						totNrInteri = 0;
						totNrRidotti = 0;
						totNrOmaggio = 0;

						result.Text += "_______________________________________________________<br /><br />";

						foreach (VisitaProgrammata item in visits.OrderBy(x => x.Ora_Visita))
						{
							texts.Clear();

							VisitaPrenotata vpr = dalPrenotatedVisit.GetItem(item.Id_VisitaPrenotata);
							if (vpr != null)
							{
								visitLanguage = dalLanguage.GetItem(vpr.Id_Lingua);
								if (visitLanguage != null)
									visitLang = dalLanguageTranslate.GetItemByLanguageID(visitLanguage.Id_Lingua, prenotationLanguage.Id_Lingua).Descrizione;
							}
							visitHour = !string.IsNullOrEmpty(item.Ora_Visita) ? item.Ora_Visita : string.Empty;

							visitorsNumber = item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
							visitorsNumber += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
							visitorsNumber += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

							ticketFullPriceTotal += item.Nr_Interi != null ? (decimal)item.Nr_Interi * ticketFullPrice : 0;
							ticketReducedPriceTotal += item.Nr_Ridotti != null ? (decimal)item.Nr_Ridotti * ticketReducedPrice : 0;
							totNrInteri += item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
							totNrRidotti += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
							totNrOmaggio += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D4", false, new List<string> { "<b>" + visitHour + "</b> - " }, ";", VariablePosition.End, 0, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D5", false, new List<string> { "<b>" + visitorsNumber.ToString() + "</b> - " }, ";", VariablePosition.End, 0, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D6", false, new List<string> { "<b>" + visitLang + "</b>" }, ";", VariablePosition.End, 0, 0));

							if (texts.Count > 0)
								result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

							result.Text += "<br /><br />";
						}

						result.Text += "_______________________________________________________";

						texts.Clear();

						if (totNrInteri > 0)
						{
							spInteri = 2;
							spRidotti = 1;
						}
						else
							if (totNrRidotti > 0)
								spRidotti = 2;

						if (totNrInteri > 0)
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D13", false, new List<string> { "<b>" + ticketFullPrice.ToString() + "</b>" }, ";", VariablePosition.End, spInteri, 0));

						if (totNrRidotti > 0)
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D14", false, new List<string> { "<b>" + ticketReducedPrice.ToString() + "</b>" }, ";", VariablePosition.End, spRidotti, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

						if (totNrInteri > 0 || totNrRidotti > 0)
							result.Text += "<br />_______________________________________________________<br />";

						texts.Clear();

						if (totNrInteri > 0)
						{
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + totNrInteri.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D8", false, new List<string> { "<b>" + ticketFullPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
						}

						if (totNrRidotti > 0)
						{
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D9", false, new List<string> { "<b>" + totNrRidotti.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D10", false, new List<string> { "<b>" + ticketReducedPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
						}

						if (totNrOmaggio > 0)
						{
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D11", false, new List<string> { "<b>" + totNrOmaggio.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D12", 0, 0));
						}

						result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);

						if (totNrInteri > 0 && totNrRidotti > 0)
						{
							result.Text += "<br />_______________________________________________________<br />";

							texts.Clear();

							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + (totNrInteri + totNrRidotti).ToString() + "</b>" }, ";", VariablePosition.End, 2, 0));
							texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D15", false, new List<string> { "<b>" + (ticketFullPriceTotal + ticketReducedPriceTotal).ToString("c") + "</b>" }, ";", VariablePosition.End, 0, 0));
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);
						}

						texts.Clear();

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "B", 2, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "C", 2, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));						
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> {  "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, false);
						else
						{
							result.success = false;
							result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
						}

						break;

					case TipoRisposta.AnnullataDaUfficioScavi:
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "O", 0, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "3", false, new List<string> { ticketFullPrice.ToString() }, ";", VariablePosition.End, 2, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));						
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> {  "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);
						else
						{
							result.success = false;
							result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
						}
						break;

					case TipoRisposta.NecropoliChiusa:
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "P1", false, new List<string> { prenotationDate1 }, ";", VariablePosition.End, 0, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "P2", false, new List<string> { prenotationDate2 }, ";", VariablePosition.End, 0, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "P3", 0, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));						
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> {  "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);
						else
						{
							result.success = false;
							result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
						}
						break;

					case TipoRisposta.VisiteComplete:
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "N1", false, new List<string> { prenotationDate1 }, ";", VariablePosition.End, 0, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "N2", false, new List<string> { prenotationDate2 }, ";", VariablePosition.End, 0, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "N3", 0, 0));

						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));						
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> {  "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
						texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

						if (texts.Count > 0)
							result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts);
						else
						{
							result.success = false;
							result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
						}

						break;
				}
			}
			return result;
		}

        public MailCreationResult CreatePetitionerVisitNoticeBody(Prenotazione prenotation, List<VisitaProgrammata> visits, bool confirmed, IN_VIAEntities _context)
        {
            MailCreationResult result = new MailCreationResult();
            List<TextPartFilterItem> texts = new List<TextPartFilterItem>();
            LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
           

            string reference = string.Empty;
            string receipt = string.Empty;
            string prenotationDate1 = string.Empty;
            string prenotationDate2 = string.Empty;
            string visitDateShort = string.Empty;
            string visitDateLong = string.Empty;
            string visitHour = string.Empty;
            int visitorsNumber = 0;
            string visitor = string.Empty;
            decimal ticketFullPrice = 0;
            decimal ticketFullPriceTotal = 0;
            decimal ticketReducedPrice = 0;
            decimal ticketReducedPriceTotal = 0;
            string totalPayment = string.Empty;
            string paymentDate = string.Empty;
            string visitLang = string.Empty;
            int confirmDays = 0;
            string closingPart = string.Empty;
            int visitsLanguageCount = 0;
            bool enableOnlinePayment = true;

            TimeSpan dateDiff = new TimeSpan();

            LK_Lingua prenotationLanguage = null;
            LK_Lingua visitLanguage = null;
            LK_User user = null;
            Richiedente petitioner = null;

            Parametri p = null;

            p = _context.Parametris.FirstOrDefault(px => px.Chiave == "biglietto_intero");
            if (p != null)
                decimal.TryParse(p.Valore, out ticketFullPrice);
            
            p = _context.Parametris.FirstOrDefault(px => px.Chiave == "biglietto_ridotto");
            if (p != null)
                decimal.TryParse(p.Valore, out ticketReducedPrice);

            //p = dalParameters.GetItem("onlinePaymentDaysAfterAllowed");
            //if (p != null)
            //    bool.TryParse(p.Valore, out enableOnlinePayment);

            if (prenotation != null)
            {
                //Si ricava il tsto relativo ai giorni di chiusura dall'anno della data di preferenza 
                //impostata nella richiesta di prenotazione (la data "DAL").
                closingPart = "CH" + prenotation.Dt_VisiteDA.Year.ToString();

                //Si ricava il protocollo
                reference = _context.V_Prenotazione.FirstOrDefault(rx => rx.Id_Prenotazione == prenotation.Id_Prenotazione).NProtocollo.ToString();

                //Si ricavano le date di preferenza del richiedente
                prenotationDate1 = prenotation.Dt_VisiteDA != null ? prenotation.Dt_VisiteDA.ToShortDateString() : string.Empty;
                prenotationDate2 = prenotation.Dt_VisiteA != null ? prenotation.Dt_VisiteA.ToShortDateString() : string.Empty;

                //Si ricava il richiedente
                petitioner = _context.Richiedenti.SingleOrDefault(rx => rx.Id_Richiedente == prenotation.Id_Richiedente);

                //Si ricava il visitatore responsabile
                visitor = prenotation.Responsabile;

                //Si ricava l'utente del Vaticano che effettua l'operazione
                user = _context.LK_Users.FirstOrDefault(usx => usx.Id_User == (int)prenotation.Id_User);

                //Si ricava la lingua di risposta della prenotazione
                prenotationLanguage = _context.LK_Lingue.FirstOrDefault(rx => rx.Id_Lingua == prenotation.Id_LinguaRisposta);

                List<Pagamento> payment = _context.Pagamenti.Where(rx => rx.Id_Prenotazione == prenotation.Id_Prenotazione && rx.Fl_Annullato == false).ToList();
                
                if (payment != null)
                {
                    foreach (Pagamento pym in payment)
                    {
                        if (receipt.Length > 0)
                            receipt += ", ";
                        receipt += pym.Ricevuta;
                    }
                }

                //Si ricavano le visite programmate per ricavarne successivamente il numero di lingue presenti nelle visite
                List<V_VisiteProgrammate> vp = _context.V_VisiteProgrammate.Where(vpx => vpx.Id_Prenotazione == prenotation.Id_Prenotazione).ToList();
                                                vp.ForEach(x => x.IsEmpty = false);
                                                vp.ForEach(x => x.IsErasable = true);
                
                if (vp != null)
                    visitsLanguageCount = vp.Select(x => x.Id_Lingua).Count();

            }

            if (visits != null && visits.Count > 0)
            {
                //Data della visita in formato lungo e corto
                visitDateShort = visits[0].Dt_Visita.ToShortDateString();
                visitDateLong = visits[0].Dt_Visita.ToString("D", CultureInfo.CreateSpecificCulture(prenotationLanguage.Simbolo));

                //Dato che tutte le visite si svolgono nello stesso giorno, è sufficiente
                //la prima per ricavare il giorno di qualsiasi visita
                dateDiff = visits[0].Dt_Visita.Date.Subtract(DateTime.Now.Date);
                if (dateDiff.Days < 7)
                    int.TryParse(_context.Parametris.FirstOrDefault(px => px.Chiave == "data_visita_minore_7").Valore, out confirmDays);

                if (dateDiff.Days >= 7 && dateDiff.Days <= 30)
                    int.TryParse(_context.Parametris.FirstOrDefault(px => px.Chiave == "data_visita_fino_30").Valore, out confirmDays);

                if (dateDiff.Days > 30)
                    int.TryParse(_context.Parametris.FirstOrDefault(px => px.Chiave == "data_visita_oltre_30").Valore, out confirmDays);

            }

            if (petitioner == null)
            {
                result.ErrorList += "\n - Manca il richiedente";
                result.success = false;
            }

            if (user == null)
            {
                result.ErrorList += "\n - Manca l'utente che ha registrato la prenotazione";
                result.success = false;
            }

            if (string.IsNullOrEmpty(reference))
            {
                result.ErrorList += "\n - Manca il protocollo della prenotazione";
                result.success = false;
            }

            if (prenotationLanguage == null)
            {
                result.ErrorList += "\n - Manca la lingua di risposta della prenotazione";
                result.success = false;
            }

            if (
                ((TipoRisposta)prenotation.Id_TipoRisposta == TipoRisposta.VisitaProgrammata || prenotation.Id_TipoRisposta == (int)TipoRisposta.ConfermaNoRicevuta)
                && string.IsNullOrEmpty(visitor))
            {
                result.ErrorList += "\n - Manca il nome del capogruppo";
                result.success = false;
            }

            if ((TipoRisposta)prenotation.Id_TipoRisposta == TipoRisposta.NecropoliChiusa && string.IsNullOrEmpty(prenotationDate1))
            {
                result.ErrorList += "\n - Manca la data iniziale della prenotazione";
                result.success = false;
            }

            if ((TipoRisposta)prenotation.Id_TipoRisposta == TipoRisposta.NecropoliChiusa && string.IsNullOrEmpty(prenotationDate2))
            {
                result.ErrorList += "\n - Manca la data finale della prenotazione";
                result.success = false;
            }

            if (result.success)
            {
                int spInteri = 0;
                int spRidotti = 0;
                int totNrInteri = 0;
                int totNrRidotti = 0;
                int totNrOmaggio = 0;

                switch ((TipoRisposta)prenotation.Id_TipoRisposta)
                {
                    case TipoRisposta.RichiestaIncompleta:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "M1", false, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "M2", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "3", false, new List<string> { ticketFullPrice.ToString() }, ";", VariablePosition.End, 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }

                        break;

                    case TipoRisposta.VisitaProgrammata:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "A", 0, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D1", false, new List<string> { "<b>" + reference + "</b>" }, ";", VariablePosition.End, 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D2", false, new List<string> { "<b>" + visitor + "</b>" }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D3", false, new List<string> { "<b>" + visitDateLong + "</b>" }, ";", VariablePosition.End, 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);

                        //Dettagli visita
                        result.Text += "<br /><br />";

                        result.Text += "_______________________________________________________<br /><br />";

                        foreach (VisitaProgrammata item in visits.OrderBy(x => x.Ora_Visita))
                        {
                            texts.Clear();

                            VisitaPrenotata vpr = _context.VisitePrenotate.FirstOrDefault(vpx => vpx.Id_VisitaPrenotata == item.Id_VisitaPrenotata);
                            
                            if (vpr != null)
                            {
                                visitLanguage = _context.LK_Lingue.FirstOrDefault(rx => rx.Id_Lingua == vpr.Id_Lingua);

                                if (visitLanguage != null)
                                {
                                    visitLang = _context.LK_DescrizioneLingua.FirstOrDefault(rx => rx.Id_Lingua == visitLanguage.Id_Lingua && rx.Id_LinguaDescrizione == prenotationLanguage.Id_Lingua).Descrizione;
                                    //visitLang = dalLanguageTranslate.GetItemByLanguageID(visitLanguage.Id_Lingua, prenotationLanguage.Id_Lingua).Descrizione;
                                }
                            }

                            visitHour = !string.IsNullOrEmpty(item.Ora_Visita) ? item.Ora_Visita : string.Empty;

                            visitorsNumber = item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
                            visitorsNumber += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
                            visitorsNumber += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

                            ticketFullPriceTotal += item.Nr_Interi != null ? (decimal)item.Nr_Interi * ticketFullPrice : 0;
                            ticketReducedPriceTotal += item.Nr_Ridotti != null ? (decimal)item.Nr_Ridotti * ticketReducedPrice : 0;
                            totNrInteri += item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
                            totNrRidotti += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
                            totNrOmaggio += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D4", false, new List<string> { "<b>" + visitHour + "</b> - " }, ";", VariablePosition.End, 0, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D5", false, new List<string> { "<b>" + visitorsNumber.ToString() + "</b> - " }, ";", VariablePosition.End, 0, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D6", false, new List<string> { "<b>" + visitLang + "</b>" }, ";", VariablePosition.End, 0, 0));

                            if (texts.Count > 0)
                                result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context, false);

                            result.Text += "<br /><br />";
                        }

                        result.Text += "_______________________________________________________";

                        texts.Clear();

                        spInteri = 0;
                        spRidotti = 0;

                        if (totNrInteri > 0)
                        {
                            spInteri = 2;
                            spRidotti = 1;
                        }
                        else
                            if (totNrRidotti > 0)
                                spRidotti = 2;

                        if (totNrInteri > 0)
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D13", false, new List<string> { "<b>" + ticketFullPrice.ToString() + "</b>" }, ";", VariablePosition.End, spInteri, 0));

                        if (totNrRidotti > 0)
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D14", false, new List<string> { "<b>" + ticketReducedPrice.ToString() + "</b>" }, ";", VariablePosition.End, spRidotti, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                        if (totNrInteri > 0 || totNrRidotti > 0)
                            result.Text += "<br />_______________________________________________________<br />";

                        texts.Clear();

                        if (totNrInteri > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + totNrInteri.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D8", false, new List<string> { "<b>" + ticketFullPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
                        }

                        if (totNrRidotti > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D9", false, new List<string> { "<b>" + totNrRidotti.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D10", false, new List<string> { "<b>" + ticketReducedPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
                        }

                        if (totNrOmaggio > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D11", false, new List<string> { "<b>" + totNrOmaggio.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D12", 0, 0));
                        }

                        result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                        if (totNrInteri > 0 && totNrRidotti > 0)
                        {
                            result.Text += "<br />_______________________________________________________";

                            texts.Clear();

                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + (totNrInteri + totNrRidotti).ToString() + "</b>" }, ";", VariablePosition.End, 2, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D15", false, new List<string> { "<b>" + (ticketFullPriceTotal + ticketReducedPriceTotal).ToString("c") + "</b>" }, ";", VariablePosition.End, 0, 0));
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);
                        }

                        texts.Clear();

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F1", false, new List<string> { confirmDays.ToString() }, ";", VariablePosition.End, 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F2", false, new List<string> { DateTime.Now.Date.ToShortDateString() }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "F3", 0, 0));

                        if (totNrInteri > 0 || totNrRidotti > 0)
                        {
                            //if ((totNrInteri + totNrRidotti + totNrOmaggio) < int.Parse(dalParameters.GetItem("visitatori_no_anticipo").Valore))
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "G", 2, 0));
                            //else
                            //{
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "H", 2, 0));
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I1", false, new List<string> { confirmDays.ToString() }, ";", VariablePosition.End, 2, 0));
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I1b", false, new List<string> { DateTime.Now.Date.ToShortDateString() }, ";", VariablePosition.End, 0, 0));
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I2", false, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I3", false, new List<string> { visitorsNumber.ToString() }, ";", VariablePosition.End, 0, 0));
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I4", false, new List<string> { visitDateLong, visitHour }, " ", VariablePosition.End, 0, 0));
                            //    texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I5", 0, 0));
                            //}

                            int onlinePaymentDaysAfterAllowed = Convert.ToInt32(_context.Parametris.FirstOrDefault(px => px.Chiave == "onlinePaymentDaysAfterAllowed").Valore);
 
                            if (dateDiff.Days <= onlinePaymentDaysAfterAllowed)
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "G", 2, 0));
                            else
                            {
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "H", 2, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I1", false, new List<string> { confirmDays.ToString() }, ";", VariablePosition.End, 2, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I1b", false, new List<string> { DateTime.Now.Date.ToShortDateString() }, ";", VariablePosition.End, 0, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, enableOnlinePayment ? "I2b" : "I2", false, new List<string> { reference }, ";", VariablePosition.End, 0, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I3", false, new List<string> { visitorsNumber.ToString() }, ";", VariablePosition.End, 0, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "I4", false, new List<string> { visitDateLong, visitHour }, " ", VariablePosition.End, 0, 0));
                                texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, enableOnlinePayment ? "I5b" : "I5", 0, 0));
                            }

                        }

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "J", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "C", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + user.Nominativo + (!string.IsNullOrEmpty(user.Titolo) ? "<br />" + user.Titolo + " " : string.Empty) }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }

                        break;

                    case TipoRisposta.ConfermaNoRicevuta:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "A", 0, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D1", false, new List<string> { "<b>" + reference + "</b>" }, ";", VariablePosition.End, 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D2", false, new List<string> { "<b>" + visitor + "</b>" }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D3", false, new List<string> { "<b>" + visitDateLong + "</b>" }, ";", VariablePosition.End, 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);

                        //Dettagli visita
                        result.Text += "<br /><br />";

                        result.Text += "_______________________________________________________<br /><br />";

                        foreach (VisitaProgrammata item in visits.OrderBy(x => x.Ora_Visita))
                        {
                            texts.Clear();

                            VisitaPrenotata vpr = _context.VisitePrenotate.SingleOrDefault(rx => rx.Id_VisitaPrenotata == item.Id_VisitaPrenotata);
                            vpr.IsLoadedFromDb = true;

                            if (vpr != null)
                            {
                                visitLanguage = _context.LK_Lingue.FirstOrDefault(rx => rx.Id_Lingua == vpr.Id_Lingua);

                                if (visitLanguage != null)
                                {
                                    visitLang = _context.LK_DescrizioneLingua.FirstOrDefault(rx => rx.Id_Lingua == visitLanguage.Id_Lingua && rx.Id_LinguaDescrizione == prenotationLanguage.Id_Lingua).Descrizione;
                                    //visitLang = dalLanguageTranslate.GetItemByLanguageID(visitLanguage.Id_Lingua, prenotationLanguage.Id_Lingua).Descrizione;
                                }
                            }

                            visitHour = !string.IsNullOrEmpty(item.Ora_Visita) ? item.Ora_Visita : string.Empty;

                            visitorsNumber = item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
                            visitorsNumber += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
                            visitorsNumber += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

                            ticketFullPriceTotal += item.Nr_Interi != null ? (decimal)item.Nr_Interi * ticketFullPrice : 0;
                            ticketReducedPriceTotal += item.Nr_Ridotti != null ? (decimal)item.Nr_Ridotti * ticketReducedPrice : 0;
                            totNrInteri += item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
                            totNrRidotti += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
                            totNrOmaggio += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D4", false, new List<string> { "<b>" + visitHour + "</b> - " }, ";", VariablePosition.End, 0, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D5", false, new List<string> { "<b>" + visitorsNumber.ToString() + "</b> - " }, ";", VariablePosition.End, 0, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D6", false, new List<string> { "<b>" + visitLang + "</b>" }, ";", VariablePosition.End, 0, 0));

                            if (texts.Count > 0)
                                result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                            result.Text += "<br /><br />";
                        }

                        result.Text += "_______________________________________________________";

                        texts.Clear();

                        spInteri = 0;
                        spRidotti = 0;

                        if (totNrInteri > 0)
                        {
                            spInteri = 2;
                            spRidotti = 1;
                        }
                        else
                            if (totNrRidotti > 0)
                                spRidotti = 2;

                        if (totNrInteri > 0)
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D13", false, new List<string> { "<b>" + ticketFullPrice.ToString() + "</b>" }, ";", VariablePosition.End, spInteri, 0));

                        if (totNrRidotti > 0)
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D14", false, new List<string> { "<b>" + ticketReducedPrice.ToString() + "</b>" }, ";", VariablePosition.End, spRidotti, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                        if (totNrInteri > 0 || totNrRidotti > 0)
                            result.Text += "<br />_______________________________________________________<br />";

                        texts.Clear();

                        if (totNrInteri > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + totNrInteri.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D8", false, new List<string> { "<b>" + ticketFullPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
                        }

                        if (totNrRidotti > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D9", false, new List<string> { "<b>" + totNrRidotti.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D10", false, new List<string> { "<b>" + ticketReducedPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
                        }

                        if (totNrOmaggio > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D11", false, new List<string> { "<b>" + totNrOmaggio.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D12", 0, 0));
                        }

                        result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                        if (totNrInteri > 0 && totNrRidotti > 0)
                        {
                            result.Text += "<br />_______________________________________________________";

                            texts.Clear();

                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + (totNrInteri + totNrRidotti).ToString() + "</b>" }, ";", VariablePosition.End, 2, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D15", false, new List<string> { "<b>" + (ticketFullPriceTotal + ticketReducedPriceTotal).ToString("c") + "</b>" }, ";", VariablePosition.End, 0, 0));
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);
                        }

                        texts.Clear();

                        if (totNrInteri > 0 || totNrRidotti > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "G", 2, 0));
                        }

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "B", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "J", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "C", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + user.Nominativo + (!string.IsNullOrEmpty(user.Titolo) ? "<br />" + user.Titolo + " " : string.Empty) }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }

                        break;

                    case TipoRisposta.ConfermaRicevuta:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "A", 0, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D1", false, new List<string> { reference }, ";", VariablePosition.End, 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D2", false, new List<string> { visitor }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D3", false, new List<string> { visitDateLong }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "020", false, new List<string> { receipt }, ";", VariablePosition.End, 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);

                        //Dettagli visita
                        result.Text += "<br /><br />";

                        totNrInteri = 0;
                        totNrRidotti = 0;
                        totNrOmaggio = 0;

                        result.Text += "_______________________________________________________<br /><br />";

                        foreach (VisitaProgrammata item in visits.OrderBy(x => x.Ora_Visita))
                        {
                            texts.Clear();

                            //VisitaPrenotata vpr = dalPrenotatedVisit.GetItem(item.Id_VisitaPrenotata);
                            //if (vpr != null)
                            //{
                            //    visitLanguage = dalLanguage.GetItem(vpr.Id_Lingua);
                            //    if (visitLanguage != null)
                            //        visitLang = dalLanguageTranslate.GetItemByLanguageID(visitLanguage.Id_Lingua, prenotationLanguage.Id_Lingua).Descrizione;
                            //}

                            VisitaPrenotata vpr = _context.VisitePrenotate.FirstOrDefault(vpx => vpx.Id_VisitaPrenotata == item.Id_VisitaPrenotata);

                            if (vpr != null)
                            {
                                visitLanguage = _context.LK_Lingue.FirstOrDefault(rx => rx.Id_Lingua == vpr.Id_Lingua);

                                if (visitLanguage != null)
                                    visitLang = _context.LK_DescrizioneLingua.FirstOrDefault(rx => rx.Id_Lingua == visitLanguage.Id_Lingua && rx.Id_LinguaDescrizione == prenotationLanguage.Id_Lingua).Descrizione;
                            }
                            
                            
                            visitHour = !string.IsNullOrEmpty(item.Ora_Visita) ? item.Ora_Visita : string.Empty;

                            visitorsNumber = item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
                            visitorsNumber += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
                            visitorsNumber += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

                            ticketFullPriceTotal += item.Nr_Interi != null ? (decimal)item.Nr_Interi * ticketFullPrice : 0;
                            ticketReducedPriceTotal += item.Nr_Ridotti != null ? (decimal)item.Nr_Ridotti * ticketReducedPrice : 0;
                            totNrInteri += item.Nr_Interi != null ? (int)item.Nr_Interi : 0;
                            totNrRidotti += item.Nr_Ridotti != null ? (int)item.Nr_Ridotti : 0;
                            totNrOmaggio += item.Nr_Omaggio != null ? (int)item.Nr_Omaggio : 0;

                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D4", false, new List<string> { "<b>" + visitHour + "</b> - " }, ";", VariablePosition.End, 0, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D5", false, new List<string> { "<b>" + visitorsNumber.ToString() + "</b> - " }, ";", VariablePosition.End, 0, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D6", false, new List<string> { "<b>" + visitLang + "</b>" }, ";", VariablePosition.End, 0, 0));

                            if (texts.Count > 0)
                                result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                            result.Text += "<br /><br />";
                        }

                        result.Text += "_______________________________________________________";

                        texts.Clear();

                        if (totNrInteri > 0)
                        {
                            spInteri = 2;
                            spRidotti = 1;
                        }
                        else
                            if (totNrRidotti > 0)
                                spRidotti = 2;

                        if (totNrInteri > 0)
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D13", false, new List<string> { "<b>" + ticketFullPrice.ToString() + "</b>" }, ";", VariablePosition.End, spInteri, 0));

                        if (totNrRidotti > 0)
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D14", false, new List<string> { "<b>" + ticketReducedPrice.ToString() + "</b>" }, ";", VariablePosition.End, spRidotti, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                        if (totNrInteri > 0 || totNrRidotti > 0)
                            result.Text += "<br />_______________________________________________________<br />";

                        texts.Clear();

                        if (totNrInteri > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + totNrInteri.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D8", false, new List<string> { "<b>" + ticketFullPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
                        }

                        if (totNrRidotti > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D9", false, new List<string> { "<b>" + totNrRidotti.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D10", false, new List<string> { "<b>" + ticketReducedPriceTotal.ToString() + "</b>" }, ";", VariablePosition.End, 0, 0));
                        }

                        if (totNrOmaggio > 0)
                        {
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D11", false, new List<string> { "<b>" + totNrOmaggio.ToString() + "</b>" }, ";", VariablePosition.End, 1, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D12", 0, 0));
                        }

                        result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);

                        if (totNrInteri > 0 && totNrRidotti > 0)
                        {
                            result.Text += "<br />_______________________________________________________<br />";

                            texts.Clear();

                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D7", false, new List<string> { "<b>" + (totNrInteri + totNrRidotti).ToString() + "</b>" }, ";", VariablePosition.End, 2, 0));
                            texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "D15", false, new List<string> { "<b>" + (ticketFullPriceTotal + ticketReducedPriceTotal).ToString("c") + "</b>" }, ";", VariablePosition.End, 0, 0));
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);
                        }

                        texts.Clear();

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "B", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "C", 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts,_context, false);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }

                        break;

                    case TipoRisposta.AnnullataDaUfficioScavi:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "O", 0, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "3", false, new List<string> { ticketFullPrice.ToString() }, ";", VariablePosition.End, 2, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }
                        break;

                    case TipoRisposta.NecropoliChiusa:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "P1", false, new List<string> { prenotationDate1 }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "P2", false, new List<string> { prenotationDate2 }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "P3", 0, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }
                        break;

                    case TipoRisposta.VisiteComplete:
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "N1", false, new List<string> { prenotationDate1 }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "N2", false, new List<string> { prenotationDate2 }, ";", VariablePosition.End, 0, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "N3", 0, 0));

                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1A", 2, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, closingPart, false, new List<string> { "<br /><br />" + (!string.IsNullOrEmpty(user.Titolo) ? user.Titolo + " " : string.Empty) + user.Nominativo }, ";", VariablePosition.End, 1, 0));
                        texts.Add(new TextPartFilterItem(prenotationLanguage.Id_Lingua, "1B", 1, 0));

                        if (texts.Count > 0)
                            result.Text += dalText.GetBody(prenotationLanguage.Id_Lingua, (int)prenotation.Id_TipoRisposta, petitioner.Id_Richiedente, texts, _context);
                        else
                        {
                            result.success = false;
                            result.ErrorList += "\nSi è verificato un errore nella composizione del corpo dell'email.\nLa lavorazione non è stata salvata oppure non sono presenti visite per questo protocollo (la richiesta non ha un tipo di risposta valido).";
                        }

                        break;
                }
            }
            return result;
        }

        #region Testi Per Tipo Risposta
        public void GetSubjectAndBody(int languageID, int responseTypeID,out string subject, out string body)
        {
            LK_TestoStandard_Dal dalText = new LK_TestoStandard_Dal();
            subject = "";
            body = "";

            LK_TestoRisposta tr = dalText.GetTestoRisposta(languageID, responseTypeID);
            subject = tr.Testo;
            body = tr.Corpo;
            
        }
        #endregion
	}

	public class MailCreationResult
	{
		#region Public Properties

		public string Text { get; set; }
		public string ErrorList { get; set; }
		public bool success { get; set; }

		#endregion// Public Properties

		public MailCreationResult()
		{
			Text = string.Empty;
			ErrorList = string.Empty;
			success = true;
		}

	}
}

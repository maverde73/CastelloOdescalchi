using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace Scv_Dal
{
    public class EasyTicketing_DAL
    {
        public List<LK_TipoVisita> GetVisitTypes()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<LK_TipoVisita> ItemsList = new List<LK_TipoVisita>();

                ItemsList = _context.LK_TipiVisita.OrderBy(vx => vx.Ordine).ToList();

                ItemsList.ForEach(tvx => tvx.Descrizione = tvx.Descrizione + " (" + tvx.Simbolo + ")");

                return ItemsList;
            }
        }


        public void AddNewVisit(DateTime dataVisita,
                                int idTipoVisita,
                                VisitaProgrammata visitaProgrammata,
                                Pagamento pagamento,
                                int idLingua,
                                string cognome,
                                string nome,
                                string email,
                                int idUtente,
                                int? idRichiedente,
                                out string protocollo,
                                out int idVisitaProgrammata)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        Prenotazione prenotazione = new Prenotazione();

                        prenotazione.Dt_VisiteDA = dataVisita;
                        prenotazione.Dt_VisiteA = dataVisita;
                        prenotazione.Id_TipoVisita = idTipoVisita;
                        prenotazione.Fl_AM = true;
                        prenotazione.Fl_PM = true;
                        prenotazione.Id_LinguaRisposta = idLingua;
                        prenotazione.Tot_Visitatori = (short)((visitaProgrammata.Nr_Interi != null ? (short)visitaProgrammata.Nr_Interi : 0) +
                                                      (visitaProgrammata.Nr_Ridotti != null ? (short)visitaProgrammata.Nr_Ridotti : 0) +
                                                      (visitaProgrammata.Nr_Scontati != null ? (short)visitaProgrammata.Nr_Scontati : 0) +
                                                      (visitaProgrammata.Nr_Cumulativi != null ? (short)visitaProgrammata.Nr_Cumulativi : 0) +
                                                      (visitaProgrammata.Nr_Omaggio != null ? (short)visitaProgrammata.Nr_Omaggio : 0));
                        prenotazione.Id_TipoRisposta = 3;
                        prenotazione.Id_TipoConferma = 1;
                        prenotazione.Dt_Prenotazione = dataVisita;
                        prenotazione.Protocollo = Helper_Dal.GetNewProgressive("P", prenotazione.Dt_Prenotazione.Year).Progr_UltimoUscito.ToString();

                        Richiedente richiedente = null;
                        prenotazione.Id_User = idUtente;
                        if (idRichiedente == null)
                        {
                            richiedente = new Richiedente();
                            richiedente.Cognome = "da definire";
                            richiedente.Nome = "da definire";
                            richiedente.Email = "dadefinire@ddd.it";
                            richiedente.Id_User = idUtente;
                            richiedente.Dt_Update = DateTime.Now;
                            richiedente.Id_LinguaAbituale = idLingua;
                            _context.Richiedenti.AddObject(richiedente);
                            _context.SaveChanges();
                            idRichiedente = richiedente.Id_Richiedente;
                        }
                        else
                            richiedente = _context.Richiedenti.FirstOrDefault(rx => rx.Id_Richiedente == idRichiedente);

                        if (!string.IsNullOrEmpty(richiedente.Nome))
                            prenotazione.Responsabile = richiedente.Cognome + " " + richiedente.Nome;
                        else
                            prenotazione.Responsabile = richiedente.Cognome;
                        prenotazione.Id_Richiedente = (int)idRichiedente;
                        _context.Prenotazioni.AddObject(prenotazione);
                        _context.SaveChanges();

                        LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
                        LK_Progressivi pr = dalPR.GetSingleItem("P")[0];

                        if (pr != null)
                        {
                            if (pr.Anno != prenotazione.Dt_Prenotazione.Year)
                            {
                                pr.Anno = pr.Anno > 0 ? prenotazione.Dt_Prenotazione.Year : 0;
                                pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
                            }
                            else
                                pr.Progr_UltimoUscito++;


                            if (pr.Tipo != string.Empty)
                                _context.AttachUpdated(pr);
                            else
                                _context.LK_Progressivi.AddObject(pr);

                            _context.SaveChanges();
                        }

                        VisitaPrenotata visitaPrenotata = new VisitaPrenotata();
                        visitaPrenotata.Id_Prenotazione = prenotazione.Id_Prenotazione;
                        visitaPrenotata.Id_TipoVisita = idTipoVisita;
                        visitaPrenotata.Dt_Update = DateTime.Now;
                        visitaPrenotata.Id_Lingua = idLingua;
                        visitaPrenotata.Nr_Visitatori = prenotazione.Tot_Visitatori;
                        _context.VisitePrenotate.AddObject(visitaPrenotata);
                        _context.SaveChanges();

                        visitaProgrammata.Id_VisitaPrenotata = visitaPrenotata.Id_VisitaPrenotata;
                        visitaProgrammata.Id_User = idUtente;
                        visitaProgrammata.Dt_Update = DateTime.Now;
                        visitaProgrammata.Dt_Visita = dataVisita;
                        _context.VisiteProgrammate.AddObject(visitaProgrammata);

                        pagamento.Id_Prenotazione = prenotazione.Id_Prenotazione;
                        pagamento.Dt_Pagamento = DateTime.Now;
                        pagamento.Dt_Update = DateTime.Now.Date;

                        pr = dalPR.GetSingleItem("R")[0];

                        if (pr != null)
                        {
                            if (pr.Anno != pagamento.Dt_Pagamento.Value.Year)
                            {
                                pr.Anno = pr.Anno > 0 ? pagamento.Dt_Pagamento.Value.Year : 0;
                                pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
                            }
                            else
                                pr.Progr_UltimoUscito++;

                            if (pr.Tipo != string.Empty)
                                _context.AttachUpdated(pr);
                            else
                                _context.LK_Progressivi.AddObject(pr);
                        }

                        pagamento.Ricevuta = pr.Anno == 0 ? pr.Progr_UltimoUscito.ToString() : (pr.Anno.ToString() + "/") + pr.Progr_UltimoUscito.ToString();

                        _context.Pagamenti.AddObject(pagamento);
                        _context.SaveChanges();

                        transaction.Commit();

                        protocollo = prenotazione.Protocollo;
                        idVisitaProgrammata = visitaProgrammata.Id_VisitaProgrammata;
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

        public void UpdateVisit(int idPrenotazione,
                                DateTime dataVisita,
                                int idTipoVisita,
                                VisitaProgrammata visitaProgrammata,
                                Pagamento pagamento,
                                int idLingua,
                                string cognome,
                                string nome,
                                string email,
                                int idUtente)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {

                        Prenotazione prenotazione = _context.Prenotazioni.FirstOrDefault(px => px.Id_Prenotazione == idPrenotazione);

                        prenotazione.Dt_VisiteDA = dataVisita;
                        prenotazione.Dt_VisiteA = dataVisita;
                        prenotazione.Id_TipoVisita = idTipoVisita;
                        prenotazione.Id_LinguaRisposta = idLingua;
                        prenotazione.Tot_Visitatori = (short)((visitaProgrammata.Nr_Interi != null ? (short)visitaProgrammata.Nr_Interi : 0) +
                                                      (visitaProgrammata.Nr_Ridotti != null ? (short)visitaProgrammata.Nr_Ridotti : 0) +
                                                      (visitaProgrammata.Nr_Scontati != null ? (short)visitaProgrammata.Nr_Scontati : 0) +
                                                      (visitaProgrammata.Nr_Cumulativi != null ? (short)visitaProgrammata.Nr_Cumulativi : 0) +
                                                      (visitaProgrammata.Nr_Omaggio != null ? (short)visitaProgrammata.Nr_Omaggio : 0));

                        _context.AttachUpdated(visitaProgrammata);
                        _context.SaveChanges();

                        if (pagamento != null)
                        {
                            pagamento.Id_Prenotazione = prenotazione.Id_Prenotazione;
                            pagamento.Dt_Pagamento = DateTime.Now;
                            pagamento.Dt_Update = DateTime.Now.Date;

                            LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
                            LK_Progressivi pr = dalPR.GetSingleItem("P")[0];

                            pr = dalPR.GetSingleItem("R")[0];

                            if (pr != null)
                            {
                                if (pr.Anno != pagamento.Dt_Pagamento.Value.Year)
                                {
                                    pr.Anno = pr.Anno > 0 ? pagamento.Dt_Pagamento.Value.Year : 0;
                                    pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
                                }
                                else
                                    pr.Progr_UltimoUscito++;

                                if (pr.Tipo != string.Empty)
                                    _context.AttachUpdated(pr);
                                else
                                    _context.LK_Progressivi.AddObject(pr);
                            }

                            pagamento.Ricevuta = pr.Anno == 0 ? pr.Progr_UltimoUscito.ToString() : (pr.Anno.ToString() + "/") + pr.Progr_UltimoUscito.ToString();

                            _context.Pagamenti.AddObject(pagamento);
                            _context.SaveChanges();
                        }

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


        public void UpdatePrinted(int idVisitaProgrammata, int lastProgTicket)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var visitaProgrammata = _context.VisiteProgrammate.FirstOrDefault(vpx => vpx.Id_VisitaProgrammata == idVisitaProgrammata);

                if (visitaProgrammata.Nr_Interi != null)
                    visitaProgrammata.Nr_InteriConsegnati = (short)visitaProgrammata.Nr_Interi;

                if (visitaProgrammata.Nr_Ridotti != null)
                    visitaProgrammata.Nr_RidottiConsegnati = (short)visitaProgrammata.Nr_Ridotti;

                if (visitaProgrammata.Nr_Scontati != null)
                    visitaProgrammata.Nr_ScontatiConsegnati = (short)visitaProgrammata.Nr_Scontati;

                if (visitaProgrammata.Nr_Cumulativi != null)
                {
                    //visitaProgrammata.Nr_CumulativiConsegnati = (short)visitaProgrammata.Nr_Cumulativi;
                    if (((short)visitaProgrammata.Nr_Cumulativi) > 0)
                    {
                        visitaProgrammata.Nr_CumulativiConsegnati = 1;
                    }
                    //visitaProgrammata.Nr_CumulativiConsegnati = ((short)visitaProgrammata.Nr_Cumulativi) ? ;
                }


                var lastProg = _context.LK_Progressivi.FirstOrDefault(px => px.Tipo == "TK");
                lastProg.Progr_UltimoUscito = lastProgTicket;

                _context.SaveChanges();
            }
        }

        public bool DeletePrenotation(int idPrenotazione)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ObjectParameter retMessage = new ObjectParameter("RETMESSAGE", typeof(String));
                _context.SP_DELETE_CASCADE("dbo","Prenotazione",string.Format("Id_Prenotazione = {0}",idPrenotazione.ToString()),retMessage);
                return (retMessage.Value.ToString() == "OK");
            }
        }

    }
}

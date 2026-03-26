using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using System.Collections.ObjectModel;
using Scv_Model;

namespace Scv_Dal
{
	public class Mandato_Dal
	{
		public bool ExistMandato(DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return (_context.Mandatoes.First(x => x.Dt_Mandato == date)) != null;
			}
		}

		public DateTime? GetLastDateMandato(DateTime? exludeDate)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<Mandato> list = new List<Mandato>();

				list.AddRange(_context.Mandatoes);

				if (list.Count > 0)
					if (exludeDate == null)
						return (list.Max(x => x.Dt_Mandato));
					else
					{
                        if (exludeDate == list.Max(x => x.Dt_Mandato))
                            return Convert.ToDateTime(exludeDate);

      					list = list.Where(x => x.Dt_Mandato != exludeDate).ToList();
						if (list.Count > 0)
							return (list.Max(x => x.Dt_Mandato));
						else
							return (DateTime?)null;
					}
				else
					return (DateTime?)null;
			}
		}

		public Mandato GetMandato(DateTime date)
		{
			Mandato obj = null;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{

						//Si ricava eventuale mandato sulla base di 'date'
						obj = _context.Mandatoes.FirstOrDefault(x => x.Dt_Mandato == date);

						//Se il mandato è stato trovato si procede con le altre operazioni
						if (obj != null)
						{
							//calcolo biglietto A
							DateTime dt = date.Date;
							UltimoNumeroBiglietto u = _context.UltimoNumeroBigliettoes.FirstOrDefault(x => x.Dt_Numero == dt);
							//Se per qualche ragione non esiste il record dell'ultimo numero biglietto corrispondente alla data del mandato
							//e il NumeroA dell'attuale mandato è nullo o zero, si calcolano i biglietti venduti e si ripristina
							//il record
							if (u == null)
							{
								int NrBigliettiTotale = 0;
								//Si ottengono le visite in data creazione mandato (oggi)
								List<V_VisiteProgrammate> visits = _context.V_VisiteProgrammate.Where(x => x.Dt_Visita == dt).ToList();

								//Si itera fra le visite programmate e si sommanio i biglietti consegnati
								foreach (V_VisiteProgrammate vp in visits)
								{
									NrBigliettiTotale += (vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0);
									NrBigliettiTotale += (vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0);
									NrBigliettiTotale += (vp.Nr_OmaggioConsegnati != null ? (int)vp.Nr_OmaggioConsegnati : 0);
                                    NrBigliettiTotale += (vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0);
                                    NrBigliettiTotale += (vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0);
								}

								u = new UltimoNumeroBiglietto();
								u.Dt_Numero = dt;
								u.Nr = ((int)obj.Nr_BigliettoDa + NrBigliettiTotale) - 1;

								_context.UltimoNumeroBigliettoes.AddObject(u);
								_context.SaveChanges();
							}
							//Si assegna il numero biglietto A
							obj.Nr_BigliettoA = u.Nr;

							//aggiornamento mandato
							_context.AttachUpdated(obj);

							//eliminazione record ultimobiglietto
							_context.UltimoNumeroBigliettoes.DeleteObject(u);

							_context.SaveChanges();

							transaction.Commit();
						}
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
			
			return obj;
		}

		public ObservableCollection<MandatoDettaglio> GetMandatoDettaglioList(int idMandato, DateTime dataMandato)
		{
			ObservableCollection<MandatoDettaglio> list = new ObservableCollection<MandatoDettaglio>();
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				Mandato mnd = GetMandato(dataMandato);
				if(mnd != null)
				{
					list = new ObservableCollection<MandatoDettaglio>(_context.MandatoDettaglios.Where(x => x.Id_Mandato == idMandato));
					foreach (MandatoDettaglio item in list)
					{
						foreach (MandatoStruttura str in _context.MandatoStrutturas)
						{
							if (str.Taglio == item.Taglio)
							{
								item.IsValueReadOnly = DateTime.Now.Date == mnd.Dt_Mandato ? !str.Fl_ValoreEditabile : true;
								item.IsNumberReadOnly = DateTime.Now.Date != mnd.Dt_Mandato;

								decimal taglio = 0;
								decimal.TryParse(item.Taglio.Replace(".", ","), out taglio);
								if (item.IsValueReadOnly && taglio > 0)
								{
									item.Valore = item.Numero * taglio;
									item.ValidateValue = false;
								}
								else
								{
									//item.ValidateValue = true;
								}
							}
						}
					}
				}
			}

			return list;
		}

        public int InsertOrUpdate(Mandato master, List<MandatoDettaglio> details, DateTime dt, bool onlinePaymentEnabled = false, bool refreshOnlinePayments = false)
		{
			int id = 0;
			LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
			LK_Progressivi pr = null;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						bool updating = false;

						//Creazione mandato
						if (master.Id_Mandato != 0)
						{
							updating = true;
						    _context.AttachUpdated(master);
						}
						else
						{
							//Calcolo biglietto DA
							master.Nr_BigliettoDa = 1;//temporaneo
							//Si trova la data precedente a questo mandato
							DateTime? previousObjDate = GetLastDateMandato(DateTime.Now.Date);
							if (previousObjDate != null)
							{
								//Se esiste si cerca il mandato con la data precedente trovata
								Mandato previousObj = _context.Mandatoes.FirstOrDefault(x => x.Dt_Mandato == previousObjDate);
								if (previousObj != null)
								{
									//Se il mandato precedente esiste, e ha un "bigliettoA" valido, si assegna
									//al "BigliettoDA" del nuovo mandato, maggiorato di 1. Altrimenti si assegna  al nuovo mandato
									if (previousObj.Nr_BigliettoA != null)
										master.Nr_BigliettoDa = previousObj.Nr_BigliettoA + 1;
								}
							}

							//calcolo biglietto A
							UltimoNumeroBiglietto u = _context.UltimoNumeroBigliettoes.FirstOrDefault(x => x.Dt_Numero == dt);
							//Se per qualche ragione non esiste il record dell'ultimo numero biglietto corrispondente alla data del mandato
							//e il NumeroA dell'attuale mandato è nullo o zero, si calcolano i biglietti venduti e si ripristina
							//il record
							if (u == null)
							{
								int NrBigliettiTotale = 0;
								//Si ottengono le visite in data creazione mandato (oggi)
								List<V_VisiteProgrammate> visits = _context.V_VisiteProgrammate.Where(x => x.Dt_Visita == dt).ToList();
							
								//Si itera fra le visite programmate e si sommanio i biglietti consegnati
								foreach (V_VisiteProgrammate vp in visits)
								{
									NrBigliettiTotale += (vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0);
									NrBigliettiTotale += (vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0);
									NrBigliettiTotale += (vp.Nr_OmaggioConsegnati != null ? (int)vp.Nr_OmaggioConsegnati : 0);
                                    NrBigliettiTotale += (vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0);
                                    NrBigliettiTotale += (vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0);
								}

								u = new UltimoNumeroBiglietto();
								u.Dt_Numero = dt;
								u.Nr = ((int)master.Nr_BigliettoDa + NrBigliettiTotale) - 1;

								_context.UltimoNumeroBigliettoes.AddObject(u);
								_context.SaveChanges();





							}
							//Si assegna il numero biglietto A
							master.Nr_BigliettoA = u.Nr;

							_context.Mandatoes.AddObject(master);

							//eliminazione record ultimobiglietto
							_context.UltimoNumeroBigliettoes.DeleteObject(u);
							//_context.SaveChanges();
						}

						_context.SaveChanges();

						id = master.Id_Mandato;

						if (!updating)
						{
							//Progressivo mandato
							pr = _context.LK_Progressivi.FirstOrDefault(rx => rx.Tipo == "MA");

							if (pr != null)
							{
								if (pr.Anno != master.Dt_Mandato.Year)
								{
									pr.Anno = pr.Anno > 0 ? master.Dt_Mandato.Year : 0;
									pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
								}
								else
									pr.Progr_UltimoUscito++;

								if (pr.Tipo != string.Empty)
									_context.AttachUpdated(pr);
								else
									_context.LK_Progressivi.AddObject(pr);

								_context.SaveChanges();

								//aggiornamento numero mandato
								master.Nr_Mandato = pr.Progr_UltimoUscito;
								_context.AttachUpdated(master);
								_context.SaveChanges();
							}

							//Creazione nuovi dettagli
							MandatoDettaglio md = null;
							foreach (MandatoStruttura str in _context.MandatoStrutturas.ToList())
							{
								md = new MandatoDettaglio();
								md.Id_Mandato = master.Id_Mandato;
								md.Taglio = str.Taglio;

								_context.MandatoDettaglios.AddObject(md);

								_context.SaveChanges();
							}

                            if (onlinePaymentEnabled)
                            {
                                //Assegnazione al nuovo mandato dei pagamenti online
                                //non ancora inclusi in un mandato
                                var idPagamenti = _context.V_ImportoPagamento.Where(x => x.Dt_Pagamento != null && x.Dt_Pagamento <= master.Dt_Mandato && x.Id_TipoPagamento == 1 && x.Id_Mandato == null).Select(x => x.Id_Pagamento).ToList();

                                foreach (int item in idPagamenti)
                                {
                                    var pagamento = _context.Pagamenti.FirstOrDefault(px => px.Id_Pagamento == item);
                                    pagamento.Id_Mandato = master.Id_Mandato;
                                    _context.SaveChanges();
                                }
                            }

						}
						else
						{
                            if (refreshOnlinePayments)
                            {
                                if (onlinePaymentEnabled)
                                {
                                    //Assegnazione al nuovo mandato dei pagamenti online
                                    //non ancora inclusi in un mandato
                                    var idPagamenti = _context.V_ImportoPagamento.Where(x => x.Dt_Pagamento != null && x.Dt_Pagamento <= master.Dt_Mandato && x.Id_TipoPagamento == 1 && x.Id_Mandato == null).Select(x => x.Id_Pagamento).ToList();
                                    foreach (int item in idPagamenti)
                                    {
                                        var pagamento = _context.Pagamenti.FirstOrDefault(px => px.Id_Pagamento == item);
                                        pagamento.Id_Mandato = master.Id_Mandato;
                                        _context.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                foreach (MandatoDettaglio md in details)
                                {
                                    MandatoDettaglio m = _context.MandatoDettaglios.FirstOrDefault(x => x.Id_MandatoDettagli == md.Id_MandatoDettagli);
                                    m.Numero = md.Numero;
                                    m.Valore = md.Valore;
                                    _context.AttachUpdated(m);
                                    _context.SaveChanges();
                                }
                            }
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

			return id;
		}

		public PrintMandatoArgs GetTotaleBiglietti(PrintMandatoArgs obj, DateTime date)
		{
			decimal prezzoIntero = 0;
			decimal prezzoRidotto = 0;
            decimal prezzoScontato = 0;
            decimal prezzoCumulativo = 0;

			Parametri_Dal dalParametri = new Parametri_Dal();

            //decimal.TryParse(dalParametri.GetItem("biglietto_intero").Valore, out prezzoIntero);
            //decimal.TryParse(dalParametri.GetItem("biglietto_ridotto").Valore, out prezzoRidotto);

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_VisiteProgrammate> visits = _context.V_VisiteProgrammate.Where(x => x.Dt_Visita == date).ToList();
				
				
				obj.NrVisite = visits.Count;

				foreach (V_VisiteProgrammate vp in visits)
				{
                    prezzoIntero = 0;
                    prezzoRidotto = 0;
                    prezzoScontato = 0;
                    prezzoCumulativo = 0;

                    var prenotazione = _context.Prenotazioni.FirstOrDefault(px => px.Id_Prenotazione == vp.Id_Prenotazione);
                    var tipoVisita = _context.LK_TipiVisita.FirstOrDefault(tv => tv.Id_TipoVisita == prenotazione.Id_TipoVisita);

                    if (tipoVisita.PrezzoIntero != null)
                        prezzoIntero = (decimal)tipoVisita.PrezzoIntero;

                    if (tipoVisita.PrezzoRidotto != null)
                        prezzoRidotto = (decimal)tipoVisita.PrezzoRidotto;

                    if (tipoVisita.PrezzoScontato != null)
                        prezzoScontato = (decimal)tipoVisita.PrezzoScontato;

                    if (tipoVisita.PrezzoCumulativo != null)
                        prezzoCumulativo = (decimal)tipoVisita.PrezzoCumulativo;


					obj.NrVisitatori +=
						(
							(vp.Nr_Interi != null ? (int)vp.Nr_Interi : 0)
							+
							(vp.Nr_Ridotti != null ? (int)vp.Nr_Ridotti : 0)
							+
							(vp.Nr_Omaggio != null ? (int)vp.Nr_Omaggio : 0)
                            +
                            (vp.Nr_Scontati != null ? (int)vp.Nr_Scontati : 0)
                            +
                            (vp.Nr_Cumulativi != null ? (int)vp.Nr_Cumulativi : 0)
						);

					obj.NrBigliettiInteriTotale += (vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0);
					obj.NrBigliettiRidottiTotale += (vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0);
					obj.NrBigliettiOmaggioTotale += (vp.Nr_OmaggioConsegnati != null ? (int)vp.Nr_OmaggioConsegnati : 0);
                    obj.NrBigliettiScontatiTotale += (vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0);
                    obj.NrBigliettiCumulativiTotale += (vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0);

                  
                    obj.BigliettiInteriTotale += ((vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0) * prezzoIntero);
                    obj.BigliettiRidottiTotale += ((vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0) * prezzoRidotto);
                    obj.BigliettiScontatiTotale += ((vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0) * prezzoScontato);
                    obj.BigliettiCumulativiTotale += prezzoCumulativo;
				}

                obj.NrBigliettiTotale = (obj.NrBigliettiInteriTotale + obj.NrBigliettiRidottiTotale + obj.NrBigliettiOmaggioTotale + obj.NrBigliettiScontatiTotale + obj.NrBigliettiCumulativiTotale);

                obj.BigliettiTotale = obj.BigliettiInteriTotale + obj.BigliettiRidottiTotale + obj.BigliettiScontatiTotale + obj.BigliettiCumulativiTotale;
				obj.NrPrenotatiNonVenuti = obj.NrVisitatori - obj.NrBigliettiTotale;

				Mandato mnd = _context.Mandatoes.FirstOrDefault(x => x.Dt_Mandato == date);
				if(mnd != null)
				{
					obj.NrBigliettiDa = mnd.Nr_BigliettoDa != null ? (int)mnd.Nr_BigliettoDa : 0;
					obj.NrBigliettiA = mnd.Nr_BigliettoA != null ? (int)mnd.Nr_BigliettoA : 0;
				}
			}

			return obj;
		}

		public PrintMandatoArgs GetBigliettiIncassati(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<int> prenotationsIDs = new List<int>(); //raggruppa le prenotazioni

				decimal importo = 0;

				List<V_ImportoPagamento> ips = _context.V_ImportoPagamento.Where(x => x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento != null && x.Dt_Pagamento == date).ToList();
				foreach (V_ImportoPagamento ip in ips)
				{
                    importo = (ip.Importo_Interi != null ? (decimal)ip.Importo_Interi : 0) + (ip.Importo_Ridotti != null ? (decimal)ip.Importo_Ridotti : 0) + (ip.Importo_Scontati != null ? (decimal)ip.Importo_Scontati : 0) + (ip.Importo_Cumulativi != null ? (decimal)ip.Importo_Cumulativi : 0);
					obj.BigliettiIncassatiTotaleContanti += ip.Id_TipoPagamento == 3 ? importo : 0;
					obj.BigliettiIncassatiTotalePos += ip.Id_TipoPagamento == 4 ? importo : 0;
					obj.BigliettiIncassatiTotaleAssegno += ip.Id_TipoPagamento == 5 ? importo : 0;
				}
			}

			obj.BigliettiIncassatiTotale =
				obj.BigliettiIncassatiTotaleContanti
				+
				obj.BigliettiIncassatiTotalePos
				+
				obj.BigliettiIncassatiTotaleAssegno
				;

			return obj;
		}

		public PrintMandatoArgs GetBigliettiRimborsati(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_Mandato> movements = _context.V_Mandato.Where(x => x.Dt_Movimento == date & x.Id_TipoMovimento == 14 && x.Id_TipoPagamento == 3).ToList();

				if (movements.Count > 0)
				{
					obj.Rimborso1Valore = movements[0].PrezzoVendita != null ? (decimal)movements[0].PrezzoVendita : 0;
					obj.Rimborso1Nota = movements[0].Nota != null ? movements[0].Nota.Length > 29 ? movements[0].Nota.Substring(0, 30) : movements[0].Nota : string.Empty;
				}
				if (movements.Count > 1)
				{
					obj.Rimborso2Valore = movements[1].PrezzoVendita != null ? (decimal)movements[1].PrezzoVendita : 0;
					obj.Rimborso2Nota = movements[1].Nota != null ? movements[1].Nota.Length > 29 ? movements[1].Nota.Substring(0, 30) : movements[1].Nota : string.Empty;
				}
				if (movements.Count > 2)
				{
					obj.Rimborso3Valore = movements[2].PrezzoVendita != null ? (decimal)movements[2].PrezzoVendita : 0;
					obj.Rimborso3Nota = movements[2].Nota != null ? movements[2].Nota.Length > 29 ? movements[2].Nota.Substring(0, 30) : movements[2].Nota : string.Empty;
				}

				return obj;
			}
		}

		public PrintMandatoArgs GetVenditaArticoli(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<Movimento> movements = _context.Movimenti
					.Where(x => 
						x.Dt_Movimento == date 
						&&
						x.Id_TipoMovimento == 3 //Vendita a privati
						).ToList();
				foreach (Movimento mv in movements)
				{
					obj.VenditaArticoliTotaleContanti += mv.Id_TipoPagamento == 3 ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : 0;
					obj.VenditaArticoliTotalePos += mv.Id_TipoPagamento == 4 ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : 0;
					obj.VenditaArticoliTotaleAssegno += mv.Id_TipoPagamento == 5 ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : 0;
				}

				obj.VenditaArticoliTotale = obj.VenditaArticoliTotaleContanti + obj.VenditaArticoliTotalePos + obj.VenditaArticoliTotaleAssegno;
			}

			return obj;
		}

		public PrintMandatoArgs GetAltroArticoli(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<Movimento> movements = _context.Movimenti
					.Where(x =>	
						x.Dt_Movimento == date.Date
						&&
						(
							x.Id_TipoMovimento == 9 //compenso guide (in)
						)
						).ToList();
				foreach (Movimento mv in movements)
				{
					if ((mv.Id_TipoMovimento == 8 || mv.Id_TipoMovimento == 9))
					{
						obj.AltroTotaleContantiIn += mv.Id_TipoPagamento == 3 ? (mv.Nr_Pezzi != null && mv.Nr_Pezzi > 0) ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : (decimal)mv.PrezzoVendita : 0;
						obj.AltroTotalePosIn += mv.Id_TipoPagamento == 4 ? (mv.Nr_Pezzi != null && mv.Nr_Pezzi > 0) ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : (decimal)mv.PrezzoVendita : 0;
						obj.AltroTotaleAssegnoIn += mv.Id_TipoPagamento == 5 ? (mv.Nr_Pezzi != null && mv.Nr_Pezzi > 0) ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : (decimal)mv.PrezzoVendita : 0;
					}
					else
					{
						obj.AltroTotaleContantiOut += mv.Id_TipoPagamento == 3 ? (mv.Nr_Pezzi != null && mv.Nr_Pezzi > 0) ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : (decimal)mv.PrezzoVendita : 0;
						obj.AltroTotalePosOut += mv.Id_TipoPagamento == 4 ? (mv.Nr_Pezzi != null && mv.Nr_Pezzi > 0) ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : (decimal)mv.PrezzoVendita : 0;
						obj.AltroTotaleAssegnoOut += mv.Id_TipoPagamento == 5 ? (mv.Nr_Pezzi != null && mv.Nr_Pezzi > 0) ? (decimal)mv.PrezzoVendita * (short)mv.Nr_Pezzi : (decimal)mv.PrezzoVendita : 0;
					}					
				}

				obj.AltroTotaleIn = obj.AltroTotaleContantiIn + obj.AltroTotalePosIn + obj.AltroTotaleAssegnoIn;
				obj.AltroTotaleOut = obj.AltroTotaleContantiOut + obj.AltroTotalePosOut + obj.AltroTotaleAssegnoOut;
			}

			return obj;
		}

		public PrintMandatoArgs GetDepositoAnticipi(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

                //List<V_ImportoPagamento> ips = _context.V_ImportoPagamento.Where(x => x.Dt_Visita > date && x.Dt_Pagamento != null && x.Dt_Pagamento == date).ToList();

                List<V_ImportoPagamento> ips = null;

                if (obj.OnlinePaymentEnabled)
                {
                    if (obj.IdMandato == 0)
                        ips = _context.V_ImportoPagamento.Where(x => (x.Dt_Visita > date && x.Dt_Pagamento != null && x.Dt_Pagamento == date.Date && x.Id_TipoPagamento != 1) || (x.Dt_Pagamento != null && x.Dt_Pagamento <= date && x.Id_TipoPagamento == 1 && x.Id_Mandato == null)).ToList();
                    else
                        ips = _context.V_ImportoPagamento.Where(x => (x.Dt_Visita > date && x.Dt_Pagamento != null && x.Dt_Pagamento == date.Date && x.Id_TipoPagamento != 1) || (x.Dt_Pagamento != null && x.Dt_Pagamento <= date && x.Id_TipoPagamento == 1 && x.Id_Mandato == obj.IdMandato)).ToList();
                }
                else
                    ips = _context.V_ImportoPagamento.Where(x => x.Dt_Visita > date && x.Dt_Pagamento != null && x.Dt_Pagamento == date).ToList();
                
                
				decimal importo = 0;
				foreach (V_ImportoPagamento ip in ips)
				{
                    importo = (ip.Importo_Interi != null ? (decimal)ip.Importo_Interi : 0) + (ip.Importo_Ridotti != null ? (decimal)ip.Importo_Ridotti : 0) + (ip.Importo_Scontati != null ? (decimal)ip.Importo_Scontati : 0) + (ip.Importo_Cumulativi != null ? (decimal)ip.Importo_Cumulativi : 0);
					obj.DepositoAnticipiTotaleOnline += ip.Id_TipoPagamento == 1 ? importo : 0;
					obj.DepositoAnticipiTotaleIOR += ip.Id_TipoPagamento == 2 ? importo : 0;
					obj.DepositoAnticipiTotaleContanti += ip.Id_TipoPagamento == 3 ? importo : 0;
					obj.DepositoAnticipiTotalePos += ip.Id_TipoPagamento == 4 ? importo : 0;
					obj.DepositoAnticipiTotaleAssegno += ip.Id_TipoPagamento == 5 ? importo : 0;
				}

				obj.DepositoAnticipiTotale = 
					obj.DepositoAnticipiTotaleOnline 
					+ obj.DepositoAnticipiTotaleIOR 
					+ obj.DepositoAnticipiTotaleContanti 
					+ obj.DepositoAnticipiTotalePos 
					+ obj.DepositoAnticipiTotaleAssegno;
			}

			return obj;
		}

		public PrintMandatoArgs GetBigliettiPrelevati(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
                //List<V_ImportoPagamento> ips = _context.V_ImportoPagamento.Where(x => x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento < date).ToList();

                List<V_ImportoPagamento> ips = null;
                if (obj.OnlinePaymentEnabled)
                {
                   
                    if (obj.IdMandato == 0)
                        ips = _context.V_ImportoPagamento.Where(x => (x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento < date.Date && x.Id_TipoPagamento != 1) || (x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento <= date.Date && x.Id_TipoPagamento == 1 && x.Id_Mandato == null)).ToList();
                    else
                        ips = _context.V_ImportoPagamento.Where(x => (x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento < date.Date && x.Id_TipoPagamento != 1) || (x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento <= date.Date && x.Id_TipoPagamento == 1 && x.Id_Mandato == obj.IdMandato)).ToList();
                }
                else
                    ips = _context.V_ImportoPagamento.Where(x => x.Dt_Visita == date && x.Dt_Pagamento != null && x.Dt_Pagamento < date).ToList();
                
                decimal importo = 0;
				foreach (V_ImportoPagamento ip in ips)
				{
                    importo = (ip.Importo_Interi != null ? (decimal)ip.Importo_Interi : 0) + (ip.Importo_Ridotti != null ? (decimal)ip.Importo_Ridotti : 0) + (ip.Importo_Scontati != null ? (decimal)ip.Importo_Scontati : 0) + (ip.Importo_Cumulativi != null ? (decimal)ip.Importo_Cumulativi : 0);
					obj.BigliettiPrelevatiTotaleOnline += ip.Id_TipoPagamento == 1 ? importo : 0;
					obj.BigliettiPrelevatiTotaleIOR += ip.Id_TipoPagamento == 2 ? importo : 0;
					obj.BigliettiPrelevatiTotaleContanti += ip.Id_TipoPagamento == 3 ? importo : 0;
					obj.BigliettiPrelevatiTotalePos += ip.Id_TipoPagamento == 4 ? importo : 0;
					obj.BigliettiPrelevatiTotaleAssegno += ip.Id_TipoPagamento == 5 ? importo : 0;
				}

				obj.BigliettiPrelevatiTotale =
					obj.BigliettiPrelevatiTotaleOnline
					+ obj.BigliettiPrelevatiTotaleIOR
					+ obj.BigliettiPrelevatiTotaleContanti
					+ obj.BigliettiPrelevatiTotalePos
					+ obj.BigliettiPrelevatiTotaleAssegno;
			}

			return obj;
		}

		public PrintMandatoArgs GetBigliettiRinunciati(PrintMandatoArgs obj, DateTime date)
		{
			decimal prezzoIntero = 0;
			decimal prezzoRidotto = 0;
            decimal prezzoScontato = 0;
            decimal prezzoCumulativo = 0;

			int NrInteri = 0;
			int NrInteriConsegnati = 0;

			int NrRidotti = 0;
			int NrRidottiConsegnati = 0;

			int NrOmaggio = 0;
			int NrOmaggioConsegnati = 0;

            int NrScontati = 0;
            int NrScontatiConsegnati = 0;

            int NrCumulativi = 0;
            int NrCumulativiConsegnati = 0;

			Parametri_Dal dalParametri = new Parametri_Dal();

            //decimal.TryParse(dalParametri.GetItem("biglietto_intero").Valore, out prezzoIntero);
            //decimal.TryParse(dalParametri.GetItem("biglietto_ridotto").Valore, out prezzoRidotto);

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_VisiteProgrammate> visits = _context.V_VisiteProgrammate.Where(x => x.Dt_Visita == date).ToList();


				obj.NrVisite = visits.Count;

				foreach (V_VisiteProgrammate vp in visits)
				{
					VisitaPrenotata vpr = _context.VisitePrenotate.FirstOrDefault(x => x.Id_VisitaPrenotata == vp.Id_VisitaPrenotata);
					{
						if(vpr != null)
						{
							Prenotazione pr = _context.Prenotazioni.FirstOrDefault(x => x.Id_Prenotazione == vpr.Id_Prenotazione);
							{
								if(pr != null)
								{

                                    var tipoVisita = _context.LK_TipiVisita.FirstOrDefault(tvx => tvx.Id_TipoVisita == pr.Id_TipoVisita);

                                    prezzoIntero = 0;
                                    prezzoRidotto = 0;
                                    prezzoScontato = 0;
                                    prezzoCumulativo = 0;

                                    if (tipoVisita.PrezzoIntero != null)
                                        prezzoIntero = (decimal)tipoVisita.PrezzoIntero;

                                    if (tipoVisita.PrezzoRidotto != null)
                                        prezzoRidotto = (decimal)tipoVisita.PrezzoRidotto;

                                    if (tipoVisita.PrezzoScontato != null)
                                        prezzoScontato = (decimal)tipoVisita.PrezzoScontato;

                                    if (tipoVisita.PrezzoCumulativo != null)
                                        prezzoCumulativo = (decimal)tipoVisita.PrezzoCumulativo;

									List<Pagamento> pgms = _context.Pagamenti.Where(x => x.Id_Prenotazione == pr.Id_Prenotazione && x.Dt_Pagamento != null && x.Dt_Pagamento < date).ToList();
									{
										if(pgms != null)
										{
											foreach(Pagamento pgm in pgms)
											{
												if (pgm.Importo > 0)
												{
													NrInteri += vp.Nr_Interi != null ? (int)vp.Nr_Interi : 0;
													NrInteriConsegnati += vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0;

													NrRidotti += vp.Nr_Ridotti != null ? (int)vp.Nr_Ridotti : 0;
													NrRidottiConsegnati += vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0;

													NrOmaggio += vp.Nr_Omaggio != null ? (int)vp.Nr_Omaggio : 0;
													NrOmaggioConsegnati += vp.Nr_OmaggioConsegnati != null ? (int)vp.Nr_OmaggioConsegnati : 0;

                                                    NrScontati += vp.Nr_Scontati != null ? (int)vp.Nr_Scontati : 0;
                                                    NrScontatiConsegnati += vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0;

                                                    NrCumulativi += vp.Nr_Cumulativi != null ? (int)vp.Nr_Cumulativi : 0;
                                                    NrCumulativiConsegnati += vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0;

                                                    obj.BigliettiRinunciatiTotaleInteri += ((vp.Nr_Interi != null ? (int)vp.Nr_Interi : 0) - (vp.Nr_InteriConsegnati != null ? (int)vp.Nr_InteriConsegnati : 0)) * prezzoIntero;
                                                    obj.BigliettiRinunciatiTotaleRidotti += ((vp.Nr_Ridotti != null ? (int)vp.Nr_Ridotti : 0) - (vp.Nr_RidottiConsegnati != null ? (int)vp.Nr_RidottiConsegnati : 0)) * prezzoRidotto;
                                                    obj.BigliettiRinunciatiTotaleScontati += ((vp.Nr_Scontati != null ? (int)vp.Nr_Scontati : 0) - (vp.Nr_ScontatiConsegnati != null ? (int)vp.Nr_ScontatiConsegnati : 0)) * prezzoScontato;
                                                    obj.BigliettiRinunciatiTotaleCumulativi += ((vp.Nr_Cumulativi != null ? (int)vp.Nr_Cumulativi : 0) - (vp.Nr_CumulativiConsegnati != null ? (int)vp.Nr_CumulativiConsegnati : 0)) * prezzoCumulativo;
												}
											}
										}
									}
								}
							}
						}
					}
				}

				obj.BigliettiRinunciatiNrTotaleInteri = NrInteri - NrInteriConsegnati;
				obj.BigliettiRinunciatiNrTotaleRidotti = NrRidotti - NrRidottiConsegnati;
                obj.BigliettiRinunciatiNrTotaleScontati = NrScontati - NrScontatiConsegnati;
                obj.BigliettiRinunciatiNrTotaleCumulativi = NrCumulativi - NrCumulativiConsegnati;
				obj.BigliettiRinunciatiNrTotale = obj.BigliettiRinunciatiNrTotaleInteri + obj.BigliettiRinunciatiNrTotaleRidotti;
                obj.BigliettiRinunciatiTotale = obj.BigliettiRinunciatiTotaleInteri + obj.BigliettiRinunciatiTotaleRidotti;
			}

			return obj;
		}

		public PrintMandatoArgs GetRimanenza(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_ImportoPagamento> ips = _context.V_ImportoPagamento.Where(x => x.Dt_Visita > date && x.Dt_Pagamento != null && x.Dt_Pagamento <= date).ToList();
				foreach (V_ImportoPagamento vp in ips)
                    obj.RimanenzaTotale += ((vp.Importo_Interi != null ? (decimal)vp.Importo_Interi : 0) + (vp.Importo_Ridotti != null ? (decimal)vp.Importo_Ridotti : 0) + (vp.Importo_Scontati != null ? (decimal)vp.Importo_Scontati : 0) + (vp.Importo_Cumulativi != null ? (decimal)vp.Importo_Cumulativi : 0));
			}

			return obj;
		}

		public PrintMandatoArgs GetTagli(PrintMandatoArgs obj, DateTime date)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				Mandato mnd = _context.Mandatoes.FirstOrDefault(x => x.Dt_Mandato == date);
				if (mnd != null)
				{
					List<MandatoDettaglio> details = _context.MandatoDettaglios.Where(x => x.Id_Mandato == mnd.Id_Mandato).ToList();
					if (details != null)
					{
						obj.taglio500Num = details[0].Numero;
						obj.taglio500Val = details[0].Valore;

						obj.taglio200Num = details[1].Numero;
						obj.taglio200Val = details[1].Valore;

						obj.taglio100Num = details[2].Numero;
						obj.taglio100Val = details[2].Valore;

						obj.taglio50Num = details[3].Numero;
						obj.taglio50Val = details[3].Valore;

						obj.taglio20Num = details[4].Numero;
						obj.taglio20Val = details[4].Valore;

						obj.taglio10Num = details[5].Numero;
						obj.taglio10Val = details[5].Valore;

						obj.taglio5Num = details[6].Numero;
						obj.taglio5Val = details[6].Valore;

						obj.taglio2Num = details[7].Numero;
						obj.taglio2Val = details[7].Valore;

						obj.taglio1Num = details[8].Numero;
						obj.taglio1Val = details[8].Valore;

						obj.taglio050Num = details[9].Numero;
						obj.taglio050Val = details[9].Valore;

						obj.taglio020Num = details[10].Numero;
						obj.taglio020Val = details[10].Valore;

						obj.taglio010Num = details[11].Numero;
						obj.taglio010Val = details[11].Valore;

						obj.taglio005Num = details[12].Numero;
						obj.taglio005Val = details[12].Valore;

						obj.taglioAssegnoNum = details[13].Numero;
						obj.taglioAssegnoVal = details[13].Valore;

						obj.taglioNumTotale = details.Sum(x => x.Numero);
						obj.taglioValTotale = details.Sum(x => x.Valore);
					}
				}
			}

			return obj;
		}

        public List<V_TipoVisita_TipoBiglietto> GetTipiVisiteImporti(DateTime date)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_TipoVisita_TipoBiglietto.Where(tvx => tvx.Dt_Visita == date).OrderBy(tvx => tvx.Ordine).ToList();
            }
        }
	}
}

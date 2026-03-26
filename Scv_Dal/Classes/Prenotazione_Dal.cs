using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using System.Collections.ObjectModel;

namespace Scv_Dal
{
    public class Prenotazione_Dal
    {
        public Prenotazione_Dal()
        {
        }

        public List<Prenotazione> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<Prenotazione> ItemsList = new List<Prenotazione>();

                ExpressionBuilder<Prenotazione> eb = new ExpressionBuilder<Prenotazione>();

                ObjectQuery<Prenotazione> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.Prenotazioni.AsQueryable().Where(expQuery) as ObjectQuery<Prenotazione>;
                    }
                    else
                    {
                        oQItemsList = _context.Prenotazioni.AsQueryable() as ObjectQuery<Prenotazione>;
                    }
                }
                else
                    oQItemsList = _context.Prenotazioni.AsQueryable() as ObjectQuery<Prenotazione>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<Prenotazione>;
                    if (pageSize > 0)
                        ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
                    else
                        ItemsList = oQItemsList.ToList();
                }
                else
                    ItemsList = oQItemsList.Include("Richiedente").ToList();

                return ItemsList;
            }
        }

        public List<V_Prenotazione> GetFilteredList_V_Prenotazione(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_Prenotazione> ItemsList = new List<V_Prenotazione>();

                ExpressionBuilder<V_Prenotazione> eb = new ExpressionBuilder<V_Prenotazione>();

                ObjectQuery<V_Prenotazione> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_Prenotazione.AsQueryable().Where(expQuery) as ObjectQuery<V_Prenotazione>;
                    }
                    else
                    {
                        oQItemsList = _context.V_Prenotazione.AsQueryable() as ObjectQuery<V_Prenotazione>;
                    }
                }
                else
                    oQItemsList = _context.V_Prenotazione.AsQueryable() as ObjectQuery<V_Prenotazione>;

                filteredRowsCount = 0;

                ItemsList = oQItemsList.OrderByDescending(px => px.Id_Prenotazione).ThenByDescending(px => px.Dt_Prenotazione).ToList();

                return ItemsList;
            }
        }

        public List<V_Prenotazione> GetFilteredList_V_Prenotazione(List<V_Prenotazione> ItemsList, List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ExpressionBuilder<V_Prenotazione> eb = new ExpressionBuilder<V_Prenotazione>();

                IEnumerable<V_Prenotazione> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = ItemsList.AsQueryable().Where(expQuery);
                    }
                    else
                    {
                        oQItemsList = ItemsList.AsQueryable();
                    }
                }
                else
                    oQItemsList = ItemsList.AsQueryable();

                filteredRowsCount = 0;

                ItemsList = oQItemsList.OrderByDescending(px => px.Id_Prenotazione).ThenByDescending(px => px.Dt_Prenotazione).ToList();

                return ItemsList;
            }
        }

        public List<int> GetMax_V_Prenotazione(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_Prenotazione> ItemsList = new List<V_Prenotazione>();

                ExpressionBuilder<V_Prenotazione> eb = new ExpressionBuilder<V_Prenotazione>();

                ObjectQuery<V_Prenotazione> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_Prenotazione.AsQueryable().Where(expQuery) as ObjectQuery<V_Prenotazione>;
                    }
                    else
                    {
                        oQItemsList = _context.V_Prenotazione.AsQueryable() as ObjectQuery<V_Prenotazione>;
                    }
                }
                else
                    oQItemsList = _context.V_Prenotazione.AsQueryable() as ObjectQuery<V_Prenotazione>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_Prenotazione>;
                    if (pageSize > 0)
                        ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
                    else
                        ItemsList = oQItemsList.ToList();
                }
                else
                    ItemsList = oQItemsList.ToList();

                for (int i = ItemsList.Count; i < pageSize; i++)
                    ItemsList.Add(new V_Prenotazione());

                return Enumerable.Range(0, ItemsList.Count).ToList(); ;
            }
        }

        public List<VisitaPrenotata> GetFilteredList_VisitaPrenotata(int prenotationID, List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<VisitaPrenotata> ItemsList = new List<VisitaPrenotata>();

                ExpressionBuilder<VisitaPrenotata> eb = new ExpressionBuilder<VisitaPrenotata>();

                ObjectQuery<VisitaPrenotata> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.VisitePrenotate.AsQueryable().Where(expQuery) as ObjectQuery<VisitaPrenotata>;
                    }
                    else
                    {
                        oQItemsList = _context.VisitePrenotate.AsQueryable() as ObjectQuery<VisitaPrenotata>;
                    }
                }
                else
                    oQItemsList = _context.VisitePrenotate.AsQueryable() as ObjectQuery<VisitaPrenotata>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<VisitaPrenotata>;
                    if (pageSize > 0)
                        ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
                    else
                        ItemsList = oQItemsList.ToList();
                }
                else
                    ItemsList = oQItemsList.ToList();

                ItemsList.ForEach(x => x.IsEmpty = false);
                ItemsList.ForEach(x => x.IsErasable = true);

                return ItemsList.Where(x => x.Id_Prenotazione == prenotationID).ToList();
            }
        }

        public List<V_TipiConfermaCounts> GetTipiConfermaCounts()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_TipiConfermaCounts.ToList();
            }
        }

        public Prenotazione GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                Prenotazione p = _context.Prenotazioni
                    .Include("VisitaPrenotatas")
                    .Include("LK_Lingua")
					.Include("LK_TipoVisita")
                    .Include("Richiedente")
                    .Include("Richiedente.LK_Titolo")
                    .Include("Richiedente.LK_Organizzazione")
                    .Include("Richiedente.LK_Citta")
					.Include("Richiedente.LK_Lingua")
					.SingleOrDefault(rx => rx.Id_Prenotazione == id);
                //p.VisitaPrenotatas.ToList().ForEach(x => x.IsEmpty = false);
                return p;
            }
        }

		public V_Prenotazione Get_V_Item(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.V_Prenotazione
					.FirstOrDefault(rx => rx.Id_Prenotazione == id); 
			}
		}

        public List<Prenotazione> GetSingleItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.Prenotazioni.Where(rx => rx.Id_Prenotazione == id).ToList();
            }
        }

		public bool InsertOrUpdate(int IdUser, Prenotazione prenotation, Richiedente petitioner, LK_Titolo petitionerTitle, LK_Organizzazione petitionerOrganization, LK_Citta petitionerCity, List<VisitaPrenotata> prenotationVisits, out int Id_Prenotazione)
		{
			bool ok = true;

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						DateTime dtNow = DateTime.Now;


						//LE SEGUENTI DATE NON DEVONO PRESENTARE LA COMPONENTE 'TIME'
						prenotation.Dt_Prenotazione = prenotation.Dt_Prenotazione.Date;
						prenotation.Dt_VisiteDA = prenotation.Dt_VisiteDA.Date;
						prenotation.Dt_VisiteA = prenotation.Dt_VisiteA.Date;

						if(prenotation.Id_Prenotazione == 0)
							prenotation.Protocollo = Helper_Dal.GetNewProgressive("P", prenotation.Dt_Prenotazione.Year).Progr_UltimoUscito.ToString().PadLeft(8, '0');

						//Titolo Richiedente
						if (petitionerTitle != null)
						{
							if (petitionerTitle.Id_Titolo != 0)
								_context.AttachUpdated(petitionerTitle);
							else
								_context.LK_Titoli.AddObject(petitionerTitle);

							_context.SaveChanges();
							petitioner.Id_Titolo = petitionerTitle.Id_Titolo;
						}

						//Organizzazione Richiedente
						if (petitionerOrganization != null)
						{
							if (petitionerOrganization.Id_Organizzazione != 0)
								_context.AttachUpdated(petitionerOrganization);
							else
								_context.LK_Organizzazioni.AddObject(petitionerOrganization);

							_context.SaveChanges();
							petitioner.Id_Organizzazione = petitionerOrganization.Id_Organizzazione;
						}

						//Citta Richiedente
						if (petitionerCity != null)
						{
							if (petitionerCity.Id_Citta != 0)
								_context.AttachUpdated(petitionerCity);
							else
								_context.LK_Citta.AddObject(petitionerCity);

							_context.SaveChanges();
							petitioner.Id_Citta = petitionerCity.Id_Citta;
						}

						//Richiedente
						if (petitioner.Id_Richiedente != 0)
						{
							if (PetitionerCheckUpdates(petitioner))
							{
								petitioner.Id_User = IdUser;
								petitioner.Dt_Update = dtNow;
							}
							_context.AttachUpdated(petitioner);
						}
						else
						{
							petitioner.Id_User = IdUser;
							petitioner.Dt_Update = dtNow;
							_context.Richiedenti.AddObject(petitioner);
						}

						_context.SaveChanges();

						prenotation.Id_Richiedente = petitioner.Id_Richiedente;

						//Prenotazione
						if (prenotation.Id_Prenotazione != 0)
						{
							prenotation.Id_User = IdUser;
							prenotation.Dt_Update = dtNow;

							_context.AttachUpdated(prenotation);
						}
						else
						{
							prenotation.Id_User = IdUser;
							prenotation.Dt_Update = dtNow;

							_context.Prenotazioni.AddObject(prenotation);

							LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
							LK_Progressivi pr = dalPR.GetSingleItem("P")[0];

							if (pr != null)
							{
								if (pr.Anno != prenotation.Dt_Prenotazione.Year)
								{
									pr.Anno = pr.Anno > 0 ? prenotation.Dt_Prenotazione.Year : 0;
									pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
								}
								else
									pr.Progr_UltimoUscito++;

								//if (
								//    _context.LK_Progressivi
								//        .Where(x => x.Tipo == "P" && x.Progr_UltimoUscito == pr.Progr_UltimoUscito) != null
								//    )
								//    pr.Progr_UltimoUscito++;

								if (pr.Tipo != string.Empty)
									_context.AttachUpdated(pr);
								else
									_context.LK_Progressivi.AddObject(pr);
							}
						}

						_context.SaveChanges();

						//Visite prenotate
						foreach (VisitaPrenotata v in prenotationVisits)
						{
							if (v.Id_VisitaPrenotata == 0)
							{
								v.Id_Prenotazione = prenotation.Id_Prenotazione;
								_context.VisitePrenotate.AddObject(v);
							}
							else
							{
								VisitaPrenotata vp = _context.VisitePrenotate.FirstOrDefault(x => x.Id_VisitaPrenotata == v.Id_VisitaPrenotata);
								if (vp != null)
								{
									vp.Id_Lingua = v.Id_Lingua;
									vp.Nr_Visitatori = v.Nr_Visitatori;
									_context.AttachUpdated(vp);
								}
							}
						}
						
						_context.SaveChanges();

						transaction.Commit();

						Id_Prenotazione = prenotation.Id_Prenotazione;

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

			return ok;
		}

		public int ChangePrenotationResponseTypeID(Prenotazione prenotation)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						Prenotazione p = _context.Prenotazioni.FirstOrDefault(x => x.Id_Prenotazione == prenotation.Id_Prenotazione);
						if (p != null)
						{
							p.Id_TipoRisposta = prenotation.Id_TipoRisposta;
							_context.AttachUpdated(prenotation);
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

			return prenotation.Id_Prenotazione;
		}

		public int ChangePrenotationConfirmation(Prenotazione prenotation)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						Prenotazione p = _context.Prenotazioni.FirstOrDefault(x => x.Id_Prenotazione == prenotation.Id_Prenotazione);
						if (p != null)
						{
							_context.AttachUpdated(prenotation);
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

			return prenotation.Id_Prenotazione;
		}

        public bool CheckPrenotation(Prenotazione prenotation, ObservableCollection<VisitaPrenotata> prenotationVisits, out string message)
        {
            message = "";

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                var prenotatioSaved =  _context.Prenotazioni.FirstOrDefault(rx => rx.Id_Prenotazione == prenotation.Id_Prenotazione);
                //if(prenotation.Id_TipoRisposta == 2)
                //if(prenotatioSaved.Tot_Visitatori < 
            }


            return true;
        }

        public void DeleteVisitObject(VisitaPrenotata obj)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.Attach(obj);
                _context.DeleteObject(obj);
            }
        }

        public void SaveObject()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
        }

        public void DeleteObject(Prenotazione obj)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {

                        //Cancello le visite della prenotazione
                        List<VisitaPrenotata> list = _context.VisitePrenotate.Where(x => x.Id_Prenotazione == obj.Id_Prenotazione).ToList();
                        foreach (VisitaPrenotata g in list)
                        {
                            _context.VisitePrenotate.Attach(g);
                            _context.VisitePrenotate.DeleteObject(g);
                        }
                        _context.SaveChanges();

                        //Cancello la prenotazione
                        Prenotazione prObj = _context.Prenotazioni.FirstOrDefault(x => x.Id_Prenotazione == obj.Id_Prenotazione);
                        _context.Prenotazioni.Attach(prObj);
                        _context.Prenotazioni.DeleteObject(prObj);
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

        public string GetNewPrenotationProtocol()
        {

            int count = 0;

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                count = _context.Prenotazioni.Count();
            }

            string newProtocol = (count + 1).ToString().PadLeft(8, '0');

            return newProtocol;
        }

        public bool PrenotationCheckUpdates(Prenotazione prenotation)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                Prenotazione objBeforeUpdates = _context.Prenotazioni.FirstOrDefault(rx => rx.Id_Prenotazione == prenotation.Id_Prenotazione);

                return _context.CheckIfObjectUpdated(prenotation, objBeforeUpdates);
            }
        }

        public bool PetitionerCheckUpdates(Richiedente petitioner)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                Richiedente objBeforeUpdates = _context.Richiedenti.FirstOrDefault(rx => rx.Id_Richiedente == petitioner.Id_Richiedente);

                return _context.CheckIfObjectUpdated(petitioner, objBeforeUpdates);
            }
        }

        public bool VisitaPrenotataCheckUpdates(VisitaPrenotata vp)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                VisitaPrenotata objBeforeUpdates = _context.VisitePrenotate.FirstOrDefault(rx => rx.Id_VisitaPrenotata == vp.Id_VisitaPrenotata);

                if (vp.Id_Lingua != objBeforeUpdates.Id_Lingua
                    || vp.Nr_Visitatori != objBeforeUpdates.Nr_Visitatori
                    || vp.Id_TipoVisita != objBeforeUpdates.Id_TipoVisita)

                    return true;
                else
                    return false;
            }
        }
        
        public bool TitleCheckUpdates(LK_Titolo title)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                LK_Titolo objBeforeUpdates = _context.LK_Titoli.FirstOrDefault(rx => rx.Id_Titolo == title.Id_Titolo);

                return _context.CheckIfObjectUpdated(title, objBeforeUpdates);
            }
        }

        public bool OrganizationCheckUpdates(LK_Organizzazione organization)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                LK_Organizzazione objBeforeUpdates = _context.LK_Organizzazioni.FirstOrDefault(rx => rx.Id_Organizzazione == organization.Id_Organizzazione);

                return _context.CheckIfObjectUpdated(organization, objBeforeUpdates);
            }
        }

        public bool OrganizationCheckUpdates(LK_Citta city)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                LK_Citta objBeforeUpdates = _context.LK_Citta.FirstOrDefault(rx => rx.Id_Citta == city.Id_Citta);

                return _context.CheckIfObjectUpdated(city, objBeforeUpdates);
            }
        }
    }
}

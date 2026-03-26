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
    public class Guida_Dal
    {
        public Guida_Dal()
        {
        }

        public List<Guida> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<Guida> ItemsList = new List<Guida>();

                ExpressionBuilder<Guida> eb = new ExpressionBuilder<Guida>();

                ObjectQuery<Guida> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.Guide.AsQueryable().Where(expQuery) as ObjectQuery<Guida>;
                    }
                    else
                    {
                        oQItemsList = _context.Guide.AsQueryable() as ObjectQuery<Guida>;
                    }
                }
                else
                    oQItemsList = _context.Guide.AsQueryable() as ObjectQuery<Guida>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<Guida>;
                    if (pageSize > 0)
                        ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
                    else
                        ItemsList = oQItemsList.ToList();
                }
                else
                    ItemsList = oQItemsList.ToList();

                Parametri_Dal dal = new Parametri_Dal();
                decimal compenso = 0;
                decimal.TryParse(dal.GetItem("compenso_guida").Valore, out compenso);
                ItemsList.ForEach(x => x.Compenso = compenso);

                return ItemsList;
            }
        }

        public List<V_Guide> GetFilteredList_V_Guide(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_Guide> ItemsList = new List<V_Guide>();

                ExpressionBuilder<V_Guide> eb = new ExpressionBuilder<V_Guide>();

                ObjectQuery<V_Guide> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_Guide.AsQueryable().Where(expQuery) as ObjectQuery<V_Guide>;
                    }
                    else
                    {
                        oQItemsList = _context.V_Guide.AsQueryable() as ObjectQuery<V_Guide>;
                    }
                }
                else
                    oQItemsList = _context.V_Guide.AsQueryable() as ObjectQuery<V_Guide>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_Guide>;
                    if (pageSize > 0)
                        ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
                    else
                        ItemsList = oQItemsList.ToList();
                }
                else
                    ItemsList = oQItemsList.ToList();

                return ItemsList;
            }
        }

        public List<V_Guide> GetList()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.V_Guide.ToList();
            }
        }

        public List<V_Guide> GetListByIdLinguaAndDataVisita(int idLingua, DateTime dtVisita)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (idLingua == 2)
                    return _context.V_Guide.ToList();
                else
                    return _context.V_Guide.Where(gx => gx.Id_Guida < 10).ToList();
            }
        }

        public Guida GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                Guida p = _context.Guide.Include("GuidaLinguas").SingleOrDefault(rx => rx.Id_Guida == id);
				if (p != null)
				{
					Parametri_Dal dal = new Parametri_Dal();
					decimal compenso = 0;
					decimal.TryParse(dal.GetItem("compenso_guida").Valore, out compenso);
					p.Compenso = compenso;
				}

                return p;
            }
        }

        public List<Guida> GetSingleItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                Parametri_Dal dal = new Parametri_Dal();
                decimal compenso = 0;
                decimal.TryParse(dal.GetItem("compenso_guida").Valore, out compenso);
                List<Guida> g = _context.Guide.Include("GuidaLinguas").Where(rx => rx.Id_Guida == id).ToList();
                if (g != null)
                    g.ForEach(x => x.Compenso = compenso);
                return g;
            }
        }

		public List<V_GuideIncaricate> GetGuideIncaricate(DateTime date)
		{
			List<V_GuideIncaricate> list = new List<V_GuideIncaricate>();
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				list.AddRange(_context.V_GuideIncaricate
					.Where(x => x.Dt_Visita.Day == date.Day && x.Dt_Visita.Month == date.Month && x.Dt_Visita.Year == date.Year)
					.OrderBy(x => x.Cognome)
					.ThenBy(x => x.Nome)
					.ThenBy(x => x.Ora_Visita)
					);

				foreach (V_GuideIncaricate item in list)
					if (item.LinguaVisita == null)
						item.LinguaVisita = new LK_Lingua_Dal().GetItem(item.IdLinguaVisita).Descrizione;
			}
			return list;
		}

		public List<V_AssegnazioniGuideVisite> GetAssegnazioniGuideVisite(DateTime date)
		{
			List<V_AssegnazioniGuideVisite> list = new List<V_AssegnazioniGuideVisite>();
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				list = _context.V_AssegnazioniGuideVisite
					.Where(x => x.Dt_Visita == date)
					.OrderBy(x => x.Ora_Visita)
					.ThenBy(x => x.Protocollo)
					.ToList()
					;
			}

			return list;
		}

        public int InsertOrUpdate(Guida obj, List<GuidaDisponibile> shifts, List<GuidaLingua> guideLanguages)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {

                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
						//if guide is a master for his college, set other guides' master
						//flag to false for the same college.
						if ((obj.Id_Collegio != null && obj.Id_Collegio > 0) && obj.Fl_Capofila == true)
						{
							_context.Guide.Where(x => x.Id_Collegio == obj.Id_Collegio).ToList().ForEach(x => x.Fl_Capofila = false);
							_context.SaveChanges();
						}

						obj.Dt_Update = DateTime.Now.Date;

                        if (obj.Id_Guida != 0)
                            _context.AttachUpdated(obj);
                        else
                            _context.Guide.AddObject(obj);

                        _context.SaveChanges();

                        //delete all guide shifts
                        _context.GuideDisponibili.Where(gdx => gdx.Id_Guida == obj.Id_Guida).ToList().ForEach(_context.GuideDisponibili.DeleteObject);
                        _context.SaveChanges();

                        //assign guide id to guide shifts and save them
                        for (int i = 0; i < shifts.Count; i++)
                        {
                            shifts[i].Id_Guida = obj.Id_Guida;
                            _context.GuideDisponibili.AddObject(shifts[i]);
                        }
                        _context.SaveChanges();

                        //delete all guide languages
                        _context.GuideLingue.Where(gdx => gdx.Id_Guida == obj.Id_Guida).ToList().ForEach(_context.GuideLingue.DeleteObject);
                        _context.SaveChanges();

                        //assign guide id to guide languages and save them
                        for (int i = 0; i < guideLanguages.Count; i++)
                        {
                            guideLanguages[i].Id_Guida = obj.Id_Guida;
                            _context.GuideLingue.AddObject(guideLanguages[i]);
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

            return obj.Id_Guida;
        }

        public void SaveObject()
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
        }

        public void DeleteObject(Guida obj)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (_context.Connection.State != System.Data.ConnectionState.Open)
                    _context.Connection.Open();

                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {

                        //Cancello le lingue della guida
                        List<GuidaLingua> lng = _context.GuideLingue.Where(x => x.Id_Guida == obj.Id_Guida).ToList();
                        foreach (GuidaLingua g in lng)
                        {
                            _context.GuideLingue.Attach(g);
                            _context.GuideLingue.DeleteObject(g);
                        }
                        _context.SaveChanges();


                        //Cancello le disponibilità della guida
                        List<GuidaDisponibile> disp = _context.GuideDisponibili.Where(x => x.Id_Guida == obj.Id_Guida).ToList();
                        foreach (GuidaDisponibile g in disp)
                        {
                            _context.GuideDisponibili.Attach(g);
                            _context.DeleteObject(g);
                        }
                        _context.SaveChanges();

                        //Cancello la guida
                        Guida guida = _context.Guide.FirstOrDefault(x => x.Id_Guida == obj.Id_Guida);
                        _context.Guide.Attach(guida);
                        _context.Guide.DeleteObject(guida);
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using System.Globalization;
using System.Threading;

namespace Scv_Dal
{
	public class GuidaDisponibile_Dal
	{
		public List<GuidaDisponibile> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<GuidaDisponibile> ItemsList = new List<GuidaDisponibile>();

				ExpressionBuilder<GuidaDisponibile> eb = new ExpressionBuilder<GuidaDisponibile>();

				ObjectQuery<GuidaDisponibile> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.GuideDisponibili.AsQueryable().Where(expQuery) as ObjectQuery<GuidaDisponibile>;
					}
					else
					{
						oQItemsList = _context.GuideDisponibili.AsQueryable() as ObjectQuery<GuidaDisponibile>;
					}
				}
				else
					oQItemsList = _context.GuideDisponibili.AsQueryable() as ObjectQuery<GuidaDisponibile>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<GuidaDisponibile>;
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


		public List<V_GuideDisponibili> GetV_GuideDisponibili(V_EvidenzeGiornaliereGroup currentItem)
        {
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_GuideDisponibili> ItemsList = new List<V_GuideDisponibili>();
				List<V_GuideDisponibili> RetItemsList = new List<V_GuideDisponibili>();

				if (currentItem != null)
				{
					CultureInfo culture = CultureInfo.CreateSpecificCulture("it-IT");
					Thread.CurrentThread.CurrentCulture = culture;
					Thread.CurrentThread.CurrentUICulture = culture;

					TimeSpan tsTimeoraVisita = TimeSpan.ParseExact(currentItem.Ora_Visita, "g", culture);
					string ora_inizio_visite_pm = _context.Parametris.FirstOrDefault(px => px.Chiave == "ora_inizio_visite_pm").Valore;
					TimeSpan tsOra_inizio_visite_pm = TimeSpan.ParseExact(ora_inizio_visite_pm, "g", culture);

					if (currentItem.Fl_AccettaGuida == null || currentItem.Fl_AccettaGuida == false)
					{
						//SE FL_AVVISAGUIDA == FALSE SOLO CAPOFILA + GUIDE SINGOLE
						ItemsList = _context.V_GuideDisponibili.Where(gx => 
							gx.Id_Lingua == currentItem.Id_Lingua
							&& gx.Anno == currentItem.Dt_Visita.Year
							&& gx.Mese == currentItem.Dt_Visita.Month
							&& gx.Giorno == currentItem.Dt_Visita.Day
							&& (gx.Fl_AM == (tsTimeoraVisita < tsOra_inizio_visite_pm) || gx.Fl_PM == (tsTimeoraVisita >= tsOra_inizio_visite_pm))
							&& (gx.Fl_Capofila == true || gx.NumGuideDelCollegio == 1)).ToList();
					}
					else
					{
						//SE FL_AVVISAGUIDA == TRUE TUTTE LE GUIDE ORDINATE PER COLLEGIO + CAPOFILA
						ItemsList = _context.V_GuideDisponibili.Where(gx => 
							gx.Id_Lingua == currentItem.Id_Lingua
							&& gx.Anno == currentItem.Dt_Visita.Year
							&& gx.Mese == currentItem.Dt_Visita.Month
							&& gx.Giorno == currentItem.Dt_Visita.Day
							&& (gx.Fl_AM == (tsTimeoraVisita < tsOra_inizio_visite_pm) || gx.Fl_PM == (tsTimeoraVisita >= tsOra_inizio_visite_pm)))
							.OrderBy(gx => gx.Id_Collegio).ThenBy(gx => gx.Fl_Capofila)
							.ToList()
							;
					}

					RetItemsList = ItemsList.OrderBy(x => x.Cognome).ThenBy(x => x.Nome).ToList();

					V_GuideDisponibili gdEmpty = new V_GuideDisponibili();
					gdEmpty.Id_Guida = 0;
					gdEmpty.Cognome = "N.D.";
					gdEmpty.Nome = string.Empty;
					RetItemsList.Insert(0, gdEmpty);
				}

				return RetItemsList;
			}
        }

		public List<V_GuideDisponibili> GetV_GuideDisponibili(V_EvidenzeGiornaliere currentItem)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_GuideDisponibili> ItemsList = new List<V_GuideDisponibili>();
				List<V_GuideDisponibili> RetItemsList = new List<V_GuideDisponibili>();

				if (currentItem != null)
				{
					CultureInfo culture = CultureInfo.CreateSpecificCulture("it-IT");
					Thread.CurrentThread.CurrentCulture = culture;
					Thread.CurrentThread.CurrentUICulture = culture;

					TimeSpan tsTimeoraVisita = TimeSpan.ParseExact(currentItem.Ora_Visita, "g", culture);
					string ora_inizio_visite_pm = _context.Parametris.FirstOrDefault(px => px.Chiave == "ora_inizio_visite_pm").Valore;
					TimeSpan tsOra_inizio_visite_pm = TimeSpan.ParseExact(ora_inizio_visite_pm, "g", culture);

					if (currentItem.Fl_AccettaGuida == null || currentItem.Fl_AccettaGuida == false)
					{
						//SE FL_AVVISAGUIDA == FALSE SOLO CAPOFILA + GUIDE SINGOLE
						ItemsList = _context.V_GuideDisponibili.Where(gx => 
							gx.Id_Lingua == currentItem.Id_Lingua
							&& gx.Anno == currentItem.Dt_Visita.Year
							&& gx.Mese == currentItem.Dt_Visita.Month
							&& gx.Giorno == currentItem.Dt_Visita.Day
							&& ((gx.Fl_AM == true && (tsTimeoraVisita < tsOra_inizio_visite_pm)) || (gx.Fl_PM == true &&  (tsTimeoraVisita >= tsOra_inizio_visite_pm)))
							&& (gx.Fl_Capofila == true || gx.NumGuideDelCollegio == 1)).ToList();
					}
					else
					{
						//SE FL_AVVISAGUIDA == TRUE TUTTE LE GUIDE ORDINATE PER COLLEGIO + CAPOFILA
						ItemsList = _context.V_GuideDisponibili.Where(gx => 
							gx.Id_Lingua == currentItem.Id_Lingua
							&& gx.Anno == currentItem.Dt_Visita.Year
							&& gx.Mese == currentItem.Dt_Visita.Month
							&& gx.Giorno == currentItem.Dt_Visita.Day
							&& ((gx.Fl_AM == true && (tsTimeoraVisita < tsOra_inizio_visite_pm)) || (gx.Fl_PM == true && (tsTimeoraVisita >= tsOra_inizio_visite_pm))))
							.OrderBy(gx => gx.Id_Collegio).ThenBy(gx => gx.Fl_Capofila)
							.ToList()
							;
					}

					RetItemsList = ItemsList.OrderBy(x => x.Cognome).ThenBy(x => x.Nome).ToList();

					V_GuideDisponibili gdEmpty = new V_GuideDisponibili();
					gdEmpty.Id_Guida = 0;
					gdEmpty.Cognome = "N.D.";
					gdEmpty.Nome = string.Empty;
					RetItemsList.Insert(0, gdEmpty);
				}

				return RetItemsList;
			}
        }

		public List<GuidaDisponibile> GetShiftsByGuideID(int guideID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<GuidaDisponibile> ItemsList = new List<GuidaDisponibile>();

				ItemsList = _context.GuideDisponibili.Where(x => x.Id_Guida == guideID).ToList();

				return ItemsList;
			}
		}

		public GuidaDisponibile GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				GuidaDisponibile p = _context.GuideDisponibili.SingleOrDefault(rx => rx.Id_GuidaDisponibile == id);
				return p;
			}
		}

		public List<GuidaDisponibile> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.GuideDisponibili.Where(rx => rx.Id_GuidaDisponibile == id).ToList();
			}
		}

		public int InsertOrUpdate(GuidaDisponibile obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						if (obj.Id_GuidaDisponibile != 0)
							_context.AttachUpdated(obj);
						else
							_context.GuideDisponibili.AddObject(obj);

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

			return obj.Id_GuidaDisponibile;
		}

		public void SaveObject()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.SaveChanges();
			}
		}

		public void DeleteShiftByGuideID(int guideID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.GuideDisponibili.Where(gdx => gdx.Id_Guida == guideID).ToList().ForEach(_context.GuideDisponibili.DeleteObject);
			}
		}
	}
}

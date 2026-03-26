using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using System.Globalization;
using System.Threading;
using Scv_Model;

namespace Scv_Dal
{
	public class LK_Chiusura_Dal
	{
		public List<LK_Chiusura> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_Chiusura> ItemsList = new List<LK_Chiusura>();

				ExpressionBuilder<LK_Chiusura> eb = new ExpressionBuilder<LK_Chiusura>();

				ObjectQuery<LK_Chiusura> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_Chiusura.AsQueryable().Where(expQuery) as ObjectQuery<LK_Chiusura>;
					}
					else
					{
						oQItemsList = _context.LK_Chiusura.AsQueryable() as ObjectQuery<LK_Chiusura>;
					}
				}
				else
					oQItemsList = _context.LK_Chiusura.AsQueryable() as ObjectQuery<LK_Chiusura>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_Chiusura>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				return ItemsList.OrderBy(x => x.Anno).ThenBy(x => x.Mese).ThenBy(x => x.Giorno).ToList();
			}
		}

		public LK_Chiusura GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				LK_Chiusura p = _context.LK_Chiusura.SingleOrDefault(rx => rx.Id_Chiusura == id);
				return p;
			}
		}

		public List<LK_Chiusura> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Chiusura.Where(rx => rx.Id_Chiusura == id).ToList();
			}
		}

		public List<YearItem> GetExistingYears(List<int> exclude)
		{
			List<YearItem> list = new List<YearItem>();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<int> yearsIDs = new List<int>();

				foreach (LK_Chiusura c in _context.LK_Chiusura.OrderBy(x => x.Anno))
					if (!yearsIDs.Contains(c.Anno) && !exclude.Contains(c.Anno))
					{
						yearsIDs.Add(c.Anno);
						list.Add(new YearItem(c.Anno));
					}
			}

			return list;
		}

		public List<LK_Chiusura> GetYear(int year)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Chiusura.Where(x => x.Anno == year).ToList();
			}	
		}

		public bool YearExists(int year)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Chiusura.FirstOrDefault(x => x.Anno == year) != null;
			}
		}

        public bool CheckDateTime(DateTime dtToCheck, string oraVisita)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                bool chkd = true;
                var objChiusura = _context.LK_Chiusura.FirstOrDefault(cx => cx.Anno == dtToCheck.Year && cx.Mese == dtToCheck.Month && cx.Giorno == dtToCheck.Day);
                
                if (objChiusura == null)
                {
                    int dayOfWeek = Convert.ToInt32(dtToCheck.DayOfWeek);
                    if (dayOfWeek == 0)
                        dayOfWeek = 7;
                    objChiusura = _context.LK_Chiusura.FirstOrDefault(cx => cx.Anno == dtToCheck.Year && cx.Mese == 0 && cx.Giorno == dayOfWeek);
                }


                if (objChiusura == null)
                {
                    chkd = true;
                }
                else
                {

                    if (objChiusura.Fl_AM && objChiusura.Fl_PM)
                    {
                        chkd = false;
                    }
                    else
                    {
                        CultureInfo culture = CultureInfo.CreateSpecificCulture("it-IT");
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                        TimeSpan tsTimeoraVisita = TimeSpan.ParseExact(oraVisita, "g", culture);

                        string ora_inizio_visite_pm = _context.Parametris.FirstOrDefault(px => px.Chiave == "ora_inizio_visite_pm").Valore;

                        TimeSpan tsOra_inizio_visite_pm = TimeSpan.ParseExact(ora_inizio_visite_pm, "g", culture);

                        if (objChiusura.Fl_AM)
                        {
                            if(tsTimeoraVisita < tsOra_inizio_visite_pm)
                                chkd = false;
                        }
                        else if (objChiusura.Fl_PM)
                        {
                            if (tsTimeoraVisita >= tsOra_inizio_visite_pm)
                                chkd = false;
                        }
                    }

                }

                return chkd;
            }

            
        }

		public ClosingDateResult CheckDate(DateTime dateToCheck)
		{
			ClosingDateResult cdr = new ClosingDateResult();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
					List<LK_Chiusura> objChiusura = _context.LK_Chiusura.Where(cx =>
						cx.Anno == dateToCheck.Year
						&&
						(
							(cx.Mese == dateToCheck.Month && cx.Giorno == dateToCheck.Day)
							||
							(cx.Mese == 0 && cx.Giorno == (dateToCheck.DayOfWeek == 0 ? 7 : (int)dateToCheck.DayOfWeek))
						)
					).ToList();

					if (objChiusura != null)
					{
						foreach (LK_Chiusura obj in objChiusura)
						{
							if (obj.Fl_AM)
								cdr.ClosedAM = true;
							if (obj.Fl_PM)
								cdr.ClosedPM = true;
						}
					}

				return cdr;
			}
		}


		public ClosingDateResult CheckDate(DateTime dateStart, DateTime dateEnd)
		{
			ClosingDateResult cdr = new ClosingDateResult();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<DateTime> dates = new List<DateTime>();
				DateTime currentDate = dateStart;
				int d = 0;
				while (currentDate.AddDays(d) <= dateEnd)
					dates.Add(currentDate.AddDays(d++));

				foreach (DateTime date in dates)
				{
					cdr = CheckDate(date);
					if (cdr.Closed)
						break;
				}

				return cdr;
			}
		}

		public int InsertOrUpdate(LK_Chiusura obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						if (obj.Id_Chiusura != 0)
							_context.AttachUpdated(obj);
						else
							_context.LK_Chiusura.AddObject(obj);

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

			return obj.Id_Chiusura;
		}

		public void SaveObject()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.SaveChanges();
			}
		}

		public void DeleteObject(LK_Chiusura obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						//Cancello la chiusura
						LK_Chiusura o = _context.LK_Chiusura.FirstOrDefault(x => x.Id_Chiusura == obj.Id_Chiusura);
						_context.LK_Chiusura.Attach(o);
						_context.LK_Chiusura.DeleteObject(o);
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

		public void DuplicateYear(int sourceYear, int destinationYear)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						//Eliminazione anno di destinazione
						List<LK_Chiusura> dl = _context.LK_Chiusura.Where(x => x.Anno == destinationYear).ToList();
						foreach (LK_Chiusura o in dl)
						{
							_context.LK_Chiusura.Attach(o);
							_context.LK_Chiusura.DeleteObject(o);
							_context.SaveChanges();
						}

						//Duplicazione
						List<LK_Chiusura> sl = _context.LK_Chiusura.Where(x => x.Anno == sourceYear && x.Fl_Fisso).ToList();
						foreach (LK_Chiusura o in sl)
						{
							LK_Chiusura obj = new LK_Chiusura();
							obj.Date = o.Date;
							obj.Giorno = o.Giorno;
							obj.Mese = o.Mese;
							obj.Anno = (short)destinationYear;
							obj.Fl_AM = o.Fl_AM;
							obj.Fl_PM = o.Fl_PM;
							obj.Fl_Fisso = o.Fl_Fisso;
							obj.Nota = o.Nota;

							_context.LK_Chiusura.AddObject(obj);
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
	}
}

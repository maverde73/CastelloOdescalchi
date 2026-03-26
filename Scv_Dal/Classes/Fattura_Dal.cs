using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;
using Scv_Model;

namespace Scv_Dal
{
	public class Fattura_Dal
	{
		public List<Fattura> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			//_context = new SCV_DEVEntities();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<Fattura> ItemsList = new List<Fattura>();

				ExpressionBuilder<Fattura> eb = new ExpressionBuilder<Fattura>();

				ObjectQuery<Fattura> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.Fatture.AsQueryable().Where(expQuery) as ObjectQuery<Fattura>;
					}
					else
					{
						oQItemsList = _context.Fatture.AsQueryable() as ObjectQuery<Fattura>;
					}
				}
				else
					oQItemsList = _context.Fatture.AsQueryable() as ObjectQuery<Fattura>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<Fattura>;
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

		public List<Fattura> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Fatture.Where(rx => rx.Id_Fattura == id).ToList();
			}
		}

		public Fattura GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Fatture.FirstOrDefault(rx => rx.Id_Fattura == id);
			}
		}

		public int InsertOrUpdate(List<MovementMaster> list)
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
						Fattura obj = null;
						List<int> storeIDs = new List<int>();

						foreach (MovementMaster m in list.OrderBy(x => x.StoreID).ToList())
						{							
							//Se cambia l'esercizio, o se è il primo, viene creata una
							//nuova fattura e viene avanzato il progressivo fatture.
							//Quindi viene assegnato l'id fattura al movimento corrente.
							//Se l'esercizio esiste già, ovvero se è stata già creata
							//una fattura per l'esercizio corrente, non viene creata alcuna
							//fattura, ma l'id di quella creata continua ad essere assegnato
							//al movimento corrente. 
							//Essendo la lista ordinata per esercizio, cambiato un esercizio
							//esso non si ripresenterà più una volta esauriti i movimenti
							//relativi ad esso.
							if (!storeIDs.Contains((int)m.StoreID))
							{
								storeIDs.Add((int)m.StoreID);
								obj = new Fattura();
								obj.Dt_Fattura = DateTime.Now.Date;
								obj.Id_EsercizioVendita = m.StoreID;
								obj.Id_User = 0;
								obj.Tot_Fattura = string.Empty;

								_context.Fatture.AddObject(obj);

								_context.SaveChanges();

								id = obj.Id_Fattura;

								//Progressivo fattura
								pr = _context.LK_Progressivi.FirstOrDefault(rx => rx.Tipo == "F");

								if (pr != null)
								{
									if (pr.Anno != obj.Dt_Fattura.Year)
									{
										pr.Anno = pr.Anno > 0 ? obj.Dt_Fattura.Year : 0;
										pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito + 1;
									}
									else
										pr.Progr_UltimoUscito++;

									if (pr.Tipo != string.Empty)
										_context.AttachUpdated(pr);
									else
										_context.LK_Progressivi.AddObject(pr);

									_context.SaveChanges();

									//aggiornamento numero fattura
									obj.Nr_Fattura = pr.Progr_UltimoUscito;
									_context.AttachUpdated(obj);
									_context.SaveChanges();

								}
							}

							List<Movimento> mv = _context.Movimenti.Where(x => x.Identificativo == m.ID).ToList();
							foreach (Movimento mov in mv)
							{
								mov.Id_Fattura = id;
								_context.AttachUpdated(mov);
								_context.SaveChanges();
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

		public void SaveObject()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.SaveChanges();
			}
		}

		public void DeleteObject(Fattura obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{

						_context.Fatture.DeleteObject(_context.Fatture.FirstOrDefault(x => x.Id_Fattura == obj.Id_Fattura));
						_context.SaveChanges();

						//Progressivo fattura
						LK_Progressivi_Dal dalPR = new LK_Progressivi_Dal();
						LK_Progressivi pr = dalPR.GetSingleItem("F")[0];

						if (pr != null)
						{
							if (pr.Anno != obj.Dt_Fattura.Year)
							{
								pr.Anno = pr.Anno > 0 ? obj.Dt_Fattura.Year : 0;
								pr.Progr_UltimoUscito = pr.Anno > 0 ? 1 : pr.Progr_UltimoUscito - 1;
							}
							else
								pr.Progr_UltimoUscito--;

							if (pr.Tipo != string.Empty)
								_context.AttachUpdated(pr);
							else
								_context.LK_Progressivi.AddObject(pr);
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

		public void RecordPaymentDates(List<Invoice> obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						Fattura o = null;

						foreach (Invoice ob in obj.Where(x => x.PaymentDate != null).ToList())
						{							
							o = _context.Fatture.FirstOrDefault(x => x.Id_Fattura == ob.Id_Fattura);
							o.Dt_Pagamento = ob.PaymentDate;
							_context.AttachUpdated(o);
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

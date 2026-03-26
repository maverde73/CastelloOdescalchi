using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class EsercizioVendita_Dal
	{
		public List<EsercizioVendita> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<EsercizioVendita> ItemsList = new List<EsercizioVendita>();

				ExpressionBuilder<EsercizioVendita> eb = new ExpressionBuilder<EsercizioVendita>();

				ObjectQuery<EsercizioVendita> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.EserciziVendita.AsQueryable().Where(expQuery) as ObjectQuery<EsercizioVendita>;
					}
					else
					{
						oQItemsList = _context.EserciziVendita.AsQueryable() as ObjectQuery<EsercizioVendita>;
					}
				}
				else
					oQItemsList = _context.EserciziVendita.AsQueryable() as ObjectQuery<EsercizioVendita>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<EsercizioVendita>;
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

		public EsercizioVendita GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				EsercizioVendita p = _context.EserciziVendita.SingleOrDefault(rx => rx.Id_EsercizioVendita == id);
				return p;
			}
		}

		public List<EsercizioVendita> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.EserciziVendita.Where(rx => rx.Id_EsercizioVendita == id).ToList();
			}
		}

		public List<EsercizioVendita> GetList()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.EserciziVendita.ToList();
			}
		}

		public int InsertOrUpdate(EsercizioVendita obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						if (obj.Id_EsercizioVendita != 0)
							_context.AttachUpdated(obj);
						else
							_context.EserciziVendita.AddObject(obj);

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

			return obj.Id_EsercizioVendita;
		}

		public void SaveObject()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.SaveChanges();
			}
		}

		public void DeleteObject(EsercizioVendita obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						EsercizioVendita ob = _context.EserciziVendita.FirstOrDefault(x => x.Id_EsercizioVendita == obj.Id_EsercizioVendita);
						_context.EserciziVendita.Attach(ob);
						_context.EserciziVendita.DeleteObject(ob);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class Articolo_Dal
	{
        public List<Articolo> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<Articolo> ItemsList = new List<Articolo>();

                ExpressionBuilder<Articolo> eb = new ExpressionBuilder<Articolo>();

                ObjectQuery<Articolo> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.Articoli.AsQueryable().Where(expQuery) as ObjectQuery<Articolo>;
                    }
                    else
                    {
						oQItemsList = _context.Articoli.AsQueryable() as ObjectQuery<Articolo>;
                    }
                }
                else
					oQItemsList = _context.Articoli.AsQueryable() as ObjectQuery<Articolo>;

                filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<Articolo>;
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
        
	    public List<V_Articoli> GetFilteredList_V_Articoli(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<V_Articoli> ItemsList = new List<V_Articoli>();

                ExpressionBuilder<V_Articoli> eb = new ExpressionBuilder<V_Articoli>();

                ObjectQuery<V_Articoli> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_Articoli.AsQueryable().Where(expQuery) as ObjectQuery<V_Articoli>;
                    }
                    else
                    {
                        oQItemsList = _context.V_Articoli.AsQueryable() as ObjectQuery<V_Articoli>;
                    }
                }
                else
                    oQItemsList = _context.V_Articoli.AsQueryable() as ObjectQuery<V_Articoli>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_Articoli>;
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

        public Articolo GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				Articolo p = _context.Articoli.SingleOrDefault(rx => rx.Id_Articolo == id);
                return p;
            }
        }

        public List<Articolo> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.Articoli.Where(rx => rx.Id_Articolo == id).ToList();
            }
		}

        public int InsertOrUpdate(Articolo obj)
        {
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						if (obj.Id_Articolo != 0)
							_context.AttachUpdated(obj);
						else
							_context.Articoli.AddObject(obj);

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

			return obj.Id_Articolo;
        }

		public void SaveObject()
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
		}

		public void DeleteObject(Articolo obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{

						//Cancello l'articolo
						Articolo articolo = _context.Articoli.FirstOrDefault(x => x.Id_Articolo == obj.Id_Articolo);
                        _context.Articoli.Attach(articolo);
                        _context.Articoli.DeleteObject(articolo);
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

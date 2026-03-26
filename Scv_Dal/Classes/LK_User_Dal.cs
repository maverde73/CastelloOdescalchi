using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Scv_Entities;
using System.Data.Objects;
using Scv_Model.Common;

namespace Scv_Dal
{
	public class LK_User_Dal
	{

		public LK_User Login(string username, string password)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				try
				{
					return _context.LK_Users.SingleOrDefault(rx => rx.Identificativo == username && rx.PSW == password && rx.Fl_Attivo == true);
				}
				catch (Exception e)
				{
					return null;
				}
            }
		}

	    public List<LK_User> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                List<LK_User> ItemsList = new List<LK_User>();

                ExpressionBuilder<LK_User> eb = new ExpressionBuilder<LK_User>();

                ObjectQuery<LK_User> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.LK_Users.AsQueryable().Where(expQuery) as ObjectQuery<LK_User>;
                    }
                    else
                    {
						oQItemsList = _context.LK_Users.AsQueryable() as ObjectQuery<LK_User>;
                    }
                }
                else
					oQItemsList = _context.LK_Users.AsQueryable() as ObjectQuery<LK_User>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_User>;
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
        
        public LK_User GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				LK_User p = _context.LK_Users.SingleOrDefault(rx => rx.Id_User == id);
                return p;
            }
        }

        public List<LK_User> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                return _context.LK_Users.Where(rx => rx.Id_User == id).ToList();
            }
		}

        public int InsertOrUpdate(LK_User obj)
        {
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{

				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{
						if (obj.Id_User != 0)
							_context.AttachUpdated(obj);
						else
							_context.LK_Users.AddObject(obj);

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

			return obj.Id_User;
        }

		public void SaveObject()
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                _context.SaveChanges();
            }
		}

		public void DeleteObject(LK_User obj)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (_context.Connection.State != System.Data.ConnectionState.Open)
					_context.Connection.Open();

				using (var transaction = _context.Connection.BeginTransaction())
				{
					try
					{


						//Controllo sul primo amministratore
						int minID = 0;
						try
						{
							minID = _context.LK_Users.Where(x => x.Id_User == obj.Id_User && x.Fl_Amministratore == true).Min(x => x.Id_User);
						}
						catch
						{

						}
						LK_User u = _context.LK_Users.FirstOrDefault(x => x.Id_User == minID);

						//Se è il primo amministratore
						if (u != null)
						{
							throw new Exception("Impossibile eliminare il primo amministratore del sistema");
						}
						else
						{
							//Cancello l'utente
							u = _context.LK_Users.FirstOrDefault(x => x.Id_User == obj.Id_User);
							_context.LK_Users.Attach(u);
							_context.LK_Users.DeleteObject(u);
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
		}

	}
}

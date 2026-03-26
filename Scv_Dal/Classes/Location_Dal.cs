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
	public class Location_Dal
	{

        public ObservableCollection<LK_Citta> GetLocations()
		{
            ObservableCollection<LK_Citta> list = null;

			try
			{
                using (IN_VIAEntities _context = new IN_VIAEntities())
				{
					string[] orderByField = new String[] { "Nome" };

                    ExpressionBuilder<LK_Citta> eb = new ExpressionBuilder<LK_Citta>();

                    ObjectQuery<LK_Citta> oQItemsList = null;

                    oQItemsList = _context.LK_Citta.AsQueryable() as ObjectQuery<LK_Citta>;

                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, "string") as ObjectQuery<LK_Citta>;

                    list = new ObservableCollection<LK_Citta>(oQItemsList);
				}
			}
			catch (Exception e)
			{

			}

			return list;
		}

		public LK_Citta GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Citta.SingleOrDefault(rx => rx.Id_Citta == id);
			}
		}

		public List<LK_Citta> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Citta.Where(rx => rx.Id_Citta == id).ToList();
			}
		}

		public int InsertOrUpdate(LK_Citta objToUpdate)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				if (objToUpdate.Id_Citta != 0)
				{
					_context.AttachUpdated(objToUpdate);
				}
				else
				{
					_context.LK_Citta.AddObject(objToUpdate);
				}

				_context.SaveChanges();

				return objToUpdate.Id_Citta;
			}
		}

		public void SaveObject()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				_context.SaveChanges();
			}
		}

	}
	
}

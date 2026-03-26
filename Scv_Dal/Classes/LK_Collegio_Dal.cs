using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class LK_Collegio_Dal
	{
		public LK_Collegio_Dal()
        {
        }

        public ObservableCollection<LK_Collegio> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				ObservableCollection<LK_Collegio> ItemsList = new ObservableCollection<LK_Collegio>();

				ExpressionBuilder<LK_Collegio> eb = new ExpressionBuilder<LK_Collegio>();

                ObjectQuery<LK_Collegio> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_Collegi.AsQueryable().Where(expQuery) as ObjectQuery<LK_Collegio>;
                    }
                    else
                    {
						oQItemsList = _context.LK_Collegi.AsQueryable() as ObjectQuery<LK_Collegio>;
                    }
                }
                else
					oQItemsList = _context.LK_Collegi.AsQueryable() as ObjectQuery<LK_Collegio>;

                filteredRowsCount = 0;

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_Collegio>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<LK_Collegio>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<LK_Collegio>(oQItemsList.ToList());
                }
                else
                    ItemsList =  new ObservableCollection<LK_Collegio>(oQItemsList.ToList());

                return ItemsList;
            }
        }

        public LK_Collegio GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.LK_Collegi.SingleOrDefault(rx => rx.Id_Collegio == id);
            }
        }

        public List<LK_Collegio> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.LK_Collegi.Where(rx => rx.Id_Collegio == id).ToList();
            }
		}

		public List<LK_Collegio> GetSingleItem(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Collegi.Where(rx => rx.Descrizione.Equals(text)).ToList();
			}
		}

        public int InsertOrUpdate(LK_Collegio objToUpdate)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (objToUpdate.Id_Collegio != 0)
                {
                    _context.AttachUpdated(objToUpdate);
                }
                else
                {
					_context.LK_Collegi.AddObject(objToUpdate);
                }

                _context.SaveChanges();

                return objToUpdate.Id_Collegio;
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

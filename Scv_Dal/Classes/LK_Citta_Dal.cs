using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Scv_Model.Common;
using Scv_Entities;
using System.Data.Objects;

namespace Scv_Dal
{
	public class LK_Citta_Dal
	{
		//Scv_Entities.SCV_DEVEntities _context = null;

		public LK_Citta_Dal()
        {
            //_context = new SCV_DEVEntities();
            //_context.ContextOptions.LazyLoadingEnabled = false;
        }

        public ObservableCollection<LK_Citta> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				ObservableCollection<LK_Citta> ItemsList = new ObservableCollection<LK_Citta>();

				ExpressionBuilder<LK_Citta> eb = new ExpressionBuilder<LK_Citta>();

                ObjectQuery<LK_Citta> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_Citta.AsQueryable().Where(expQuery) as ObjectQuery<LK_Citta>;
                    }
                    else
                    {
						oQItemsList = _context.LK_Citta.AsQueryable() as ObjectQuery<LK_Citta>;
                    }
                }
                else
					oQItemsList = _context.LK_Citta.AsQueryable() as ObjectQuery<LK_Citta>;

                filteredRowsCount = 0;
                //filteredRowsCount = oQItemsList.Count();

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_Citta>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<LK_Citta>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<LK_Citta>(oQItemsList.ToList());
                }
                else
                    ItemsList =  new ObservableCollection<LK_Citta>(oQItemsList.ToList());

                return ItemsList;
            }
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

		public List<LK_Citta> GetSingleItem(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Citta.Where(rx => rx.Nome.Equals(text)).ToList();
			}
		}

		public LK_Citta GetItemByText(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Citta.FirstOrDefault(x => x.Nome.ToUpper().Trim().Equals(text.ToUpper().Trim()));
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
                    //objToUpdate.DT_INS = objToUpdate.DT_UPD;
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

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
	public class LK_Titolo_Dal
	{
		//Scv_Entities.SCV_DEVEntities _context = null;

		public LK_Titolo_Dal()
        {
            //_context = new SCV_DEVEntities();
            //_context.ContextOptions.LazyLoadingEnabled = false;
        }

        public ObservableCollection<LK_Titolo> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				ObservableCollection<LK_Titolo> ItemsList = new ObservableCollection<LK_Titolo>();

				ExpressionBuilder<LK_Titolo> eb = new ExpressionBuilder<LK_Titolo>();

                ObjectQuery<LK_Titolo> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_Titoli.AsQueryable().Where(expQuery) as ObjectQuery<LK_Titolo>;
                    }
                    else
                    {
						oQItemsList = _context.LK_Titoli.AsQueryable() as ObjectQuery<LK_Titolo>;
                    }
                }
                else
					oQItemsList = _context.LK_Titoli.AsQueryable() as ObjectQuery<LK_Titolo>;

                filteredRowsCount = 0;
                //filteredRowsCount = oQItemsList.Count();

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_Titolo>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<LK_Titolo>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<LK_Titolo>(oQItemsList.ToList());
                }
                else
                    ItemsList =  new ObservableCollection<LK_Titolo>(oQItemsList.ToList());

                return new ObservableCollection<LK_Titolo>(ItemsList.Distinct().OrderBy(x => x.Sigla));
            }
        }

        public LK_Titolo GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.LK_Titoli.SingleOrDefault(rx => rx.Id_Titolo == id);
            }
        }

        public List<LK_Titolo> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.LK_Titoli.Where(rx => rx.Id_Titolo == id).ToList();
            }
		}

		public LK_Titolo GetItemByText(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Titoli.FirstOrDefault(x => x.Sigla.ToUpper().Trim().Equals(text.ToUpper().Trim()));
			}
		}

        public int InsertOrUpdate(LK_Titolo objToUpdate)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (objToUpdate.Id_Titolo != 0)
                {
                    _context.AttachUpdated(objToUpdate);
                }
                else
                {
                    //objToUpdate.DT_INS = objToUpdate.DT_UPD;
					_context.LK_Titoli.AddObject(objToUpdate);
                }

                _context.SaveChanges();

                return objToUpdate.Id_Titolo;
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

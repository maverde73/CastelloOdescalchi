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
	public class LK_Organizzazione_Dal
	{
		//Scv_Entities.SCV_DEVEntities _context = null;

		public LK_Organizzazione_Dal()
        {
            //_context = new SCV_DEVEntities();
            //_context.ContextOptions.LazyLoadingEnabled = false;
        }

        public ObservableCollection<LK_Organizzazione> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				ObservableCollection<LK_Organizzazione> ItemsList = new ObservableCollection<LK_Organizzazione>();

				ExpressionBuilder<LK_Organizzazione> eb = new ExpressionBuilder<LK_Organizzazione>();

                ObjectQuery<LK_Organizzazione> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_Organizzazioni.AsQueryable().Where(expQuery) as ObjectQuery<LK_Organizzazione>;
                    }
                    else
                    {
						oQItemsList = _context.LK_Organizzazioni.AsQueryable() as ObjectQuery<LK_Organizzazione>;
                    }
                }
                else
					oQItemsList = _context.LK_Organizzazioni.AsQueryable() as ObjectQuery<LK_Organizzazione>;

                filteredRowsCount = 0;
                //filteredRowsCount = oQItemsList.Count();

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_Organizzazione>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<LK_Organizzazione>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<LK_Organizzazione>(oQItemsList.ToList());
                }
                else
                    ItemsList =  new ObservableCollection<LK_Organizzazione>(oQItemsList.ToList());

                return ItemsList;
            }
        }

        public LK_Organizzazione GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.LK_Organizzazioni.SingleOrDefault(rx => rx.Id_Organizzazione == id);
            }
        }

        public List<LK_Organizzazione> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				return _context.LK_Organizzazioni.Where(rx => rx.Id_Organizzazione == id).ToList();
            }
		}

		public List<LK_Organizzazione> GetSingleItem(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_Organizzazioni.Where(rx => rx.Descrizione.Equals(text)).ToList();
			}
		}

		public LK_Organizzazione GetItemByText(string text)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return	_context.LK_Organizzazioni.FirstOrDefault(x => x.Descrizione.ToUpper().Trim().Equals(text.ToUpper().Trim()));
			}
		}

        public int InsertOrUpdate(LK_Organizzazione objToUpdate)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (objToUpdate.Id_Organizzazione != 0)
                {
                    _context.AttachUpdated(objToUpdate);
                }
                else
                {
                    //objToUpdate.DT_INS = objToUpdate.DT_UPD;
					_context.LK_Organizzazioni.AddObject(objToUpdate);
                }

                _context.SaveChanges();

                return objToUpdate.Id_Organizzazione;
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

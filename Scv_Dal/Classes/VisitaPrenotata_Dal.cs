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
	public class VisitaPrenotata_Dal
	{
		public VisitaPrenotata_Dal()
        {
        }

        public ObservableCollection<VisitaPrenotata> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber,out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				ObservableCollection<VisitaPrenotata> ItemsList = new ObservableCollection<VisitaPrenotata>();

				ExpressionBuilder<VisitaPrenotata> eb = new ExpressionBuilder<VisitaPrenotata>();

                ObjectQuery<VisitaPrenotata> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.VisitePrenotate.AsQueryable().Where(expQuery) as ObjectQuery<VisitaPrenotata>;
                    }
                    else
                    {
						oQItemsList = _context.VisitePrenotate.AsQueryable() as ObjectQuery<VisitaPrenotata>;
                    }
                }
                else
					oQItemsList = _context.VisitePrenotate.AsQueryable() as ObjectQuery<VisitaPrenotata>;

                filteredRowsCount = 0;
                //filteredRowsCount = oQItemsList.Count();

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<VisitaPrenotata>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<VisitaPrenotata>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<VisitaPrenotata>(oQItemsList.ToList());
                }
                else
                    ItemsList =  new ObservableCollection<VisitaPrenotata>(oQItemsList.ToList());

				ItemsList.ToList().ForEach(x => x.IsEmpty = false);
				ItemsList.ToList().ForEach(x => x.IsErasable = true);
				ItemsList.ToList().ForEach(x => x.IsLoadedFromDb = true);

				return ItemsList;
            }
        }

        public ObservableCollection<V_VisitePrenotate> GetVFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
        {
            //_context = new SCV_DEVEntities();

            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                ObservableCollection<V_VisitePrenotate> ItemsList = new ObservableCollection<V_VisitePrenotate>();

                ExpressionBuilder<V_VisitePrenotate> eb = new ExpressionBuilder<V_VisitePrenotate>();

                ObjectQuery<V_VisitePrenotate> oQItemsList = null;

                if (listArgument != null)
                {
                    if (listArgument.Count > 0)
                    {
                        var expQuery = eb.WhereExpression(listArgument);
                        oQItemsList = _context.V_VisitePrenotate.AsQueryable().Where(expQuery) as ObjectQuery<V_VisitePrenotate>;
                    }
                    else
                    {
                        oQItemsList = _context.V_VisitePrenotate.AsQueryable() as ObjectQuery<V_VisitePrenotate>;
                    }
                }
                else
                    oQItemsList = _context.V_VisitePrenotate.AsQueryable() as ObjectQuery<V_VisitePrenotate>;

                filteredRowsCount = 0;
                //filteredRowsCount = oQItemsList.Count();

                if (orderByField != null)
                {
                    oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<V_VisitePrenotate>;
                    if (pageSize > 0)
                        ItemsList = new ObservableCollection<V_VisitePrenotate>(oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList());
                    else
                        ItemsList = new ObservableCollection<V_VisitePrenotate>(oQItemsList.ToList());
                }
                else
                    ItemsList = new ObservableCollection<V_VisitePrenotate>(oQItemsList.ToList());

				ItemsList.ToList().ForEach(x => x.IsEmpty = false);
				ItemsList.ToList().ForEach(x => x.IsErasable = true);
				ItemsList.ToList().ForEach(x => x.IsLoadedFromDb = true);

                return ItemsList;
            }
        }

        public VisitaPrenotata GetItem(int id)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				VisitaPrenotata obj = _context.VisitePrenotate.SingleOrDefault(rx => rx.Id_VisitaPrenotata == id);
				obj.IsLoadedFromDb = true;
				return obj;
            }
        }

        public List<VisitaPrenotata> GetSingleItem(int id)
		{
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
				List<VisitaPrenotata> obj = _context.VisitePrenotate.Where(rx => rx.Id_VisitaPrenotata == id).ToList();
				obj.ForEach(x => x.IsLoadedFromDb = true);
				return obj;
            }
		}

        public int InsertOrUpdate(VisitaPrenotata objToUpdate)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
                if (objToUpdate.Id_VisitaPrenotata != 0)
                {
                    _context.AttachUpdated(objToUpdate);
                }
                else
                {
                    //objToUpdate.DT_INS = objToUpdate.DT_UPD;
					_context.VisitePrenotate.AddObject(objToUpdate);
                }

                _context.SaveChanges();

                return objToUpdate.Id_VisitaPrenotata;
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

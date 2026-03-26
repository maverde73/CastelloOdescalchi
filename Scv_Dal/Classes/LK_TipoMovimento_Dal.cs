using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class LK_TipoMovimento_Dal
	{

		public List<LK_TipoMovimento> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			//_context = new SCV_DEVEntities();

			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoMovimento> ItemsList = new List<LK_TipoMovimento>();

				ExpressionBuilder<LK_TipoMovimento> eb = new ExpressionBuilder<LK_TipoMovimento>();

				ObjectQuery<LK_TipoMovimento> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_TipiMovimento.AsQueryable().Where(expQuery) as ObjectQuery<LK_TipoMovimento>;
					}
					else
					{
						oQItemsList = _context.LK_TipiMovimento.AsQueryable() as ObjectQuery<LK_TipoMovimento>;
					}
				}
				else
					oQItemsList = _context.LK_TipiMovimento.AsQueryable() as ObjectQuery<LK_TipoMovimento>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_TipoMovimento>;
					if (pageSize > 0)
						ItemsList = oQItemsList.Skip(pageSize * pageNumber).Take(pageSize).ToList();
					else
						ItemsList = oQItemsList.ToList();
				}
				else
					ItemsList = oQItemsList.ToList();

				return ItemsList.Where(x => x.Fl_Attivo == true).ToList();
			}
		}        

		public List<LK_TipoMovimento> GetItems()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoMovimento> ItemsList = new List<LK_TipoMovimento>();

				ExpressionBuilder<LK_TipoMovimento> eb = new ExpressionBuilder<LK_TipoMovimento>();

				ObjectQuery<LK_TipoMovimento> oQItemsList = null;


				oQItemsList = _context.LK_Lingue.AsQueryable() as ObjectQuery<LK_TipoMovimento>;

				ItemsList = oQItemsList.ToList();

				return ItemsList.Where(x => x.Fl_Attivo == true).ToList();
			}
		}

		public List<LK_TipoMovimento> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiMovimento.Where(rx => rx.Id_TipoMovimento == id).ToList();
			}
		}

		public LK_TipoMovimento GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiMovimento.FirstOrDefault(rx => rx.Id_TipoMovimento == id);
			}
		}


		public List<LK_TipoMovimento> GetList()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiMovimento.Where(x => x.Fl_Attivo == true).ToList();
			}
		}

	}
}

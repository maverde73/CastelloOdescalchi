using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class LK_TipoPagamento_Dal
	{
		public List<LK_TipoPagamento> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoPagamento> ItemsList = new List<LK_TipoPagamento>();

				ExpressionBuilder<LK_TipoPagamento> eb = new ExpressionBuilder<LK_TipoPagamento>();

				ObjectQuery<LK_TipoPagamento> oQItemsList = null;

				if (listArgument != null)
				{
					if (listArgument.Count > 0)
					{
						var expQuery = eb.WhereExpression(listArgument);
						oQItemsList = _context.LK_TipiPagamento.AsQueryable().Where(expQuery) as ObjectQuery<LK_TipoPagamento>;
					}
					else
					{
						oQItemsList = _context.LK_TipiPagamento.AsQueryable() as ObjectQuery<LK_TipoPagamento>;
					}
				}
				else
					oQItemsList = _context.LK_TipiPagamento.AsQueryable() as ObjectQuery<LK_TipoPagamento>;

				filteredRowsCount = 0;

				if (orderByField != null)
				{
					oQItemsList = eb.OrderedQuery(oQItemsList, orderByField, orderByType) as ObjectQuery<LK_TipoPagamento>;
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

		public List<LK_TipoPagamento> GetItems()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoPagamento> ItemsList = new List<LK_TipoPagamento>();

				ExpressionBuilder<LK_TipoPagamento> eb = new ExpressionBuilder<LK_TipoPagamento>();

				ObjectQuery<LK_TipoPagamento> oQItemsList = null;

				oQItemsList = _context.LK_TipiPagamento.AsQueryable() as ObjectQuery<LK_TipoPagamento>;

				ItemsList = oQItemsList.ToList();

				return ItemsList;
			}
		}

		public LK_TipoPagamento GetItem(int paymentTypeID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiPagamento.FirstOrDefault(x => x.Id_TipoPagamento == paymentTypeID);
			}
		}



		public List<LK_TipoPagamento> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiPagamento.Where(rx => rx.Id_TipoPagamento == id).ToList();
			}
		}

		public List<LK_TipoPagamento> GetList()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiPagamento.ToList();
			}
		}
	}
}

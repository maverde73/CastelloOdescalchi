using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class LK_TipoConferma_Dal
	{
		public List<LK_TipoConferma> GetItems()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoConferma> ItemsList = new List<LK_TipoConferma>();

				ExpressionBuilder<LK_TipoConferma> eb = new ExpressionBuilder<LK_TipoConferma>();

				ObjectQuery<LK_TipoConferma> oQItemsList = null;

				oQItemsList = _context.LK_TipiConferma.AsQueryable() as ObjectQuery<LK_TipoConferma>;

				ItemsList = oQItemsList.ToList();

				return ItemsList;
			}
		}

		public List<LK_TipoConferma> GetSingleItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiConferma.Where(rx => rx.Id_TipoConferma == id).ToList();
			}
		}

		public LK_TipoConferma GetDefault()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiConferma.FirstOrDefault(rx => rx.Fl_Default == true);
			}
		}
	}
}

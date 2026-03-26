using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class LK_TipoArticolo_Dal
	{
		public List<LK_TipoArticolo> GetItems()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoArticolo> ItemsList = new List<LK_TipoArticolo>();

				ExpressionBuilder<LK_TipoArticolo> eb = new ExpressionBuilder<LK_TipoArticolo>();

				ObjectQuery<LK_TipoArticolo> oQItemsList = null;


				oQItemsList = _context.LK_TipiArticolo.AsQueryable() as ObjectQuery<LK_TipoArticolo>;

				ItemsList = oQItemsList.ToList();

				return ItemsList;
			}
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;
using Scv_Model.Common;
using System.Data.Objects;

namespace Scv_Dal
{
	public class GuidaLingua_Dal
	{
		public List<GuidaLingua> GetItems()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<GuidaLingua> ItemsList = new List<GuidaLingua>();

				ExpressionBuilder<GuidaLingua> eb = new ExpressionBuilder<GuidaLingua>();

				ObjectQuery<GuidaLingua> oQItemsList = null;


				oQItemsList = _context.GuideLingue.AsQueryable() as ObjectQuery<GuidaLingua>;

				ItemsList = oQItemsList.ToList();

				return ItemsList;
			}
		}

		public List<V_GuidaLingua> GetItemsByGuideID(int guideID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<V_GuidaLingua> ItemsList = new List<V_GuidaLingua>();

				ItemsList = _context.V_GuidaLingua.ToList().Where(x => x.Id_Guida == guideID).ToList();

				return ItemsList.Where(x => x.Id_Guida == guideID).ToList();
			}
		}

		public List<V_GuidaLingua> GetVList()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.V_GuidaLingua.ToList();
			}
		}
	}
}

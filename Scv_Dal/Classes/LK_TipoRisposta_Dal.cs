using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;

namespace Scv_Dal
{
	public class LK_TipoRisposta_Dal
	{

		public LK_TipoRisposta GetItem(int id)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiRisposta.SingleOrDefault(rx => rx.Id_TipoRisposta == id);
			}
		}

		public List<LK_TipoRisposta> GetItems()
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_TipiRisposta.Where(x => x.Fl_Attivo == true).OrderBy(x => x.Ordine).ToList();
			}
		}

		public List<LK_TipoRisposta> GetList(int firstItemID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				List<LK_TipoRisposta> list = new List<LK_TipoRisposta>();
				list.AddRange(_context.LK_TipiRisposta.Where(x => x.Id_TipoRisposta != firstItemID && x.Fl_Attivo == true).OrderBy(x => x.Ordine).ToList());
				LK_TipoRisposta obj = GetItem(firstItemID);
				if(obj != null)
					list.Insert(0, obj);
				return list;
			}
		}

	}
}

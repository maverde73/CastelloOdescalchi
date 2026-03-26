using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;

namespace Scv_Dal
{
	public class LK_DescrizioneLingua_Dal
	{
		public LK_DescrizioneLingua GetItemByLanguageID(int languageID, int descriptionLanguageID)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.LK_DescrizioneLingua.FirstOrDefault(rx => rx.Id_Lingua == languageID && rx.Id_LinguaDescrizione == descriptionLanguageID);
			}
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Entities;

namespace Scv_Dal
{
	public class Parametri_Dal
	{

		public Parametri GetItem(string key)
		{
			using (IN_VIAEntities _context = new IN_VIAEntities())
			{
				return _context.Parametris.SingleOrDefault(rx => rx.Chiave == key);
			}
		}


        public LK_TipoVisita GetTipoVisitaByIdPrenotazione(int idPrenotazione)
        {
            using (IN_VIAEntities _context = new IN_VIAEntities())
            {
               Prenotazione prenotazione = _context.Prenotazioni.FirstOrDefault(prx => prx.Id_Prenotazione == idPrenotazione);
               return _context.LK_TipiVisita.FirstOrDefault(tvx => tvx.Id_TipoVisita == prenotazione.Id_TipoVisita);
            }
        }

	}
}

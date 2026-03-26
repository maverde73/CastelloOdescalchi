using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public enum TipoRisposta
	{
		RichiestaIncompleta = 1,
		VisitaProgrammata = 2,
		ConfermaNoRicevuta = 10,
		ConfermaRicevuta = 3,
		AnnullataDaUfficioScavi = 6,
		NecropoliChiusa = 7,
		VisiteComplete = 8,
		DaLavorare = 9
	}
}

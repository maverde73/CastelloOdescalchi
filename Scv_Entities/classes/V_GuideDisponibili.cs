using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public partial class V_GuideDisponibili
	{
		public string Nominativo
		{
			get { return (Cognome != null ? Cognome + " " : string.Empty) + (Nome != null ? Nome : string.Empty); }
		}
	}
}

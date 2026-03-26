using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public partial class V_GuideIncaricate
	{
		public string Nominativo
		{
			get { return this.Cognome + " " + this.Nome; }
		}
	}
}

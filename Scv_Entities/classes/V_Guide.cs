using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public partial class V_Guide
	{

		public string Nominativo
		{
			get { return this.Cognome + " " + this.Nome; }
		}

		public string Stato
		{
			get { return this.Fl_Attivo == true ? "Attivo" : "Non attivo"; }
		}

		public string Capofila
		{
			get { return this.Fl_Capofila == true ? "Si" : "no"; }
		}


	}
}

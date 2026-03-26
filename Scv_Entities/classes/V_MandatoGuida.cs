using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public partial class V_MandatoGuida
	{
		public string Nominativo
		{
			get { return this.Cognome + " " + this.Nome; }
		}

		public decimal TotaleComplessivo
		{
			get { return (this.TotaleNecropoli != null ? (decimal)this.TotaleNecropoli : 0) + (this.TotaleAltro != null ? (decimal)this.TotaleAltro : 0); }
		}

		public int NumeroComplessivo
		{
			get { return (this.NrNecropoli != null ? (int)this.NrNecropoli : 0) + (this.NrAltro != null ? (int)this.NrAltro : 0); }
		}

		public decimal Saldo
		{
			get 
			{
				decimal acc = this.AccontoAltro != null ? (decimal)this.AccontoAltro : 0;
				decimal sal = ((acc <= TotaleComplessivo) ? TotaleComplessivo - acc : 0);

				return sal; 
			}
		}

		//public V_MandatoGuida()
		//{

		//    this.NrNecropoli = 0;
		//    this.TotaleNecropoli = 0;
		//    this.NrAltro = 0;
		//    this.TotaleAltro = 0;
		//    this.AccontoAltro = 0;

		//}
	}
}

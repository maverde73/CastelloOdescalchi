using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class EmailBody
	{
		#region Private Fields

		private List<string> testiStandard = null;

		#endregion// Private Fields



		#region Public Properties

		public string Mittente { get; set; }

		public string TipoRisposta { get; set; }

		public string Richiedente { get; set; }

		public List<string> TestiStandard
		{
			get
			{
				if (testiStandard == null)
					testiStandard = new List<string>();
				return testiStandard;
			}
			set { testiStandard = value; }
		}
		
		#endregion// Public Properties



		#region Constructors

		public EmailBody()
		{ }

		public EmailBody(string tipoRIsposta, string richiedente, List<string> testiStandard)
		{
			this.TipoRisposta = TipoRisposta;
			this.Richiedente = richiedente;
			this.TestiStandard = testiStandard;
		}

		#endregion// Constructors
	}
}

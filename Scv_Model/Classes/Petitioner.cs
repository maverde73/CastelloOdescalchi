using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class Petitioner : Person, IDataErrorInfo
	{
		public int ID { get; set; }
		public string Notes { get; set; }

		public Petitioner()
		{
			this.ID = 0;
			this.Notes = string.Empty;
		}

		public string Error
		{
			get { throw new NotImplementedException(); }
		}

		public string this[string columnName]
		{
			get
			{
				string result = null;

				switch (columnName)
				{
					case "Name":
						if (String.IsNullOrEmpty(Name))
							result = "Nome obbligatorio";
						break;

					case "MotherLanguageID":
						if (MotherLanguageID < 1)
							result = "Linguaggio obbligatorio";
						break;
				}

				return result;
			}
		}

	}
}

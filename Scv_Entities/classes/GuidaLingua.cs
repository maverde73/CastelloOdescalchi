using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Scv_Entities
{
	public partial class GuidaLingua : EntityObject
	{
		public GuidaLingua()
		{

		}

		public GuidaLingua(int id_GuidaLingua, int id_Guida, int id_Lingua)
		{
			this._Id_GuidaLingua = id_GuidaLingua;
			this.Id_Guida = id_Guida;
			this.Id_Lingua = id_Lingua;
		}

		public GuidaLingua(int id_Lingua, bool isDefault)
		{
			this.Id_Lingua = id_Lingua;
			this.Fl_Madre = isDefault;
		}

	}
}

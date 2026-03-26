using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class Address
	{
		public Address()
		{
			this.Country = string.Empty;
			this.Region = string.Empty;
			this.Province = string.Empty;
			this.Town = string.Empty;
			this.StreetName = string.Empty;
			this.StreetNumber = string.Empty;
			this.Cap = string.Empty;
			this.PersonalPhone = string.Empty;
			this.HomePhone = string.Empty;
			this.OfficePhone = string.Empty;
			this.Email = string.Empty;
		}

		#region Public Properties

		public string Country { get; set; }
		public string Region { get; set; }
		public string Province { get; set; }
		public string Town { get; set; }
		public string StreetName { get; set; }
		public string StreetNumber { get; set; }
		public string Cap { get; set; }
		public string PersonalPhone { get; set; }
		public string HomePhone { get; set; }
		public string OfficePhone { get; set; }
		public string Email { get; set; }

		#endregion
	}
}

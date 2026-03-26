using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scv_Model
{
	public class Person : Detail
	{
		private Address address = null;

		public Person()
		{
			Name = string.Empty;
			Surname = string.Empty;
			Title = string.Empty;
			Address = new Address();
			MotherLanguageID = 0;
			MotherLanguage = string.Empty;
		}
		#region Public Properties

		public string Name { get; set; }
		public string Surname { get; set; }
		public string Title { get; set; }
		public Address Address
		{
			get
			{
				if (address == null)
					address = new Address();
				return address;
			}
			set { address = value; }
		}
		public int MotherLanguageID{get;set;}
		public string MotherLanguage { get; set; }
		public string CompleteAddress
		{
			get
			{
				StringBuilder strAddress = new StringBuilder();
				strAddress.Append(Address.StreetName);
				strAddress.Append(Address.StreetNumber.Length > 0 ? ", " + Address.StreetNumber : string.Empty);

				StringBuilder cntAddress = new StringBuilder();
				cntAddress.Append(Address.Cap);
				cntAddress.Append(Address.Province.Length > 0 ? " " + Address.Province : string.Empty);

				StringBuilder completeAddress = new StringBuilder();
				completeAddress.Append(strAddress.ToString());
				completeAddress.Append(cntAddress.ToString().Length > 0 ? " - " + cntAddress.ToString() : string.Empty);

				return completeAddress.ToString();
			}
		}

		#endregion

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Presentation
{
	public class PetitionerSearchItem : EntityObject
	{
		public int ID { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}

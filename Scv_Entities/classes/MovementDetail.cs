using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Entities
{
	public class MovementDetail
	{
		public int ID { get; set; }
		public int MovementID { get; set; }
		public int MovementTypeID { get; set; }
		public int GuideID { get; set; }
		public string ISBN { get; set; }
		public int ArticleID { get; set; }
		public string Article { get; set; }
		public string ArticleType { get; set; }
		public decimal? Quantity { get; set; }
		public decimal PublicPrice { get; set; }
		public decimal? Price { get; set; }
		public decimal? Total 
		{
			get 
			{
				bool Calc =
					MovementTypeID == 3
					||
					MovementTypeID == 7
					||
					MovementTypeID == 8
					||
					MovementTypeID == 9
					||
					MovementTypeID == 11
					||
					MovementTypeID == 12
					||
					MovementTypeID == 14
					;
				return Calc? (Quantity * Price) : null;
			
			}		
		} 
	}
}

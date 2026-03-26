using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class PagerData
	{
		private List<int> range = null;

		private List<DateItem> dates = null;



		public List<int> Range
		{
			get
			{
				if (range == null)
					range = new List<int>();
				return range;
			}
			set { range = value; }
		}

		public List<DateItem> Dates
		{
			get
			{
				if (dates == null)
					dates = new List<DateItem>();
				return dates;
			}
			set { dates = value; }
		}

		public int PageSize { get; set; }

		public int PageIndex { get; set; }
	}

	public class DateItem
	{
		public DateTime Date { get; set; }
		public int DateRecords { get; set; }

		public  DateItem()
		{
		}

		public DateItem(DateTime date, int dateRecords)
		{
			this.Date = date;
			this.DateRecords = dateRecords;
		}
	}
}

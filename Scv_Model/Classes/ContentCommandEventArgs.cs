using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model
{
	public class ContentCommandEventArgs
	{
		#region Private Fields

		private BaseFilter filter = null;

		#endregion// Private Fields



		#region Public Properties

		public ContentCommandType CommandType { get; set; }

		public BaseFilter Filter
		{
			get
			{
				if (filter == null)
					filter = new BaseFilter();
				return filter;
			}
			set { filter = value; }
		}

		#endregion// Public Properties


		#region Constructors

		public ContentCommandEventArgs()
		{}

		#endregion// Constructors



	}
}

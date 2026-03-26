using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;

namespace Scv_Model.Common
{
	class PagedCollectionView : IPagedCollectionView 
	{
		public bool CanChangePage
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsPageChanging
		{
			get { throw new NotImplementedException(); }
		}

		public int ItemCount
		{
			get { throw new NotImplementedException(); }
		}

		public bool MoveToFirstPage()
		{
			throw new NotImplementedException();
		}

		public bool MoveToLastPage()
		{
			throw new NotImplementedException();
		}

		public bool MoveToNextPage()
		{
			throw new NotImplementedException();
		}

		public bool MoveToPage(int pageIndex)
		{
			throw new NotImplementedException();
		}

		public bool MoveToPreviousPage()
		{
			throw new NotImplementedException();
		}

		public event EventHandler<EventArgs> PageChanged;

		public event EventHandler<PageChangingEventArgs> PageChanging;

		public int PageIndex
		{
			get { throw new NotImplementedException(); }
		}

		public int PageSize
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int TotalItemCount
		{
			get { throw new NotImplementedException(); }
		}
	}
}

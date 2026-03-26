using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scv_Model.Common;
using System.ComponentModel;

namespace Scv_Model
{
	public class BaseFilter : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events



		#region Private Fields

		private List<string> sort = new List<string>();
		private List<MethodArgument> args = new List<MethodArgument>();
		private SortDirection sortDirection;
		private int pageSize = 0;
		private int pageNumber = 0;

		#endregion// Private Fields



		#region Public Properties

		public string[] Sort
		{
			get { return sort.ToArray(); }
		}

		public List<MethodArgument> Args
		{
			get { return args; }
			set { args = value; OnPropertyChanged(this, "Args"); }
		}

		public SortDirection SortDirection
		{
			get { return sortDirection; }
			set { sortDirection = value; OnPropertyChanged(this, "SortDirection"); } 
		}

		public int PageSize
		{
			get { return pageSize; }
			set { pageSize = value; OnPropertyChanged(this, "PageSize"); }
		}

		public int PageNumber
		{
			get { return pageNumber; }
			set { pageNumber = value; OnPropertyChanged(this, "PAgeNumber"); }
		}

		#endregion// Puiblic Properties



		#region Public Methods

		public void AddSortField(string sortField)
		{
			if (!Sort.Contains(sortField))
			{
				sort.Add(sortField);
				OnPropertyChanged(this, "Args");
			}
		}

		public void RemoveSortField(string sortField)
		{
			sort.Remove(sortField);
			OnPropertyChanged(this, "Args");
		}

		public void SetFilter(string field, Utilities.ValueType valueType, List<object> values)
		{			
			for(int i = 0; i < Args.Count; i++)
			{
				if (Args[i].Field == field)
				{
					Args.RemoveAt(i);
					break;
				}
			}

			MethodArgument arg = new MethodArgument();
			arg.Field = field;
			arg.ValueType = valueType;
			arg.Values = values;
			Args.Add(arg);
			OnPropertyChanged(this, "Args");
		}

		public void SetFilter(string field, Utilities.ValueType valueType, object value)
		{
			for (int i = 0; i < Args.Count; i++)
			{
				if (Args[i].Field == field)
				{
					Args.RemoveAt(i);
					break;
				}
			}

			MethodArgument arg = new MethodArgument();
			arg.Field = field;
			arg.ValueType = valueType;
			arg.Values = new List<object>() { value };
			Args.Add(arg);
			OnPropertyChanged(this, "Args");
		}

		public void SetFilter(string field, Utilities.ValueType valueType, List<object> values, Utilities.SQLOperator sqlOperator)
		{
			for (int i = 0; i < Args.Count; i++)
			{
				if (Args[i].Field == field)
				{
					Args.RemoveAt(i);
					break;
				}
			}

			MethodArgument arg = new MethodArgument();
			arg.Field = field;
			arg.ValueType = valueType;
			arg.Values = values;
			arg.SQLOperator = sqlOperator;
			Args.Add(arg);
			OnPropertyChanged(this, "Args");
		}

		public void SetFilter(string field, Utilities.ValueType valueType, object value, Utilities.SQLOperator sqlOperator)
		{
			for (int i = 0; i < Args.Count; i++)
			{
				if (Args[i].Field == field)
				{
					Args.RemoveAt(i);
					break;
				}
			}

			MethodArgument arg = new MethodArgument();
			arg.Field = field;
			arg.ValueType = valueType;
			arg.Values = new List<object>() { value };
			arg.SQLOperator = sqlOperator;
			Args.Add(arg);
			OnPropertyChanged(this, "Args");
		}

		public void AddFilter(string field, Utilities.ValueType valueType, List<object> values)
		{
			foreach (MethodArgument a in Args)
				if (a.Field == field)
					return;

			MethodArgument arg = new MethodArgument();
			arg.Field = field;
			arg.ValueType = valueType;
			arg.Values = values;
			Args.Add(arg);
			OnPropertyChanged(this, "Args");
		}

		public void AddFilter(string field, Utilities.ValueType valueType, object value)
		{
			foreach (MethodArgument a in Args)
				if (a.Field == field)
					return;

			MethodArgument arg = new MethodArgument();
			arg.Field = field;
			arg.ValueType = valueType;
			arg.Values = new List<object>() { value };
			Args.Add(arg);
			OnPropertyChanged(this, "Args");
		}

		public void RemoveFilter(string fieldName)
		{
			for (int i = 0; i < Args.Count; i++)
			{
				if (Args[i].Field == fieldName)
				{
					Args.RemoveAt(i);
					OnPropertyChanged(this, "Args");
					break;
				}
			}
		}

		public MethodArgument GetFilter(string fieldName)
		{
			return Args.FirstOrDefault(x => x.Field == fieldName);
		}

		public List<MethodArgument> GetFilters()
		{
			return Args;
		}

		#endregion// Public Methods


		#region Event Handlers

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handlers
	}

	public enum SortDirection
	{
		ASC,
		DESC
	}
}

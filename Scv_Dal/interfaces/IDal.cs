using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Scv_Model.Common;
using System.Data.Objects.DataClasses;

namespace Scv_Dal
{
	public interface IDal
	{
		ObservableCollection<EntityObject> GetFilteredList(List<MethodArgument> listArgument, string[] orderByField, string orderByType, int pageSize, int pageNumber, out int filteredRowsCount);
		EntityObject GetItem(int id);
		List<EntityObject> GetSingleItem(int id);
		List<EntityObject> GetSingleItem(string text);
		int InsertOrUpdate(EntityObject objToUpdate);
		void SaveObject();

	}
}

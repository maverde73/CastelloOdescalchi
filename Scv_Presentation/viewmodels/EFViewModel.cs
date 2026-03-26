
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Telerik.Windows.Data;
using Scv_Entities;
using Scv_Dal;
using System.Data.Objects;

namespace Presentation
{
    public class EFViewModel
    {
        private readonly QueryableEntityCollectionView<Richiedente> richiedentiView;
        private readonly Scv_Entities.IN_VIAEntities objectContext = new IN_VIAEntities();


        public EFViewModel()
		{
			//this.objectContext = new NorthwindEntities();
            ObjectQuery<Richiedente> a = new ObjectQuery<Richiedente>("SELECT * FROM RICHIEDENTE WHERE Id_Richiedente > 0", objectContext);
            this.richiedentiView = new QueryableEntityCollectionView<Richiedente>(a, "Richiedenti", new List<string>());
		}

        public QueryableEntityCollectionView<Richiedente> RichiedentiView
		{
            get { return this.richiedentiView; }
		}
    }
}

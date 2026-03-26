using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Scv_Entities;
using Scv_Dal;
using Telerik.Windows.Controls;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for PetitionerManager.xaml
	/// </summary>
	public partial class PetitionerManager : BaseControl, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion // Events


		#region Properties

		public Richiedente SelectedItem
		{
			get { return ((PetitionerViewModel)DataContext).SelectedItem; }
			set { ((PetitionerViewModel)DataContext).SelectedItem = value; }
		}

		public LK_Citta Location
		{
			get { return lcm.SelectedItem; }
		}

		public LK_Titolo Title
		{
			get { return tmn.SelectedItem; }
		}

		public LK_Organizzazione Organization
		{
			get { return omn.SelectedItem; }
		}

		public LK_Lingua Language
		{
			get { return lmn.SelectedItem; }
		}

		#endregion // Properties


		#region Constructors

		public PetitionerManager()
		{
			DataContext = new PetitionerViewModel();
			((PetitionerViewModel)this.DataContext).PropertyChanged += new PropertyChangedEventHandler(OnViewModelPropertyChanged);
			
			InitializeComponent();			
		}

		#endregion // Constructors


		#region Event Handling

		private void ControlValueChanged(object sender, EventArgs e)
		{
			Dictionary<object, object> d = Helper.GetControlValue(sender);
			if (d != null)
			{
				switch (d["name"].ToString())
				{
					case "cmb":
						SelectedItem.Nome = d["value"].ToString();
						break;
				}
			}
		}

		protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, e);
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SelectedItem":
					if (lcm.SelectedItem != null && SelectedItem.Id_Citta != null)
						lcm.SelectItem((int)SelectedItem.Id_Citta);

					if (tmn.SelectedItem != null && SelectedItem.Id_Titolo != null)
						tmn.SelectItem((int)SelectedItem.Id_Titolo);

					if (omn.SelectedItem != null && SelectedItem.Id_Organizzazione!= null)
						omn.SelectItem((int)SelectedItem.Id_Organizzazione);

					if (lmn.SelectedItem != null && SelectedItem.Id_LinguaAbituale != null)
						lmn.SelectItem((int)SelectedItem.Id_LinguaAbituale);

					break;
			}

			if (PropertyChanged != null)
				PropertyChanged(sender, e);

		}

		private void cmb_SelectionChanged(object sender, EventArgs e)
		{
			SelectedItem = ((PetitionerViewModel)DataContext).AutoCompleteBoxSelectedItem;
		}

		private void cmb_SearchTextChanged(object sender, EventArgs e)
		{
			RadAutoCompleteBox rac = sender as RadAutoCompleteBox;

			if (rac != null)
			{
				if (rac.FilteredItems.GetType().Name == "WhereSelectEnumerableIterator`2")
				{
					BackgroundWorker SearchPetitionerWorker = new BackgroundWorker();
					SearchPetitionerWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(((PetitionerViewModel)DataContext).GetItemByText);
					SearchPetitionerWorker.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(SearchItemCompleted);
					SearchPetitionerWorker.RunWorkerAsync(rac.SearchText);
				}
			}
		}

		private void SearchItemCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var res = e.Result;
			if (res != null)
				SelectedItem = (Richiedente)res;
			else
			{
				SelectedItem = new Richiedente();
				SelectedItem.Cognome = cmb.SearchText;
			}
		}

		#endregion // Event Handling


		#region Public Methods

		public void SelectItem(int itemID)
		{
			((PetitionerViewModel)DataContext).SelectItem(itemID);
            if (!string.IsNullOrEmpty(((PetitionerViewModel)DataContext).SelectedItem.Nome))
                cmb.SearchText = ((PetitionerViewModel)DataContext).SelectedItem.Nome;
            else
                cmb.SearchText = "";
		}

		public override void UpdateChildren()
		{
			Location_Dal locDal = new Location_Dal();
			SelectedItem.Id_Citta = locDal.InsertOrUpdate(Location);

			LK_Titolo_Dal titDal = new LK_Titolo_Dal();
			SelectedItem.Id_Titolo = titDal.InsertOrUpdate(Title);

			LK_Organizzazione_Dal orgDal = new LK_Organizzazione_Dal();
			SelectedItem.Id_Organizzazione = orgDal.InsertOrUpdate(Organization);

			//LK_Lingua_Dal lngDal = new LK_Lingua_Dal();
			SelectedItem.Id_LinguaAbituale = lmn.SelectedItem.Id_Lingua;
		}

		#endregion// Public Methods
	}
}

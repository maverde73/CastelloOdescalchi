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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Scv_Dal;
using Scv_Entities;
using ShiftManager;
using System.Data.Objects.DataClasses;
using Scv_Model;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndGuideDetail.xaml
	/// </summary>
	public partial class wndGuideDetail : BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private List<ShiftItem> days = null;

		private List<DisabledShiftItem> closings = null;

		private int _errors = 0;

        GuideViewModel pVM = null;

		#endregion// Private fields



		#region Public Properties

		public ObservableCollection<ValidationError> ValidationErrors
		{
			get
			{
				if (validationErrors == null)
					validationErrors = new ObservableCollection<ValidationError>();
				return validationErrors;
			}
			set { validationErrors = value; }
		}

		public List<ShiftItem> Days
		{
			get
			{
				if (days == null)
					days = new List<ShiftItem>();
				return days;
			}
			set { days = value; }
		}

		public List<DisabledShiftItem> Closings
		{
			get
			{
				if (closings == null)
					closings = new List<DisabledShiftItem>();
				return closings;
			}
			set { closings = value; }
		}

		#endregion // Properties



		#region Main Binding Properties

		CollectionViewSource cvsGuide = null;

		#endregion// Main Binding Properties



		#region Constructors

		public wndGuideDetail(int detailID)
		        : base(detailID)
		{

			InitializeComponent();

			cvsGuide = (CollectionViewSource)FindResource("cvsGuide");

            pVM = new GuideViewModel(detailID);

			BaseFilter filter = new BaseFilter();
			filter.SortDirection = SortDirection.ASC;
			int count = 0;
			List<LK_Chiusura> ch = new LK_Chiusura_Dal().GetFilteredList(filter.Args, filter.Sort, filter.SortDirection.ToString(), filter.PageSize, filter.PageNumber, out count);
			DisabledShiftItem di = null;
			foreach (LK_Chiusura c in ch)
			{
				di = new DisabledShiftItem();
				di.Year = c.Anno;
				di.Month = c.Mese;
				di.Day = c.Giorno;
				di.IsCyclic = di.Month == 0;
				di.Am = !c.Fl_AM;
				di.Pm = !c.Fl_PM;
				Closings.Add(di);
			}

			List<GuidaDisponibile> gd = new GuidaDisponibile_Dal().GetShiftsByGuideID(DetailID);
			List<ShiftItem> days = new List<ShiftItem>();
			ShiftItem si = null;
			foreach (GuidaDisponibile g in gd)
			{
				si = new ShiftItem();
				si.ID = g.Id_GuidaDisponibile;
				si.Year = g.Anno;
				si.Month = g.Mese;
				si.Day = g.Giorno;
				si.Am = g.Fl_AM != null ? (bool)g.Fl_AM : false;
				si.Pm = g.Fl_PM != null ? (bool)g.Fl_PM : false;
				days.Add(si);
			}
						
			ShiftMng.DisabledItems = Closings;
			ShiftMng.AddDays(days);

			cvsGuide.Source = pVM.SrcGuide;
			if (((List<Guida>)pVM.SrcGuide)[0].Fl_Capofila == true)
				chkCapofila.IsEnabled = false;


			cmbGuideTitle.DataContext = pVM;
			cmbGuideCollege.DataContext = pVM;
			lstLanguages.DataContext = pVM;

            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

			
		}


		#endregion // Constructors



		#region Event Handling

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
            string message = string.Empty;

            if (ValidationErrors.Count > 0)
                foreach (ValidationError err in ValidationErrors)
                    message += "\n" + err.ErrorContent.ToString();

            if (message.Length > 0)
            {
                message = "Impossibile continuare a causa dei seguenti errori:" + message;
                MessageBox.Show(message, "Errori", MessageBoxButton.OK);
                return;
            }

            Guida_Dal guiDal = new Guida_Dal();

            Guida Guide = ((List<Guida>)cvsGuide.Source)[0];
			Guide.Id_User = User.Id_User;
			Guide.Nome = Helper_Dal.UpperCaseWords(Guide.Nome);
			Guide.Cognome = Helper_Dal.UpperCaseWords(Guide.Cognome);

			if (Guide.Id_Collegio == 0) Guide.Id_Collegio = null;

			List<GuidaLingua> GuideLanguages = new List<GuidaLingua>();
			foreach (LK_Lingua l in pVM.AvailableLanguages)
				if (l.IsSelected)
					GuideLanguages.Add(new GuidaLingua(l.Id_Lingua, l.IsDefault));

			if (GuideLanguages.Count == 0 || GuideLanguages.FirstOrDefault(x => x.Fl_Madre == true) == null)
			{
				MessageBox.Show("Impossibile salvare la guida: Non ha lingue associate oppure non è stata scelta una lingua madre.", "Errore lingue guida");
				return;
			}

			DetailID = guiDal.InsertOrUpdate(Guide, GetGuideShifts(), GuideLanguages);

            this.Close();
		}
    
		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Eliminare il record? (L'azione non è annullabile)", "Eliminazione record", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				Guida_Dal guiDal = new Guida_Dal();

				Guida Guide = ((List<Guida>)cvsGuide.Source)[0];

				guiDal.DeleteObject(Guide);

				this.Close();
			}
		}

		private void radDefaultLanguage_Click(object sender, RoutedEventArgs e)
		{
			RadioButton rbt = sender as RadioButton;
			if (rbt != null)
			{
				int id = 0;
				int.TryParse(rbt.CommandParameter.ToString(), out id);
				foreach (LK_Lingua l in pVM.AvailableLanguages)
				{
					if (l.Id_Lingua == id)
					{
						l.IsDefault = true;
						l.IsSelected = true;
						break;
					}
				}
			}
		}

		private void chkAll_Checked(object sender, RoutedEventArgs e)
		{
			CheckBox chk = sender as CheckBox;
			if (chk != null)
				if ((bool)chkAll.IsChecked)
					foreach (LK_Lingua l in pVM.AvailableLanguages)
						l.IsSelected = true;
		}	
		
		#endregion // Event Handling



		#region Public Methods


		#endregion// Public Methods



		#region Private Methods

		private List<GuidaDisponibile> GetGuideShifts()
		{
			List<GuidaDisponibile> list = new List<GuidaDisponibile>();
			List<ShiftItem> shifts = ShiftMng.GetShifts();
			GuidaDisponibile obj = null;

			foreach (ShiftItem si in shifts)
			{
				obj = new GuidaDisponibile();
				obj.Id_GuidaDisponibile = si.ID;
				obj.Id_Guida = DetailID;
				obj.Anno = (short)si.Year;
				obj.Mese = (short)si.Month;
				obj.Giorno = (short)si.Day;
				obj.Fl_AM = si.Am;
				obj.Fl_PM = si.Pm;
				list.Add(obj);
			}

			return list;
		}

		#endregion// Private Methods



		#region Overrides

		protected override void SetLayout()
		{
            //btnSave.Visibility = DetailID == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            //btnUpdate.Visibility = DetailID > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            //btnDelete.Visibility = DetailID > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
		}

		#endregion// Overrides



		#region Error Handling

		private void Confirm_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _errors == 0;
			e.Handled = true;
		}

		private void Confirm_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
		}

		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			if (e.Action == ValidationErrorEventAction.Added)
			{
				ValidationErrors.Add(e.Error);
				_errors++;
			}
			else
			{
				ValidationErrors.Remove(e.Error);
				_errors--;
			}
		}

		#endregion// Error Handling

	}
}
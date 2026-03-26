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
using System.Collections.ObjectModel;
using Scv_Dal;
using Scv_Entities;
using System.ComponentModel;

namespace Presentation.Pages
{
	/// <summary>
	/// Interaction logic for pgSettingsPersonal.xaml
	/// </summary>
	public partial class pgSettingsPersonal : BaseContentPage, INotifyPropertyChanged
	{
		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

        UserViewModel pVM = null;

		#endregion// Private fields



		#region Public Properties

		public LK_User SelectedItem
		{
			get { return this.User; }
			set { this.User = value; OnPropertyChanged(this, "SelectedItem"); }
		}


		#endregion // Properties



		#region Main Binding Properties


		#endregion// Main Binding Properties



		#region Constructors

		public pgSettingsPersonal()
		        : base()
		{
			this.DataContext = this;
			InitializeComponent();

			btnSave.Click += new RoutedEventHandler(btnSave_Click);
			this.BasePropertyChanged += new PropertyChangedEventHandler(pgSettingsPersonal_OnBasePropertyChanged);
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

            LK_User_Dal dal = new LK_User_Dal();

			DetailID = dal.InsertOrUpdate(User);
		}

		void pgSettingsPersonal_OnBasePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string propertyName = e.PropertyName;
			if (propertyName == "User")
				propertyName = "SelectedItem";
			OnPropertyChanged(sender, propertyName);
		}

		protected void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

    
		#endregion // Event Handling



		#region Public Methods


		#endregion// Public Methods



		#region Private Methods

		#endregion// Private Methods



		#region Overrides

		#endregion// Overrides



		#region Error Handling

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
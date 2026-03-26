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
using Presentation.viewmodels;
using System.IO;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndExtractPaymentData.xaml
	/// </summary>
	public partial class wndExtractPaymentData: BaseDetailPage, INotifyPropertyChanged
	{
		#region Events

		#endregion 



		#region Private Fields

		private ObservableCollection<ValidationError> validationErrors = null;

		private int _errors = 0;

		ExtractPaymentDataViewModel vm = null;

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

		#endregion // Properties




		#region Constructors

		public wndExtractPaymentData()
		        : base()
		{

			vm = new ExtractPaymentDataViewModel();
			this.DataContext = vm;
			InitializeComponent();

			btnExtract.Click += new RoutedEventHandler(btnExtract_Click);
			btnExport.Click += new RoutedEventHandler(btnExport_Click);

		}

		#endregion // Constructors



		#region Event Handling

		void btnExtract_Click(object sender, RoutedEventArgs e)
		{
			DoExtract();
		}

		void btnExport_Click(object sender, RoutedEventArgs e)
		{
			DoSave();
		}

		#endregion // Event Handling



		#region Public Methods


		#endregion// Public Methods



		#region Private Methods

		private void DoExtract()
		{
			vm.ExtractData();
		}

		private void DoSave()
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

			dlg.DefaultExt = "csv";

			dlg.FileName = "export";

			Nullable<bool> result = dlg.ShowDialog();

			if (result == true)
			{
				using (StreamWriter sw = new StreamWriter(dlg.FileName))
				{
					foreach(String s in vm.CSVData)
						sw.WriteLine(s);

					sw.Close();
				}				
			}
		}

		#endregion// Private Methods



		#region Overrides

		protected override void SetLayout()
		{

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
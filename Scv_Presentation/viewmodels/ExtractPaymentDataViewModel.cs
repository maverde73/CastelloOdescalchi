using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Scv_Model;
using Scv_Model.Classes;
using Scv_Dal;
using Scv_Entities;

namespace Presentation.viewmodels
{
	public class ExtractPaymentDataViewModel : INotifyPropertyChanged
	{
		#region Public Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion// Public Events




		#region Private Fields

		private DateTime objSearchDateStart;

		private DateTime objSearchDateEnd;
		
		private DateTime objScanDateStart;

		private CsvFieldSeparationTypeItem objSeparationType;

		private ExportType objExportType;

		private List<PaymentDataItem> objPaymentData = null;

		private List<string> csvData = null;

		private List<ExportType> availableExportTypes = null;

		private List<CsvFieldSeparationTypeItem> availableSeparationTypes = null;

		#endregion// Private Fields





		#region Properties

		bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public DateTime ObjSearchDateStart
		{
			get { return objSearchDateStart; }
			set { objSearchDateStart = value; OnPropertyChanged(this, "ObjSearchDateStart"); }
		}

		public DateTime ObjSearchDateEnd
		{
			get { return objSearchDateEnd; }
			set { objSearchDateEnd = value; OnPropertyChanged(this, "ObjSearchDateEnd"); }
		}

		public DateTime ObjScanDateStart
		{
			get { return objScanDateStart; }
			set { objScanDateStart = value; OnPropertyChanged(this, "ObjScanDateStart"); }
		}

		public CsvFieldSeparationTypeItem ObjSeparationType
		{
			get { return objSeparationType; }
			set { objSeparationType = value; OnPropertyChanged(this, "ObjSeparationType"); }
		}

		public ExportType ObjExportType
		{
			get { return objExportType; }
			set { objExportType = value; OnPropertyChanged(this, "ObjExportType"); }
		}

		public List<PaymentDataItem> ObjPaymentData
		{
			get
			{
				if (objPaymentData == null)
					objPaymentData = new List<PaymentDataItem>();
				return objPaymentData;
			}
			set { objPaymentData = value; OnPropertyChanged(this, "ObjPaymentData"); }
		}

		public List<string> CSVData
		{
			get
			{
				if (csvData == null)
					csvData = new List<string>();
				return csvData;
			}
			set { csvData = value; OnPropertyChanged(this, "CSVData"); }
		}

		public List<ExportType> AvailableExportTypes
		{
			get
			{
				if (availableExportTypes == null)
					availableExportTypes = new List<ExportType>();
				return availableExportTypes;
			}
			set { availableExportTypes = value; OnPropertyChanged(this, "AvailableExportTypes"); }
		}

		public List<CsvFieldSeparationTypeItem> AvailableSeparationTypes
		{
			get
			{
				if (availableSeparationTypes == null)
					availableSeparationTypes = new List<CsvFieldSeparationTypeItem>();
				return availableSeparationTypes;
			}
			set { availableSeparationTypes = value; OnPropertyChanged(this, "AvailableSeparationTypes"); }
		}

		#endregion // Properties




		#region Constructors

		public ExtractPaymentDataViewModel()
		{
			LoadAvailableExportTypes();
			LoadAvailableSeparationTypes();

			ObjSearchDateStart = DateTime.Now.Date;
			ObjSearchDateEnd = DateTime.Now.Date;
			ObjScanDateStart = DateTime.Now.Date;

			ObjSeparationType = AvailableSeparationTypes[0];
			ObjExportType = AvailableExportTypes[0];
		}

		#endregion// Constructors





		#region Event Handling

		private void OnPropertyChanged(object sender, string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion// Event Handling





		#region Main Methods

		private void LoadAvailableExportTypes()
		{
			AvailableExportTypes.Add(new ExportType(1, "CSV"));
			AvailableExportTypes.Add(new ExportType(2, "XLS"));
		}

		private void LoadAvailableSeparationTypes()
		{
			List<CsvFieldSeparationTypeItem> list = new List<CsvFieldSeparationTypeItem>();

			list.Add(new CsvFieldSeparationTypeItem(1, "Tab", "\t"));
			list.Add(new CsvFieldSeparationTypeItem(1, "Spazio", " "));
			list.Add(new CsvFieldSeparationTypeItem(1, ",", ","));
			list.Add(new CsvFieldSeparationTypeItem(1, ";", ";"));

			AvailableSeparationTypes = list;
		}

		public void ExtractData()
		{
			Pagamento_Dal dal = new Pagamento_Dal();
			ObjPaymentData = dal.GetPaymentData(ObjSearchDateStart, ObjSearchDateEnd, ObjScanDateStart);
			CSVData = dal.GetCSVPaymentData(ObjPaymentData, ObjSeparationType.Code, 2);
		}

		#endregion // Main Methods
	}
}

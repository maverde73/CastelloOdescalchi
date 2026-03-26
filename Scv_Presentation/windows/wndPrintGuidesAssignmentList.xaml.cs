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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Scv_Dal;
using Scv_Entities;
using ShiftManager;
using System.Data.Objects.DataClasses;
using Microsoft.Reporting.WinForms;
using System.IO;

namespace Presentation
{
	/// <summary>
	/// Interaction logic for wndPrintGuidesAssignmentList.xaml
	/// </summary>
	public partial class wndPrintGuidesAssignmentList : BasePrintPage
	{
		#region Events

		#endregion 



		#region Private Fields

		private bool IsReportLoaded = false;

		#endregion// Private fields




		#region Constructors

		public wndPrintGuidesAssignmentList()
		{

			InitializeComponent();
			this.PropertyChanged += new PropertyChangedEventHandler(base_PropertyChanged);
			rv.Load += new EventHandler(rv_Load);
		}

		void base_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}


		#endregion // Constructors



		#region Event Handling

		private void PrepareReport()
		{
			if (!IsReportLoaded)
			{

				string exeFolder = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
				string reportPath = Path.Combine(exeFolder, @"reports\GuidesAssignmentList.rdlc"); 
				if (File.Exists(reportPath))
				{
					ReportDataSource dataSource = null;

					dataSource = new ReportDataSource();
					dataSource.Name = "DsGuidesAssignmentList";
					dataSource.Value = DsEvidenzeGiornaliere;
					this.rv.LocalReport.DataSources.Add(dataSource);
					this.rv.LocalReport.ReportPath = reportPath;

					PrepareHeader();

					this.rv.RefreshReport();

					IsReportLoaded = true;
				}
			}
		}

		private void PrepareHeader()
		{
			try
			{
				ReportParameterCollection reportParameters = new ReportParameterCollection();
				reportParameters.Add(new ReportParameter("DataReport", this.PrintAssegnazioneGuideVisiteArgs.VisitDate.ToShortDateString()));
				this.rv.LocalReport.SetParameters(reportParameters);
			}
			catch (Exception ex)
			{

			}
		}

		void rv_Load(object sender, EventArgs e)
		{
			PrepareReport();
		}

		#endregion // Event Handling



		#region Private Methods

		#endregion// Private Methods


	}
}
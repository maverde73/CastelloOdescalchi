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
	/// Interaction logic for wndPrintGuidesPayroll.xaml
	/// </summary>
	public partial class wndPrintGuidesPayroll : BasePrintPage
	{
		#region Events

		#endregion 



		#region Private Fields

		private bool IsReportLoaded = false;

		#endregion// Private fields




		#region Constructors

		public wndPrintGuidesPayroll()
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
				string reportPath = Path.Combine(exeFolder, @"reports\GuidesPayroll.rdlc"); 
				if (File.Exists(reportPath))
				{
					ReportDataSource dataSource = null;

					dataSource = new ReportDataSource();
					dataSource.Name = "dsGuidesPayroll";
					dataSource.Value = DsGuidesPayroll;
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
				reportParameters.Add(new ReportParameter("TotalNecropoli", PrintGuidePayrollArgs.TotalNecropoli.ToString()));
				reportParameters.Add(new ReportParameter("TotalAltro", PrintGuidePayrollArgs.TotalAltro.ToString()));
				reportParameters.Add(new ReportParameter("TotalAcconto", PrintGuidePayrollArgs.TotalAcconto.ToString()));
				reportParameters.Add(new ReportParameter("MainTotal", PrintGuidePayrollArgs.MainTotal.ToString()));
				reportParameters.Add(new ReportParameter("Cut1", PrintGuidePayrollArgs.Cut1.ToString()));
				reportParameters.Add(new ReportParameter("Cut2", PrintGuidePayrollArgs.Cut2.ToString()));
				reportParameters.Add(new ReportParameter("Cut3", PrintGuidePayrollArgs.Cut3.ToString()));
				reportParameters.Add(new ReportParameter("Cut1Pieces", PrintGuidePayrollArgs.Cut1Pieces.ToString()));
				reportParameters.Add(new ReportParameter("Cut2Pieces", PrintGuidePayrollArgs.Cut2Pieces.ToString()));
				reportParameters.Add(new ReportParameter("Cut3Pieces", PrintGuidePayrollArgs.Cut3Pieces.ToString()));
				reportParameters.Add(new ReportParameter("Cut1Total", PrintGuidePayrollArgs.Cut1Total.ToString()));
				reportParameters.Add(new ReportParameter("Cut2Total", PrintGuidePayrollArgs.Cut2Total.ToString()));
                reportParameters.Add(new ReportParameter("Cut3Total", PrintGuidePayrollArgs.Cut3Total.ToString()));
                reportParameters.Add(new ReportParameter("MonthYear", PrintGuidePayrollArgs.MonthName + " " + PrintGuidePayrollArgs.YearNumber.ToString()));
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
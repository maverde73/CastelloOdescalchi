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
	/// Interaction logic for wndPrintMandatoAmministrazione.xaml
	/// </summary>
	public partial class wndPrintMandatoAmministrazione : BasePrintPage
	{
		#region Events

		#endregion



		#region Private Fields

		private bool IsReportLoaded = false;

		#endregion// Private fields



		#region Constructors

		public wndPrintMandatoAmministrazione()
		{

			InitializeComponent();
			this.PropertyChanged += new PropertyChangedEventHandler(base_PropertyChanged);
			rv.Load += new EventHandler(rv_Load);
		}

		void base_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "DsMandato":
					//PrepareReport();
					break;
			}
		}


		#endregion // Constructors



		#region Event Handling

		private void PrepareReport()
		{
			if (!IsReportLoaded)
			{

				string exeFolder = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                string reportPath = "";

                if (StatsTipiVisite == "")
                {
                    reportPath = Path.Combine(exeFolder, @"reports\MandatoAmministrazione.rdlc");
                    //string reportPath = root + "\\reports\\WarehouseMovements.rdlc";
                    if (File.Exists(reportPath))
                    {
                        ReportDataSource dataSource = new ReportDataSource();
                        ReportDataSource dataSource1 = new ReportDataSource();

                        try
                        {
                            this.rv.LocalReport.ReportPath = reportPath;

                            dataSource1.Name = "dsTipiVisite";
                            dataSource1.Value = DsPrintTipiVisitaImporti;
                            this.rv.LocalReport.DataSources.Add(dataSource1);


                            dataSource.Name = "dsMandato";
                            dataSource.Value = DsPrintMandato;
                            this.rv.LocalReport.DataSources.Add(dataSource);

                            PrepareHeader();
                        }
                        catch (Exception e)
                        {

                        }
                        

                        this.rv.RefreshReport();
                        //this.rv.Show();

                        IsReportLoaded = true;
                    }
                }
                else
                {
                    reportPath = Path.Combine(exeFolder, @"reports\rptStat.rdlc");
                    //string reportPath = root + "\\reports\\WarehouseMovements.rdlc";
                    if (File.Exists(reportPath))
                    {
                        ReportDataSource dataSource = new ReportDataSource();
 
                        try
                        {
                            this.rv.LocalReport.ReportPath = reportPath;
                            dataSource.Name = "dsStats";
                            dataSource.Value = DsStats;
                            this.rv.LocalReport.DataSources.Add(dataSource);
                            PrepareHeader();
                        }
                        catch (Exception e)
                        {

                        }


                        this.rv.RefreshReport();
                        //this.rv.Show();

                        IsReportLoaded = true;
                    }
                }


			}
		}

		private void PrepareHeader()
		{
			try
			{
				ReportParameterCollection reportParameters = new ReportParameterCollection();

                if (StatsTipiVisite == "")
                {
                    reportParameters.Add(new ReportParameter("DataMandato", this.DsPrintMandato[0].DtMandato));
                    reportParameters.Add(new ReportParameter("NrProgressivo", this.DsPrintMandato[0].NrProgressivo.ToString()));
                    this.rv.LocalReport.SetParameters(reportParameters);
                }




				
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

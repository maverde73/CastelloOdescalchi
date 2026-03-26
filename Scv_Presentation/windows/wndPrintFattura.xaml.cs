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
    /// Interaction logic for wndPrintReceipt.xaml
    /// </summary>
    public partial class wndPrintFattura : BasePrintPage
    {
        #region Events
        #endregion

        #region Private Fields
        private bool IsReportLoaded = false;
        FatturaViewModel vm = new FatturaViewModel();
        #endregion// Private fields

        #region Constructors
        public wndPrintFattura()
        {

            InitializeComponent();
            this.PropertyChanged += new PropertyChangedEventHandler(base_PropertyChanged);
            rv.Load += new EventHandler(rv_Load);
        }

        void base_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DsWarehouseMovements":
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
                string reportPath = Path.Combine(exeFolder, @"reports\Fattura.rdlc");
                //string reportPath = root + "\\reports\\WarehouseMovements.rdlc";
                if (File.Exists(reportPath))
                {
                    this.rv.LocalReport.ReportPath = reportPath;
                    List<ReportParameter> reportParameters = new List<ReportParameter>();
                    V_Prenotazione prenotazione = vm.getPrenotazione(this.ObjPagamento.Id_Prenotazione);
                    V_RichiedenteFull richiedente = vm.getRichiedente(prenotazione.Id_Richiedente);

                    string egrGentSpett = "Egr.";

                    if (richiedente.Is_PF)
                    {
                        if (richiedente.Sesso == "F")
                            egrGentSpett = "Gent.ma";
                    }
                    else
                        egrGentSpett = "Spett.le";

                    string nome = string.IsNullOrEmpty(richiedente.Nome) ? "" : richiedente.Nome;

                    reportParameters.Add(new ReportParameter("spettegregio", egrGentSpett));
                    reportParameters.Add(new ReportParameter("nominativo", richiedente.Cognome.ToUpper() + " " + nome.ToUpper()));
                    reportParameters.Add(new ReportParameter("indirizzo", richiedente.Indirizzo));
                    reportParameters.Add(new ReportParameter("citta", richiedente.CAP + " " + richiedente.Citta));
                    reportParameters.Add(new ReportParameter("piva", richiedente.CF_PIVA));
                    reportParameters.Add(new ReportParameter("progressivo", ObjPagamento.Ricevuta));
                    reportParameters.Add(new ReportParameter("data", DateTime.Now.ToString("dd/MM/yyyy")));
                    
                    string oggetto = "Vi forniamo ricevuta relativa a Vs {0}"
                                      +"{1}" 
                                      +"ingressi per {2}"
                                      +"{3}" 
                                      +"in data {4}."
                                      +"{5}" 
                                      +"Accessi museo esenti art. 10 c.1.22";

                    oggetto = string.Format(oggetto
                                            , ObjPagamento.Nr_Biglietti
                                            , System.Environment.NewLine
                                            , prenotazione.TipoVisita
                                            , System.Environment.NewLine
                                            , vm.getDataVisita(ObjPagamento.Id_Prenotazione).ToString("dd/MM/yyyy")
                                            , System.Environment.NewLine);

                    reportParameters.Add(new ReportParameter("oggetto", oggetto));
                    reportParameters.Add(new ReportParameter("totale0", ObjPagamento.Importo.ToString()));
                    reportParameters.Add(new ReportParameter("totale1", ObjPagamento.Importo.ToString()));
                    reportParameters.Add(new ReportParameter("descrpagamento", "Pagamento EFFETTUATO"));
                    this.rv.LocalReport.SetParameters(reportParameters);
                    this.rv.RefreshReport();
                    IsReportLoaded = true;
                }
            }
        }

        private void PrepareHeader()
        {
            try
            {
                //ReportParameterCollection reportParameters = new ReportParameterCollection();
                //reportParameters.Add(new ReportParameter("MainTotal", this.PrintMovementArgs.MainTotal.ToString()));
                //this.rv.LocalReport.SetParameters(reportParameters);
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

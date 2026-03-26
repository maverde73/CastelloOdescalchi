using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using ClosedXML.Excel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.Data;
using Scv_Entities;
using System.Collections.ObjectModel;

namespace Presentation
{
    public class ExcelReport
    {
        public void SaveExcel(ObservableCollection<V_EvidenzeGiornaliere> SrcEvidenzeGiornaliere, string filePath, string filtri)
        {
            try
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add(filtri);
                    

                    IXLCell StartCell = null;
                    IXLCell EndCell = null;
                    IXLRange rg = null;

                    //StartCell = ws.Cell(1, 1);
                    //EndCell = ws.Cell(2, 10);
                    //rg = ws.Range(StartCell, EndCell);
                    //rg.Merge();

                    //ws.Cell(1, 1).Value = "Titolo";
                    //rg.Select();
                    //rg.Style.Font.Bold = true;
                    //rg.Style.Font.FontName = "Arial";
                    //rg.Style.Font.FontSize = 10;
                    //rg.Style.Fill.BackgroundColor = XLColor.LightGray;

                    int firstRowIndex = 1;

                    //intestazione 
                    ws.Cell(firstRowIndex, 1).Value = "Data";
                    ws.Cell(firstRowIndex, 1).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 2).Value = "Ora";
                    ws.Cell(firstRowIndex, 2).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 3).Value = "Lingua";
                    ws.Cell(firstRowIndex, 3).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 4).Value = "Guida";
                    ws.Cell(firstRowIndex, 4).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 5).Value = "Acc.";
                    ws.Cell(firstRowIndex, 5).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 6).Value = "Tipo Visita";
                    ws.Cell(firstRowIndex, 6).Style.Font.Bold = true;
                    //ws.Cell(firstRowIndex, 7).Value = "Totale visitatori";
                    //ws.Cell(firstRowIndex, 7).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 7).Value = "Protocollo";
                    ws.Cell(firstRowIndex, 7).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 8).Value = "Interi";
                    ws.Cell(firstRowIndex, 8).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 9).Value = "Ridotti";
                    ws.Cell(firstRowIndex, 9).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 10).Value = "Scontati";
                    ws.Cell(firstRowIndex, 10).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 11).Value = "Cumulativi";
                    ws.Cell(firstRowIndex, 11).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 12).Value = "Omaggio";
                    ws.Cell(firstRowIndex, 12).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 13).Value = "Emessi";
                    ws.Cell(firstRowIndex, 13).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 14).Value = "Visitatore";
                    ws.Cell(firstRowIndex, 14).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 15).Value = "Ricevuta";
                    ws.Cell(firstRowIndex, 15).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 16).Value = "Importo";
                    ws.Cell(firstRowIndex, 16).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 17).Value = "Tipo Pagamento";
                    ws.Cell(firstRowIndex, 17).Style.Font.Bold = true;
                    ws.Cell(firstRowIndex, 18).Value = "Data Pagamento";
                    ws.Cell(firstRowIndex, 18).Style.Font.Bold = true;


                    V_EvidenzeGiornaliere visitaPrec = null;
                    

                    for (int i = 0; i < SrcEvidenzeGiornaliere.Count; i++)
                    {
                        var visita = SrcEvidenzeGiornaliere[i];

                        firstRowIndex++;

                        if (visita.VisitDate != null)
                        {
                            visitaPrec = visita;
                            ws.Cell(firstRowIndex, 1).Value = visita.VisitDate;

                            ws.Cell(firstRowIndex, 2).Value = visita.Ora_Visita;

                            ws.Cell(firstRowIndex, 3).Value = visita.LinguaVisita;

                            ws.Cell(firstRowIndex, 4).Value = visita.Nominativo;
                        }
                        else
                        {
                            ws.Cell(firstRowIndex, 1).Value = visitaPrec.VisitDate;

                            ws.Cell(firstRowIndex, 2).Value = visitaPrec.Ora_Visita;

                            ws.Cell(firstRowIndex, 3).Value = visitaPrec.LinguaVisita;

                            ws.Cell(firstRowIndex, 4).Value = visitaPrec.Nominativo;
                        }

                        ws.Cell(firstRowIndex, 5).Value = visita.AccettaGuida;

                        ws.Cell(firstRowIndex, 6).Value = visita.TipoVisita;

                        //ws.Cell(firstRowIndex, 7).Value = visita.NrV;

                        ws.Cell(firstRowIndex, 7).Value = visita.NProtocollo;

                        ws.Cell(firstRowIndex, 8).Value = visita.Nr_Interi;

                        ws.Cell(firstRowIndex, 9).Value = visita.Nr_Ridotti;

                        ws.Cell(firstRowIndex, 10).Value = visita.Nr_Scontati;

                        ws.Cell(firstRowIndex, 11).Value = visita.Nr_Cumulativi;

                        ws.Cell(firstRowIndex, 12).Value = visita.Nr_Omaggio;

                        ws.Cell(firstRowIndex, 13).Value = visita.Consegnati;

                        ws.Cell(firstRowIndex, 14).Value = visita.Responsabile;

                        ws.Cell(firstRowIndex, 15).Value = visita.Ricevuta;

                        ws.Cell(firstRowIndex, 16).Value = visita.Importo.Replace("(T)","").Replace("€","");
                        ws.Cell(firstRowIndex, 16).Style.NumberFormat.Format = "€ #,##0.00";

                        ws.Cell(firstRowIndex, 17).Value = visita.Simbolo;

                        ws.Cell(firstRowIndex, 18).Value = visita.Dt_Pagamento;
                    }

                   
                    ws.Columns().AdjustToContents();

                    workbook.SaveAs(filePath);
                }

            }
            catch (Exception ex)
            {
                
                throw ex;
            }

          


        }
    }
}

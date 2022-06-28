using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace POC
{
    public partial class Utility
    {

        /// <summary>
        /// FUNCTION FOR EXPORT TO EXCEL
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="worksheetName"></param>
        /// <param name="saveAsLocation"></param>
        /// <returns></returns>
        public static bool WriteDataTableToExcel(System.Data.DataTable dt, List<string> ignoreColumns, string worksheetName, string saveAsLocation, string ReporType)
        {
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook excelworkBook;
            Microsoft.Office.Interop.Excel.Worksheet excelSheet;
            Microsoft.Office.Interop.Excel.Range excelCellrange;
            DataTable dataTable = dt.Copy();
            try
            {
                // Start Excel and get Application object.
                excel = new Microsoft.Office.Interop.Excel.Application
                {

                    // for making Excel visible
                    Visible = false,
                    DisplayAlerts = false
                };

                // Creation a new Workbook
                excelworkBook = excel.Workbooks.Add(Type.Missing);

                // Workk sheet
                excelSheet = (Microsoft.Office.Interop.Excel.Worksheet)excelworkBook.ActiveSheet;
                excelSheet.Name = worksheetName;


                excelSheet.Cells[1, 1] = ReporType;
                excelSheet.Cells[1, 2] = "Date : " + DateTime.Now.ToShortDateString();
                int columnCount = dataTable.Columns.Count;
                if (ignoreColumns != null)
                {
                    columnCount -= ignoreColumns.Count();
                    foreach (string str in ignoreColumns)
                    {
                        dataTable.Columns.Remove(str);
                    }
                }
                // loop through each row and add values to our sheet
                int rowcount = 2;

                foreach (DataRow datarow in dataTable.Rows)
                {
                    rowcount += 1;
                    for (int i = 1; i <= columnCount; i++)
                    {
                        // on the first iteration we add the column headers
                        if (rowcount == 3)
                        {
                            excelSheet.Cells[2, i] = dataTable.Columns[i - 1].ColumnName;
                            excelSheet.Cells.Font.Color = System.Drawing.Color.Black;

                        }
                        excelSheet.Cells[rowcount, i] = datarow[i - 1].ToString();
                        //for alternate rows
                        if (rowcount > 3)
                        {
                            if (i == dataTable.Columns.Count)
                            {
                                if (rowcount % 2 == 0)
                                {
                                    excelCellrange = excelSheet.Range[excelSheet.Cells[rowcount, 1], excelSheet.Cells[rowcount, dataTable.Columns.Count]];
                                    FormattingExcelCells(excelCellrange, "#e6e6e6", System.Drawing.Color.Black, false);
                                }

                            }
                        }
                    }
                }

                // now we resize the columns
                excelCellrange = excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[rowcount, dataTable.Columns.Count]];
                excelCellrange.EntireColumn.AutoFit();
                Microsoft.Office.Interop.Excel.Borders border = excelCellrange.Borders;
                border.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                border.Weight = 2d;


                excelCellrange = excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[2, dataTable.Columns.Count]];
                FormattingExcelCells(excelCellrange, "#cccccc", System.Drawing.Color.Black, true);


                //now save the workbook and exit Excel

                // string filePath = saveAsLocation + "\\" + worksheetName + " " + DateTime.Now.ToString("MM_dd_yyyy HH_mm_ss").ToString() + ".xlsx";
                string filePath = string.Empty;
                SaveFileDialog fdb = new SaveFileDialog
                {
                    //fdb.InitialDirectory = saveAsLocation;
                    Filter = "Excel Worksheets|*.xlsx;*.xls",
                    FileName = worksheetName + ".xlsx"
                };
                if (fdb.ShowDialog() == DialogResult.OK)
                {
                    saveAsLocation = Path.GetFullPath(fdb.FileName);
                    filePath = saveAsLocation;
                    excelworkBook.SaveAs(filePath);

                    excelworkBook.Close();
                    excel.Quit();
                    if (File.Exists(filePath))
                        Process.Start(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message.ToString());
                return false;
            }
            finally
            {
                excelSheet = null;
                excelCellrange = null;
                excelworkBook = null;
            }

        }


        public static bool WriteDataTableToExcel(System.Data.DataView dataView, List<string> ignoreColumns, string worksheetName, string saveAsLocation, string ReporType)
        {
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook excelworkBook;
            Microsoft.Office.Interop.Excel.Worksheet excelSheet;
            Microsoft.Office.Interop.Excel.Range excelCellrange;
            DataTable dataTable = dataView.Table.Copy();

            try
            {
                // Start Excel and get Application object.
                excel = new Microsoft.Office.Interop.Excel.Application
                {

                    // for making Excel visible
                    Visible = false,
                    DisplayAlerts = false
                };

                // Creation a new Workbook
                excelworkBook = excel.Workbooks.Add(Type.Missing);

                // Workk sheet
                excelSheet = (Microsoft.Office.Interop.Excel.Worksheet)excelworkBook.ActiveSheet;
                excelSheet.Name = worksheetName;


                excelSheet.Cells[1, 1] = ReporType;
                excelSheet.Cells[1, 2] = "Date : " + DateTime.Now.ToShortDateString();


                int columnCount = dataTable.Columns.Count - ignoreColumns.Count();
                foreach (string str in ignoreColumns)
                {
                    dataTable.Columns.Remove(str);
                }
                // loop through each row and add values to our sheet
                int rowcount = 2;

                foreach (DataRowView datarow in dataView)
                {
                    rowcount += 1;
                    for (int i = 1; i <= columnCount; i++)
                    {
                        // on the first iteration we add the column headers
                        if (rowcount == 3)
                        {
                            excelSheet.Cells[2, i] = dataTable.Columns[i - 1].ColumnName;
                            excelSheet.Cells.Font.Color = System.Drawing.Color.Black;

                        }
                        excelSheet.Cells[rowcount, i] = datarow[i - 1].ToString();
                        //for alternate rows
                        if (rowcount > 3)
                        {
                            if (i == dataTable.Columns.Count)
                            {
                                if (rowcount % 2 == 0)
                                {
                                    excelCellrange = excelSheet.Range[excelSheet.Cells[rowcount, 1], excelSheet.Cells[rowcount, dataTable.Columns.Count]];
                                    FormattingExcelCells(excelCellrange, "#e6e6e6", System.Drawing.Color.Black, false);
                                }

                            }
                        }
                    }
                }

                // now we resize the columns
                excelCellrange = excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[rowcount, dataTable.Columns.Count]];
                excelCellrange.EntireColumn.AutoFit();
                Microsoft.Office.Interop.Excel.Borders border = excelCellrange.Borders;
                border.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                border.Weight = 2d;


                excelCellrange = excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[2, dataTable.Columns.Count]];
                FormattingExcelCells(excelCellrange, "#cccccc", System.Drawing.Color.Black, true);


                //now save the workbook and exit Excel

                string filePath = saveAsLocation + "\\" + worksheetName + " " + DateTime.Now.ToString("MM_dd_yyyy HH_mm_ss").ToString() + ".xlsx";
                excelworkBook.SaveAs(filePath);

                excelworkBook.Close();
                excel.Quit();
                if (File.Exists(filePath))
                    Process.Start(filePath);



                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message.ToString());
                return false;
            }
            finally
            {
                excelSheet = null;
                excelCellrange = null;
                excelworkBook = null;
            }

        }

        /// <summary>
        /// FUNCTION FOR FORMATTING EXCEL CELLS
        /// </summary>
        /// <param name="range"></param>
        /// <param name="HTMLcolorCode"></param>
        /// <param name="fontColor"></param>
        /// <param name="IsFontbool"></param>
        public static void FormattingExcelCells(Microsoft.Office.Interop.Excel.Range range, string HTMLcolorCode, System.Drawing.Color fontColor, bool IsFontbool)
        {
            range.Interior.Color = System.Drawing.ColorTranslator.FromHtml(HTMLcolorCode);
            range.Font.Color = System.Drawing.ColorTranslator.ToOle(fontColor);
            if (IsFontbool)
            {
                range.Font.Bold = IsFontbool;
            }
        }

    }

}

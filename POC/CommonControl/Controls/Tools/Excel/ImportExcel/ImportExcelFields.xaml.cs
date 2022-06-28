using Autodesk.Revit.UI;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace POC.Internal
{
    /// <summary>
    /// Interaction logic for SampleDialog.xaml
    /// </summary>
    public partial class ImportExcelFieldsUserControl : UserControl
    {
        private Microsoft.Office.Interop.Excel.Application _excelApp;
        private Microsoft.Office.Interop.Excel.Workbook _excelBook;
        private Microsoft.Office.Interop.Excel.Worksheet _workSheet;
        private Microsoft.Office.Interop.Excel.Range _range;
        private string _path = string.Empty;
        private DataTable _dataTable = new DataTable();
        int _totalNumberOfRow = 0;
        public bool IsSerialNumber
        {
            get { return (bool)GetValue(IsSerialNumberProperty); }
            set { SetValue(IsSerialNumberProperty, value); }
        }


        public static readonly DependencyProperty IsSerialNumberProperty =
            DependencyProperty.Register("IsSerialNumber", typeof(bool), typeof(ImportExcelFieldsUserControl), new PropertyMetadata(false));


        public ImportExcelFieldsUserControl()
        {
            InitializeComponent();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _path = openFileDialog.FileName;
                if (File.Exists(_path))
                {

                    _excelApp = new Microsoft.Office.Interop.Excel.Application();
                    _excelBook = _excelApp.Workbooks.Open(_path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    List<string> sheetnames = new List<string>();
                    try
                    {
                        foreach (Microsoft.Office.Interop.Excel.Worksheet worksheet in _excelBook.Worksheets)
                        {
                            sheetnames.Add(worksheet.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Some error has occured. \n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        _excelBook.Close(null, null, null);
                        _excelApp.Quit();
                    }
                    excelsheet.ItemsSource = sheetnames;
                    excelsheet.SelectedIndex = 0;
                    if (sheetnames.Count > 0)
                    {
                        txtHeaderIndex.Text = "1";
                    }
                }
            }

        }



        private void TxtHeaderIndex_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtHeaderIndex.Text) && Convert.ToInt32(txtHeaderIndex.Text) > 0)
            {
                try
                {

                    if (excelsheet.SelectedItem != null)
                    {
                        _excelApp = new Microsoft.Office.Interop.Excel.Application();
                        _excelBook = _excelApp.Workbooks.Open(_path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                        _workSheet = _excelBook.Worksheets.Item[excelsheet.SelectedIndex + 1];
                        Microsoft.Office.Interop.Excel.Range excelRange = _workSheet.UsedRange;

                        _range = _workSheet.UsedRange;
                        int colCnt = _range.Columns.Count;
                        int rowCount = _range.Rows.Count;

                        txtRowEnd.Text = rowCount.ToString();
                        _totalNumberOfRow = rowCount;
                        _dataTable = new DataTable();
                        string concatString = string.Empty;
                        for (colCnt = 1; colCnt <= excelRange.Columns.Count; colCnt++)
                        {
                            string strColumn = "";
                            strColumn = (string)(excelRange.Cells[Convert.ToInt32(txtHeaderIndex.Text), colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                            if (strColumn != null)
                            {
                                concatString += strColumn + "\r\n";
                                _dataTable.Columns.Add(strColumn, typeof(string));
                            }
                        }
                        txtHeaderIndex.ToolTip = concatString;
                    }
                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                finally
                {
                    _excelBook.Close(null, null, null);
                    _excelApp.Quit();
                }
                txtRowStart.Text = Convert.ToString(Convert.ToInt32(txtHeaderIndex.Text.Trim()) + 1);
            }
        }

      


        private void PreviewTextInputHeaderIndex(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }



        private void BtnUpload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {

                if (excelsheet.SelectedItem != null && !string.IsNullOrEmpty(txtHeaderIndex.Text) && Convert.ToInt32(txtHeaderIndex.Text) > 0)
                {
                    _excelApp = new Microsoft.Office.Interop.Excel.Application();
                    _excelBook = _excelApp.Workbooks.Open(_path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    _workSheet = _excelBook.Worksheets.Item[excelsheet.SelectedIndex + 1];
                    Microsoft.Office.Interop.Excel.Range excelRange = _workSheet.UsedRange;

                    _range = _workSheet.UsedRange;
                    int colCnt = _range.Columns.Count;
                    int rowCnt = _range.Rows.Count;
                    int rowStart = Convert.ToInt32(txtRowStart.Text);
                    int rowEnd = Convert.ToInt32(txtRowEnd.Text);
                    string strCellData = "";
                    //rowEnd = excelRange.Rows.Count > rowEnd ? (rowEnd - 1) : excelRange.Rows.Count;
                    for (rowCnt = rowStart; rowCnt <= rowEnd; rowCnt++)
                    {
                        string strData = "";
                        for (colCnt = 1; colCnt <= excelRange.Columns.Count; colCnt++)
                        {
                            var value = (excelRange.Cells[rowCnt, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                            strCellData = value != null ? Convert.ToString(value) : "";
                            strData += strCellData + "|";
                        }
                        strData = strData.Remove(strData.Length - 1, 1);
                        _dataTable.Rows.Add(strData.Split('|'));

                    }

                    _excelBook.Close(true, null, null);
                    _excelApp.Quit();
                    _dataTable = _dataTable.Rows
                         .Cast<DataRow>()
                         .Where(row => !row.ItemArray.All(field => field is DBNull ||
                                                          string.IsNullOrWhiteSpace(field as string)))
                         .CopyToDataTable();
                    _dataTable.AcceptChanges();
                    if (IsSerialNumber)
                        AddAutoIncrementColumn(_dataTable);
                    ImportExcelVM importExcelViewModel = new ImportExcelVM
                    {
                        isCanceled = false,
                        dataTable = _dataTable
                    };
                    this.DataContext = importExcelViewModel;
                }
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }
        public void AddAutoIncrementColumn(DataTable dt)
        {
            string columnName = "Sno";
            if (!dt.Columns.Contains(columnName))
            {
                DataColumn column = new DataColumn
                {
                    ColumnName = columnName,
                    DataType = System.Type.GetType("System.Int32")
                };
                dt.Columns.Add(column);
            }
            int index = 0;

            foreach (DataRow row in dt.Rows)
            {
                row.SetField(dt.Columns[columnName], ++index);
            }
            dt.Columns[columnName].SetOrdinal(0);
        }

      

      

        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((System.Windows.FrameworkElement)sender).Tag == null)
            {
                btnCancel.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D85922");
                btnCancel.BorderBrush = btnCancel.Background;
                cancelIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                cancelText.Foreground = cancelIcon.Foreground;
            }
            else
            {
                btnUpload.Opacity = 1;
            }
        }

        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (((System.Windows.FrameworkElement)sender).Tag == null)
            {
                btnCancel.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                btnCancel.BorderBrush = btnCancel.Background;
                cancelIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
                cancelText.Foreground = cancelIcon.Foreground;

            }
            else
            {
                btnUpload.Opacity = 0.6;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ImportExcelVM importExcelViewModel = new ImportExcelVM
            {
                isCanceled = true
            };
            this.DataContext = importExcelViewModel;

        }

        private void txtRowStart_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRowStart.Text) && Convert.ToInt32(txtRowStart.Text) <= Convert.ToInt32(txtHeaderIndex.Text))
            {
                txtRowStart.Text = Convert.ToString(Convert.ToInt32(txtHeaderIndex.Text.Trim()) + 1);
            }
        }

        private void txtRowEnd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRowEnd.Text) && Convert.ToInt32(txtRowEnd.Text) <= Convert.ToInt32(txtRowStart.Text))
            {
                txtRowEnd.Text = Convert.ToString(_totalNumberOfRow);
            }
        }

        private void txtValue_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}

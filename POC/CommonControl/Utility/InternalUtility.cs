using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace POC
{
    public static partial class Utility
    {
        private static readonly Random random = new Random();
        public static string GetEnumDisplayName(this Enum enumValue)
        {
            string displayName;
            displayName = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = enumValue.ToString();
            }
            return displayName;
        }
        public static void Swap<T>(ref T start, ref T end, bool isSwap = true)
        {

            if (isSwap == true)
            {
                T tempswap = end;
                end = start;
                start = tempswap;
            }

        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        //Author - Arunachalam        
        public static double InchesToDouble(string a_Value)
        {
            double l_OutValue = 0;
            try
            {
                if (a_Value.Contains("'"))
                {
                    l_OutValue = FeetnInchesToDouble(a_Value);
                }
                else
                {
                    if (double.TryParse(a_Value, out l_OutValue))
                    {
                        return l_OutValue / 12;
                    }
                    string[] splitValues = a_Value.Split('\"');
                    splitValues = splitValues[0].Split(new char[] { ' ', '/' });
                    if (splitValues.Length == 2 || splitValues.Length == 3)
                    {
                        if (double.TryParse(splitValues[0], out double a) && double.TryParse(splitValues[1], out double b))
                        {
                            if (splitValues.Length == 2)
                            {
                                l_OutValue = a / b;
                            }
                            if (splitValues.Length == 3)
                            {
                                if (double.TryParse(splitValues[2], out double c))
                                {
                                    l_OutValue = a + (b / c);
                                }
                            }
                        }
                        l_OutValue /= 12;
                    }
                    else if (splitValues.Length == 1)
                    {
                        if (double.TryParse(splitValues[0], out double a))
                        {
                            l_OutValue = a / 12;
                        }
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Offset value should not contain any letters/special characters except periods,double quotes,forward slash(/)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return l_OutValue;
        }
        //Author - Arunachalam
        public static double FeetnInchesToDouble(string a_Value)
        {
            try
            {
                double l_OutValue;
                if (a_Value.Contains("'"))
                {
                    string[] splittedString = a_Value.Split('\'');
                    if (splittedString.Length > 1)
                    {
                        if (double.Parse(splittedString[0]) > 0)
                            l_OutValue = double.Parse(splittedString[0]) + FeetnInchesToDouble(splittedString[1].Split('\"')[0].Trim());
                        else
                        {
                            l_OutValue = (-double.Parse(splittedString[0])) + FeetnInchesToDouble(splittedString[1].Split('\"')[0].Trim());
                            l_OutValue = -l_OutValue;
                        }
                    }
                    else
                    {
                        l_OutValue = double.Parse(splittedString[0]);
                    }
                }
                else
                {
                    if (double.TryParse(a_Value, out l_OutValue))
                    {
                        return l_OutValue / 12;
                    }
                    string[] splitValues = a_Value.Split('\"');
                    splitValues = splitValues[0].Split(new char[] { ' ', '/' });
                    if (splitValues.Length == 1)
                    {
                        if (double.TryParse(splitValues[0], out l_OutValue))
                        {
                            return l_OutValue / 12;
                        }
                    }
                    if (splitValues.Length == 2 || splitValues.Length == 3)
                    {
                        if (double.TryParse(splitValues[0], out double a) && double.TryParse(splitValues[1], out double b))
                        {
                            if (splitValues.Length == 2)
                            {
                                l_OutValue = a / b;
                                l_OutValue /= 12;
                            }
                            if (splitValues.Length == 3)
                            {
                                if (a > 0)
                                {
                                    if (double.TryParse(splitValues[2], out double c))
                                    {
                                        l_OutValue = a + (b / c);
                                    }
                                }
                                else
                                {
                                    if (double.TryParse(splitValues[2], out double c))
                                    {
                                        l_OutValue = (-a) + (b / c);
                                    }
                                }
                                l_OutValue /= 12;
                            }
                        }
                    }
                }
                return l_OutValue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        static long Gcd(long a, long b)
        {
            if (a == 0)
                return b;
            else if (b == 0)
                return a;
            if (a < b)
                return Gcd(a, b % a);
            else
                return Gcd(b, a % b);
        }
        // Function to convert decimal to fraction 
        public static string DecimalToFraction(double number)
        {
            string OutStr = string.Empty;
            // Fetch integral value of the decimal 
            double intVal = Math.Floor(number);
            if (intVal <= 0)
            {
                // Fetch fractional part of the decimal 
                double fVal = number - intVal;

                // Consider precision value to 
                // convert fractional part to 
                // integral equivalent 
                long pVal = 1000000000;

                // Calculate GCD of integral 
                // equivalent of fractional 
                // part and precision value 
                long gcdVal = Gcd((long)Math.Round(
                                fVal * pVal), pVal);

                // Calculate num and deno 
                long num = (long)Math.Round(fVal * pVal) / gcdVal;
                long deno = pVal / gcdVal;

                OutStr = (long)(intVal * deno) +
                                    num + "/" + deno;
            }
            else
            {
                string[] splittedNumber = number.ToString().Split('.');
                if (splittedNumber != null && splittedNumber.Length > 0)
                {
                    if (splittedNumber.Length > 1)
                    {
                        if (int.TryParse(splittedNumber[0], out int a) && int.TryParse(splittedNumber[1], out int b))
                        {
                            int c = 0;
                            string decimalDigit = c.ToString() + '.' + b.ToString();
                            double d = Convert.ToDouble(decimalDigit);
                            OutStr = a.ToString() + " " + DecimalToFraction(d);
                        }
                    }
                    else
                    {
                        if (int.TryParse(splittedNumber[0], out int a))
                        {
                            OutStr = a.ToString();
                        }
                    }
                }
            }
            return OutStr;
        }

        public static double RoundOfDecimal(double splitLength)
        {
            string[] listSplit = splitLength.ToString().Split('.');
            if (listSplit.Count() > 1 && listSplit[1].Length >= 2)
            {
                double convertedValue = Convert.ToInt32(listSplit[1].Substring(0, 2));
                double value = convertedValue / 10;
                double truncateValue = Math.Truncate(value);
                convertedValue += (10 - (convertedValue - (truncateValue * 10)));
                string stringValue = listSplit[0] + "." + convertedValue;
                return Convert.ToDouble(stringValue);
            }
            return splitLength;
        }

        public static Size MeasureString(string candidate,object field)
        {

            FormattedText formattedText = null;
            if (field is TextBlock textBlock)
            {

                formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);
            }
            else if(field is ComboBox comboBox)
            {
                formattedText = new FormattedText(
              candidate,
              CultureInfo.CurrentCulture,
              FlowDirection.LeftToRight,
              new Typeface(comboBox.FontFamily, comboBox.FontStyle, comboBox.FontWeight, comboBox.FontStretch),
              comboBox.FontSize,
              Brushes.Black,
              new NumberSubstitution(),
              1);
            }
            

            return new Size(formattedText.Width, formattedText.Height);
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}

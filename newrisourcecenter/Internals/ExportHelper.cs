using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace newrisourcecenter.Internals
{
    public class ExportHelper
    {
            /// <summary>
            /// Exports a list of objects to Excel. Objects go in the Rows while Object Properties go in the Columns
            /// </summary>
            /// <param name="objects">List of objects to export.</param>
            /// <param name="filePath">Location to save file to. Does not need to exist</param>
            /// <param name="fileName">Name of excel file</param>
            public static void ExportToExcel<T>(IEnumerable<T> objects, string filePath, string fileName)
            {
                // Add \ to end of file name if it doesn't exist. Just want to be consistant
                if (!filePath.EndsWith(@"\"))
                    filePath += @"\";

                // Create directory if it doesn't exist
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                // Create a new workbook.
                IWorkbook workbook = new XSSFWorkbook();

                // Get the active sheet 
                ISheet sheet = workbook.CreateSheet("Sheet 1");

                try
                {
                    var data = GetObjectArray<T>(objects);
                    // If at least one record got converted successfully
                    if (data.Length > 1)
                    {
                        IFont font = workbook.CreateFont();
                        font.IsBold = true;
                        font.FontHeightInPoints = 11;
                        ICellStyle boldStyle = workbook.CreateCellStyle();
                        boldStyle.SetFont(font);
                        for (int x = 0;x < data.GetLength(0);x += 1)
                        {
                            IRow row = sheet.CreateRow(x);
                            for (int y = 0; y < data.GetLength(1); y += 1)
                            {
                                ICell cell = row.CreateCell(y);
                                var cellVal = data[x, y];
                                cell.SetCellValue((cellVal != null ? cellVal.ToString() : ""));
                                if(x == 0)
                                {
                                    sheet.AutoSizeColumn(y);
                                    cell.CellStyle = boldStyle;
                                }
                            }
                        }                        
                    }

                    // Save workbook
                    using (FileStream stream = new FileStream(string.Format("{0}{1}", new object[] { filePath, fileName }), FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    // Close
                    sheet = null;
                    workbook.Close();
                    workbook = null;
                }

                // Clean up 
                // NOTE: When in release mode, this does the trick 
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            /// <summary>
            /// Takes a List of objects objects and converts the objects and their properties into a rectangular array of objects
            /// </summary>
            /// <param name="objects">List of objects to flatten</param>
            /// <returns>Rectangular array where objects are stored in [0] and properties are stored in [1]</returns>
            private static object[,] GetObjectArray<T>(IEnumerable<T> objects)
            {
                // Get list of object properties
                PropertyInfo[] properties = typeof(T).GetProperties();

                // Create rectangular array based on # of objects and # of object properties
                object[,] data = new object[objects.Count() + 1, properties.Length];

                // Loop through properties on object
                for (int j = 0; j < properties.Count(); j++)
                {
                    // Write the property name into the first row of the array
                    data[0, j] = properties[j].Name.Replace("_", " ");

                    // Loop through objects and write out the specified property of each one into the array
                    for (int i = 0; i < objects.Count(); i++)
                    {
                        data[i + 1, j] = properties[j].GetValue(objects.ElementAt(i), null);
                    }
                }

                // Return rectangular array
                return data;
            }

            /// <summary>
            /// Takes an Integer and converts it into Excel's column header code.
            /// For example: 1 = A; 2 = B; 27 = AA;
            /// </summary>
            /// <param name="colNumber">Number of Column in Excel. 1 = A</param>
            /// <returns>string that Excel can use</returns>
            private static string GetExcelColumn(int colNumber)
            {
                // If value is zero or less, return an empty string
                if (colNumber <= 0)
                    return string.Empty;

                // If the value is less than or equal to 26 (Z), the column header
                // is only one character long. If it's greater, call this recursively
                // to get the first letter(s) of the column code.
                string first = (colNumber <= 26 ? string.Empty :
                    GetExcelColumn((int)Math.Floor((colNumber - 1) / 26.00)));

                // Get the final letter in the column code
                int second = colNumber % 26;
                if (second == 0) second = 26;
                char finalLetter = (char)('A' + second - 1);            // Excel column header is the first part + the final character
                return string.Format("{0}{1}", new object[] { first, finalLetter });
            }
    }
}
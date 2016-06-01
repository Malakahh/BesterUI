using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using BesterUI.Helpers;
using System.IO;

namespace Classification_App
{
    class NoveltyExcel
    {
        private static Excel.Application MyApp = null; //The Application
        public static object missingValue = System.Reflection.Missing.Value; //Used for filler purpose
        public bool BooksOpen { get; private set; }
        private string statsFolderPath = null;

        public NoveltyExcel(string path)
        {
            foreach (var process in Process.GetProcessesByName("excel"))
            {
                process.Kill();
            }
            statsFolderPath = path + "/" + "Stats/";
            MyApp = new Excel.Application() { Visible = false };
        }

        private bool CreateOrOpenFiles()
        {
            Log.LogMessage("Opening books");
            try
            {
                if (!Directory.Exists(statsFolderPath))
                {
                    Directory.CreateDirectory(statsFolderPath);
                    Excel.Workbook currentBook = MyApp.Workbooks.Add(missingValue);
                    //Added Standard (Front, first, last)
                    CreateStandardBookSetup(currentBook);
                    //Remove default sheet
                    foreach (Excel.Worksheet wS in currentBook.Worksheets)
                    {
                        if (wS.Name == "Sheet1")
                        {
                            wS.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static void CreateStandardBookSetup(Excel.Workbook workBook)
        {
            //Create frontpage
            Excel.Worksheet overview = workBook.Sheets.Add(workBook.Sheets[workBook.Sheets.Count]);
            overview.Name = "Overview";

            //Create First
            Excel.Worksheet first = workBook.Sheets.Add(workBook.Sheets[workBook.Sheets.Count]);
            first.Name = "First";

            //Create Last
            Excel.Worksheet last = workBook.Sheets.Add(workBook.Sheets[workBook.Sheets.Count]);
            last.Name = "Last";

            WriteOverviewMeta(overview);

        }

        private static void WriteOverviewMeta(Excel.Worksheet workSheet)
        {
            #region [Names]
            workSheet.Cells[1, 1] = "A2High";
            workSheet.Cells[6, 1] = "A2Low";
            workSheet.Cells[11, 1] = "A3";
            workSheet.Cells[16, 1] = "V2High";
            workSheet.Cells[21, 1] = "V2Low";
            workSheet.Cells[26, 1] = "V3";
            #endregion

            #region [Average & standard deviation markers]
            //A2High
            workSheet.Cells[3, 1] = "AVG";
            workSheet.Cells[4, 1] = "STD";
            //A2Low
            workSheet.Cells[8, 1] = "AVG";
            workSheet.Cells[9, 1] = "STD";
            //A3
            workSheet.Cells[13, 1] = "AVG";
            workSheet.Cells[14, 1] = "STD";
            //V2High
            workSheet.Cells[18, 1] = "AVG";
            workSheet.Cells[19, 1] = "STD";
            //V2Low
            workSheet.Cells[23, 1] = "AVG";
            workSheet.Cells[24, 1] = "STD";
            //V3
            workSheet.Cells[28, 1] = "AVG";
            workSheet.Cells[29, 1] = "STD";
            #endregion

            #region [Score Labels]
            List<string> scoring2Labels = new List<string> { "Accuracy", "WFScore", "F1", "F2", "P1", "P2", "R1", "R2" };
            List<string> scoring3Labels = new List<string> { "Accuracy", "WFScore", "F1", "F2", "F3", "P1", "P2", "P3", "R1", "R2", "R3" };
            //A2High
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[2, i + 2] = scoring2Labels[i];
            }

            //A2Low
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[7, i + 2] = scoring2Labels[i];
            }

            //A3
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[12, i + 2] = scoring3Labels[i];
            }

            //V2High
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[17, i + 2] = scoring2Labels[i];
            }

            //V2Low
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[22, i + 2] = scoring2Labels[i];
            }

            //V3
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[27, i + 2] = scoring3Labels[i];
            }

            #endregion

            #region[Score Formulas]
         /*   string avgFormula = "=AVERAGE(First:Last!C";
            string stdevFormula = "=STDEV.P(First:Last!C";
            string endFormula = ")";
            //A2High AVG and Stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[3, i + 2] = avgFormula + (i + A2HighStart) + endFormula;
                workSheet.Cells[4, i + 2] = stdevFormula + (i + A2HighStart) + endFormula;
            }
          */ 


            #endregion

        }
    }
}

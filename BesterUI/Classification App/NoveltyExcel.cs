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
        public Excel.Workbook currentBook;

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
                    currentBook = MyApp.Workbooks.Add(missingValue);
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

        public void CreateStandardBookSetup(Excel.Workbook workBook)
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
        public void AddDataToPerson(string name, NoveltyResult predictResult)
        {
            Log.LogMessage("Writing results from " + name + " to excel files");
            foreach (Excel.Worksheet ws in currentBook.Sheets)
            {
                if (ws.Name == name)
                {
                    WriteResult(ws, name, predictResult);
                }
            }
        }
        public void WriteSheetMeta(Excel.Worksheet workSheet, string name)
        {
            /*
            % hit
            % covered
            Score value
            Parameter
            */
            workSheet.Cells[1, 1] = name;
            workSheet.Cells[3, 1] = "Recall";
            workSheet.Cells[4, 1] = "Covered";
            workSheet.Cells[5, 1] = "Score";
            workSheet.Cells[6, 1] = "C";
            workSheet.Cells[7, 1] = "Gamma";
            workSheet.Cells[8, 1] = "Kernel";

        }

        

        public void WriteResult(Excel.Worksheet workSheet, string name, NoveltyResult result)
        {
            /*
            % hit
            % covered
            Score value
            Parameter
            */
            workSheet.Cells[1, 2] = name;
            workSheet.Cells[3, 2] = (double)result.events.Where(x => x.isHit).Count() / result.events.Count; ;
            workSheet.Cells[4, 2] = (((double)result.poi.GetFlaggedAreas().Where(x => x.Item2 > result.start).Sum(x => (x.Item2 - x.Item1)) > 0) ? ((double)result.poi.GetFlaggedAreas().Where(x => x.Item2 > result.start).Sum(x => (x.Item2 - x.Item1)))/(result.start - result.end) : 0);
            workSheet.Cells[5, 2] = result.CalculateScore();
            workSheet.Cells[6, 2] = result.parameter.C;
            workSheet.Cells[7, 2] = result.parameter.Gamma;
            workSheet.Cells[8, 2] = result.parameter.Kernel.ToString();

        }


        private void WriteOverviewMeta(Excel.Worksheet workSheet)
        {
            #region [Average & standard deviation markers]
            //hits, covered, Score 
            workSheet.Cells[1, 1] = "Overview";
            workSheet.Cells[3, 1] = "HitAverage";
            workSheet.Cells[4, 1] = "CoveredAverage";
            workSheet.Cells[2, 2] = "Average";
            workSheet.Cells[2, 3] = "SD";
            #endregion

            #region[Formulas]
            string avgFormula = "=AVERAGE(First:Last!C";
            string stdevFormula = "=STDEV.P(First:Last!C";
            string endFormula = ")";

            //Average
            workSheet.Cells[3, 2] = $"{avgFormula}B3{endFormula}";
            workSheet.Cells[4, 2] = $"{avgFormula}B4{endFormula}";

            //SD
            workSheet.Cells[3, 3] = $"{stdevFormula}B3{endFormula}";
            workSheet.Cells[4, 3] = $"{stdevFormula}B4{endFormula}";
            #endregion

        }
    }
}

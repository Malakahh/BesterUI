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
            CreateOrOpenFiles();
        }



        public void Save()
        {
            Log.LogMessage("Saving Excel Files");
            currentBook.Save();
        }

        public void CloseHandler()
        {
            Log.LogMessage("Closing, saving and quiting excel");
            if (MyApp != null)
            {
                currentBook.Save();
                MyApp.Quit();
                MyApp = null;
            }
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
                    currentBook.SaveAs(statsFolderPath + "result");
                    //Added Standard (Front, first, last)
                    CreateStandardBookSetup(currentBook);
                    //Remove default sheetBook
                    foreach (Excel.Worksheet wS in currentBook.Worksheets)
                    {
                        if (wS.Name == "Sheet1")
                        {
                            wS.Delete();
                        }
                    }
                    BooksOpen = true;
                }
                else if (!File.Exists(statsFolderPath + "result.xlsx"))
                {
                    currentBook = MyApp.Workbooks.Add(missingValue);
                    currentBook.SaveAs(statsFolderPath + "result");
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
                    BooksOpen = true;
                }
                else
                {
                    currentBook = MyApp.Workbooks.Open(statsFolderPath + "result.xlsx", missingValue,
                                                            false,
                                                            missingValue,
                                                            missingValue,
                                                            missingValue,
                                                            true,
                                                            missingValue,
                                                            missingValue,
                                                            true,
                                                            missingValue,
                                                            missingValue,
                                                            missingValue);
                    BooksOpen = true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            currentBook.Save();
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

        public void AddPersonToBooks(string name)
        {
            Log.LogMessage("Adding " + name + " to excel files");
            try
            {
                if (BooksOpen)
                {
                    bool shouldAddPerson = true;
                    //Get Current book
                    foreach (Excel.Worksheet sheet in currentBook.Sheets)
                    {
                        if (sheet.Name == name)
                        {
                            Log.LogMessage("Person already exist in excel book:" + sheet.Name + ", skipping adding");
                            shouldAddPerson = false;
                            break;
                        }
                    }
                    if (shouldAddPerson == true)
                    {

                        //Add person
                        Excel.Worksheet lastSheet = currentBook.Sheets["Last"];
                        Excel.Worksheet currentSheet = (Excel.Worksheet)currentBook.Sheets.Add(lastSheet);
                        currentSheet.Name = name;

                        //Write standard data
                        WriteSheetMeta(currentSheet, name);
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void AddDataToPerson(string name, Dictionary<SENSOR, NoveltyResult> predictResult)
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
            workSheet.Cells[1, 1] = name;
            workSheet.Cells[3, 1] = "EventHits";
            workSheet.Cells[4, 1] = "EventMisses";
            workSheet.Cells[5, 1] = "EventsTotal";
            workSheet.Cells[5, 1] = "Covered";
            workSheet.Cells[6, 1] = "Score";
            workSheet.Cells[7, 1] = "TP";
            workSheet.Cells[8, 1] = "FP";
            workSheet.Cells[9, 1] = "TN";
            workSheet.Cells[10, 1] = "FN";
            workSheet.Cells[11, 1] = "C";
            workSheet.Cells[12, 1] = "Gamma";
            workSheet.Cells[13, 1] = "Nu";
            workSheet.Cells[14, 1] = "Kernel";

        }



        public void WriteResult(Excel.Worksheet workSheet, string name, Dictionary<SENSOR, NoveltyResult> result)
        {
            /*
            % hit
            % covered
            Score value
            Parameter
            */
            int counter = 3;
            /*Numre passer ikke mere*/
            //GSR
            workSheet.Cells[2, 2] = "GSR";
            /*3*/
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].CalculateScore();
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].CalculateHitResult().misses;
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].parameter.C;
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].parameter.Gamma;
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].parameter.Nu;
            /*12*/
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].parameter.Kernel.ToString();
            counter++;

            workSheet.Cells[counter++, 2] = "EEG";
            /*15*/
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].CalculateScore();
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].CalculateHitResult().misses;
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].parameter.C;
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].parameter.Gamma;
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].parameter.Nu;
            /*24*/
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].parameter.Kernel.ToString();
            counter++;

            workSheet.Cells[counter++, 2] = "FACE";
            /*27*/
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].CalculateScore();
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].CalculateHitResult().misses;
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].parameter.C;
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].parameter.Gamma;
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].parameter.Nu;
            /*36*/
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].parameter.Kernel.ToString();

            counter++;

            workSheet.Cells[counter++, 2] = "HR";
            /*39*/
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].CalculateScore();
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].CalculateHitResult().misses;
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].parameter.C;
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].parameter.Gamma;
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].parameter.Nu;
            /*48*/
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].parameter.Kernel.ToString();

        }


        private void WriteOverviewMeta(Excel.Worksheet workSheet)
        {
            #region [Average & standard deviation markers]
            //hits, covered, Score 
            workSheet.Cells[1, 1] = "Overview";
            workSheet.Cells[3, 1] = "GSR";
            workSheet.Cells[4, 1] = "HitAverage";
            workSheet.Cells[5, 1] = "CoveredAverage";
            workSheet.Cells[2, 2] = "Average";
            workSheet.Cells[2, 3] = "SD";

            workSheet.Cells[7, 1] = "EEG";
            workSheet.Cells[8, 1] = "HitAverage";
            workSheet.Cells[9, 1] = "CoveredAverage";

            workSheet.Cells[11, 1] = "FACE";
            workSheet.Cells[12, 1] = "HitAverage";
            workSheet.Cells[13, 1] = "CoveredAverage";

            workSheet.Cells[14, 1] = "HR";
            workSheet.Cells[15, 1] = "HitAverage";
            workSheet.Cells[16, 1] = "CoveredAverage";
            #endregion

            #region[Formulas]
            string avgFormula = "=AVERAGE(First:Last!B";
            string stdevFormula = "=STDEV.P(First:Last!B";
            string endFormula = ")";

            //Average
            workSheet.Cells[4, 2] = $"{avgFormula}3{endFormula}";
            workSheet.Cells[5, 2] = $"{avgFormula}4{endFormula}";

            workSheet.Cells[8, 2] = $"{avgFormula}11{endFormula}";
            workSheet.Cells[9, 2] = $"{avgFormula}12{endFormula}";

            workSheet.Cells[12, 2] = $"{avgFormula}19{endFormula}";
            workSheet.Cells[13, 2] = $"{avgFormula}20{endFormula}";

            workSheet.Cells[15, 2] = $"{avgFormula}27{endFormula}";
            workSheet.Cells[16, 2] = $"{avgFormula}28{endFormula}";

            //SD
            workSheet.Cells[4, 3] = $"{stdevFormula}3{endFormula}";
            workSheet.Cells[5, 3] = $"{stdevFormula}4{endFormula}";

            workSheet.Cells[8, 3] = $"{stdevFormula}11{endFormula}";
            workSheet.Cells[9, 3] = $"{stdevFormula}12{endFormula}";

            workSheet.Cells[12, 3] = $"{stdevFormula}19{endFormula}";
            workSheet.Cells[13, 3] = $"{stdevFormula}20{endFormula}";

            workSheet.Cells[15, 3] = $"{stdevFormula}27{endFormula}";
            workSheet.Cells[16, 3] = $"{stdevFormula}28{endFormula}";
            #endregion

        }
    }
}

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
        private enum Book { HIT, COVERED};
        private static Excel.Application MyApp = null; //The Application
        public static object missingValue = System.Reflection.Missing.Value; //Used for filler purpose
        public bool BooksOpen { get; private set; }
        private Dictionary<Book, Excel.Workbook> books = new Dictionary<Book, Excel.Workbook>(); //Dictionary of the different books
        private string statsFolderPath = null;

        public NoveltyExcel(string path, string book)
        {
            foreach (var process in Process.GetProcessesByName("excel"))
            {
                process.Kill();
            }
            statsFolderPath = path + "/" + "Stats/";
            MyApp = new Excel.Application() { Visible = false };
            CreateOrOpenFiles(book);
        }



        public void Save()
        {
            Log.LogMessage("Saving Excel Files");
            foreach (Book book in books.Keys)
            {
                books[book].Save();
            }
        }

        public void CloseHandler()
        {
            Log.LogMessage("Closing, saving and quiting excel");
            if (MyApp != null)
            {
                foreach (Book book in books.Keys)
                {
                    books[book].Save();
                    books[book].Close(true);
                }
                books.Clear();
                MyApp.Quit();
                MyApp = null;
            }
        }
        List<Book> fileNames = new List<Book> { Book.HIT, Book.COVERED};
        private bool CreateOrOpenFiles(string book)
        {
            foreach (Book theBook in fileNames)
            {
                Log.LogMessage("Opening books");
                try
                {
                    string BookString = theBook.ToString() + book;
                    if (!Directory.Exists(statsFolderPath))
                    {
                        Directory.CreateDirectory(statsFolderPath);
                        var currentBook = MyApp.Workbooks.Add(missingValue);
                        currentBook.SaveAs(statsFolderPath + BookString);
                        books.Add(theBook, currentBook);
                        //Added Standard (Front, first, last)
                        CreateStandardBookSetup(currentBook, BookString);
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
                    else if (!File.Exists(statsFolderPath + BookString + ".xlsx"))
                    {
                        var currentBook = MyApp.Workbooks.Add(missingValue);
                        currentBook.SaveAs(statsFolderPath + BookString);
                        books.Add(theBook, currentBook);
                        //Added Standard (Front, first, last)
                        CreateStandardBookSetup(currentBook, BookString);
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
                        var currentBook = MyApp.Workbooks.Open(statsFolderPath + BookString + ".xlsx", missingValue,
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
                        books.Add(theBook, currentBook);
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public void CreateStandardBookSetup(Excel.Workbook workBook, string type)
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

            if (type == "result")
            {
                WriteOverviewMeta(overview);
            }
            else if (type == "voting")
            {
                WriteOverviewVoting(overview);
            }

        }

        public void AddPersonToBooks(string name)
        {
            Log.LogMessage("Adding " + name + " to excel files");
            try
            {
                if (BooksOpen)
                {
                    foreach (Book book in fileNames)
                    {
                        bool shouldAddPerson = true;
                        //Get Current book
                        foreach (Excel.Worksheet sheet in books[book].Sheets)
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
                            Excel.Workbook currentBook = books[book];
                            //Add person
                            Excel.Worksheet lastSheet = currentBook.Sheets["Last"];
                            Excel.Worksheet currentSheet = (Excel.Worksheet)currentBook.Sheets.Add(lastSheet);
                            currentSheet.Name = name;

                            //Write standard data
                            WriteSheetMeta(currentSheet, name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

    /*    public void AddPersonToVotingBook(string name)
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
                        WriteVotingSheetMeta(currentSheet, name);
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        public void AddVotingDataToPerson(string name, Dictionary<int, NoveltyResult> predictResult)
        {
            Log.LogMessage("Writing voting results from " + name + " to excel files");
            foreach (Excel.Worksheet ws in currentBook.Sheets)
            {
                if (ws.Name == name)
                {
                    WriteVotingResult(ws, predictResult);
                }
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
        }*/
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
            workSheet.Cells[counter++, 2] = result[SENSOR.GSR].CalculateHitScore();
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
            workSheet.Cells[counter++, 2] = result[SENSOR.EEG].CalculateHitScore();
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
            workSheet.Cells[counter++, 2] = result[SENSOR.FACE].CalculateHitScore();
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
            workSheet.Cells[counter++, 2] = result[SENSOR.HR].CalculateHitScore();
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

        public void WriteVotingSheetMeta(Excel.Worksheet workSheet, string name)
        {
            workSheet.Cells[1, 1] = name;
            workSheet.Cells[3, 1] = "Score";
            workSheet.Cells[4, 1] = "HitsEvents";
            workSheet.Cells[5, 1] = "TotalEvents";
            workSheet.Cells[6, 1] = "Hits";
            workSheet.Cells[7, 1] = "Misses";


        }
        public void WriteVotingResult(Excel.Worksheet workSheet, Dictionary<int, NoveltyResult> result)
        {

            int counter = 3;
            workSheet.Cells[2, 2] = "1";
            /*3*/
            workSheet.Cells[counter++, 2] = result[1].CalculateHitScore();
            workSheet.Cells[counter++, 2] = result[1].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[1].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[1].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[1].CalculateHitResult().misses;
            /*7*/

            workSheet.Cells[counter++, 2] = "2";
            /*9*/
            workSheet.Cells[counter++, 2] = result[2].CalculateHitScore();
            workSheet.Cells[counter++, 2] = result[2].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[2].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[2].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[2].CalculateHitResult().misses;
            /*13*/

            workSheet.Cells[counter++, 2] = "3";
            /*15*/
            workSheet.Cells[counter++, 2] = result[3].CalculateHitScore();
            workSheet.Cells[counter++, 2] = result[3].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[3].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[3].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[3].CalculateHitResult().misses;
            /*29*/

            workSheet.Cells[counter++, 2] = "4";
            /*31*/
            workSheet.Cells[counter++, 2] = result[4].CalculateHitScore();
            workSheet.Cells[counter++, 2] = result[4].CalculateHitResult().eventHits;
            workSheet.Cells[counter++, 2] = result[4].CalculateHitResult().eventsTotal;
            workSheet.Cells[counter++, 2] = result[4].CalculateHitResult().hits;
            workSheet.Cells[counter++, 2] = result[4].CalculateHitResult().misses;
            /*35*/

        }
        
        private void WriteOverviewVoting(Excel.Worksheet workSheet)
        {
            #region [Labels]
            workSheet.Cells[1, 1] = "Overview";
            workSheet.Cells[3, 1] = "1";
            workSheet.Cells[4, 1] = "Score";
            workSheet.Cells[5, 1] = "HitsEvents";
            workSheet.Cells[6, 1] = "TotalEvents";
            workSheet.Cells[7, 1] = "Hits";
            workSheet.Cells[8, 1] = "Misses";

            workSheet.Cells[3, 2] = "Average";
            workSheet.Cells[3, 3] = "SD";

            workSheet.Cells[10, 1] = "2";
            workSheet.Cells[11, 1] = "Score";
            workSheet.Cells[12, 1] = "HitsEvents";
            workSheet.Cells[13, 1] = "TotalEvents";
            workSheet.Cells[14, 1] = "Hits";
            workSheet.Cells[15, 1] = "Misses";

            workSheet.Cells[17, 1] = "3";
            workSheet.Cells[18, 1] = "Score";
            workSheet.Cells[19, 1] = "HitsEvents";
            workSheet.Cells[20, 1] = "TotalEvents";
            workSheet.Cells[21, 1] = "Hits";
            workSheet.Cells[22, 1] = "Misses";

            workSheet.Cells[24, 1] = "4";
            workSheet.Cells[25, 1] = "Score";
            workSheet.Cells[26, 1] = "HitsEvents";
            workSheet.Cells[27, 1] = "TotalEvents";
            workSheet.Cells[28, 1] = "Hits";
            workSheet.Cells[29, 1] = "Misses";
            #endregion
            #region [Formulas]
            string avgFormula = "=AVERAGE(First:Last!B";
            string stdevFormula = "=STDEV.P(First:Last!B";
            string endFormula = ")";
            #endregion
            #region [Calculations]    
            int AverageCounter = 3;
            workSheet.Cells[4, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[5, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[6, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[7, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[8, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            AverageCounter++;
            workSheet.Cells[11, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[12, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[13, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[14, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[15, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            AverageCounter++;

            workSheet.Cells[18, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[19, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[20, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[21, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[22, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";

            AverageCounter++;
            workSheet.Cells[25, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[26, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[27, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[28, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";
            workSheet.Cells[29, 2] = $"{avgFormula}{AverageCounter++}{endFormula}";

            int SDCounter = 3;
            workSheet.Cells[4, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[5, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[6, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[7, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[8, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            SDCounter++;
            workSheet.Cells[11, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[12, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[13, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[14, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[15, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            SDCounter++;

            workSheet.Cells[18, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[19, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[20, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[21, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[22, 2] = $"{avgFormula}{SDCounter++}{endFormula}";

            SDCounter++;
            workSheet.Cells[25, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[26, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[27, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[28, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            workSheet.Cells[29, 2] = $"{avgFormula}{SDCounter++}{endFormula}";
            #endregion

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

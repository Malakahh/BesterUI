using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using Classification_App;
using BesterUI.Helpers;
using System.Diagnostics;


namespace Classification_App
{
    class ExcelHandler
    {
        public enum Book { GSR, EEG, HR, FACE, Stacking, Boosting, Voting };
        private static Excel.Application MyApp = null; //The Application
        private Dictionary<Book, Excel.Workbook> books = new Dictionary<Book, Excel.Workbook>(); //Dictionary of the different books
        public static object missingValue = System.Reflection.Missing.Value; //Used for filler purpose
        public bool BooksOpen { get; private set; }
        private string statsFolderPath = null;

        public ExcelHandler(string path)
        {
            foreach (var process in Process.GetProcessesByName("excel"))
            {
                process.Kill();
            }
            statsFolderPath = path + "/" + "Stats/";
            MyApp = new Excel.Application() { Visible = false };
            BooksOpen = CreateNOpenFiles();
        }


        /// <summary>
        /// Opens the different books at a certain path, if they do not exists create instead
        /// </summary>
        /// <param name="Path">Path of where to save/open the files</param>
        /// 
        List<Book> fileNames = new List<Book> { Book.GSR, Book.EEG, Book.HR, Book.FACE, Book.Stacking, Book.Boosting, Book.Voting };
        private bool CreateNOpenFiles()
        {
            Log.LogMessage("Opening books");
            foreach (Book book in fileNames)
            {
                try
                {
                    if (!Directory.Exists(statsFolderPath))
                    {
                        Directory.CreateDirectory(statsFolderPath);
                        Excel.Workbook currentBook = MyApp.Workbooks.Add(missingValue);
                        currentBook.SaveAs(statsFolderPath + book);
                        books.Add(book, currentBook);
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
                        CheckWriteAccess(currentBook);

                    }
                    else if (!File.Exists(statsFolderPath + Enum.GetName(typeof(Book), book) + ".xlsx"))
                    {
                        Excel.Workbook currentBook = MyApp.Workbooks.Add(missingValue);
                        currentBook.SaveAs(statsFolderPath + book);
                        books.Add(book, currentBook);
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
                        CheckWriteAccess(currentBook);
                    }
                    else
                    {
                        Excel.Workbook currentBook = MyApp.Workbooks.Open(statsFolderPath + Enum.GetName(typeof(Book), book) + ".xlsx", missingValue,
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
                        CheckWriteAccess(currentBook);
                        books.Add(book, currentBook);
                    }
                }
                catch (Exception ex)
                {
                    MessageError(ex.ToString());
                    return false;
                }
            }
            return true;
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
                                Log.LogMessage("Person already exist in excel book:" + books[book].Name + ", skipping adding");
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
                            WriteSheetMetaData(currentSheet, name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageError(ex.ToString());
                return;
            }
        }

        public void AddDataToPerson(string name, Book book, PredictionResult predictResult, SAMDataPoint.FeelingModel model)
        {
            Log.LogMessage("Writing results from " + name + " to excel files");
            foreach (Excel.Worksheet ws in books[book].Sheets)
            {
                if (ws.Name == name)
                {
                    WriteResult(ws, predictResult, model);
                }
            }
        }

        public void AddBookToBook(string path)
        {
            Excel.Workbook otherBook = MyApp.Workbooks.Open(path, missingValue,
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

            for (int i = ((Excel.Worksheet)otherBook.Sheets["First"]).Index; i < ((Excel.Worksheet)otherBook.Sheets["Last"]).Index; i++)
            {
            }

        }


        public void Save()
        {
            Log.LogMessage("Saving Excel Files");
            foreach (Book book in books.Keys)
            {
                books[book].Save();
            }
        }

        public void CloseBooks()
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

        #region [Helper functions]

        private void WriteSheetMetaData(Excel.Worksheet workSheet, string name)
        {
            int i = 1;
            //Name
            workSheet.Cells[i++, 1] = name;

            //A2High
            workSheet.Cells[i, 1] = "A2High"; //2
            workSheet.Cells[i++, 2] = "Accuracy"; //2
            workSheet.Cells[i++, 2] = "WFScore";
            workSheet.Cells[i++, 2] = "Fscore1";
            workSheet.Cells[i++, 2] = "Fscore2";
            workSheet.Cells[i++, 2] = "P1";
            workSheet.Cells[i++, 2] = "P2";
            workSheet.Cells[i++, 2] = "R1";
            workSheet.Cells[i++, 2] = "R2";
            workSheet.Cells[i++, 2] = "Features";
            workSheet.Cells[i++, 2] = "C";
            workSheet.Cells[i++, 2] = "Gamma";
            workSheet.Cells[i++, 2] = "Kernel";//13

            //A2Low
            workSheet.Cells[++i, 1] = "A2Low"; //15
            workSheet.Cells[i++, 2] = "Accuracy"; //15
            workSheet.Cells[i++, 2] = "WFScore";
            workSheet.Cells[i++, 2] = "Fscore1";
            workSheet.Cells[i++, 2] = "Fscore2";
            workSheet.Cells[i++, 2] = "P1";
            workSheet.Cells[i++, 2] = "P2";
            workSheet.Cells[i++, 2] = "R1";
            workSheet.Cells[i++, 2] = "R2";
            workSheet.Cells[i++, 2] = "Features";
            workSheet.Cells[i++, 2] = "C";
            workSheet.Cells[i++, 2] = "Gamma";
            workSheet.Cells[i++, 2] = "Kernel";//26

            //A3
            workSheet.Cells[++i, 1] = "A3"; //28
            workSheet.Cells[i++, 2] = "Accuracy"; //28
            workSheet.Cells[i++, 2] = "WFScore";
            workSheet.Cells[i++, 2] = "Fscore1";
            workSheet.Cells[i++, 2] = "Fscore2";
            workSheet.Cells[i++, 2] = "Fscore3";
            workSheet.Cells[i++, 2] = "P1";
            workSheet.Cells[i++, 2] = "P2";
            workSheet.Cells[i++, 2] = "P3";
            workSheet.Cells[i++, 2] = "R1";
            workSheet.Cells[i++, 2] = "R2";
            workSheet.Cells[i++, 2] = "R3";
            workSheet.Cells[i++, 2] = "Features";
            workSheet.Cells[i++, 2] = "C";
            workSheet.Cells[i++, 2] = "Gamma";
            workSheet.Cells[i++, 2] = "Kernel"; //42

            //V2High
            workSheet.Cells[++i, 1] = "V2High"; //44
            workSheet.Cells[i++, 2] = "Accuracy"; //44
            workSheet.Cells[i++, 2] = "WFScore";
            workSheet.Cells[i++, 2] = "Fscore1";
            workSheet.Cells[i++, 2] = "Fscore2";
            workSheet.Cells[i++, 2] = "P1";
            workSheet.Cells[i++, 2] = "P2";
            workSheet.Cells[i++, 2] = "R1";
            workSheet.Cells[i++, 2] = "R2";
            workSheet.Cells[i++, 2] = "Features";
            workSheet.Cells[i++, 2] = "C";
            workSheet.Cells[i++, 2] = "Gamma";
            workSheet.Cells[i++, 2] = "Kernel";//55

            //V2Low
            workSheet.Cells[++i, 1] = "V2Low"; //57
            workSheet.Cells[i++, 2] = "Accuracy"; //57
            workSheet.Cells[i++, 2] = "WFScore";
            workSheet.Cells[i++, 2] = "Fscore1";
            workSheet.Cells[i++, 2] = "Fscore2";
            workSheet.Cells[i++, 2] = "P1";
            workSheet.Cells[i++, 2] = "P2";
            workSheet.Cells[i++, 2] = "R1";
            workSheet.Cells[i++, 2] = "R2";
            workSheet.Cells[i++, 2] = "Features";
            workSheet.Cells[i++, 2] = "C";
            workSheet.Cells[i++, 2] = "Gamma";
            workSheet.Cells[i++, 2] = "Kernel";//68

            //V3
            workSheet.Cells[++i, 1] = "V3"; //70
            workSheet.Cells[i++, 2] = "Accuracy"; //70
            workSheet.Cells[i++, 2] = "WFScore";
            workSheet.Cells[i++, 2] = "Fscore1";
            workSheet.Cells[i++, 2] = "Fscore2";
            workSheet.Cells[i++, 2] = "Fscore3";
            workSheet.Cells[i++, 2] = "P1";
            workSheet.Cells[i++, 2] = "P2";
            workSheet.Cells[i++, 2] = "P3";
            workSheet.Cells[i++, 2] = "R1";
            workSheet.Cells[i++, 2] = "R2";
            workSheet.Cells[i++, 2] = "R3";
            workSheet.Cells[i++, 2] = "Features";
            workSheet.Cells[i++, 2] = "C";
            workSheet.Cells[i++, 2] = "Gamma";
            workSheet.Cells[i++, 2] = "Kernel"; //84

        }
        private const int A2HighStart = 2;
        private const int A2LowStart = 15;
        private const int A3Start = 28;
        private const int V2HighStart = 44;
        private const int V2LowStart = 57;
        private const int V3Start = 70;

        private void WriteResult(Excel.Worksheet workSheet, PredictionResult pResult, SAMDataPoint.FeelingModel feelingModel)
        {
            int counter = 0;
            switch (feelingModel)
            {
                case SAMDataPoint.FeelingModel.Arousal2High:
                    counter = A2HighStart;
                    break;
                case SAMDataPoint.FeelingModel.Arousal2Low:
                    counter = A2LowStart;
                    break;
                case SAMDataPoint.FeelingModel.Arousal3:
                    counter = A3Start;
                    break;
                case SAMDataPoint.FeelingModel.Valence2High:
                    counter = V2HighStart;
                    break;
                case SAMDataPoint.FeelingModel.Valence2Low:
                    counter = V2LowStart;
                    break;
                case SAMDataPoint.FeelingModel.Valence3:
                    counter = V3Start;
                    break;
            }

            workSheet.Cells[counter, 3] = pResult.GetAccuracy();

            counter++;
            workSheet.Cells[counter, 3] = pResult.GetAverageFScore();

            for (int f = 0; f < pResult.fscores.Count; f++)
            {
                counter++;
                if (double.IsNaN(pResult.fscores[f]))
                {
                    workSheet.Cells[counter, 3] = "NaN";
                }
                else
                {
                    workSheet.Cells[counter, 3] = pResult.fscores[f];
                }
            }

            for (int p = 0; p < pResult.precisions.Count; p++)
            {
                counter++;
                if (double.IsNaN(pResult.precisions[p]))
                {
                    workSheet.Cells[counter, 3] = "NaN";
                }
                else
                {
                    workSheet.Cells[counter, 3] = pResult.precisions[p];
                }
            }

            for (int r = 0; r < pResult.recalls.Count; r++)
            {
                counter++;
                if (double.IsNaN(pResult.recalls[r]))
                {
                    workSheet.Cells[counter, 3] = "NaN";
                }
                else
                {
                    workSheet.Cells[counter, 3] = pResult.recalls[r];
                }
            }
            counter++;
            for (int i = 3; i < pResult.features.Count + 3; i++)
            {
                workSheet.Cells[counter, i] = pResult.features[i - 3].name;
            }


            counter++;
            workSheet.Cells[counter, 3] = pResult.svmParams.C;

            counter++;
            workSheet.Cells[counter, 3] = pResult.svmParams.Gamma;

            counter++;
            workSheet.Cells[counter, 3] = pResult.svmParams.Kernel;


        }

        /// <summary>
        /// If this function throws an exception, then there is no write access
        /// </summary>
        /// <param name="workBook"></param>
        private void CheckWriteAccess(Excel.Workbook workBook)
        {
            string temp = workBook.Sheets[1].Cells[1, 1].value;
            workBook.Sheets[1].Cells[1, 1] = "writing";
            workBook.Sheets[1].Cells[1, 1] = temp;

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
            string avgFormula = "=AVERAGE(First:Last!C";
            string stdevFormula = "=STDEV.P(First:Last!C";
            string endFormula = ")";
            //A2High AVG and Stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[3, i + 2] = avgFormula + (i + A2HighStart) + endFormula;
                workSheet.Cells[4, i + 2] = stdevFormula + (i + A2HighStart) + endFormula;
            }
            //A2Low AVG and Stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[8, i + 2] = avgFormula + (i + A2LowStart) + endFormula;
                workSheet.Cells[9, i + 2] = stdevFormula + (i + A2LowStart) + endFormula;
            }
            //A3 AVG and Stdev
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[13, i + 2] = avgFormula + (i + A3Start) + endFormula;
                workSheet.Cells[14, i + 2] = stdevFormula + (i + A3Start) + endFormula;
            }
            //V2High AVG and stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[18, i + 2] = avgFormula + (i + V2HighStart) + endFormula;
                workSheet.Cells[19, i + 2] = stdevFormula + (i + V2HighStart) + endFormula;
            }
            //V2Low AVG and stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[23, i + 2] = avgFormula + (i + V2LowStart) + endFormula;
                workSheet.Cells[24, i + 2] = stdevFormula + (i + V2LowStart) + endFormula;
            }
            //V3 AVG and Stdev
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[28, i + 2] = avgFormula + (i + V3Start) + endFormula;
                workSheet.Cells[29, i + 2] = stdevFormula + (i + V3Start) + endFormula;
            }


            #endregion

        }

        private void MessageError(string message)
        {
            Log.LogMessage("Something went wrong in ExcelHandler: " + message);
        }

        #endregion
    }
}

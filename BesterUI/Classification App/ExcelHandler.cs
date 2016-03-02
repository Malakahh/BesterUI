using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using Classification_App;
using BesterUI.Helpers;


namespace Classification_App
{
    class ExcelHandler
    {
        public enum Book { GSR, EEG, HR, FACE, Stacking, Boosting, Voting };
        private static Excel.Application MyApp = null; //The Application
        private Dictionary<Book, Excel.Workbook> books = new Dictionary<Book, Excel.Workbook>(); //Dictionary of the different books
        object missingValue = System.Reflection.Missing.Value; //Used for filler purpose
        public bool BooksOpen { get; private set; }
        private string statsFolderPath = null;

        public ExcelHandler(string path)
        {
            statsFolderPath = path + "/" + "Stats/";
            MyApp = new Excel.Application() { Visible = false };
            BooksOpen = CreateNOpenFiles();
        }


        /// <summary>
        /// Opens the different books at a certain path, if they do not exists create instead
        /// </summary>
        /// <param name="Path">Path of where to save/open the files</param>
        /// 
        List<Book> fileNames = new List<Book> { Book.GSR, Book.EEG, Book.HR, Book.Stacking, Book.Boosting, Book.Voting };
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
                        //Get Current book
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
            foreach (Book book in books.Keys)
            {
                books[book].Save();
                books[book].Close(true);
            }
            books.Clear();
            MyApp.Quit();
            MyApp = null;
        }

        #region [Helper functions]

        private void WriteSheetMetaData(Excel.Worksheet workSheet, string name)
        {
            int i = 1;
            //Name
            workSheet.Cells[i++, 1] = name;

            //A2
            workSheet.Cells[i, 1] = "A2"; //2
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

            //A3
            workSheet.Cells[++i, 1] = "A3"; //15
            workSheet.Cells[i++, 2] = "Accuracy"; //15
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
            workSheet.Cells[i++, 2] = "Kernel"; //29

            //A2
            workSheet.Cells[++i, 1] = "V2"; //31
            workSheet.Cells[i++, 2] = "Accuracy"; //31
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
            workSheet.Cells[i++, 2] = "Kernel";//42

            //V3
            workSheet.Cells[++i, 1] = "V3"; //44
            workSheet.Cells[i++, 2] = "Accuracy"; //44
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
            workSheet.Cells[i++, 2] = "Kernel"; //58

        }
        private const int A2Start = 2;
        private const int A3Start = 15;
        private const int V2Start = 31;
        private const int V3Start = 44;

        private void WriteResult(Excel.Worksheet workSheet, PredictionResult pResult, SAMDataPoint.FeelingModel feelingModel)
        {
            int counter = 0;
            switch (feelingModel)
            {
                case SAMDataPoint.FeelingModel.Arousal2High:
                case SAMDataPoint.FeelingModel.Arousal2Low:
                    counter = A2Start;
                    break;
                case SAMDataPoint.FeelingModel.Arousal3:
                    counter = A3Start;
                    break;
                case SAMDataPoint.FeelingModel.Valence2High:
                case SAMDataPoint.FeelingModel.Valence2Low:
                    counter = V2Start;
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

        private void CreateStandardBookSetup(Excel.Workbook workBook)
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

        private void WriteOverviewMeta(Excel.Worksheet workSheet)
        {
            #region [Names]
            workSheet.Cells[1, 1] = "A2";
            workSheet.Cells[6, 1] = "A3";
            workSheet.Cells[11, 1] = "V2";
            workSheet.Cells[16, 1] = "V3";
            #endregion

            #region [Average & standard deviation markers]
            //A2
            workSheet.Cells[3, 1] = "AVG";
            workSheet.Cells[4, 1] = "STD";
            //A3
            workSheet.Cells[8, 1] = "AVG";
            workSheet.Cells[9, 1] = "STD";
            //A4
            workSheet.Cells[13, 1] = "AVG";
            workSheet.Cells[14, 1] = "STD";
            //A5
            workSheet.Cells[18, 1] = "AVG";
            workSheet.Cells[19, 1] = "STD";
            #endregion

            #region [Score Labels]
            List<string> scoring2Labels = new List<string> { "Accuracy", "WFScore", "F1", "F2", "P1", "P2", "R1", "R2" };
            List<string> scoring3Labels = new List<string> { "Accuracy", "WFScore", "F1", "F2", "F3", "P1", "P2", "P3", "R1", "R2", "R3" };
            //A2
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[2, i + 2] = scoring2Labels[i];
            }

            //V2
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[12, i + 2] = scoring2Labels[i];
            }

            //A3
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[7, i + 2] = scoring3Labels[i];
            }

            //V3
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[17, i + 2] = scoring3Labels[i];
            }

            #endregion

            #region[Score Formulas]
            string avgFormula = "=AVERAGE(First:Last!C";
            string stdevFormula = "=STDEV.S(First:Last!C"; //TODO: Find out whether to use stdev.s or .p
            string endFormula = ")";
            //A2 AVG and Stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[3, i + 2] = avgFormula + (i + 2) + endFormula;
                workSheet.Cells[4, i + 2] = stdevFormula + (i + 2) + endFormula;
            }
            //A3 AVG and Stdev
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[8, i + 2] = avgFormula + (i + 15) + endFormula;
                workSheet.Cells[9, i + 2] = stdevFormula + (i + 15) + endFormula;
            }
            //V2 AVG and stdev
            for (int i = 0; i < scoring2Labels.Count; i++)
            {
                workSheet.Cells[13, i + 2] = avgFormula + (i + 31) + endFormula;
                workSheet.Cells[14, i + 2] = stdevFormula + (i + 31) + endFormula;
            }
            //V3 AVG and Stdev
            for (int i = 0; i < scoring3Labels.Count; i++)
            {
                workSheet.Cells[18, i + 2] = avgFormula + (i + 44) + endFormula;
                workSheet.Cells[19, i + 2] = stdevFormula + (i + 44) + endFormula;
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

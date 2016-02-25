using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using Classification_App;


namespace Classification_App
{
    class ExcelHandler
    {
        public enum Book { GSR, EEG, HR, Stacking, Boosting, Voting };
        private static Excel.Application MyApp = null; //The Application
        private Dictionary<Book, Excel.Workbook> books = new Dictionary<Book, Excel.Workbook>(); //Dictionary of the different books
        object missingValue = System.Reflection.Missing.Value; //Used for filler purpose
        private bool booksOpen = false;
        private string statsFolderPath = null;

        public ExcelHandler(string path)
        {
            statsFolderPath = path + "/Stats/";
            MyApp = new Excel.Application() { Visible = false };
        }


        /// <summary>
        /// Opens the different books at a certain path, if they do not exists create instead
        /// </summary>
        /// <param name="Path">Path of where to save/open the files</param>
        /// 
            List<Book> fileNames = new List<Book> { Book.GSR, Book.EEG, Book.HR, Book.Stacking, Book.Boosting, Book.Voting };
        public void CreateNOpenFiles()
        {
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

                        books.Add(book, currentBook);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
            booksOpen = true;
        }

        public void AddPersonToBooks(string name)
        {
            try
            {
                if (booksOpen)
                {
                    foreach (Book book in fileNames)
                    {
                        //Get Current book
                        Excel.Workbook currentBook = books[book];

                        //Add person
                        Excel.Worksheet currentSheet = (Excel.Worksheet)currentBook.Sheets.Add(currentBook.Sheets[currentBook.Sheets.Count]);
                        currentSheet.Name = name;

                        //Write standard data
                        WriteSheetMetaData(currentSheet, name);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        public void AddDataToPerson(string name, Book book, PredictionResult predictResult, SAMDataPoint.FeelingModel model)
        {
            foreach (Excel.Worksheet ws in books[book].Sheets)
            {
                if (ws.Name == name)
                {
                    WriteResult(ws, predictResult, model);
                }
            }
        }

        public void CloseBooks()
        {
            foreach (Book book in books.Keys)
            {
                books[book].Save();
                books[book].Close(true);
            }
        }

        #region [Helper functions]

        private void WriteSheetMetaData(Excel.Worksheet workSheet, string name)
        {
            //Name
            workSheet.Cells[1, 1] = name;
            
            //A2
            workSheet.Cells[2, 1] = "A2";
            workSheet.Cells[2, 2] = "Features";
            workSheet.Cells[3, 2] = "C";
            workSheet.Cells[4, 2] = "Gamma";
            workSheet.Cells[5, 2] = "Kernel";
            workSheet.Cells[6, 2] = "R1";
            workSheet.Cells[7, 2] = "R2";
            workSheet.Cells[8, 2] = "P1";
            workSheet.Cells[9, 2] = "P2";
            workSheet.Cells[10, 2] = "Fscore1";
            workSheet.Cells[11, 2] = "Fscore2";
            workSheet.Cells[12, 2] = "WFScore";
            


            //A3
            workSheet.Cells[14, 1] = "A3";
            workSheet.Cells[14, 2] = "Features";
            workSheet.Cells[15, 2] = "C";
            workSheet.Cells[16, 2] = "Gamma";
            workSheet.Cells[17, 2] = "Kernel";
            workSheet.Cells[18, 2] = "R1";
            workSheet.Cells[19, 2] = "R2";
            workSheet.Cells[20, 2] = "R3";
            workSheet.Cells[21, 2] = "P1";
            workSheet.Cells[22, 2] = "P2";
            workSheet.Cells[23, 2] = "P3";
            workSheet.Cells[24, 2] = "Fscore1";
            workSheet.Cells[25, 2] = "Fscore2";
            workSheet.Cells[26, 2] = "Fscore3";
            workSheet.Cells[27, 2] = "WFScore";

            //V2
            workSheet.Cells[29, 1] = "V2";
            workSheet.Cells[29, 2] = "Features";
            workSheet.Cells[30, 2] = "C";
            workSheet.Cells[31, 2] = "Gamma";
            workSheet.Cells[32, 2] = "Kernel";
            workSheet.Cells[33, 2] = "R1";
            workSheet.Cells[34, 2] = "R2";
            workSheet.Cells[35, 2] = "P1";
            workSheet.Cells[36, 2] = "P2";
            workSheet.Cells[37, 2] = "Fscore1";
            workSheet.Cells[38, 2] = "Fscore2";
            workSheet.Cells[39, 2] = "WFScore";

            //V3
            workSheet.Cells[41, 1] = "V3";
            workSheet.Cells[41, 2] = "Features";
            workSheet.Cells[42, 2] = "C";
            workSheet.Cells[43, 2] = "Gamma";
            workSheet.Cells[44, 2] = "Kernel";
            workSheet.Cells[45, 2] = "R1";
            workSheet.Cells[46, 2] = "R2";
            workSheet.Cells[47, 2] = "P1";
            workSheet.Cells[48, 2] = "P2";
            workSheet.Cells[49, 2] = "Fscore1";
            workSheet.Cells[50, 2] = "Fscore2";
            workSheet.Cells[51, 2] = "WFScore";

        }
        private const int A2Start = 2;
        private const int A3Start = 14;
        private const int V2Start = 29;
        private const int V3Start = 41;

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

            for (int i = 3; i < pResult.features.Count+ 3; i++)
            {
                workSheet.Cells[counter, i] = pResult.features[i-3].name;
            }

            counter++;
            workSheet.Cells[counter, 3] = pResult.svmParams.C;

            counter++;
            workSheet.Cells[counter, 3] = pResult.svmParams.Gamma;

            counter++;
            workSheet.Cells[counter, 3] = pResult.svmParams.Kernel;
            
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
            counter++;

            workSheet.Cells[counter, 3] = pResult.AverageFScore();
            

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
        }

        #endregion
    }
}

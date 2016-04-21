namespace Classification_App
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.hrBar = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.faceBar = new System.Windows.Forms.ProgressBar();
            this.EEG = new System.Windows.Forms.Label();
            this.eegBar = new System.Windows.Forms.ProgressBar();
            this.GSR = new System.Windows.Forms.Label();
            this.gsrBar = new System.Windows.Forms.ProgressBar();
            this.btn_RunAll = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btn_metaAll = new System.Windows.Forms.Button();
            this.prg_meta_txt = new System.Windows.Forms.Label();
            this.prg_meta = new System.Windows.Forms.ProgressBar();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btn_Anova = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lst_excel_files = new System.Windows.Forms.ListBox();
            this.btn_excel_merge = new System.Windows.Forms.Button();
            this.btn_excel_add = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txt_ExportTo = new System.Windows.Forms.TextBox();
            this.txt_ExportFrom = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.btn_StopSearch = new System.Windows.Forms.Button();
            this.txt_OffsetStep = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txt_OffsetTo = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txt_OffsetFrom = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.btn_SearchOffset = new System.Windows.Forms.Button();
            this.txt_rsquared = new System.Windows.Forms.Label();
            this.txt_slope = new System.Windows.Forms.Label();
            this.txt_intercept = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txt_PlotWindow = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cmb_PlotDataType = new System.Windows.Forms.ComboBox();
            this.scroll_PlotView = new System.Windows.Forms.HScrollBar();
            this.chart_TestData = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.txt_PlotPointSize = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txt_PlotDataOffset = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_RecallDataName = new System.Windows.Forms.Label();
            this.txt_TestDataName = new System.Windows.Forms.Label();
            this.btn_PlotLoadRecall = new System.Windows.Forms.Button();
            this.btn_PlotLoadTest = new System.Windows.Forms.Button();
            this.txt_width = new System.Windows.Forms.TextBox();
            this.txt_height = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_ExportPNG = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.threadBox = new System.Windows.Forms.ComboBox();
            this.Label = new System.Windows.Forms.Label();
            this.chk_useControlValues = new System.Windows.Forms.CheckBox();
            this.btn_CalculateResults = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_TestData)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 75);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(665, 318);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.hrBar);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.faceBar);
            this.tabPage1.Controls.Add(this.EEG);
            this.tabPage1.Controls.Add(this.eegBar);
            this.tabPage1.Controls.Add(this.GSR);
            this.tabPage1.Controls.Add(this.gsrBar);
            this.tabPage1.Controls.Add(this.btn_RunAll);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(657, 292);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Normal";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 247);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "HR";
            // 
            // hrBar
            // 
            this.hrBar.Location = new System.Drawing.Point(6, 263);
            this.hrBar.Name = "hrBar";
            this.hrBar.Size = new System.Drawing.Size(528, 23);
            this.hrBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.hrBar.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 201);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Kinect";
            // 
            // faceBar
            // 
            this.faceBar.Location = new System.Drawing.Point(6, 217);
            this.faceBar.Name = "faceBar";
            this.faceBar.Size = new System.Drawing.Size(528, 23);
            this.faceBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.faceBar.TabIndex = 10;
            // 
            // EEG
            // 
            this.EEG.AutoSize = true;
            this.EEG.Location = new System.Drawing.Point(6, 158);
            this.EEG.Name = "EEG";
            this.EEG.Size = new System.Drawing.Size(29, 13);
            this.EEG.TabIndex = 9;
            this.EEG.Text = "EEG";
            // 
            // eegBar
            // 
            this.eegBar.Location = new System.Drawing.Point(6, 174);
            this.eegBar.Name = "eegBar";
            this.eegBar.Size = new System.Drawing.Size(528, 23);
            this.eegBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.eegBar.TabIndex = 8;
            // 
            // GSR
            // 
            this.GSR.AutoSize = true;
            this.GSR.Location = new System.Drawing.Point(6, 113);
            this.GSR.Name = "GSR";
            this.GSR.Size = new System.Drawing.Size(30, 13);
            this.GSR.TabIndex = 7;
            this.GSR.Text = "GSR";
            // 
            // gsrBar
            // 
            this.gsrBar.Location = new System.Drawing.Point(6, 129);
            this.gsrBar.Name = "gsrBar";
            this.gsrBar.Size = new System.Drawing.Size(528, 23);
            this.gsrBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.gsrBar.TabIndex = 6;
            // 
            // btn_RunAll
            // 
            this.btn_RunAll.Location = new System.Drawing.Point(9, 6);
            this.btn_RunAll.Name = "btn_RunAll";
            this.btn_RunAll.Size = new System.Drawing.Size(88, 24);
            this.btn_RunAll.TabIndex = 2;
            this.btn_RunAll.Text = "Run All";
            this.btn_RunAll.UseVisualStyleBackColor = true;
            this.btn_RunAll.Click += new System.EventHandler(this.btn_RunAll_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btn_metaAll);
            this.tabPage2.Controls.Add(this.prg_meta_txt);
            this.tabPage2.Controls.Add(this.prg_meta);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(657, 292);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Meta";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btn_metaAll
            // 
            this.btn_metaAll.Location = new System.Drawing.Point(6, 6);
            this.btn_metaAll.Name = "btn_metaAll";
            this.btn_metaAll.Size = new System.Drawing.Size(88, 36);
            this.btn_metaAll.TabIndex = 7;
            this.btn_metaAll.Text = "Meta All";
            this.btn_metaAll.UseVisualStyleBackColor = true;
            this.btn_metaAll.Click += new System.EventHandler(this.btn_metaAll_Click);
            // 
            // prg_meta_txt
            // 
            this.prg_meta_txt.Location = new System.Drawing.Point(111, 16);
            this.prg_meta_txt.Name = "prg_meta_txt";
            this.prg_meta_txt.Size = new System.Drawing.Size(410, 14);
            this.prg_meta_txt.TabIndex = 15;
            this.prg_meta_txt.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // prg_meta
            // 
            this.prg_meta.Location = new System.Drawing.Point(100, 6);
            this.prg_meta.Name = "prg_meta";
            this.prg_meta.Size = new System.Drawing.Size(434, 36);
            this.prg_meta.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prg_meta.TabIndex = 14;
            this.prg_meta.Click += new System.EventHandler(this.prg_meta_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btn_Anova);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.lst_excel_files);
            this.tabPage3.Controls.Add(this.btn_excel_merge);
            this.tabPage3.Controls.Add(this.btn_excel_add);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(657, 292);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Excel Merger";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btn_Anova
            // 
            this.btn_Anova.Location = new System.Drawing.Point(165, 3);
            this.btn_Anova.Name = "btn_Anova";
            this.btn_Anova.Size = new System.Drawing.Size(75, 23);
            this.btn_Anova.TabIndex = 4;
            this.btn_Anova.Text = "ANOVA";
            this.btn_Anova.UseVisualStyleBackColor = true;
            this.btn_Anova.Click += new System.EventHandler(this.btn_Anova_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Files to merge";
            // 
            // lst_excel_files
            // 
            this.lst_excel_files.FormattingEnabled = true;
            this.lst_excel_files.Location = new System.Drawing.Point(3, 77);
            this.lst_excel_files.Name = "lst_excel_files";
            this.lst_excel_files.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lst_excel_files.Size = new System.Drawing.Size(520, 212);
            this.lst_excel_files.TabIndex = 2;
            this.lst_excel_files.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_excel_files_KeyDown);
            // 
            // btn_excel_merge
            // 
            this.btn_excel_merge.Location = new System.Drawing.Point(84, 3);
            this.btn_excel_merge.Name = "btn_excel_merge";
            this.btn_excel_merge.Size = new System.Drawing.Size(75, 23);
            this.btn_excel_merge.TabIndex = 1;
            this.btn_excel_merge.Text = "Merge";
            this.btn_excel_merge.UseVisualStyleBackColor = true;
            this.btn_excel_merge.Click += new System.EventHandler(this.btn_excel_merge_Click);
            // 
            // btn_excel_add
            // 
            this.btn_excel_add.Location = new System.Drawing.Point(3, 3);
            this.btn_excel_add.Name = "btn_excel_add";
            this.btn_excel_add.Size = new System.Drawing.Size(75, 23);
            this.btn_excel_add.TabIndex = 0;
            this.btn_excel_add.Text = "Add xlsx";
            this.btn_excel_add.UseVisualStyleBackColor = true;
            this.btn_excel_add.Click += new System.EventHandler(this.btn_excel_add_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.txt_ExportTo);
            this.tabPage4.Controls.Add(this.txt_ExportFrom);
            this.tabPage4.Controls.Add(this.label18);
            this.tabPage4.Controls.Add(this.label19);
            this.tabPage4.Controls.Add(this.btn_StopSearch);
            this.tabPage4.Controls.Add(this.txt_OffsetStep);
            this.tabPage4.Controls.Add(this.label17);
            this.tabPage4.Controls.Add(this.txt_OffsetTo);
            this.tabPage4.Controls.Add(this.label16);
            this.tabPage4.Controls.Add(this.txt_OffsetFrom);
            this.tabPage4.Controls.Add(this.label15);
            this.tabPage4.Controls.Add(this.btn_SearchOffset);
            this.tabPage4.Controls.Add(this.txt_rsquared);
            this.tabPage4.Controls.Add(this.txt_slope);
            this.tabPage4.Controls.Add(this.txt_intercept);
            this.tabPage4.Controls.Add(this.label14);
            this.tabPage4.Controls.Add(this.label13);
            this.tabPage4.Controls.Add(this.label12);
            this.tabPage4.Controls.Add(this.txt_PlotWindow);
            this.tabPage4.Controls.Add(this.label11);
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.cmb_PlotDataType);
            this.tabPage4.Controls.Add(this.scroll_PlotView);
            this.tabPage4.Controls.Add(this.chart_TestData);
            this.tabPage4.Controls.Add(this.txt_PlotPointSize);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.txt_PlotDataOffset);
            this.tabPage4.Controls.Add(this.label7);
            this.tabPage4.Controls.Add(this.txt_RecallDataName);
            this.tabPage4.Controls.Add(this.txt_TestDataName);
            this.tabPage4.Controls.Add(this.btn_PlotLoadRecall);
            this.tabPage4.Controls.Add(this.btn_PlotLoadTest);
            this.tabPage4.Controls.Add(this.txt_width);
            this.tabPage4.Controls.Add(this.txt_height);
            this.tabPage4.Controls.Add(this.label6);
            this.tabPage4.Controls.Add(this.label5);
            this.tabPage4.Controls.Add(this.btn_ExportPNG);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(657, 292);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Plotting";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txt_ExportTo
            // 
            this.txt_ExportTo.Location = new System.Drawing.Point(164, 131);
            this.txt_ExportTo.Name = "txt_ExportTo";
            this.txt_ExportTo.Size = new System.Drawing.Size(46, 20);
            this.txt_ExportTo.TabIndex = 38;
            // 
            // txt_ExportFrom
            // 
            this.txt_ExportFrom.Location = new System.Drawing.Point(164, 110);
            this.txt_ExportFrom.Name = "txt_ExportFrom";
            this.txt_ExportFrom.Size = new System.Drawing.Size(46, 20);
            this.txt_ExportFrom.TabIndex = 37;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(132, 113);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(30, 13);
            this.label18.TabIndex = 36;
            this.label18.Text = "From";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(140, 133);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(20, 13);
            this.label19.TabIndex = 35;
            this.label19.Text = "To";
            // 
            // btn_StopSearch
            // 
            this.btn_StopSearch.Location = new System.Drawing.Point(128, 183);
            this.btn_StopSearch.Name = "btn_StopSearch";
            this.btn_StopSearch.Size = new System.Drawing.Size(42, 23);
            this.btn_StopSearch.TabIndex = 34;
            this.btn_StopSearch.Text = "Stop";
            this.btn_StopSearch.UseVisualStyleBackColor = true;
            this.btn_StopSearch.Visible = false;
            this.btn_StopSearch.Click += new System.EventHandler(this.btn_StopSearch_Click);
            // 
            // txt_OffsetStep
            // 
            this.txt_OffsetStep.Location = new System.Drawing.Point(75, 269);
            this.txt_OffsetStep.Name = "txt_OffsetStep";
            this.txt_OffsetStep.Size = new System.Drawing.Size(46, 20);
            this.txt_OffsetStep.TabIndex = 33;
            this.txt_OffsetStep.Text = "50";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(19, 272);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(50, 13);
            this.label17.TabIndex = 32;
            this.label17.Text = "Stepsize:";
            // 
            // txt_OffsetTo
            // 
            this.txt_OffsetTo.Location = new System.Drawing.Point(75, 248);
            this.txt_OffsetTo.Name = "txt_OffsetTo";
            this.txt_OffsetTo.Size = new System.Drawing.Size(46, 20);
            this.txt_OffsetTo.TabIndex = 31;
            this.txt_OffsetTo.Text = "10000";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(46, 251);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(23, 13);
            this.label16.TabIndex = 30;
            this.label16.Text = "To:";
            // 
            // txt_OffsetFrom
            // 
            this.txt_OffsetFrom.Location = new System.Drawing.Point(75, 227);
            this.txt_OffsetFrom.Name = "txt_OffsetFrom";
            this.txt_OffsetFrom.Size = new System.Drawing.Size(46, 20);
            this.txt_OffsetFrom.TabIndex = 29;
            this.txt_OffsetFrom.Text = "-5000";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(36, 230);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(33, 13);
            this.label15.TabIndex = 28;
            this.label15.Text = "From:";
            // 
            // btn_SearchOffset
            // 
            this.btn_SearchOffset.Location = new System.Drawing.Point(6, 183);
            this.btn_SearchOffset.Name = "btn_SearchOffset";
            this.btn_SearchOffset.Size = new System.Drawing.Size(116, 23);
            this.btn_SearchOffset.TabIndex = 27;
            this.btn_SearchOffset.Text = "Search for offset";
            this.btn_SearchOffset.UseVisualStyleBackColor = true;
            this.btn_SearchOffset.Click += new System.EventHandler(this.btn_SearchOffset_Click);
            // 
            // txt_rsquared
            // 
            this.txt_rsquared.AutoSize = true;
            this.txt_rsquared.Location = new System.Drawing.Point(204, 260);
            this.txt_rsquared.Name = "txt_rsquared";
            this.txt_rsquared.Size = new System.Drawing.Size(13, 13);
            this.txt_rsquared.TabIndex = 26;
            this.txt_rsquared.Text = "0";
            // 
            // txt_slope
            // 
            this.txt_slope.AutoSize = true;
            this.txt_slope.Location = new System.Drawing.Point(204, 244);
            this.txt_slope.Name = "txt_slope";
            this.txt_slope.Size = new System.Drawing.Size(13, 13);
            this.txt_slope.TabIndex = 25;
            this.txt_slope.Text = "0";
            // 
            // txt_intercept
            // 
            this.txt_intercept.AutoSize = true;
            this.txt_intercept.Location = new System.Drawing.Point(204, 227);
            this.txt_intercept.Name = "txt_intercept";
            this.txt_intercept.Size = new System.Drawing.Size(13, 13);
            this.txt_intercept.TabIndex = 24;
            this.txt_intercept.Text = "0";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(145, 258);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(58, 13);
            this.label14.TabIndex = 23;
            this.label14.Text = "RSquared:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(166, 242);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(37, 13);
            this.label13.TabIndex = 22;
            this.label13.Text = "Slope:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(151, 226);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(52, 13);
            this.label12.TabIndex = 21;
            this.label12.Text = "Intercept:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_PlotWindow
            // 
            this.txt_PlotWindow.Location = new System.Drawing.Point(439, 48);
            this.txt_PlotWindow.Name = "txt_PlotWindow";
            this.txt_PlotWindow.Size = new System.Drawing.Size(47, 20);
            this.txt_PlotWindow.TabIndex = 20;
            this.txt_PlotWindow.Text = "10000";
            this.txt_PlotWindow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_PlotWindow_KeyDown);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(361, 51);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 19;
            this.label11.Text = "View window:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(497, 53);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(140, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "Red = test    |    Blue = recall";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(392, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(60, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Data Type:";
            // 
            // cmb_PlotDataType
            // 
            this.cmb_PlotDataType.FormattingEnabled = true;
            this.cmb_PlotDataType.Location = new System.Drawing.Point(458, 11);
            this.cmb_PlotDataType.Name = "cmb_PlotDataType";
            this.cmb_PlotDataType.Size = new System.Drawing.Size(179, 21);
            this.cmb_PlotDataType.TabIndex = 16;
            this.cmb_PlotDataType.SelectedValueChanged += new System.EventHandler(this.cmb_PlotDataType_SelectedValueChanged);
            // 
            // scroll_PlotView
            // 
            this.scroll_PlotView.LargeChange = 1000;
            this.scroll_PlotView.Location = new System.Drawing.Point(275, 273);
            this.scroll_PlotView.Maximum = 10000;
            this.scroll_PlotView.Name = "scroll_PlotView";
            this.scroll_PlotView.Size = new System.Drawing.Size(362, 16);
            this.scroll_PlotView.SmallChange = 100;
            this.scroll_PlotView.TabIndex = 15;
            this.scroll_PlotView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scroll_PlotView_Scroll);
            // 
            // chart_TestData
            // 
            this.chart_TestData.BackColor = System.Drawing.Color.DarkGray;
            chartArea2.Name = "ChartArea1";
            this.chart_TestData.ChartAreas.Add(chartArea2);
            this.chart_TestData.Location = new System.Drawing.Point(275, 69);
            this.chart_TestData.Name = "chart_TestData";
            series2.ChartArea = "ChartArea1";
            series2.Name = "Series1";
            this.chart_TestData.Series.Add(series2);
            this.chart_TestData.Size = new System.Drawing.Size(362, 201);
            this.chart_TestData.TabIndex = 13;
            this.chart_TestData.Text = "chart1";
            // 
            // txt_PlotPointSize
            // 
            this.txt_PlotPointSize.Location = new System.Drawing.Point(71, 89);
            this.txt_PlotPointSize.Name = "txt_PlotPointSize";
            this.txt_PlotPointSize.Size = new System.Drawing.Size(46, 20);
            this.txt_PlotPointSize.TabIndex = 12;
            this.txt_PlotPointSize.Text = "0.4";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 92);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Point size:";
            // 
            // txt_PlotDataOffset
            // 
            this.txt_PlotDataOffset.Location = new System.Drawing.Point(75, 206);
            this.txt_PlotDataOffset.Name = "txt_PlotDataOffset";
            this.txt_PlotDataOffset.Size = new System.Drawing.Size(46, 20);
            this.txt_PlotDataOffset.TabIndex = 10;
            this.txt_PlotDataOffset.Text = "0";
            this.txt_PlotDataOffset.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_PlotDataOffset_KeyDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 209);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Current offset:";
            // 
            // txt_RecallDataName
            // 
            this.txt_RecallDataName.AutoSize = true;
            this.txt_RecallDataName.Location = new System.Drawing.Point(128, 40);
            this.txt_RecallDataName.Name = "txt_RecallDataName";
            this.txt_RecallDataName.Size = new System.Drawing.Size(10, 13);
            this.txt_RecallDataName.TabIndex = 8;
            this.txt_RecallDataName.Text = "-";
            // 
            // txt_TestDataName
            // 
            this.txt_TestDataName.AutoSize = true;
            this.txt_TestDataName.Location = new System.Drawing.Point(128, 11);
            this.txt_TestDataName.Name = "txt_TestDataName";
            this.txt_TestDataName.Size = new System.Drawing.Size(10, 13);
            this.txt_TestDataName.TabIndex = 7;
            this.txt_TestDataName.Text = "-";
            // 
            // btn_PlotLoadRecall
            // 
            this.btn_PlotLoadRecall.Location = new System.Drawing.Point(6, 35);
            this.btn_PlotLoadRecall.Name = "btn_PlotLoadRecall";
            this.btn_PlotLoadRecall.Size = new System.Drawing.Size(116, 23);
            this.btn_PlotLoadRecall.TabIndex = 6;
            this.btn_PlotLoadRecall.Text = "Load Recall Data";
            this.btn_PlotLoadRecall.UseVisualStyleBackColor = true;
            this.btn_PlotLoadRecall.Click += new System.EventHandler(this.btn_PlotLoadRecall_Click);
            // 
            // btn_PlotLoadTest
            // 
            this.btn_PlotLoadTest.Location = new System.Drawing.Point(6, 6);
            this.btn_PlotLoadTest.Name = "btn_PlotLoadTest";
            this.btn_PlotLoadTest.Size = new System.Drawing.Size(116, 23);
            this.btn_PlotLoadTest.TabIndex = 5;
            this.btn_PlotLoadTest.Text = "Load Test Data";
            this.btn_PlotLoadTest.UseVisualStyleBackColor = true;
            this.btn_PlotLoadTest.Click += new System.EventHandler(this.btn_PlotLoadTest_Click);
            // 
            // txt_width
            // 
            this.txt_width.Location = new System.Drawing.Point(71, 131);
            this.txt_width.Name = "txt_width";
            this.txt_width.Size = new System.Drawing.Size(46, 20);
            this.txt_width.TabIndex = 4;
            this.txt_width.Text = "800";
            // 
            // txt_height
            // 
            this.txt_height.Location = new System.Drawing.Point(71, 110);
            this.txt_height.Name = "txt_height";
            this.txt_height.Size = new System.Drawing.Size(46, 20);
            this.txt_height.TabIndex = 3;
            this.txt_height.Text = "800";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 113);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Height:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Width:";
            // 
            // btn_ExportPNG
            // 
            this.btn_ExportPNG.Location = new System.Drawing.Point(6, 64);
            this.btn_ExportPNG.Name = "btn_ExportPNG";
            this.btn_ExportPNG.Size = new System.Drawing.Size(116, 23);
            this.btn_ExportPNG.TabIndex = 0;
            this.btn_ExportPNG.Text = "Export .png";
            this.btn_ExportPNG.UseVisualStyleBackColor = true;
            this.btn_ExportPNG.Click += new System.EventHandler(this.btn_ExportPNG_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 395);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(661, 98);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Status: ";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(52, 9);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(86, 13);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Please load data";
            // 
            // threadBox
            // 
            this.threadBox.FormattingEnabled = true;
            this.threadBox.Location = new System.Drawing.Point(446, 70);
            this.threadBox.Name = "threadBox";
            this.threadBox.Size = new System.Drawing.Size(107, 21);
            this.threadBox.TabIndex = 8;
            this.threadBox.SelectedIndexChanged += new System.EventHandler(this.threadBox_SelectedIndexChanged);
            // 
            // Label
            // 
            this.Label.AutoSize = true;
            this.Label.Location = new System.Drawing.Point(443, 54);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(65, 13);
            this.Label.TabIndex = 9;
            this.Label.Text = "Thread Prio.";
            // 
            // chk_useControlValues
            // 
            this.chk_useControlValues.AutoSize = true;
            this.chk_useControlValues.Location = new System.Drawing.Point(269, 70);
            this.chk_useControlValues.Name = "chk_useControlValues";
            this.chk_useControlValues.Size = new System.Drawing.Size(143, 17);
            this.chk_useControlValues.TabIndex = 16;
            this.chk_useControlValues.Text = "Use IAPS Control Values";
            this.chk_useControlValues.UseVisualStyleBackColor = true;
            // 
            // btn_CalculateResults
            // 
            this.btn_CalculateResults.Location = new System.Drawing.Point(12, 34);
            this.btn_CalculateResults.Name = "btn_CalculateResults";
            this.btn_CalculateResults.Size = new System.Drawing.Size(149, 24);
            this.btn_CalculateResults.TabIndex = 14;
            this.btn_CalculateResults.Text = "Calculate All Results";
            this.btn_CalculateResults.UseVisualStyleBackColor = true;
            this.btn_CalculateResults.Click += new System.EventHandler(this.btn_CalculateResults_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 505);
            this.Controls.Add(this.btn_CalculateResults);
            this.Controls.Add(this.chk_useControlValues);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.threadBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_TestData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btn_RunAll;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar hrBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar faceBar;
        private System.Windows.Forms.Label EEG;
        private System.Windows.Forms.ProgressBar eegBar;
        private System.Windows.Forms.Label GSR;
        private System.Windows.Forms.ProgressBar gsrBar;
        private System.Windows.Forms.Button btn_metaAll;
        private System.Windows.Forms.ComboBox threadBox;
        private System.Windows.Forms.Label Label;
        private System.Windows.Forms.ProgressBar prg_meta;
        private System.Windows.Forms.Label prg_meta_txt;
        private System.Windows.Forms.CheckBox chk_useControlValues;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lst_excel_files;
        private System.Windows.Forms.Button btn_excel_merge;
        private System.Windows.Forms.Button btn_excel_add;
        private System.Windows.Forms.Button btn_Anova;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox txt_width;
        private System.Windows.Forms.TextBox txt_height;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_ExportPNG;
        private System.Windows.Forms.Button btn_PlotLoadTest;
        private System.Windows.Forms.Label txt_TestDataName;
        private System.Windows.Forms.Button btn_PlotLoadRecall;
        private System.Windows.Forms.Label txt_RecallDataName;
        private System.Windows.Forms.TextBox txt_PlotDataOffset;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_PlotPointSize;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.HScrollBar scroll_PlotView;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_TestData;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmb_PlotDataType;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txt_PlotWindow;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label txt_rsquared;
        private System.Windows.Forms.Label txt_slope;
        private System.Windows.Forms.Label txt_intercept;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btn_SearchOffset;
        private System.Windows.Forms.TextBox txt_OffsetStep;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txt_OffsetTo;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txt_OffsetFrom;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btn_StopSearch;
        private System.Windows.Forms.TextBox txt_ExportTo;
        private System.Windows.Forms.TextBox txt_ExportFrom;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button btn_CalculateResults;
    }
}


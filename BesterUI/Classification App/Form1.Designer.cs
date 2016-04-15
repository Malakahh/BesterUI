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
            this.label2 = new System.Windows.Forms.Label();
            this.lst_excel_files = new System.Windows.Forms.ListBox();
            this.btn_excel_merge = new System.Windows.Forms.Button();
            this.btn_excel_add = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.threadBox = new System.Windows.Forms.ComboBox();
            this.Label = new System.Windows.Forms.Label();
            this.chk_useControlValues = new System.Windows.Forms.CheckBox();
            this.btn_Anova = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 75);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(548, 318);
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
            this.tabPage1.Size = new System.Drawing.Size(540, 292);
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
            this.tabPage2.Size = new System.Drawing.Size(540, 292);
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
            this.tabPage3.Size = new System.Drawing.Size(540, 292);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Excel Merger";
            this.tabPage3.UseVisualStyleBackColor = true;
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
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 395);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(548, 98);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 505);
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
    }
}


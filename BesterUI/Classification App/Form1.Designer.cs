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
            this.addMachineBtn = new System.Windows.Forms.Button();
            this.chk_ParameterOptimizationNormal = new System.Windows.Forms.CheckBox();
            this.chk_FeatureOptimizationNormal = new System.Windows.Forms.CheckBox();
            this.btn_RunNormal = new System.Windows.Forms.Button();
            this.chklist_Features = new System.Windows.Forms.CheckedListBox();
            this.chklst_SvmConfigurations = new System.Windows.Forms.CheckedListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.boostingCB = new System.Windows.Forms.CheckBox();
            this.votingCB = new System.Windows.Forms.CheckBox();
            this.chklst_meta = new System.Windows.Forms.CheckedListBox();
            this.stackingCB = new System.Windows.Forms.CheckBox();
            this.metaRunBtn = new System.Windows.Forms.Button();
            this.btn_LoadData = new System.Windows.Forms.Button();
            this.btn_RunAll = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.btn_metaAll = new System.Windows.Forms.Button();
            this.threadBox = new System.Windows.Forms.ComboBox();
            this.Label = new System.Windows.Forms.Label();
            this.prg_meta = new System.Windows.Forms.ProgressBar();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
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
            this.tabPage1.Controls.Add(this.addMachineBtn);
            this.tabPage1.Controls.Add(this.chk_ParameterOptimizationNormal);
            this.tabPage1.Controls.Add(this.chk_FeatureOptimizationNormal);
            this.tabPage1.Controls.Add(this.btn_RunNormal);
            this.tabPage1.Controls.Add(this.chklist_Features);
            this.tabPage1.Controls.Add(this.chklst_SvmConfigurations);
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
            this.hrBar.Size = new System.Drawing.Size(167, 23);
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
            this.faceBar.Size = new System.Drawing.Size(167, 23);
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
            this.eegBar.Size = new System.Drawing.Size(167, 23);
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
            this.gsrBar.Size = new System.Drawing.Size(167, 23);
            this.gsrBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.gsrBar.TabIndex = 6;
            // 
            // addMachineBtn
            // 
            this.addMachineBtn.Location = new System.Drawing.Point(336, 257);
            this.addMachineBtn.Name = "addMachineBtn";
            this.addMachineBtn.Size = new System.Drawing.Size(151, 23);
            this.addMachineBtn.TabIndex = 5;
            this.addMachineBtn.Text = "Add Machine";
            this.addMachineBtn.UseVisualStyleBackColor = true;
            this.addMachineBtn.Click += new System.EventHandler(this.addMachineBtn_Click);
            // 
            // chk_ParameterOptimizationNormal
            // 
            this.chk_ParameterOptimizationNormal.AutoSize = true;
            this.chk_ParameterOptimizationNormal.Location = new System.Drawing.Point(7, 89);
            this.chk_ParameterOptimizationNormal.Name = "chk_ParameterOptimizationNormal";
            this.chk_ParameterOptimizationNormal.Size = new System.Drawing.Size(134, 17);
            this.chk_ParameterOptimizationNormal.TabIndex = 4;
            this.chk_ParameterOptimizationNormal.Text = "Parameter Optimization";
            this.chk_ParameterOptimizationNormal.UseVisualStyleBackColor = true;
            // 
            // chk_FeatureOptimizationNormal
            // 
            this.chk_FeatureOptimizationNormal.AutoSize = true;
            this.chk_FeatureOptimizationNormal.Location = new System.Drawing.Point(7, 66);
            this.chk_FeatureOptimizationNormal.Name = "chk_FeatureOptimizationNormal";
            this.chk_FeatureOptimizationNormal.Size = new System.Drawing.Size(122, 17);
            this.chk_FeatureOptimizationNormal.TabIndex = 3;
            this.chk_FeatureOptimizationNormal.Text = "Feature Optimization";
            this.chk_FeatureOptimizationNormal.UseVisualStyleBackColor = true;
            // 
            // btn_RunNormal
            // 
            this.btn_RunNormal.Location = new System.Drawing.Point(6, 37);
            this.btn_RunNormal.Name = "btn_RunNormal";
            this.btn_RunNormal.Size = new System.Drawing.Size(75, 23);
            this.btn_RunNormal.TabIndex = 2;
            this.btn_RunNormal.Text = "Run";
            this.btn_RunNormal.UseVisualStyleBackColor = true;
            this.btn_RunNormal.Click += new System.EventHandler(this.btn_Run_Click);
            // 
            // chklist_Features
            // 
            this.chklist_Features.FormattingEnabled = true;
            this.chklist_Features.Location = new System.Drawing.Point(336, 6);
            this.chklist_Features.Name = "chklist_Features";
            this.chklist_Features.Size = new System.Drawing.Size(151, 244);
            this.chklist_Features.TabIndex = 1;
            // 
            // chklst_SvmConfigurations
            // 
            this.chklst_SvmConfigurations.FormattingEnabled = true;
            this.chklst_SvmConfigurations.Location = new System.Drawing.Point(179, 6);
            this.chklst_SvmConfigurations.Name = "chklst_SvmConfigurations";
            this.chklst_SvmConfigurations.Size = new System.Drawing.Size(151, 274);
            this.chklst_SvmConfigurations.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.boostingCB);
            this.tabPage2.Controls.Add(this.votingCB);
            this.tabPage2.Controls.Add(this.chklst_meta);
            this.tabPage2.Controls.Add(this.stackingCB);
            this.tabPage2.Controls.Add(this.metaRunBtn);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(540, 292);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Meta";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // boostingCB
            // 
            this.boostingCB.AutoSize = true;
            this.boostingCB.Location = new System.Drawing.Point(27, 141);
            this.boostingCB.Name = "boostingCB";
            this.boostingCB.Size = new System.Drawing.Size(67, 17);
            this.boostingCB.TabIndex = 4;
            this.boostingCB.Text = "Boosting";
            this.boostingCB.UseVisualStyleBackColor = true;
            // 
            // votingCB
            // 
            this.votingCB.AutoSize = true;
            this.votingCB.Location = new System.Drawing.Point(27, 118);
            this.votingCB.Name = "votingCB";
            this.votingCB.Size = new System.Drawing.Size(56, 17);
            this.votingCB.TabIndex = 3;
            this.votingCB.Text = "Voting";
            this.votingCB.UseVisualStyleBackColor = true;
            // 
            // chklst_meta
            // 
            this.chklst_meta.FormattingEnabled = true;
            this.chklst_meta.Location = new System.Drawing.Point(151, 8);
            this.chklst_meta.Name = "chklst_meta";
            this.chklst_meta.Size = new System.Drawing.Size(183, 274);
            this.chklst_meta.TabIndex = 2;
            // 
            // stackingCB
            // 
            this.stackingCB.AutoSize = true;
            this.stackingCB.Location = new System.Drawing.Point(27, 95);
            this.stackingCB.Name = "stackingCB";
            this.stackingCB.Size = new System.Drawing.Size(68, 17);
            this.stackingCB.TabIndex = 1;
            this.stackingCB.Text = "Stacking";
            this.stackingCB.UseVisualStyleBackColor = true;
            // 
            // metaRunBtn
            // 
            this.metaRunBtn.Location = new System.Drawing.Point(27, 66);
            this.metaRunBtn.Name = "metaRunBtn";
            this.metaRunBtn.Size = new System.Drawing.Size(75, 23);
            this.metaRunBtn.TabIndex = 0;
            this.metaRunBtn.Text = "Run";
            this.metaRunBtn.UseVisualStyleBackColor = true;
            this.metaRunBtn.Click += new System.EventHandler(this.metaRunBtn_Click);
            // 
            // btn_LoadData
            // 
            this.btn_LoadData.Location = new System.Drawing.Point(12, 12);
            this.btn_LoadData.Name = "btn_LoadData";
            this.btn_LoadData.Size = new System.Drawing.Size(70, 24);
            this.btn_LoadData.TabIndex = 1;
            this.btn_LoadData.Text = "Load Data";
            this.btn_LoadData.UseVisualStyleBackColor = true;
            this.btn_LoadData.Click += new System.EventHandler(this.btn_LoadData_Click);
            // 
            // btn_RunAll
            // 
            this.btn_RunAll.Location = new System.Drawing.Point(104, 12);
            this.btn_RunAll.Name = "btn_RunAll";
            this.btn_RunAll.Size = new System.Drawing.Size(88, 24);
            this.btn_RunAll.TabIndex = 2;
            this.btn_RunAll.Text = "Run All";
            this.btn_RunAll.UseVisualStyleBackColor = true;
            this.btn_RunAll.Click += new System.EventHandler(this.btn_RunAll_Click);
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
            this.label1.Location = new System.Drawing.Point(13, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Status: ";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(62, 49);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(86, 13);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Please load data";
            // 
            // btn_metaAll
            // 
            this.btn_metaAll.Location = new System.Drawing.Point(198, 12);
            this.btn_metaAll.Name = "btn_metaAll";
            this.btn_metaAll.Size = new System.Drawing.Size(88, 24);
            this.btn_metaAll.TabIndex = 7;
            this.btn_metaAll.Text = "Meta All";
            this.btn_metaAll.UseVisualStyleBackColor = true;
            this.btn_metaAll.Click += new System.EventHandler(this.btn_metaAll_Click);
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
            // prg_meta
            // 
            this.prg_meta.Location = new System.Drawing.Point(292, 12);
            this.prg_meta.Name = "prg_meta";
            this.prg_meta.Size = new System.Drawing.Size(261, 23);
            this.prg_meta.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prg_meta.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 505);
            this.Controls.Add(this.prg_meta);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.threadBox);
            this.Controls.Add(this.btn_metaAll);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn_RunAll);
            this.Controls.Add(this.btn_LoadData);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btn_RunNormal;
        private System.Windows.Forms.CheckedListBox chklist_Features;
        private System.Windows.Forms.CheckedListBox chklst_SvmConfigurations;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox votingCB;
        private System.Windows.Forms.CheckedListBox chklst_meta;
        private System.Windows.Forms.CheckBox stackingCB;
        private System.Windows.Forms.Button metaRunBtn;
        private System.Windows.Forms.Button btn_LoadData;
        private System.Windows.Forms.CheckBox chk_ParameterOptimizationNormal;
        private System.Windows.Forms.CheckBox chk_FeatureOptimizationNormal;
        private System.Windows.Forms.Button btn_RunAll;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.CheckBox boostingCB;
        private System.Windows.Forms.Button addMachineBtn;
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
    }
}


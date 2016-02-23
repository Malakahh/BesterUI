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
            this.chk_ParameterOptimizationNormal = new System.Windows.Forms.CheckBox();
            this.chk_FeatureOptimizationNormal = new System.Windows.Forms.CheckBox();
            this.btn_RunNormal = new System.Windows.Forms.Button();
            this.chklist_Features = new System.Windows.Forms.CheckedListBox();
            this.chklst_SvmConfigurations = new System.Windows.Forms.CheckedListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.chklst_meta = new System.Windows.Forms.CheckedListBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btn_LoadData = new System.Windows.Forms.Button();
            this.btn_RunAll = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Enabled = false;
            this.tabControl1.Location = new System.Drawing.Point(12, 75);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(548, 318);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
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
            this.chklist_Features.Size = new System.Drawing.Size(151, 274);
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
            this.tabPage2.Controls.Add(this.checkBox2);
            this.tabPage2.Controls.Add(this.chklst_meta);
            this.tabPage2.Controls.Add(this.checkBox1);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(540, 292);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Meta";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(12, 145);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(80, 17);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Text = "checkBox2";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // chklst_meta
            // 
            this.chklst_meta.FormattingEnabled = true;
            this.chklst_meta.Location = new System.Drawing.Point(151, 8);
            this.chklst_meta.Name = "chklst_meta";
            this.chklst_meta.Size = new System.Drawing.Size(183, 304);
            this.chklst_meta.TabIndex = 2;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(27, 95);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(17, 51);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 0;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 505);
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
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckedListBox chklst_meta;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btn_LoadData;
        private System.Windows.Forms.CheckBox chk_ParameterOptimizationNormal;
        private System.Windows.Forms.CheckBox chk_FeatureOptimizationNormal;
        private System.Windows.Forms.Button btn_RunAll;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label statusLabel;
    }
}


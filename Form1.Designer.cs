
namespace MyPLAOptimization
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
            this.rtbOutput = new System.Windows.Forms.RichTextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblOperatorCnt = new System.Windows.Forms.Label();
            this.lblInterfaceCnt = new System.Windows.Forms.Label();
            this.lblComponentCnt = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbInputFileAddress = new System.Windows.Forms.TextBox();
            this.btnRunAlgorithm = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nudMaximumEvaluation = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbExportFileAddress = new System.Windows.Forms.TextBox();
            this.btnExportOutput = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumEvaluation)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbOutput
            // 
            this.rtbOutput.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.rtbOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.rtbOutput.Location = new System.Drawing.Point(6, 19);
            this.rtbOutput.Name = "rtbOutput";
            this.rtbOutput.Size = new System.Drawing.Size(272, 120);
            this.rtbOutput.TabIndex = 0;
            this.rtbOutput.Text = "";
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(6, 20);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFile.TabIndex = 2;
            this.btnSelectFile.Text = "Select FIle";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblOperatorCnt);
            this.groupBox1.Controls.Add(this.lblInterfaceCnt);
            this.groupBox1.Controls.Add(this.lblComponentCnt);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbInputFileAddress);
            this.groupBox1.Controls.Add(this.btnSelectFile);
            this.groupBox1.Location = new System.Drawing.Point(15, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 168);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input";
            // 
            // lblOperatorCnt
            // 
            this.lblOperatorCnt.AutoSize = true;
            this.lblOperatorCnt.Location = new System.Drawing.Point(118, 138);
            this.lblOperatorCnt.Name = "lblOperatorCnt";
            this.lblOperatorCnt.Size = new System.Drawing.Size(22, 13);
            this.lblOperatorCnt.TabIndex = 4;
            this.lblOperatorCnt.Text = "-----";
            // 
            // lblInterfaceCnt
            // 
            this.lblInterfaceCnt.AutoSize = true;
            this.lblInterfaceCnt.Location = new System.Drawing.Point(118, 107);
            this.lblInterfaceCnt.Name = "lblInterfaceCnt";
            this.lblInterfaceCnt.Size = new System.Drawing.Size(22, 13);
            this.lblInterfaceCnt.TabIndex = 4;
            this.lblInterfaceCnt.Text = "-----";
            // 
            // lblComponentCnt
            // 
            this.lblComponentCnt.AutoSize = true;
            this.lblComponentCnt.Location = new System.Drawing.Point(118, 77);
            this.lblComponentCnt.Name = "lblComponentCnt";
            this.lblComponentCnt.Size = new System.Drawing.Size(22, 13);
            this.lblComponentCnt.TabIndex = 4;
            this.lblComponentCnt.Text = "-----";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Operator Count:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Interface Count:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Component Count:";
            // 
            // tbInputFileAddress
            // 
            this.tbInputFileAddress.Enabled = false;
            this.tbInputFileAddress.Location = new System.Drawing.Point(6, 46);
            this.tbInputFileAddress.Name = "tbInputFileAddress";
            this.tbInputFileAddress.Size = new System.Drawing.Size(269, 20);
            this.tbInputFileAddress.TabIndex = 3;
            // 
            // btnRunAlgorithm
            // 
            this.btnRunAlgorithm.BackColor = System.Drawing.Color.Salmon;
            this.btnRunAlgorithm.Enabled = false;
            this.btnRunAlgorithm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.btnRunAlgorithm.Location = new System.Drawing.Point(6, 114);
            this.btnRunAlgorithm.Name = "btnRunAlgorithm";
            this.btnRunAlgorithm.Size = new System.Drawing.Size(148, 48);
            this.btnRunAlgorithm.TabIndex = 2;
            this.btnRunAlgorithm.Text = "Run";
            this.btnRunAlgorithm.UseVisualStyleBackColor = false;
            this.btnRunAlgorithm.Click += new System.EventHandler(this.btnRunAlgorithm_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nudMaximumEvaluation);
            this.groupBox2.Controls.Add(this.btnRunAlgorithm);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(306, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(160, 168);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Algorithm Setting";
            // 
            // nudMaximumEvaluation
            // 
            this.nudMaximumEvaluation.Enabled = false;
            this.nudMaximumEvaluation.Location = new System.Drawing.Point(6, 65);
            this.nudMaximumEvaluation.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudMaximumEvaluation.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMaximumEvaluation.Name = "nudMaximumEvaluation";
            this.nudMaximumEvaluation.Size = new System.Drawing.Size(148, 20);
            this.nudMaximumEvaluation.TabIndex = 3;
            this.nudMaximumEvaluation.Value = new decimal(new int[] {
            55000,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 43);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Maximum Evaluation";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rtbOutput);
            this.groupBox3.Controls.Add(this.tbExportFileAddress);
            this.groupBox3.Controls.Add(this.btnExportOutput);
            this.groupBox3.Location = new System.Drawing.Point(472, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(284, 168);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Output";
            // 
            // tbExportFileAddress
            // 
            this.tbExportFileAddress.Enabled = false;
            this.tbExportFileAddress.Location = new System.Drawing.Point(6, 142);
            this.tbExportFileAddress.Name = "tbExportFileAddress";
            this.tbExportFileAddress.Size = new System.Drawing.Size(200, 20);
            this.tbExportFileAddress.TabIndex = 3;
            // 
            // btnExportOutput
            // 
            this.btnExportOutput.Enabled = false;
            this.btnExportOutput.Location = new System.Drawing.Point(212, 142);
            this.btnExportOutput.Name = "btnExportOutput";
            this.btnExportOutput.Size = new System.Drawing.Size(66, 23);
            this.btnExportOutput.TabIndex = 2;
            this.btnExportOutput.Text = "Export";
            this.btnExportOutput.UseVisualStyleBackColor = true;
            this.btnExportOutput.Click += new System.EventHandler(this.btnExportOutput_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 190);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Software Product Line Architecture Optimization";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximumEvaluation)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbOutput;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbInputFileAddress;
        private System.Windows.Forms.Label lblOperatorCnt;
        private System.Windows.Forms.Label lblInterfaceCnt;
        private System.Windows.Forms.Label lblComponentCnt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnRunAlgorithm;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown nudMaximumEvaluation;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox tbExportFileAddress;
        private System.Windows.Forms.Button btnExportOutput;
    }
}


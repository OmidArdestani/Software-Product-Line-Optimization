namespace MyPLAOptimization
{
    partial class Form2
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnAutomation = new System.Windows.Forms.Button();
            this.lblbPath = new System.Windows.Forms.Label();
            this.PLAName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Configurability = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Reusability = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EstimatedTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InterfaceCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ComponentCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PLAName,
            this.FM,
            this.CM,
            this.Configurability,
            this.Reusability,
            this.EstimatedTime,
            this.InterfaceCount,
            this.ComponentCount});
            this.dataGridView1.Location = new System.Drawing.Point(0, 34);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(862, 512);
            this.dataGridView1.TabIndex = 0;
            // 
            // btnAutomation
            // 
            this.btnAutomation.Location = new System.Drawing.Point(12, 5);
            this.btnAutomation.Name = "btnAutomation";
            this.btnAutomation.Size = new System.Drawing.Size(99, 23);
            this.btnAutomation.TabIndex = 1;
            this.btnAutomation.Text = "Run Automation";
            this.btnAutomation.UseVisualStyleBackColor = true;
            this.btnAutomation.Click += new System.EventHandler(this.BtnAutomation_Click);
            // 
            // lblbPath
            // 
            this.lblbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblbPath.Location = new System.Drawing.Point(117, 10);
            this.lblbPath.Name = "lblbPath";
            this.lblbPath.Size = new System.Drawing.Size(733, 21);
            this.lblbPath.TabIndex = 2;
            this.lblbPath.Text = "label1";
            // 
            // PLAName
            // 
            this.PLAName.HeaderText = "PLA Name";
            this.PLAName.Name = "PLAName";
            // 
            // FM
            // 
            this.FM.HeaderText = "FM";
            this.FM.Name = "FM";
            // 
            // CM
            // 
            this.CM.HeaderText = "CM";
            this.CM.Name = "CM";
            // 
            // Configurability
            // 
            this.Configurability.HeaderText = "Configurability";
            this.Configurability.Name = "Configurability";
            // 
            // Reusability
            // 
            this.Reusability.HeaderText = "Reusability";
            this.Reusability.Name = "Reusability";
            // 
            // EstimatedTime
            // 
            this.EstimatedTime.HeaderText = "EstimatedTime";
            this.EstimatedTime.Name = "EstimatedTime";
            // 
            // InterfaceCount
            // 
            this.InterfaceCount.HeaderText = "Interface Count";
            this.InterfaceCount.Name = "InterfaceCount";
            // 
            // ComponentCount
            // 
            this.ComponentCount.HeaderText = "Component Count";
            this.ComponentCount.Name = "ComponentCount";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 546);
            this.Controls.Add(this.lblbPath);
            this.Controls.Add(this.btnAutomation);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form2";
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnAutomation;
        private System.Windows.Forms.Label lblbPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn PLAName;
        private System.Windows.Forms.DataGridViewTextBoxColumn FM;
        private System.Windows.Forms.DataGridViewTextBoxColumn CM;
        private System.Windows.Forms.DataGridViewTextBoxColumn Configurability;
        private System.Windows.Forms.DataGridViewTextBoxColumn Reusability;
        private System.Windows.Forms.DataGridViewTextBoxColumn EstimatedTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn InterfaceCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn ComponentCount;
    }
}
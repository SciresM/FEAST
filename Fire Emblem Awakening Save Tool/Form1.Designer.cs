namespace Fire_Emblem_Awakening_Save_Tool
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
            this.B_Save_DEC = new System.Windows.Forms.Button();
            this.B_Save_CMP = new System.Windows.Forms.Button();
            this.B_Open_DEC = new System.Windows.Forms.Button();
            this.TB_Path = new System.Windows.Forms.TextBox();
            this.B_Open_CMP = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // B_Save_DEC
            // 
            this.B_Save_DEC.Enabled = false;
            this.B_Save_DEC.Location = new System.Drawing.Point(12, 125);
            this.B_Save_DEC.Name = "B_Save_DEC";
            this.B_Save_DEC.Size = new System.Drawing.Size(445, 23);
            this.B_Save_DEC.TabIndex = 27;
            this.B_Save_DEC.Text = "Save Decompressed Folder";
            this.B_Save_DEC.UseVisualStyleBackColor = true;
            this.B_Save_DEC.Click += new System.EventHandler(this.B_Save_DEC_Click);
            // 
            // B_Save_CMP
            // 
            this.B_Save_CMP.Enabled = false;
            this.B_Save_CMP.Location = new System.Drawing.Point(12, 96);
            this.B_Save_CMP.Name = "B_Save_CMP";
            this.B_Save_CMP.Size = new System.Drawing.Size(445, 23);
            this.B_Save_CMP.TabIndex = 26;
            this.B_Save_CMP.Text = "Save Compressed Folder";
            this.B_Save_CMP.UseVisualStyleBackColor = true;
            this.B_Save_CMP.Click += new System.EventHandler(this.B_Save_CMP_Click);
            // 
            // B_Open_DEC
            // 
            this.B_Open_DEC.Location = new System.Drawing.Point(12, 67);
            this.B_Open_DEC.Name = "B_Open_DEC";
            this.B_Open_DEC.Size = new System.Drawing.Size(445, 23);
            this.B_Open_DEC.TabIndex = 25;
            this.B_Open_DEC.Text = "Open Decompressed Folder";
            this.B_Open_DEC.UseVisualStyleBackColor = true;
            this.B_Open_DEC.Click += new System.EventHandler(this.B_Open_DEC_Click);
            // 
            // TB_Path
            // 
            this.TB_Path.Location = new System.Drawing.Point(12, 12);
            this.TB_Path.Name = "TB_Path";
            this.TB_Path.ReadOnly = true;
            this.TB_Path.Size = new System.Drawing.Size(445, 20);
            this.TB_Path.TabIndex = 24;
            // 
            // B_Open_CMP
            // 
            this.B_Open_CMP.Location = new System.Drawing.Point(12, 38);
            this.B_Open_CMP.Name = "B_Open_CMP";
            this.B_Open_CMP.Size = new System.Drawing.Size(445, 23);
            this.B_Open_CMP.TabIndex = 23;
            this.B_Open_CMP.Text = "Open Compressed Folder";
            this.B_Open_CMP.UseVisualStyleBackColor = true;
            this.B_Open_CMP.Click += new System.EventHandler(this.B_Open_CMP_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 161);
            this.Controls.Add(this.B_Save_DEC);
            this.Controls.Add(this.B_Save_CMP);
            this.Controls.Add(this.B_Open_DEC);
            this.Controls.Add(this.TB_Path);
            this.Controls.Add(this.B_Open_CMP);
            this.MaximumSize = new System.Drawing.Size(485, 200);
            this.MinimumSize = new System.Drawing.Size(485, 200);
            this.Name = "Form1";
            this.Text = "Fire Emblem Awakening Save Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button B_Save_DEC;
        private System.Windows.Forms.Button B_Save_CMP;
        private System.Windows.Forms.Button B_Open_DEC;
        private System.Windows.Forms.TextBox TB_Path;
        private System.Windows.Forms.Button B_Open_CMP;
    }
}


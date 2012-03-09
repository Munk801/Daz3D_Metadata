namespace MetaData_Verifier
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.errorReport = new System.Windows.Forms.TextBox();
            this.DazImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.DazImage)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Cambria", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.label1.Location = new System.Drawing.Point(57, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(258, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Drag Metadata Files Here";
            // 
            // errorReport
            // 
            this.errorReport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.errorReport.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorReport.Location = new System.Drawing.Point(0, 153);
            this.errorReport.Multiline = true;
            this.errorReport.Name = "errorReport";
            this.errorReport.ReadOnly = true;
            this.errorReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.errorReport.Size = new System.Drawing.Size(384, 209);
            this.errorReport.TabIndex = 1;
            this.errorReport.TextChanged += new System.EventHandler(this.errorReport_TextChanged);
            // 
            // DazImage
            // 
            this.DazImage.BackgroundImage = global::MetaData_Verifier.Properties.Resources.daz_3d_logo_large;
            this.DazImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.DazImage.InitialImage = ((System.Drawing.Image)(resources.GetObject("DazImage.InitialImage")));
            this.DazImage.Location = new System.Drawing.Point(21, 34);
            this.DazImage.Name = "DazImage";
            this.DazImage.Size = new System.Drawing.Size(342, 92);
            this.DazImage.TabIndex = 2;
            this.DazImage.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 362);
            this.Controls.Add(this.DazImage);
            this.Controls.Add(this.errorReport);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 400);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "Form1";
            this.Text = "Metadata Verifier";
            ((System.ComponentModel.ISupportInitialize)(this.DazImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox errorReport;
        private System.Windows.Forms.PictureBox DazImage;

    }
}
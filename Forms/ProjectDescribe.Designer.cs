namespace DevTracker
{
    partial class ProjectDescribe
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtProjectID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtKeywords = new System.Windows.Forms.TextBox();
            this.btnSaveProjectDescription = new System.Windows.Forms.Button();
            this.btnCancelProjectDescription = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtProjectDescription = new System.Windows.Forms.TextBox();
            this.txtAppName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project ID:";
            // 
            // txtProjectID
            // 
            this.txtProjectID.Location = new System.Drawing.Point(123, 10);
            this.txtProjectID.Name = "txtProjectID";
            this.txtProjectID.Size = new System.Drawing.Size(201, 26);
            this.txtProjectID.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 142);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Key Words:";
            // 
            // txtKeywords
            // 
            this.txtKeywords.Location = new System.Drawing.Point(123, 138);
            this.txtKeywords.Multiline = true;
            this.txtKeywords.Name = "txtKeywords";
            this.txtKeywords.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtKeywords.Size = new System.Drawing.Size(346, 95);
            this.txtKeywords.TabIndex = 3;
            // 
            // btnSaveProjectDescription
            // 
            this.btnSaveProjectDescription.Location = new System.Drawing.Point(391, 250);
            this.btnSaveProjectDescription.Name = "btnSaveProjectDescription";
            this.btnSaveProjectDescription.Size = new System.Drawing.Size(75, 30);
            this.btnSaveProjectDescription.TabIndex = 4;
            this.btnSaveProjectDescription.Text = "&Save";
            this.btnSaveProjectDescription.UseVisualStyleBackColor = true;
            this.btnSaveProjectDescription.Click += new System.EventHandler(this.btnSaveProjectDescription_Click);
            // 
            // btnCancelProjectDescription
            // 
            this.btnCancelProjectDescription.Location = new System.Drawing.Point(291, 250);
            this.btnCancelProjectDescription.Name = "btnCancelProjectDescription";
            this.btnCancelProjectDescription.Size = new System.Drawing.Size(75, 30);
            this.btnCancelProjectDescription.TabIndex = 5;
            this.btnCancelProjectDescription.Text = "&Cancel";
            this.btnCancelProjectDescription.UseVisualStyleBackColor = true;
            this.btnCancelProjectDescription.Click += new System.EventHandler(this.btnCancelProjectDescription_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Description:";
            // 
            // txtProjectDescription
            // 
            this.txtProjectDescription.Location = new System.Drawing.Point(123, 57);
            this.txtProjectDescription.Name = "txtProjectDescription";
            this.txtProjectDescription.Size = new System.Drawing.Size(345, 26);
            this.txtProjectDescription.TabIndex = 1;
            // 
            // txtAppName
            // 
            this.txtAppName.Location = new System.Drawing.Point(122, 97);
            this.txtAppName.Name = "txtAppName";
            this.txtAppName.Size = new System.Drawing.Size(345, 26);
            this.txtAppName.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "App Name:";
            // 
            // ProjectDescribe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 291);
            this.Controls.Add(this.txtAppName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtProjectDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancelProjectDescription);
            this.Controls.Add(this.btnSaveProjectDescription);
            this.Controls.Add(this.txtKeywords);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtProjectID);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProjectDescribe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Project Description";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProjectID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtKeywords;
        private System.Windows.Forms.Button btnSaveProjectDescription;
        private System.Windows.Forms.Button btnCancelProjectDescription;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtProjectDescription;
        private System.Windows.Forms.TextBox txtAppName;
        private System.Windows.Forms.Label label5;
    }
}
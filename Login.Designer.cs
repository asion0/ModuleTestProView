namespace ModuleTestProView
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.label2 = new System.Windows.Forms.Label();
            this.backupDrvCmb = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.workNo = new System.Windows.Forms.TextBox();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.debugMode = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.refreshBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "備份磁碟機";
            // 
            // backupDrvCmb
            // 
            this.backupDrvCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.backupDrvCmb.FormattingEnabled = true;
            this.backupDrvCmb.Location = new System.Drawing.Point(97, 100);
            this.backupDrvCmb.Name = "backupDrvCmb";
            this.backupDrvCmb.Size = new System.Drawing.Size(377, 20);
            this.backupDrvCmb.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "工單號碼";
            // 
            // workNo
            // 
            this.workNo.Location = new System.Drawing.Point(97, 140);
            this.workNo.Name = "workNo";
            this.workNo.Size = new System.Drawing.Size(377, 22);
            this.workNo.TabIndex = 3;
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(382, 293);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 45);
            this.ok.TabIndex = 5;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(464, 293);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 45);
            this.cancel.TabIndex = 6;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // debugMode
            // 
            this.debugMode.Location = new System.Drawing.Point(17, 315);
            this.debugMode.Name = "debugMode";
            this.debugMode.Size = new System.Drawing.Size(75, 23);
            this.debugMode.TabIndex = 7;
            this.debugMode.Text = "Debug Mode";
            this.debugMode.UseVisualStyleBackColor = true;
            this.debugMode.Visible = false;
            // 
            // testBtn
            // 
            this.testBtn.Location = new System.Drawing.Point(17, 270);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(75, 23);
            this.testBtn.TabIndex = 8;
            this.testBtn.Text = "test";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Visible = false;
            this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // refreshBtn
            // 
            this.refreshBtn.Location = new System.Drawing.Point(480, 98);
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.Size = new System.Drawing.Size(61, 23);
            this.refreshBtn.TabIndex = 9;
            this.refreshBtn.Text = "重整";
            this.refreshBtn.UseVisualStyleBackColor = true;
            this.refreshBtn.Click += new System.EventHandler(this.refreshBtn_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 350);
            this.Controls.Add(this.refreshBtn);
            this.Controls.Add(this.testBtn);
            this.Controls.Add(this.debugMode);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.workNo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.backupDrvCmb);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Login_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox backupDrvCmb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox workNo;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button debugMode;
        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.Button refreshBtn;
    }
}
namespace ModuleTestProView
{
    partial class ModuleTestForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleTestForm));
            this.testBtn = new System.Windows.Forms.Button();
            this.testCounter = new System.Windows.Forms.Label();
            this.moduleName = new System.Windows.Forms.Label();
            this.workFormNo = new System.Windows.Forms.Label();
            this.firstWork = new System.Windows.Forms.Label();
            this.nsPortCmb = new System.Windows.Forms.ComboBox();
            this.nsPortLbl = new System.Windows.Forms.Label();
            this.blePortCmb = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.consoleLsb = new System.Windows.Forms.ListBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.backupLbl = new System.Windows.Forms.Label();
            this.productionPanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.logoPanel = new System.Windows.Forms.Panel();
            this.logoLbl = new System.Windows.Forms.Label();
            this.productionPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.logoPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // testBtn
            // 
            this.testBtn.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testBtn.Location = new System.Drawing.Point(9, 47);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(518, 72);
            this.testBtn.TabIndex = 2;
            this.testBtn.Text = "Test";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.startTesting_Click);
            // 
            // testCounter
            // 
            this.testCounter.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testCounter.Location = new System.Drawing.Point(243, 42);
            this.testCounter.Name = "testCounter";
            this.testCounter.Size = new System.Drawing.Size(123, 42);
            this.testCounter.TabIndex = 5;
            this.testCounter.Text = "000000";
            this.testCounter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // moduleName
            // 
            this.moduleName.Font = new System.Drawing.Font("Verdana", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moduleName.Location = new System.Drawing.Point(2, 3);
            this.moduleName.Name = "moduleName";
            this.moduleName.Size = new System.Drawing.Size(533, 43);
            this.moduleName.TabIndex = 7;
            this.moduleName.Text = "The device is not ready";
            this.moduleName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // workFormNo
            // 
            this.workFormNo.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.workFormNo.Location = new System.Drawing.Point(2, 42);
            this.workFormNo.Name = "workFormNo";
            this.workFormNo.Size = new System.Drawing.Size(251, 42);
            this.workFormNo.TabIndex = 7;
            this.workFormNo.Text = "A888-99988008888";
            this.workFormNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // firstWork
            // 
            this.firstWork.Font = new System.Drawing.Font("Verdana", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.firstWork.ForeColor = System.Drawing.Color.Blue;
            this.firstWork.Location = new System.Drawing.Point(3, 135);
            this.firstWork.Name = "firstWork";
            this.firstWork.Size = new System.Drawing.Size(167, 45);
            this.firstWork.TabIndex = 9;
            this.firstWork.Text = "PASS";
            // 
            // nsPortCmb
            // 
            this.nsPortCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nsPortCmb.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nsPortCmb.FormattingEnabled = true;
            this.nsPortCmb.Location = new System.Drawing.Point(228, 55);
            this.nsPortCmb.Name = "nsPortCmb";
            this.nsPortCmb.Size = new System.Drawing.Size(138, 31);
            this.nsPortCmb.Sorted = true;
            this.nsPortCmb.TabIndex = 1;
            this.nsPortCmb.SelectedIndexChanged += new System.EventHandler(this.comSel_SelectedIndexChanged);
            // 
            // nsPortLbl
            // 
            this.nsPortLbl.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nsPortLbl.Location = new System.Drawing.Point(0, 58);
            this.nsPortLbl.Name = "nsPortLbl";
            this.nsPortLbl.Size = new System.Drawing.Size(222, 23);
            this.nsPortLbl.TabIndex = 2;
            this.nsPortLbl.Text = "Fixture COM :";
            this.nsPortLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // blePortCmb
            // 
            this.blePortCmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.blePortCmb.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.blePortCmb.FormattingEnabled = true;
            this.blePortCmb.Location = new System.Drawing.Point(228, 13);
            this.blePortCmb.Name = "blePortCmb";
            this.blePortCmb.Size = new System.Drawing.Size(138, 31);
            this.blePortCmb.Sorted = true;
            this.blePortCmb.TabIndex = 1;
            this.blePortCmb.SelectedIndexChanged += new System.EventHandler(this.blePortCmb_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(222, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "BLE Board COM :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // consoleLsb
            // 
            this.consoleLsb.FormattingEnabled = true;
            this.consoleLsb.ItemHeight = 12;
            this.consoleLsb.Location = new System.Drawing.Point(14, 431);
            this.consoleLsb.Name = "consoleLsb";
            this.consoleLsb.Size = new System.Drawing.Size(533, 208);
            this.consoleLsb.TabIndex = 11;
            // 
            // connectBtn
            // 
            this.connectBtn.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectBtn.Location = new System.Drawing.Point(388, 13);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(128, 72);
            this.connectBtn.TabIndex = 12;
            this.connectBtn.Text = "Disconnect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // backupLbl
            // 
            this.backupLbl.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backupLbl.Location = new System.Drawing.Point(391, 42);
            this.backupLbl.Name = "backupLbl";
            this.backupLbl.Size = new System.Drawing.Size(105, 42);
            this.backupLbl.TabIndex = 7;
            this.backupLbl.Text = "M:\\";
            this.backupLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // productionPanel
            // 
            this.productionPanel.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.productionPanel.Controls.Add(this.label3);
            this.productionPanel.Controls.Add(this.testCounter);
            this.productionPanel.Controls.Add(this.label4);
            this.productionPanel.Controls.Add(this.label2);
            this.productionPanel.Controls.Add(this.workFormNo);
            this.productionPanel.Controls.Add(this.backupLbl);
            this.productionPanel.Location = new System.Drawing.Point(12, 13);
            this.productionPanel.Name = "productionPanel";
            this.productionPanel.Size = new System.Drawing.Size(538, 86);
            this.productionPanel.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(3, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 24);
            this.label2.TabIndex = 8;
            this.label2.Text = "工單號碼";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(392, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 24);
            this.label3.TabIndex = 8;
            this.label3.Text = "備份磁碟機";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel2.Controls.Add(this.nsPortLbl);
            this.panel2.Controls.Add(this.nsPortCmb);
            this.panel2.Controls.Add(this.connectBtn);
            this.panel2.Controls.Add(this.blePortCmb);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(12, 112);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(538, 100);
            this.panel2.TabIndex = 14;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel3.Controls.Add(this.testBtn);
            this.panel3.Controls.Add(this.moduleName);
            this.panel3.Controls.Add(this.firstWork);
            this.panel3.Location = new System.Drawing.Point(12, 224);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(538, 192);
            this.panel3.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(260, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 24);
            this.label4.TabIndex = 8;
            this.label4.Text = "生產序號";
            // 
            // logoPanel
            // 
            this.logoPanel.Controls.Add(this.logoLbl);
            this.logoPanel.Location = new System.Drawing.Point(12, 13);
            this.logoPanel.Name = "logoPanel";
            this.logoPanel.Size = new System.Drawing.Size(538, 86);
            this.logoPanel.TabIndex = 16;
            this.logoPanel.Visible = false;
            // 
            // logoLbl
            // 
            this.logoLbl.Font = new System.Drawing.Font("Verdana", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logoLbl.Location = new System.Drawing.Point(0, 16);
            this.logoLbl.Name = "logoLbl";
            this.logoLbl.Size = new System.Drawing.Size(538, 54);
            this.logoLbl.TabIndex = 0;
            this.logoLbl.Text = "ProView S3 Test";
            this.logoLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ModuleTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 657);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.productionPanel);
            this.Controls.Add(this.consoleLsb);
            this.Controls.Add(this.logoPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ModuleTestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ProView S3 Module Test";
            this.Load += new System.EventHandler(this.ModuleTestForm_Load);
            this.productionPanel.ResumeLayout(false);
            this.productionPanel.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.logoPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.Label testCounter;
        private System.Windows.Forms.Label moduleName;
        private System.Windows.Forms.Label workFormNo;
        private System.Windows.Forms.Label firstWork;
        private System.Windows.Forms.ComboBox nsPortCmb;
        private System.Windows.Forms.Label nsPortLbl;
        private System.Windows.Forms.ComboBox blePortCmb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox consoleLsb;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Label backupLbl;
        private System.Windows.Forms.Panel productionPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel logoPanel;
        private System.Windows.Forms.Label logoLbl;
    }
}


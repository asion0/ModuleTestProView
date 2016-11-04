using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;   // required for Marshal
using System.IO;
using System.Reflection;

namespace ModuleTestProView
{
    public partial class ModuleTestForm : Form
    {
        public ModuleTestForm()
        {
            InitializeComponent();
        }

        private void ModuleTestForm_Load(object sender, EventArgs e)
        {
            //TestSQLite();
            //Global.Init();
            this.Icon = Properties.Resources.ModuleTestProView;
            DoLogin();
        }

        LogReport logReport = null;
        ProductionReport productionReport = null;
        private bool ProductionLogin()
        {
            Login login = new Login();
            if (DialogResult.OK != login.ShowDialog())
            {
                return false;
            }

            try
            {
                CountingProfile.countingProfile = new CountingProfile(Login.loginInfo.workFormNumber, "Counting.ini");
                logReport = new LogReport(Login.loginInfo.workFormNumber, "Report.csv", "Login.csv");
                productionReport = new ProductionReport(Login.loginInfo.workFormNumber, Login.loginInfo.workFormNumber + ".csv");
                if (CountingProfile.countingProfile.GetCounter() == 0)
                {
                    DialogResult dr = MessageBox.Show(Program.rm.GetString("WarningFirstTime"), Program.rm.GetString("MessageBoxWarningTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                    {
                        MessageBox.Show(Program.rm.GetString("WarningBackupFile"), Program.rm.GetString("MessageBoxWarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    if (!CountingProfile.countingProfile.IncreaseCounter())
                    {
                        throw new System.Exception(); ;
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
                ErrorMessage.Show(ErrorMessage.Errors.WriteFailure);
                return false;
            }
            return true;
        }

        private bool TestLogin()
        {
            if (Environment.GetEnvironmentVariable("sfxname") == null)
            {
                Login.loginInfo.currentPath = Environment.CurrentDirectory;
            }
            else
            {   //For WinRar sfx package.
                Login.loginInfo.currentPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("sfxname"));
            }
            Login.loginInfo.workFormNumber = "ProViewS3_Test" + DateTime.Now.ToString("yyyy-MM-dd");
            Login.loginInfo.backupDrive = null;

            logReport = new LogReport(Login.loginInfo.workFormNumber, "Report.csv", "Login.csv");
            return true;
        }

        private bool CalibrationLogin()
        {
            if (Environment.GetEnvironmentVariable("sfxname") == null)
            {
                Login.loginInfo.currentPath = Environment.CurrentDirectory;
            }
            else
            {   //For WinRar sfx package.
                Login.loginInfo.currentPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("sfxname"));
            }
            Login.loginInfo.workFormNumber = "ProViewS3_Calibration" + DateTime.Now.ToString("yyyy-MM-dd"); ;
            Login.loginInfo.backupDrive = null;

            logReport = new LogReport(Login.loginInfo.workFormNumber, "Report.csv", "Login.csv");
            return true;
        }

        private bool SnReaderLogin()
        {
            if (Environment.GetEnvironmentVariable("sfxname") == null)
            {
                Login.loginInfo.currentPath = Environment.CurrentDirectory;
            }
            else
            {   //For WinRar sfx package.
                Login.loginInfo.currentPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("sfxname"));
            }
            Login.loginInfo.workFormNumber = "ProViewS3_SnReader" + DateTime.Now.ToString("yyyy-MM-dd"); ;
            Login.loginInfo.backupDrive = null;

            logReport = new LogReport(Login.loginInfo.workFormNumber, "Report.csv", "Login.csv");
            return true;
        }

        private void DoLogin()
        {
            switch (ModuleTestProfile.proViewTestProfile.functionType)
            {
                case ModuleTestProfile.FunctionType.Production:
                    if (!ProductionLogin())
                    {
                        this.Close();
                        return;
                    }
                    workFormNo.Text = Login.loginInfo.workFormNumber;
                    backupLbl.Text = Login.loginInfo.backupDrive.Name;
                    logReport.AddLogin(DateTime.Now, CountingProfile.countingProfile.GetCounter(), Login.loginInfo.backupDrive.Name, Login.loginInfo.workFormNumber);
                    break;
                case ModuleTestProfile.FunctionType.TestOnly:
                    TestLogin();
                    logReport.AddLogin(DateTime.Now, 0, "", "");
                    break;
                case ModuleTestProfile.FunctionType.Calibration:
                    CalibrationLogin();
                    logReport.AddLogin(DateTime.Now, 0, "", "");
                    break;
                case ModuleTestProfile.FunctionType.SnReader:
                    SnReaderLogin();
                    logReport.AddLogin(DateTime.Now, 0, "", "");
                    break;
            }
         
            InitMainForm();
            EmptySlotStatus();
            UpdatePanelStatus(TestStatus.NotConnect);
        }
        
        private String GetVersion()
        {


            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        private void InitMainForm()
        {
            InitComSel();
            InitBackgroundWorker();

            switch (ModuleTestProfile.proViewTestProfile.functionType)
            {
                case ModuleTestProfile.FunctionType.Production:
                    testBtn.Text = "Production";
                    this.Text = "ProView S3 Production " + GetVersion();
                    productionPanel.BackColor = System.Drawing.Color.Bisque;
                    break;
                case ModuleTestProfile.FunctionType.TestOnly:
                    testBtn.Text = "Test";
                    productionPanel.Visible = false;
                    logoPanel.BackColor = System.Drawing.Color.SkyBlue;
                    logoPanel.Visible = true;
                    logoLbl.Text = "ProView S3 Test";
                    this.Text = "ProView S3 Test " + GetVersion();
                    break;
                case ModuleTestProfile.FunctionType.Calibration:
                    testBtn.Text = "Calibration";
                    productionPanel.Visible = false;
                    logoPanel.BackColor = System.Drawing.Color.PaleGreen;
                    logoPanel.Visible = true;
                    logoLbl.Text = "ProView S3 Calibration";
                    this.Text = "ProView S3 Calibration " + GetVersion();
                    productionPanel.Visible = false;
                    nsPortLbl.Visible = false;
                    nsPortCmb.Visible = false;
                    break;
                case ModuleTestProfile.FunctionType.SnReader:
                    testBtn.Text = "Read Serial Number";
                    productionPanel.Visible = false;
                    logoPanel.BackColor = System.Drawing.Color.Orchid;
                    logoPanel.Visible = true;
                    logoLbl.Text = "ProView S3 SN Reader";
                    this.Text = "ProView S3 SN Reader " + GetVersion();
                    productionPanel.Visible = false;
                    nsPortLbl.Visible = false;
                    nsPortCmb.Visible = false;
                    break;            
            }
        }

        private void InitComSel()
        {
            string[] ports = SerialPort.GetPortNames();
            string sel = nsPortCmb.Text;
            AddComSel(nsPortCmb, ports, sel);
            sel = blePortCmb.Text;
            AddComSel(blePortCmb, ports, sel);
        }

        private void AddComSel(ComboBox c, string[] ports, string sel)
        {
            String selItem = "";
            if (c.SelectedIndex >= 0)
            {
                selItem = c.Text;
            }
            else
            {
                selItem = sel;
            }

            c.Items.Clear();
            foreach (string port in ports)
            {
                int n = c.Items.Add(port);
                if (port == selItem)
                {
                    c.SelectedIndex = n;
                }
            }
        }

        private BackgroundWorker bkConnectWorker = new BackgroundWorker();
        private BackgroundWorker bkTestWorker = new BackgroundWorker();
        private void InitBackgroundWorker()
        {
            bkConnectWorker.WorkerReportsProgress = true;
            bkConnectWorker.WorkerSupportsCancellation = true;
            bkConnectWorker.DoWork += new DoWorkEventHandler(bwConnect_DoWork);
            bkConnectWorker.ProgressChanged += new ProgressChangedEventHandler(bwConnect_ProgressChanged);
            bkConnectWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwConnect_RunWorkerCompleted);

            bkTestWorker.WorkerReportsProgress = true;
            bkTestWorker.WorkerSupportsCancellation = true;
            bkTestWorker.DoWork += new DoWorkEventHandler(bwTest_DoWork);
            bkTestWorker.ProgressChanged += new ProgressChangedEventHandler(bwTest_ProgressChanged);
            bkTestWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwTest_RunWorkerCompleted);

            //testParam.index = i;
            testParam.bwConnect = bkConnectWorker;
            testParam.bwTest = bkTestWorker;
            testParam.nsSerial = new SkytraqGps();
            testParam.bleSerial = new SkytraqProViewS();
            //testParam.parser = new GpsMsgParser();
            //testParam.log = new StringBuilder();
        }


        private enum TestStatus
        {
            NotConnect,
            WaitingMacAddress,
            ReadyToTest,
            Testing,
        }
        private TestStatus testStatus = TestStatus.NotConnect;
        private void UpdatePanelStatus(TestStatus ts)
        {
            switch (ts)
            {
                case TestStatus.NotConnect:
                    blePortCmb.Enabled = true;
                    nsPortCmb.Enabled = true;
                    if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Calibration ||
                        ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.SnReader)
                    {
                        connectBtn.Enabled = (blePortCmb.SelectedIndex == -1) ? false : true;
                    }
                    else
                    {
                        connectBtn.Enabled = ((blePortCmb.SelectedIndex == -1) ? false : true) && ((nsPortCmb.SelectedIndex == -1) ? false : true);
                    }
                    connectBtn.Text = "Connect";
                    connectBtn.ForeColor = System.Drawing.Color.Black;
                    moduleName.Text = "The device is not ready";
                    moduleName.ForeColor = System.Drawing.Color.Red;
                    testBtn.Enabled = false;
                    break;
                case TestStatus.WaitingMacAddress:
                    blePortCmb.Enabled = false;
                    nsPortCmb.Enabled = false;
                    connectBtn.Enabled = true;
                    connectBtn.Text = "Disconnect";
                    connectBtn.ForeColor = System.Drawing.Color.Red;
                    moduleName.Text = "The device is not ready";
                    moduleName.ForeColor = System.Drawing.Color.Red;
                    testBtn.Enabled = false;
                    break;
                case TestStatus.ReadyToTest:
                    blePortCmb.Enabled = false;
                    nsPortCmb.Enabled = false;
                    connectBtn.Enabled = true;
                    connectBtn.Text = "Disconnect";
                    connectBtn.ForeColor = System.Drawing.Color.Red;
                    moduleName.ForeColor = System.Drawing.Color.Blue;
                    if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Calibration ||
                        ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.SnReader)
                    {
                        testBtn.Enabled = true;
                    }
                    else
                    {
                        testBtn.Enabled = (nsPortCmb.SelectedIndex == -1) ? false : true;
                    }
                    break;
                case TestStatus.Testing:
                    blePortCmb.Enabled = false;
                    nsPortCmb.Enabled = false;
                    connectBtn.Enabled = false;
                    connectBtn.Text = "Disconnect";
                    connectBtn.ForeColor = System.Drawing.Color.Red;
                    moduleName.ForeColor = System.Drawing.Color.Blue;
                    testBtn.Enabled = false;
                    break;
            }
            testStatus = ts;
            return;
        }

        private void UpdateSlotStatus(bool b)
        {
            testResult.EndLog();
            logReport.AddItem(testResult);
            if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Production)
            {
                testCounter.Text = CountingProfile.countingProfile.GetCounter().ToString();
            }

            if (b)
            {
                firstWork.Text = "PASS";
                firstWork.ForeColor = System.Drawing.Color.Blue;
                //Write Log and production list
                if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Production)
                {
                    if (!productionReport.AddItem(testResult))
                    {
                        ErrorMessage.Show(ErrorMessage.Errors.WriteFailure);
                        this.Close();
                        return;
                    }
                }
            }
            else
            {
                firstWork.Text = "FAIL";
                firstWork.ForeColor = System.Drawing.Color.Red;
                //Write Log
            }
        }

        private void EmptySlotStatus()
        {
            if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Production)
            {
                testCounter.Text = CountingProfile.countingProfile.GetCounter().ToString();
            }
            firstWork.Text = "";
        }

        private WorkerParam testParam = new WorkerParam();
        private WorkerResultParam testResult = new WorkerResultParam();
        private void AddMessage(int i, String s)
        {
            ListBox b = consoleLsb;
            bool scroll = (b.TopIndex == b.Items.Count - (int)(b.Height / b.ItemHeight));
            consoleLsb.Items.Add(s);
            if (scroll)
            {
                b.TopIndex = b.Items.Count - (int)(b.Height / b.ItemHeight);
            }
            testResult.consoleLog.AppendFormat("{0}\n", s);
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (testStatus == TestStatus.NotConnect)
            {
                testParam.nsComPort = nsPortCmb.Text;
                testParam.bleComPort = blePortCmb.Text;
                bkConnectWorker.RunWorkerAsync(testParam);
            }
            else
            {
                bkConnectWorker.CancelAsync();
            }
        }

        private void startTesting_Click(object sender, EventArgs e)
        {
            testParam.nsComPort = nsPortCmb.Text;
            testParam.bleComPort = blePortCmb.Text;
            testResult.StartLog();
            testResult.macAddress = moduleName.Text;
            testParam.result = testResult;

            if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Calibration)
            {
                //Send BLE Command
                BleModule.StartWriteSerialNumber();
            }
            else if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.SnReader)
            {
                //Send BLE Command
                BleModule.StartWriteSerialNumber();
            }
            else
            {
                bkTestWorker.RunWorkerAsync(testParam);
            }
            UpdatePanelStatus(TestStatus.Testing);
        }

        private void bwConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerParam p = e.Argument as WorkerParam;
            BleModule t = new BleModule();

            if (t.DoConnect(p, ref e))
            {   //Cancel == true mean error return
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
            t.EndProcess(p);
        }

        private void bwTest_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerParam p = e.Argument as WorkerParam;
            TestModule t = new TestModule();

            if (t.DoTest(p))
            {   //Cancel == true means error return
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
            t.EndProcess(p);
        }

        private void bwConnect_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkerReportParam r = e.UserState as WorkerReportParam;
            if (r.reportType == WorkerReportParam.ReportType.ShowProgress)
            {
                AddMessage(r.index, r.output);
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowSerialNumber)
            {
                AddMessage(r.index, r.output);
                firstWork.Text = r.output;
                firstWork.ForeColor = System.Drawing.Color.Blue;
            }
            else if (r.reportType == WorkerReportParam.ReportType.BleConnected)
            {
                UpdatePanelStatus(TestStatus.WaitingMacAddress);
                AddMessage(r.index, r.output);
            }
            else if (r.reportType == WorkerReportParam.ReportType.BleGotMacAddress)
            {
                UpdatePanelStatus(TestStatus.ReadyToTest);
                EmptySlotStatus();
                testBtn.Select();
                moduleName.Text = r.output;
                AddMessage(r.index, String.Format("Detect new device {0}.", r.output));
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowError)
            {
                try
                {
                    WorkerParam.ErrorType er = testParam.result.error;
                    if (r.output.Length != 0)
                    {
                        AddMessage(r.index, r.output);
                    }
                    String sss = String.Format("Error :({0}) {1}", ((int)er).ToString(), er.ToString());
                    AddMessage(r.index, sss);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.ToString());
                }
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowTestError)
            {
                WorkerParam.ErrorType er = testParam.result.error;
                if (r.output.Length != 0)
                {
                    AddMessage(r.index, r.output);
                }
                AddMessage(r.index, String.Format("Error :({0}) {1}", ((int)er).ToString(), er.ToString()));
                UpdateSlotStatus(false);
                UpdatePanelStatus(TestStatus.WaitingMacAddress);
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowWriteError)
            {
                ErrorMessage.Show(ErrorMessage.Errors.WriteFailure);
                this.Close();
                return;
            }
            else if (r.reportType == WorkerReportParam.ReportType.TestDone)
            {
                if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Calibration)
                {
                    AddMessage(r.index, "Calibration Reseted.");
                }
                else if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.SnReader)
                {
                    AddMessage(r.index, "Get SN Completed.");
                }
                else
                {
                    AddMessage(r.index, "Test Completed.");
                }
                UpdateSlotStatus(true);
                UpdatePanelStatus(TestStatus.WaitingMacAddress);
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowFinished)
            {
                UpdatePanelStatus(TestStatus.NotConnect);
                moduleName.Text = "";
                AddMessage(r.index, "Cancel BLE board connection.");
            }        
        }
        
        private void bwTest_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkerReportParam r = e.UserState as WorkerReportParam;
            if (r.reportType == WorkerReportParam.ReportType.ShowProgress)
            {
                AddMessage(r.index, r.output);
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowError)
            {
                WorkerParam.ErrorType er = testParam.result.error;
                if (r.output.Length != 0)
                {
                    AddMessage(r.index, r.output);
                }
                AddMessage(r.index, String.Format("Error :({0}) {1}" ,((int)er).ToString(), er.ToString()));
                UpdateSlotStatus(false);
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowFinished)
            {
                AddMessage(r.index, "Test Completed.");
                UpdateSlotStatus(true);
            }
        }

        private void bwConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = (sender as BackgroundWorker);
            UpdatePanelStatus(TestStatus.NotConnect);
        }

        private void bwTest_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = (sender as BackgroundWorker);
            if (e.Cancelled)
            {
                UpdatePanelStatus(TestStatus.WaitingMacAddress);
            }
        }

        private void blePortCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePanelStatus(testStatus);
        }

        private void comSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePanelStatus(testStatus);
        }

        //private static int[] cheatCode = { 0x26, 0x26, 0x28, 0x28, 0x25, 0x27, 0x25, 0x27, 0x42, 0x41 };
        //private int cheats = 0;
        //private bool continueMode = false;
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    const int WM_KEYDOWN = 0x100;
        //    if ((msg.Msg == WM_KEYDOWN))
        //    {
        //        if (msg.WParam.ToInt32() == cheatCode[cheats])
        //        {
        //            if (++cheats == cheatCode.Length)
        //            {   //Complete Cheat.
        //                continueMode = (continueMode) ? false : true;
        //                MessageBox.Show("Continue Mode " + ((continueMode) ? "ON" : "OFF"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                cheats = 0;
        //            }
        //        }
        //        else
        //        {
        //            cheats = 0;
        //        }
        //    }
        //    return false;
        //}

        private const int DBT_DEVTYP_HANDLE = 6;
        private const int DBT_DEVTYP_PORT = 3;
        private const int BROADCAST_QUERY_DENY = 0x424D5144;
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // removed 
        private const int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_PORT
        {
            public int dbcp_size;
            public int dbcp_devicetype;
            public int dbcp_reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] dbcp_name;
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);        // call default p

            if (m.Msg == WM_DEVICECHANGE)
            {
                // WM_DEVICECHANGE can have several meanings depending on the WParam value...
                int msgType = m.WParam.ToInt32();
                if (msgType == DBT_DEVICEARRIVAL || msgType == DBT_DEVICEREMOVECOMPLETE)
                {
                    int devType = Marshal.ReadInt32(m.LParam, 4);
                    if (DBT_DEVTYP_PORT == devType)
                    {

                        DEV_BROADCAST_PORT vol;
                        vol = (DEV_BROADCAST_PORT)
                            Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_PORT));

                        int step = (vol.dbcp_name[1] == 0x00) ? 2 : 1;
                        StringBuilder sb = new StringBuilder(8);
                        for (int i = 0; i < vol.dbcp_name.Length; i += step)
                        {
                            if (vol.dbcp_name[i] == 0x00)
                            {
                                break;
                            }
                            sb.Append(vol.dbcp_name[i]);
                        }
                        if (testStatus == TestStatus.NotConnect || testStatus == TestStatus.ReadyToTest)
                        {
                            if (msgType == DBT_DEVICEARRIVAL)
                            {
                                AddMessage(0, sb.ToString() + " plugged-in.");
                            }
                            else
                            {
                                AddMessage(0, sb.ToString() + " removed.");
                            }
                            InitComSel();
                            UpdatePanelStatus(testStatus);
                        }

                    }
                }
            }
        }      
    }
}

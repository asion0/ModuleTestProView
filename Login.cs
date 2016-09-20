using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace ModuleTestProView
{
    public partial class Login : Form
    {
        public class LoginInfo
        {
            //工單號碼
            public String workFormNumber { get; set; }

            //登入時間
            //public DateTime loginTime { get; set; }

            //工作目錄(in RAR package or run along)
            public String currentPath { get; set; }

            //備份磁碟
            public DriveInfo backupDrive { get; set; }
        }
        public static LoginInfo loginInfo = new LoginInfo();

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            AcceptButton = ok;
            CancelButton = cancel;

            if (Environment.GetEnvironmentVariable("sfxname") == null)
            {
                Login.loginInfo.currentPath = Environment.CurrentDirectory;
            }
            else
            {   //For WinRar sfx package.
                Login.loginInfo.currentPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("sfxname"));
            }
            GetDeveiceSize();
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            GetDeveiceSize();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            if (backupDrvCmb.SelectedIndex == -1)
            {
                ErrorMessage.Show(ErrorMessage.Errors.NoBackupDevice);
                return;
            }

            if (!CheckBackupDrv(backupDeviceList[backupDrvCmb.SelectedIndex]))
            {
                return;
            }       
     
            if (!CheckWorkNo())
            {
                ErrorMessage.Show(ErrorMessage.Errors.WrongWorkingNo);
                return;
            }

            loginInfo.workFormNumber = workNo.Text;
            loginInfo.backupDrive = backupDeviceList[backupDrvCmb.SelectedIndex];

            DialogResult = DialogResult.OK;
            Close();
        }

        private List<DriveInfo> backupDeviceList = new List<DriveInfo>();
        private void GetDeveiceSize()
        {
            DriveInfo[] listDrivesInfo = DriveInfo.GetDrives();
            backupDeviceList.Clear();
            try
            {
                foreach (DriveInfo v in listDrivesInfo)
                {
                    if (v.IsReady && v.DriveType == DriveType.Removable)
                    {
                        backupDeviceList.Add(v);
                        backupDrvCmb.Items.Add(String.Format("{0}({1}) 剩餘 {2} MB", v.VolumeLabel == "" ? "NO_NAME" : v.VolumeLabel, v.Name, v.TotalSize / (1024 * 1024)));
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }

            //自動選取第一個
            if (backupDrvCmb.Items.Count > 0)
            {
                backupDrvCmb.SelectedIndex = 0;
            }
        }

        private const int MinFreeSpace = 100 * 1024 * 1024;
        private bool CheckBackupDrv(DriveInfo v)
        {
            if (v.TotalFreeSpace < MinFreeSpace)
            {
                ErrorMessage.Show(ErrorMessage.Errors.InsufficientDiskSpace);
                return false;
            }

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(v.Name + "TestFolder03668");
                di.Delete();
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
                ErrorMessage.Show(ErrorMessage.Errors.BackupDrivePermissionsDenied);
                return false;
            }
            return true;
        }

        private bool CheckWorkNo()
        {
            String s = workNo.Text;
            if (s.Length != 16 || s[4] != '-')
            {
                return false;
            }
            return true;
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
 
        private bool Test()
        {
            //OpenFileDialog ofdOpen = new System.Windows.Forms.OpenFileDialog();
            //if ((ofdOpen.InitialDirectory == null) || (ofdOpen.InitialDirectory == string.Empty))
            //{
            ////    ofdOpen.InitialDirectory = "D:\\Firmware"; 
            //}

            //ofdOpen.Filter =
            //                "Firmware files (*.bin)|*.bin|" +
            //                "All Files (*.*)|*.*";
            //ofdOpen.Title = "Open firmware binary file";
            //ofdOpen.Multiselect = false; 

            //if (ofdOpen.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
            //{
            //    return false;
            //}
            //string filename = ofdOpen.FileName; //
            //var fs = new FileStream(filename, FileMode.Open);
            //var len = (int)fs.Length;

            //int crc = 0;
            //for (int i = 0; i < 0x80000 * 2; ++i)
            //{
            //    if (i < len)
            //        crc += fs.ReadByte();
            //    else
            //        crc += 0xff;
            //    crc &= 0xffff;
            //}
            //fs.Close();
            //MessageBox.Show(crc.ToString("X4"));
            return true;
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            Test();
        }

    }
}

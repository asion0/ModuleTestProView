using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModuleTestProView
{
    class LogReport
    {

        private String logFile;
        private String loginFile;
        public LogReport(String folder, String logFileName, String loginFileName)
        {
            logFile = String.Format("{0}\\Log", Login.loginInfo.currentPath);
            if (!Directory.Exists(logFile))
            {
                Directory.CreateDirectory(logFile);
            }

            logFile = String.Format("{0}\\Log\\{1}", Login.loginInfo.currentPath, folder);
            if (!Directory.Exists(logFile))
            {
                Directory.CreateDirectory(logFile);
            }
            loginFile = logFile;
            logFile += "\\" + logFileName;
            loginFile += "\\" + loginFileName;

            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine("Test NO.,BLE Mac,Start Date,Start Time,End Time,Test Duration(Sec),Error Code,Log");
                }
            }
            if (!File.Exists(loginFile))
            {
                using (StreamWriter sw = File.CreateText(loginFile))
                {
                    sw.WriteLine("Function,Login date,Login Time,Start count,Backup drive,Production Order");
                }
            }
        }

        private static int itemNumber = 1;
        public bool AddItem(WorkerResultParam r)
        {
            try
            {
                String s = String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    itemNumber++, r.macAddress, r.testStartTime.ToString("yyyy/MM/dd"), r.testStartTime.ToString("HH:mm:ss"), 
                    r.testEndTime.ToString("HH:mm:ss"), ((double)(r.testEndTime - r.testStartTime).TotalMilliseconds / 1000.0).ToString("F3"), 
                    r.error.ToString(), r.consoleLog);

                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine(s);
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
                return false;
            }
            return true;
        }
 
        public bool AddLogin(DateTime dt, int count, String bkDrv, String po)
        {
            try
            {
                String s = String.Format("{0},{1},{2},{3},{4},{5}",
                    ModuleTestProfile.proViewTestProfile.functionType.ToString(),
                    dt.ToString("yyyy/MM/dd"), dt.ToString("HH:mm:ss"), count, bkDrv, po);
                using (StreamWriter sw = File.AppendText(loginFile))
                {
                    sw.WriteLine(s);
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
                return false;
            }
            return true;
        }


    }

    class ProductionReport
    {
        private String reportFile;
        //private String reportFolder;
        private String backupReportFile;
        public ProductionReport(String folder, String reportFileName)
        {
            reportFile = String.Format("{0}\\Log", Login.loginInfo.currentPath);
            if (!Directory.Exists(reportFile))
            {
                Directory.CreateDirectory(reportFile);
            }

            reportFile = String.Format("{0}\\Log\\{1}", Login.loginInfo.currentPath, folder);
            if (!Directory.Exists(reportFile))
            {
                Directory.CreateDirectory(reportFile);
            }
            reportFile += "\\" + reportFileName;

            backupReportFile = String.Format("{0}{1}", Login.loginInfo.backupDrive.Name, folder);
            if (!Directory.Exists(backupReportFile))
            {
                Directory.CreateDirectory(backupReportFile);
            }
            backupReportFile += "\\" + reportFileName;

            if (!File.Exists(reportFile))
            {
                using (StreamWriter sw = File.CreateText(reportFile))
                {
                    sw.WriteLine("Production SN,Status,Production Date,Production Time");
                }
                using (StreamWriter sw = File.CreateText(backupReportFile))
                {
                    sw.WriteLine("Production SN,Status,Production Date,Production Time");
                }
            }
        }

        public bool AddItem(WorkerResultParam r) 
        {
            try
            {
                String s = String.Format("{0},{1},{2},{3}", 
                    r.serialNumber, 0, r.testEndTime.ToString("yyyy/MM/dd"), r.testEndTime.ToString("HH:mm:ss"));
                using (StreamWriter sw = File.AppendText(reportFile))
                {
                    sw.WriteLine(s);
                }
                using (StreamWriter sw = File.AppendText(backupReportFile))
                {
                    sw.WriteLine(s);
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
                return false;
            }
            return true;
        }
    }
}

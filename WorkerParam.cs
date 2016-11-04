using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace ModuleTestProView
{
    public class WorkerParam
    {   //存放BackgroundWorker所需參數
        public enum ErrorType
        {
            NoError = 0,
            OpenBlePortFail,
            OpenNavSparkPortFail,
            NavSparkCmdFail,
            InitialNavSparkFail,
            DeviceSelfTestFail,
            MotorSensorTestFail,
            EnterNormaiModeFail,

            GetSerialNumberFail,
            SerialNumberNotEmpty,
            SetSerialNumberFail,
            VerifySerialNumberFail,
            CalibrationFail,

            TestNotComplete,
            TestErrSize,
        }

        public static String GetErrorString(ErrorType er)
        {
            if (ErrorType.NoError == er)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            UInt64 nErr = (UInt64)er;
            bool first = true;
            for (byte i = 0; i < 64; i++)
            {
                UInt64 tt = nErr & ((UInt64)1 << i);

                if ((nErr & ((UInt64)1 << i)) != 0)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(i.ToString());
                    first = false;
                }
            }
            return sb.ToString();
        }
        public WorkerResultParam result = null;

        //COM port for worker
        public String nsComPort;
        public String bleComPort;

        //Worker instance
        public BackgroundWorker bwConnect;
        public BackgroundWorker bwTest;

        //SerialPort instance
        public SkytraqGps nsSerial;
        public SkytraqProViewS bleSerial;
        
        //public ModuleTestProfile profile;
        //for Report
        //public DateTime startTime;
        //public long duration;
        //public StringBuilder log;
        //public String annIoPort;

    }

    public class WorkerReportParam
    {   //BackgroundWorker回報項目
        public enum ReportType
        {
            BleConnected,
            ShowProgress,
            ShowSerialNumber,
            BleGotMacAddress,
            UpdateSnrChart,
            ShowError,
            ShowTestError,
            ShowWriteError,
            //ShowWaitingGoldenSample,
            //HideWaitingGoldenSample,
            //AllTaskFinished,
            TestDone,
            ShowFinished,
        }

        public WorkerReportParam()
        {
            reportType = ReportType.ShowProgress;
        }

        public WorkerReportParam(WorkerReportParam r)
        {
            index = r.index;
            output = r.output;
            reportType = r.reportType;
        }
        public int index { get; set; }
        public String output { get; set; }
        //public ErrorType error { get; set; }
        public ReportType reportType { get; set; }
    }

    public class WorkerResultParam
    {
        public WorkerResultParam()
        {

        }

        public void StartLog()
        {
            testStartTime = DateTime.Now;
            consoleLog.Remove(0, consoleLog.Length);
            consoleLog.Append('\"');
            error = WorkerParam.ErrorType.NoError;
            serialNumber = "";
            macAddress = "";
        }

        public void EndLog()
        {
            testEndTime = DateTime.Now;
            consoleLog.Append('\"');
        }

        public DateTime testStartTime = new DateTime();
        public DateTime testEndTime = new DateTime();
        public WorkerParam.ErrorType error = WorkerParam.ErrorType.NoError;
        public StringBuilder consoleLog = new StringBuilder();
        public String serialNumber = "";
        public String macAddress = "";

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ModuleTestProView
{
    class BleModule
    {
        private const int DefaultCmdTimeout = 1000;
        private byte[] deviceMac = new byte[6];
        enum WorkingStatus
        {
            None,
            WriteSerialNumber,
            ReadSerialNumber,
        }

        private static WorkingStatus workingStatus = WorkingStatus.None;
        public static void StartWriteSerialNumber()
        {
            workingStatus = WorkingStatus.WriteSerialNumber;
        }

        public static void StartReadSerialNumber()
        {
            workingStatus = WorkingStatus.ReadSerialNumber;
        }

        public BleModule()
        {
        }

        private bool OpenBleDevice(WorkerParam p, WorkerReportParam r, int baudIdx)
        {
            PROVIEWS_RESPONSE rep = p.bleSerial.Open(p.bleComPort, baudIdx);
            
            if (PROVIEWS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.result.error = WorkerParam.ErrorType.OpenBlePortFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.BleConnected;
            r.output = String.Format("Open BLE board in {0} at {1} bps success.", p.bleComPort, p.bleSerial.GetBaudRate().ToString());
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
            Array.Clear(deviceMac, 0, deviceMac.Length);
            return true;
        }

        private bool IintialFixture(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = p.nsSerial.Open(p.nsComPort, 5);
            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.OpenNavSparkPortFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = String.Format("Open NavSpark in {0} at {1} bps success.", p.nsComPort, p.nsSerial.GetBaudRate().ToString());
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
            
            //ProViewS3 test use GPIO 28,22,20,14,13,12,10,6 and only 10 for output. 0001-0000-0101-0000-0111-0100-0100-0000 = 10 50 74 40h
            //GPIO 28,22,14,13,12,10 for input, 0001-0000-0100-0000-0111-0000-0100-0000 = 10 40 70 40h
            rep = p.nsSerial.InitControllerIO(0x10507440, 0x10407040);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.InitialNavSparkFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = String.Format("Initial NavSpark success.");
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));

            //Set GPIO 10 to high, 20 to low
            rep = p.nsSerial.SetControllerIO(0x00000400, 0x00100000);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = String.Format("Set GPIO 10 to high, 20 to low.");
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));

            p.nsSerial.Close();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = String.Format("Close {0}.", p.nsComPort);
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        const int SnLength = 32;
        private bool DoQuerySerialNumber(WorkerParam p, WorkerReportParam r)
        {
            byte[] sn = new byte[SnLength];
            PROVIEWS_RESPONSE rep = PROVIEWS_RESPONSE.NONE;
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            rep = p.bleSerial.GetSerialNumber(3000, ref sn);
            if (PROVIEWS_RESPONSE.RETURN != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowTestError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.GetSerialNumberFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            bool emptySn = true;
            foreach (byte b in sn)
            {
                if (b != 0xff)
                {
                    emptySn = false;
                    break;
                }
            }
            if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.TestOnly)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "SN: " + ((emptySn) ? "[No Serial Number]" : GetSerialNumberString(sn));
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return true;
            }
            else if(ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.SnReader)
            {
                r.reportType = WorkerReportParam.ReportType.ShowSerialNumber;
                r.output = "SN: " + ((emptySn) ? "[No Serial Number]" : GetSerialNumberString(sn));
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return true;
            }
            
            if (!emptySn)
            {
                r.reportType = WorkerReportParam.ReportType.ShowTestError;
                r.output = "Existing SN: " + GetSerialNumberString(sn);
                p.result.error = WorkerParam.ErrorType.SerialNumberNotEmpty;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            return true;
        }

        private bool DoCalibration(WorkerParam p, WorkerReportParam r)
        {
            byte[] sn = new byte[SnLength];
            PROVIEWS_RESPONSE rep = p.bleSerial.SetCalibration(DefaultCmdTimeout);
            if (PROVIEWS_RESPONSE.ACK_OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowTestError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.CalibrationFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            return true;
        }

        private string GetSerialNumberString(byte[] s)
        {   
            StringBuilder sb = new StringBuilder();
            //sb.Append("SN: ");
            for (int i = 0; i < SnLength; ++i)
            {
                if (s[i] != 0)
                {
                    sb.Append((char)s[i]);
                }
            }
            return sb.ToString();
        }

        //private int testCount = 123456;
        private void GenerateSerialNumber(ref byte[] sn)
        {
            Calendar cal = new CultureInfo("zh-CN").Calendar;
            DateTime now = DateTime.Now;
            String macHex = ByteArrayToString(deviceMac);
            int counter = CountingProfile.countingProfile.GetCounter();
            int yyyy = Convert.ToInt32(Login.loginInfo.workFormNumber.Substring(5, 3)) + 1911;
            int mm = Convert.ToInt32(Login.loginInfo.workFormNumber.Substring(8, 2));
            int dd = Convert.ToInt32(Login.loginInfo.workFormNumber.Substring(10, 2));

            sn[0] = (Byte)((yyyy % 10000) / 1000 + '0');
            sn[1] = (Byte)((yyyy % 1000) / 100 + '0');
            sn[2] = (Byte)((yyyy % 100) / 10 + '0');
            sn[3] = (Byte)((yyyy % 10) / 1 + '0');
            sn[4] = (Byte)((mm % 100) / 10 + '0');
            sn[5] = (Byte)((mm % 10) / 1 + '0');
            sn[6] = (Byte)((dd % 100) / 10 + '0');
            sn[7] = (Byte)((dd % 10) / 1 + '0');
            sn[8] = (Byte)macHex[0];
            sn[9] = (Byte)macHex[1];
            sn[10] = (Byte)macHex[3];
            sn[11] = (Byte)macHex[4];
            sn[12] = (Byte)macHex[6];
            sn[13] = (Byte)macHex[7];
            sn[14] = (Byte)macHex[9];
            sn[15] = (Byte)macHex[10];
            sn[16] = (Byte)macHex[12];
            sn[17] = (Byte)macHex[13];
            sn[18] = (Byte)macHex[15];
            sn[19] = (Byte)macHex[16];
            sn[20] = (Byte)((counter % 1000000) / 100000 + '0');
            sn[21] = (Byte)((counter % 100000) / 10000 + '0');
            sn[22] = (Byte)((counter % 10000) / 1000 + '0');
            sn[23] = (Byte)((counter % 1000) / 100 + '0');
            sn[24] = (Byte)((counter % 100) / 10 + '0');
            sn[25] = (Byte)((counter % 10) / 1 + '0');
        }

        private byte[] serialNumber = new byte[SnLength];
        private bool DoWriteSerialNumber(WorkerParam p, WorkerReportParam r)
        {
            GenerateSerialNumber(ref serialNumber);

            PROVIEWS_RESPONSE rep = p.bleSerial.SetSerialNumber(DefaultCmdTimeout, serialNumber);
            if (PROVIEWS_RESPONSE.ACK_OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowTestError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.SetSerialNumberFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Write SN: " + GetSerialNumberString(serialNumber);
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool SetGpio10(WorkerParam p, WorkerReportParam r, bool high)
        {
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            if (high)
            {
                rep = p.nsSerial.SetControllerIO(0x00000400, 0x00000000);
            }
            else
            {
                rep = p.nsSerial.SetControllerIO(0x00000000, 0x00000400);
            }
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = String.Format("Set GPIO 10 to " + (high ? "high." : "low."));
            p.bwTest.ReportProgress(0, new WorkerReportParam(r));

            return true;
        }

        private bool DoVerifySerialNumber(WorkerParam p, WorkerReportParam r)
        {
            byte[] sn = new byte[SnLength];
            PROVIEWS_RESPONSE rep = p.bleSerial.GetSerialNumber(DefaultCmdTimeout, ref sn);
            if (PROVIEWS_RESPONSE.RETURN != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowTestError;
                r.output = "";
                p.result.error = WorkerParam.ErrorType.GetSerialNumberFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            //r.reportType = WorkerReportParam.ReportType.ShowProgress;
            //r.output = "Read " + GetSerialNumberString(sn);
            //p.bwConnect.ReportProgress(0, new WorkerReportParam(r));

            if (!sn.SequenceEqual(serialNumber))
            {
                r.reportType = WorkerReportParam.ReportType.ShowTestError;
                r.output = "Read SN: " + GetSerialNumberString(sn);
                p.result.error = WorkerParam.ErrorType.VerifySerialNumberFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Serial number successfully verified";
            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private enum MacProcStatus {
            None,
            MacChanged,
            DuplicateMac,
            InvalidateMacLength,
            InvalidateMacData,
        }

        private MacProcStatus ProcessBleMacAddress(byte[] mac, int len)
        {
            if (len != 6 && len != 12)
            {
                return MacProcStatus.InvalidateMacLength;
            }
            if (len == 12)
            {
                if (mac[0] != mac[6] || mac[1] != mac[7] || mac[2] != mac[8] ||
                    mac[3] != mac[9] || mac[4] != mac[10] || mac[5] != mac[11])
                {
                    return MacProcStatus.InvalidateMacData;
                }
            }

            if (mac[0] == deviceMac[0] && mac[1] == deviceMac[1] && mac[2] == deviceMac[2] &&
                mac[3] == deviceMac[3] && mac[4] == deviceMac[4] && mac[5] == deviceMac[5])
            {
                return MacProcStatus.DuplicateMac;
            }
            else
            {
                Array.Copy(mac, deviceMac, 6);
                return MacProcStatus.MacChanged;
            }
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            for (int i = 5; i >= 0; --i)
            {
                hex.AppendFormat("{0:X2}", ba[i]);
                if (i > 0)
                    hex.AppendFormat(":");
            }
            return hex.ToString();
        }

        public bool DoConnect(WorkerParam p, ref DoWorkEventArgs e)
        {
            WorkerReportParam r = new WorkerReportParam();
            try
            {
                if (!OpenBleDevice(p, r, GpsBaudRateConverter.BaudRate2Index(9600)))
                {
                    return false;
                }

                //Calibration doesn't need fixture.
                if (ModuleTestProfile.proViewTestProfile.functionType != ModuleTestProfile.FunctionType.Calibration &&
                    ModuleTestProfile.proViewTestProfile.functionType != ModuleTestProfile.FunctionType.SnReader)
                {
                    if (!IintialFixture(p, r))
                    {
                        return false;
                    }
                }

                byte[] buff = new byte[16];
                while (!p.bwConnect.CancellationPending)
                {
                    if (workingStatus == WorkingStatus.WriteSerialNumber)
                    {
                        if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Production)
                        {
                            if (!DoQuerySerialNumber(p, r))
                            {
                                workingStatus = WorkingStatus.None;
                                continue;
                            }

                            if (!DoWriteSerialNumber(p, r))
                            {
                                workingStatus = WorkingStatus.None;
                                continue;
                            }

                            if (!DoVerifySerialNumber(p, r))
                            {
                                workingStatus = WorkingStatus.None;
                                continue;
                            }

                            if (!CountingProfile.countingProfile.IncreaseCounter())
                            {
                                r.reportType = WorkerReportParam.ReportType.ShowWriteError;
                                r.output = "";
                                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                                workingStatus = WorkingStatus.None;
                                continue;
                            }
                        }
                        else if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.TestOnly)
                        {
                            if (!DoQuerySerialNumber(p, r))
                            {
                                workingStatus = WorkingStatus.None;
                                continue;
                            }
                        }
                        else if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.Calibration)
                        {
                            if (!DoCalibration(p, r))
                            {
                                workingStatus = WorkingStatus.None;
                                continue;
                            }
                        }
                        else if (ModuleTestProfile.proViewTestProfile.functionType == ModuleTestProfile.FunctionType.SnReader)
                        {
                            if (!DoQuerySerialNumber(p, r))
                            {
                                workingStatus = WorkingStatus.None;
                                continue;
                            }
                        }

                        p.result.serialNumber = GetSerialNumberString(serialNumber);
                        p.result.error = WorkerParam.ErrorType.NoError;
                        r.reportType = WorkerReportParam.ReportType.TestDone;
                        r.output = "";
                        p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                        workingStatus = WorkingStatus.None;
                    }

                    int len = p.bleSerial.ReadMacAddress(ref buff, 800);
                    if (len > 0)
                    {
                        MacProcStatus mps = ProcessBleMacAddress(buff, len);
                        if (mps == MacProcStatus.MacChanged || mps == MacProcStatus.DuplicateMac)
                        {
                            r.reportType = WorkerReportParam.ReportType.BleGotMacAddress;
                            r.output = ByteArrayToString(buff);
                            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                        }
                        else
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowProgress;
                            r.output = String.Format("Got mac({0}) {1} {2}", len,
                               ByteArrayToString(buff), mps.ToString());
                            p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                        }
                    }
                    Thread.Sleep(20);
                }

                EndProcess(p);
                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));


            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }
            return true;

        }

        public void EndProcess(WorkerParam p)
        {
            if(p.nsSerial != null)
                p.nsSerial.Close();
            if (p.bleSerial != null)
                p.bleSerial.Close();
        }
    }

    class TestModule
    {
        private const int DefaultCmdTimeout = 1000;

        public TestModule()
        {
        }

        private bool OpenGpsDevice(WorkerParam p, WorkerReportParam r, int baudIdx)
        {
            GPS_RESPONSE rep = p.nsSerial.Open(p.nsComPort, baudIdx);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.result.error = WorkerParam.ErrorType.OpenNavSparkPortFail;
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = String.Format("Open NavSpark in {0} at {1} bps success.", p.nsComPort, p.nsSerial.GetBaudRate().ToString());
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
            }
            return true;
        }

        private bool SetGpio20(WorkerParam p, WorkerReportParam r, bool high)
        {
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            if (high)
            {
                rep = p.nsSerial.SetControllerIO(0x00100000, 0x00000000);
            }
            else
            {
                rep = p.nsSerial.SetControllerIO(0x00000000, 0x00100000);
            }
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                p.bwConnect.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = String.Format("Set GPIO 20 to " + (high ? "high." : "low."));
            p.bwTest.ReportProgress(0, new WorkerReportParam(r));

            return true;
        }

        public bool DoTest(WorkerParam p)
        {
            try
            {
                WorkerReportParam r = new WorkerReportParam();
                GPS_RESPONSE rep = GPS_RESPONSE.NONE;
                if (!OpenGpsDevice(p, r, GpsBaudRateConverter.BaudRate2Index(115200)))
                {
                    return false;
                }

                //ProViewS3 test use GPIO 28,22,20,14,13,12,10,6 and only 10 for output. 0001-0000-0101-0000-0111-0100-0100-0000 = 10 50 74 40h
                //GPIO 28,22,14,13,12,6 for input, 0001-0000-0100-0000-0111-0000-0100-0000 = 10 40 70 40h
                rep = p.nsSerial.InitControllerIO(0x10507440, 0x10407040);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.result.error = WorkerParam.ErrorType.InitialNavSparkFail;
                    p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = String.Format("Initial NavSpark success.");
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));

                //Set GPIO 20 to high
                SetGpio20(p, r, true);

                //Test ProViewS3 board self-testing result(GPIO 6, 22, 28)
                if (!TestProViewMotor(p, r))
                {
                    SetGpio20(p, r, false);
                    return false;
                }
                Thread.Sleep(ModuleTestProfile.proViewTestProfile.delayBeforeTestIo);

                //Test ProViewS3 board self-testing result(GPIO 12, 13, 14)
                if (!TestProViewResult(p, r))
                {
                    SetGpio20(p, r, false);
                    return false;
                }

                //Set GPIO 10 to low
                rep = p.nsSerial.SetControllerIO(0x00000000, 0x00000400);
                if (GPS_RESPONSE.ACK != rep)
                {
                    SetGpio20(p, r, false);
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                    p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = String.Format("Set GPIO 10 to low.");
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));

                //Thread.Sleep(ModuleTestProfile.proViewTestProfile.testModeDuration);
                Thread.Sleep(1000);
                if (!DetectModeByMotor(p, r))
                {
                    SetGpio20(p, r, false);
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.result.error = WorkerParam.ErrorType.EnterNormaiModeFail;
                    p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                SetGpio20(p, r, false);


                //Send BLE Command
                BleModule.StartWriteSerialNumber();
                //Set GPIO 10 to high
                rep = p.nsSerial.SetControllerIO(0x00000400, 0x00000000);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                    p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = String.Format("Set GPIO 10 to high.");
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }
            //EndProcess(p);
            return true;
        }

        private bool TestProViewResult(WorkerParam p, WorkerReportParam r)
        {
            UInt32 ioData = 0;
            UInt32 TestIoMask = ModuleTestProfile.proViewTestProfile.ioTestHighPassPins;
            GPS_RESPONSE rep = p.nsSerial.GetRegister(1000, 0x20001008, ref ioData);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            if ((ioData & TestIoMask) != TestIoMask)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                r.output = GetSelfTestErrorString(ioData, TestIoMask);
                p.result.error = WorkerParam.ErrorType.DeviceSelfTestFail;
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Device self-test pass.";
            p.bwTest.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool TestProViewMotor(WorkerParam p, WorkerReportParam r)
        {
            //public GPS_RESPONSE TestControllerMotor(int timeout, Byte mode, UInt32 testIO, UInt32 duration, UInt32 testCount, ref Byte passCount, ref byte[] result)
            Byte passCount = 0;
            Byte[] result = new Byte[3];
            const UInt32 TestIoMask = 0x10400040;
            GPS_RESPONSE rep = p.nsSerial.TestControllerMotor(3000, 0, TestIoMask, 0x8000, 3, ref passCount, ref result);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                r.output = "TestControllerMotor " + rep.ToString();
                p.result.error = WorkerParam.ErrorType.NavSparkCmdFail;
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            if (passCount != 3)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                r.output = String.Format("Motor status(1, 2, 3) = {0:X2} {1:X2} {2:X2}", result[0], result[1], result[2]);
                p.result.error = WorkerParam.ErrorType.MotorSensorTestFail;
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Motor sensor test pass.";
            p.bwTest.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool DetectModeByMotor(WorkerParam p, WorkerReportParam r)
        {
            Byte passCount = 0;
            Byte[] result = new Byte[3];
            const UInt32 TestIoMask = 0x10000000;
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            do
            {
                rep = p.nsSerial.TestControllerMotor(2000, 0, TestIoMask, 0x10000, 2, ref passCount, ref result);

                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "TestControllerMotor " + rep.ToString();
                    p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                    continue;
                }
                if (passCount == 1)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Enter normal mode.";
                    p.bwTest.ReportProgress(0, new WorkerReportParam(r));
                    Thread.Sleep(800);
                    break;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Waiting for enter normal mode.";
                p.bwTest.ReportProgress(0, new WorkerReportParam(r));
            } while (sw.ElapsedMilliseconds < ModuleTestProfile.proViewTestProfile.testModeDuration);
            sw.Stop();
            return (passCount == 1);
        }

        private byte CalcCheckSum16(byte[] data, int start, int len)
        {
            UInt16 checkSum = 0;
            //const U08* ptr = dataPtr;
            //int loopCount = len / sizeof(UInt16);
            //int i;

            for (int i = 0; i < len; i += sizeof(UInt16))
            {
                UInt16 word = Convert.ToUInt16(data[start + i + 1] | data[start + i] << 8);
                checkSum += word;
            }
            return Convert.ToByte(((checkSum >> 8) + (checkSum & 0xFF)) & 0xFF);
        }

        public void EndProcess(WorkerParam p)
        {
            if (p.nsSerial != null)
            {
                p.nsSerial.Close();
            }
            //p.bleSerial.Close();
        }

        private String GetSelfTestErrorString(UInt32 ioData, UInt32 ioMask)
        {
            String r = "";
            for (int i = 0; i < 32; ++i)
            {
                if (((0x1U << i) & ioMask) != 0 && ((0x1U << i) & ioData) == 0)
                {
                    r += "GPIO" + i.ToString();
                    r += ", ";
                }
            }
            r = r.Substring(0, r.Length - 2);
            r += " NG";
            return r;
        }

    }
}

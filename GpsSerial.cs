using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Runtime.Remoting.Contexts;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace ModuleTestProView
{
    [Serializable()]

    public class GpsBaudRateConverter
    {
        static int[] baudTable = { 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        public static int BaudRate2Index(int baudRate)
        {
            for (int i = 0; i < baudTable.GetLength(0); i++)
            {
                if (baudTable[i] == baudRate)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int Index2BaudRate(int index)
        {
            return baudTable[index];
        }
    }

    public class BinaryCommand
    {
        private const int CommandExtraSize = 7;
        private const int CommandHeaderSize = 4;

        private byte[] commandData;

        public BinaryCommand()
        {

        }

        public BinaryCommand(byte[] data)
        {
            SetData(data);
        }

        private void SetData(byte[] data)
        {
            commandData = new byte[CommandExtraSize + data.Length];
            data.CopyTo(commandData, CommandHeaderSize);
        }

        public byte[] GetBuffer()
        {
            byte checkSum = 0;
            for (int i = 0; i < commandData.Length - CommandExtraSize; ++i)
            {
                checkSum ^= commandData[i + CommandHeaderSize];
            }

            commandData[0] = (byte)0xA0;
            commandData[1] = (byte)0xA1;
            commandData[2] = (byte)((commandData.Length - CommandExtraSize) >> 8);
            commandData[3] = (byte)((commandData.Length - CommandExtraSize) & 0xff);
            commandData[commandData.Length - 3] = checkSum;
            commandData[commandData.Length - 2] = (byte)0x0D;
            commandData[commandData.Length - 1] = (byte)0x0A;
            return commandData;
        }

        public int Size()
        {
            return commandData.Length;
        }
    }
    
    public class ProViewSCommand
    {
        private byte[] commandData;

        public ProViewSCommand()
        {

        }

        public ProViewSCommand(byte[] data)
        {
            SetData(data);
        }

        private void SetData(byte[] data)
        {
            commandData = new byte[data.Length];
            data.CopyTo(commandData, 0);
        }

        public byte[] GetBuffer()
        {
            Crc32(ref commandData);
            return commandData;
        }

        public int Size()
        {
            return commandData.Length;
        }
        /*
        * @brief crc32 - calculates CRC32 checksum of the data buffer.
        * @param pBuf - address of the data buffer.
        * @param length - length of the buffer.
        * @return CRC32 checksum.
        */
        private UInt32 Crc32(ref byte[] buf)
        {
            /* Nibble lookup table for 0x04C11DB7 polynomial. */
            UInt32[] crc_tab = {
                0x00000000,0x04C11DB7,0x09823B6E,0x0D4326D9,
                0x130476DC,0x17C56B6B,0x1A864DB2,0x1E475005,
                0x2608EDB8,0x22C9F00F,0x2F8AD6D6,0x2B4BCB61,
                0x350C9B64,0x31CD86D3,0x3C8EA00A,0x384FBDBD
            };
            /* Initial XOR value. */
            UInt32 crc = 0xFFFFFFFF;
            int len = buf.Length - 4;
            buf[buf.Length - 4] = 0;
            buf[buf.Length - 3] = 0;
            buf[buf.Length - 2] = 0;
            buf[buf.Length - 1] = 0;

            for (int i = 0; i < len; i += 4)
            {
                UInt32 v = BitConverter.ToUInt32(buf, i);
                crc ^= v;

                /* Process 32-bits, 4 at a time, or 8 rounds.
                 * - Assumes 32-bit reg, masking index to 4-bits;
                 * - 0x04C11DB7 Polynomial used in STM32.
                 */
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
            }
            //Write back crc
            buf[buf.Length - 4] = (byte)(crc & 0xFF);
            buf[buf.Length - 3] = (byte)((crc >> 8) & 0xFF);
            buf[buf.Length - 2] = (byte)((crc >> 16) & 0xFF);
            buf[buf.Length - 1] = (byte)((crc >> 24) & 0xFF);

            return crc;
        }

    }

    public enum GPS_RESPONSE
    {
        NONE,
        ACK,
        NACK,
        TIMEOUT,
        UART_FAIL,
        UART_OK,
        CHKSUM_OK,
        CHKSUM_FAIL,
        OK,
        //        END,
        ERROR1,
        ERROR2,
        ERROR3,
        ERROR4,
        ERROR5,
        UNKNOWN,
    };
    [Synchronization]
    public class SkytraqGps
    {
        private SerialPort serial = null;

        private CultureInfo enUsCulture = CultureInfo.GetCultureInfo("en-US");

        public SkytraqGps()
        {
        }

        public int GetBaudRate()
        {
            return serial.BaudRate;
        }
        public int Ready()
        {
            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();

            int i = 0;
            while (serial.BytesToRead == 0)
            {
                ++i;
                Thread.Sleep(10);
            }
            return i;
        }
        public BackgroundWorker cancleWorker { get; set; }

        #region UART function
        public GPS_RESPONSE Open(string com, int baudrateIdx)
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
            }

            serial = new SerialPort(com, GpsBaudRateConverter.Index2BaudRate(baudrateIdx));
            try
            {
                serial.Open();
            }
            catch (Exception ex)
            {
                // serial port exception
                if (ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is IOException)
                {
                    // port unavailable
                    return GPS_RESPONSE.UART_FAIL;
                }
            }
            finally
            {

            }
            return GPS_RESPONSE.UART_OK;
        }

        public GPS_RESPONSE Close()
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
                serial.Dispose();
                serial = null;
                return GPS_RESPONSE.UART_OK;
            }
            return GPS_RESPONSE.NONE;
        }

        public string ReadLineWait()
        {
            serial.NewLine = "\n";
            return serial.ReadLine() + (Char)0x0a;
        }

        public int ReadLineNoWait(byte[] buff, int len, int timeOut)
        {
            byte data;
            int crecv = 0;
            int read_bytes;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();

                while (sw.ElapsedMilliseconds < timeOut)
                {
                    read_bytes = serial.BytesToRead;
                    while (read_bytes > 0 && crecv < len)
                    {

                        data = (byte)serial.ReadByte();
                        buff[crecv] = data;
                        crecv++;
                        read_bytes--;
                        if (data == 10 && crecv > 2 && buff[crecv - 2] == 13)
                        {
                            if (buff[0] == 0xa0)
                            {
                                int msg_len = buff[2];
                                msg_len = msg_len << 8 | buff[3];
                                if (crecv == msg_len + 7)
                                    return crecv;
                            }
                            else
                            {
                                //Debug.Print(new string(Encoding.ASCII.GetChars(buff, 0, crecv)));
                                return crecv;
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                return crecv;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return 0;
        }

        private int ReadBinLine(ref byte[] received, int timeout)
        {
            byte buffer;
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            int index = 0;
            int packetLen = 0;
            while (sw.ElapsedMilliseconds < timeout)
            {
                if (serial.BytesToRead > 0)
                {
                    buffer = (byte)serial.ReadByte();
                    if ((index == 0 && buffer == 0xA0) || received[0] == 0xA0)
                    {   //從收到A0開始儲存
                        if (index >= received.Length)
                        {   //儲存不下就傳回Timeout
                            return index;
                        }
                        received[index] = buffer;
                        if (index == 3)
                        {
                            packetLen = (received[2] << 8) | received[3];
                        }
                        index++;
                        if (buffer == 0x0A && received[index - 2] == 0x0D)
                        {
                            int b = 0;
                            ++b;
                        }
                        //if (buffer == 0x0A && received[index - 2] == 0x0D)
                        if (buffer == 0x0A && received[index - 2] == 0x0D && (packetLen + 7) == index)
                        {   //收到0x0D, 0x0A後結束
                            return index;
                        }
                    }
                    else
                    {   //捨棄非A0開頭的資料
                        continue;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            return index;
        }

        private GPS_RESPONSE WaitAck(byte id, int timeout)
        {
            //int timeout = 2000;
            const int ReceiveLength = 128;
            byte[] received = new byte[ReceiveLength];
            byte[] buffer = new byte[1];

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds < timeout)
            {
                int l = ReadBinLine(ref received, timeout);
                if (l > 8)
                {   //最小的Ack封包會有8 bytes
                    if (received[0] == 0xA0 && received[4] == 0x83 && received[5] == id)
                    {
                        return GPS_RESPONSE.ACK;
                    }
                    else if (received[0] == 0xA0 && received[4] == 0x84)
                    {
                        long spend = sw.ElapsedMilliseconds;
                        return GPS_RESPONSE.NACK;
                    }

                    Array.Clear(received, 0, received.Length);
                    continue;
                }
            }
            return GPS_RESPONSE.TIMEOUT;
        }

        public GPS_RESPONSE WaitStringAck(int timeout, String waitingFor)
        {
            //const int ReceiveLength = 512;
            //byte[] received = new byte[ReceiveLength];
            byte[] buffer = new byte[1];
            //int index = 0;

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            //String ack;
            bool start = false;
            int ackLen = waitingFor.Length;
            int iter = 0;

            while (sw.ElapsedMilliseconds < timeout)
            {
                if (serial.BytesToRead > 0)
                {
                    //if (index >= ReceiveLength)
                    //{
                    //    return GPS_RESPONSE.TIMEOUT;
                    //}
                    buffer[0] = (byte)serial.ReadByte();
                    debugSb.Append((char)buffer[0]);

                    if (!start && buffer[0] == waitingFor[0])
                    {
                        start = true;
                    }

                    if (!start)
                    {
                        continue;
                    }

                    if (buffer[0] != waitingFor[iter++])
                    {
                        start = false;
                        iter = 0;
                        continue;
                    }

                    if (iter == ackLen)
                    {
                        return GPS_RESPONSE.OK;
                    }
                    /*
                    received[index] = buffer[0];
                    index++;

                    if (buffer[0] == 0x0)
                    {
                        ack = Encoding.UTF8.GetString(received, 0, index);
                        if (ack.Equals(waitingFor))
                        {
                            return GPS_RESPONSE.OK;
                        }
                        index = 0;
                        received.Initialize();
                    }
                    */
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            return GPS_RESPONSE.TIMEOUT;
        }

        public void ClearQueue()
        {
            serial.DiscardInBuffer();
            //serial.DiscardOutBuffer();
        }

        private GPS_RESPONSE SendCmdAck(byte[] cmd, int len, int timeout)
        {
            ClearQueue();
            serial.Write(cmd, 0, len);
            return WaitAck(cmd[4], timeout);
        }

        public void SendDataNoWait(byte[] cmd, int len)
        {
            //ClearQueue();
            serial.Write(cmd, 0, len);
            //return WaitAck(cmd[4]);
        }

        public GPS_RESPONSE SendDataWaitStringAck(byte[] data, int start, int len, int timeout, String waitingFor)
        {
            //ClearQueue();
            serial.Write(data, start, len);
            return WaitStringAck(timeout, waitingFor);
        }

        private GPS_RESPONSE SendStringCmdAck(String cmd, int len, int timeout, String waitingFor)
        {
            ClearQueue();
            serial.NewLine = "\0";
            serial.WriteLine(cmd);
            return WaitStringAck(timeout, waitingFor);
        }

        private void SendStringCmdNoAck(String cmd, int len)
        {
            ClearQueue();
            serial.NewLine = "\0";
            serial.WriteLine(cmd);
            //serial.Write(cmd.ToCharArray(), 0, len);
            return;
        }

        private void SendDummyCmdNoAck(int len)
        {
            ClearQueue();
            serial.NewLine = "\0";
            byte[] buf = new byte[1];
            buf[0] = 0;
            for (int i = 0; i < len; ++i)
            {
                serial.Write(buf, 0, 1);
            }
            //serial.Write(cmd.ToCharArray(), 0, len);
            return;
        }

        public GPS_RESPONSE ChangeBaudrate(byte baudrateIndex, byte mode, bool noDelay)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[4];
            cmdData[0] = 0x05;
            cmdData[1] = 0x00;
            cmdData[2] = baudrateIndex;
            cmdData[3] = mode;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            if (retval == GPS_RESPONSE.ACK)
            {
                if (!noDelay)
                {
                    Thread.Sleep(1000);
                }
                serial.Close();
                Open(serial.PortName, baudrateIndex);
            }
            return retval;
        }

        private GPS_RESPONSE WaitReturnCommand(byte cmdId, byte[] retCmd, int timeout)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.TIMEOUT;
            //byte timeout = 10;
            byte[] received = new byte[128];
            //int timeout = 1000;

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds < timeout)
            {
                int l = ReadBinLine(ref received, timeout);
                if (cmdId == GpsMsgParser.CheckBinaryCommand(received, l))
                {
                    received.CopyTo(retCmd, 0);
                    return GPS_RESPONSE.ACK;
                }
            }
            return retval;
        }

        public GPS_RESPONSE GetRegister(int timeout, UInt32 regAddr, ref UInt32 data)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;

            byte[] cmdData = new byte[5];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x71;
            cmdData[1] = (byte)(regAddr >> 24 & 0xFF);
            cmdData[2] = (byte)(regAddr >> 16 & 0xFF);
            cmdData[3] = (byte)(regAddr >> 8 & 0xFF);
            cmdData[4] = (byte)(regAddr & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xc0, retCmd, 1000);
                data = (UInt32)retCmd[5] << 24 | (UInt32)retCmd[6] << 16 |
                    (UInt32)retCmd[7] << 8 | (UInt32)retCmd[8];
            }
            return retval;
        }

        public GPS_RESPONSE SetRegister(int timeout, UInt32 regAddr, UInt32 data)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;

            byte[] cmdData = new byte[9];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x72;
            cmdData[1] = (byte)(regAddr >> 24 & 0xFF);
            cmdData[2] = (byte)(regAddr >> 16 & 0xFF);
            cmdData[3] = (byte)(regAddr >> 8 & 0xFF);
            cmdData[4] = (byte)(regAddr & 0xFF);
            cmdData[5] = (byte)(data >> 24 & 0xFF);
            cmdData[6] = (byte)(data >> 16 & 0xFF);
            cmdData[7] = (byte)(data >> 8 & 0xFF);
            cmdData[8] = (byte)(data & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            return retval;
        }

        public GPS_RESPONSE QueryRtc(ref UInt32 rtc)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;

            byte[] cmdData = new byte[5];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x71;
            cmdData[1] = 0x20;
            cmdData[2] = 0x01;
            cmdData[3] = 0x4C;
            cmdData[4] = 0x34;

            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 1000);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xc0, retCmd, 1000);
                rtc = (UInt32)retCmd[5] << 24 | (UInt32)retCmd[6] << 16 |
                    (UInt32)retCmd[7] << 8 | (UInt32)retCmd[8];
            }
            return retval;
        }

        public GPS_RESPONSE QueryChannelDoppler(byte channel, ref UInt32 prn, ref UInt32 freq)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[2];
            cmdData[0] = 0x7B;
            cmdData[1] = channel;

            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xFE, retCmd, 2000);
                if (retval != GPS_RESPONSE.ACK)
                {
                    // int a = 0;
                }
                prn = (UInt32)retCmd[5] << 8 | (UInt32)retCmd[6];
                freq = (UInt32)retCmd[7] << 8 | (UInt32)retCmd[8];
            }
            else
            {
                //int a = 0;
            }

            return retval;
        }

        public GPS_RESPONSE QueryChannelClockOffset(UInt32 gdClockOffset, UInt32 prn, UInt32 freq, ref Int32 clkData)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[9];
            cmdData[0] = 0x7C;
            cmdData[1] = (byte)(gdClockOffset >> 24 & 0xFF); ;
            cmdData[2] = (byte)(gdClockOffset >> 16 & 0xFF); ;
            cmdData[3] = (byte)(gdClockOffset >> 8 & 0xFF); ;
            cmdData[4] = (byte)(gdClockOffset & 0xFF); ;
            cmdData[5] = (byte)(prn >> 8 & 0xFF);
            cmdData[6] = (byte)(prn & 0xFF);
            cmdData[7] = (byte)(freq >> 8 & 0xFF);
            cmdData[8] = (byte)(freq & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xFF, retCmd, 2000);
                clkData = (Int32)((UInt32)retCmd[5] << 24 | (UInt32)retCmd[6] << 16 |
                    (UInt32)retCmd[7] << 8 | (UInt32)retCmd[8]);
            }
            return retval;
        }

        public GPS_RESPONSE ConfigMessageOutput(byte type)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[3];
            cmdData[0] = 0x09;
            cmdData[1] = type;
            cmdData[2] = 0;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            return retval;
        }

        public GPS_RESPONSE TestDevice(int timeout, int retry)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[10];
            cmdData[0] = 0xA0;
            cmdData[1] = 0xA1;
            cmdData[2] = 0x00;
            cmdData[3] = 0x02;
            cmdData[4] = 0x09;
            cmdData[5] = 0x01;
            cmdData[6] = 0x00;
            cmdData[7] = 0x00;
            cmdData[8] = 0x0D;
            cmdData[9] = 0x0A;

            for (int i = 0; i < retry; ++i)
            {
                retval = SendCmdAck(cmdData, cmdData.Length, timeout);
                if (GPS_RESPONSE.NACK == retval)
                {
                    break;
                }
            }
            return retval;
        }

        public GPS_RESPONSE ConfigNmeaOutput(byte gga, byte gsa, byte gsv, byte gll, byte rmc, byte vtg, byte zda, byte attr)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[9];
            cmdData[0] = 0x08;
            cmdData[1] = gga;
            cmdData[2] = gsa;
            cmdData[3] = gsv;
            cmdData[4] = gll;
            cmdData[5] = rmc;
            cmdData[6] = vtg;
            cmdData[7] = zda;
            cmdData[8] = attr;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            return retval;
        }

        public GPS_RESPONSE FactoryReset()
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[2];
            cmdData[0] = 0x04;
            cmdData[1] = 0x01;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 5000);
            return retval;
        }

        public GPS_RESPONSE NoNmeaOutput()
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[2];
            cmdData[0] = 0x09;
            cmdData[1] = 0x00;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 1000);
            return retval;
        }

        public GPS_RESPONSE SendColdStart(int retry, int timeout)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[15];
            cmdData[0] = 0x01;
            cmdData[1] = 0x03;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            for (int i = 0; i < retry; ++i)
            {
                retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
                if (retval == GPS_RESPONSE.ACK)
                {
                    break;
                }
            }
            return retval;
        }

        public GPS_RESPONSE SetGpsEphemeris(string ephFile)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[87];
            byte[] ephData = new byte[86];
            cmdData[0] = 0x41;
            int ephStart = 0;
            FileStream fileStream = new FileStream(ephFile, FileMode.Open, FileAccess.Read);
            for (int i = 0; i < 32; ++i)
            {
                fileStream.Read(ephData, 0, 86);
                ephStart += 86;

                int zeroCount = 0;
                foreach (byte b in ephData)
                {
                    if (b == 0)
                        zeroCount++;
                }
                if (zeroCount > 60)
                    continue;

                System.Array.Copy(ephData, 0, cmdData, 1, 86);
                BinaryCommand cmd = new BinaryCommand(cmdData);

                retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 4000);
                if (retval != GPS_RESPONSE.ACK)
                {
                    break;
                }
            }
            fileStream.Close();
            return retval;
        }

        public GPS_RESPONSE ConfigNoOutput(int timeout)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[2];
            cmdData[0] = 0x09;
            cmdData[1] = 0x02;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            return retval;
        }

        /*
        * @brief crc32 - calculates CRC32 checksum of the data buffer.
        * @param pBuf - address of the data buffer.
        * @param length - length of the buffer.
        * @return CRC32 checksum.
        */
        private UInt32 Crc32(ref byte[] buf)
        {
            /* Nibble lookup table for 0x04C11DB7 polynomial. */
            UInt32[] crc_tab = {
                0x00000000,0x04C11DB7,0x09823B6E,0x0D4326D9,
                0x130476DC,0x17C56B6B,0x1A864DB2,0x1E475005,
                0x2608EDB8,0x22C9F00F,0x2F8AD6D6,0x2B4BCB61,
                0x350C9B64,0x31CD86D3,0x3C8EA00A,0x384FBDBD
            };
            /* Initial XOR value. */
            UInt32 crc = 0xFFFFFFFF;
            int len = buf.Length - 4;
            buf[buf.Length - 4] = 0;
            buf[buf.Length - 3] = 0;
            buf[buf.Length - 2] = 0;
            buf[buf.Length - 1] = 0;

            for (int i = 0; i < len; i += 4)
            {
                UInt32 v = BitConverter.ToUInt32(buf, i);
                crc ^= v;

                /* Process 32-bits, 4 at a time, or 8 rounds.
                 * - Assumes 32-bit reg, masking index to 4-bits;
                 * - 0x04C11DB7 Polynomial used in STM32.
                 */
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
                crc = (crc << 4) ^ crc_tab[crc >> 28];
            }
            //Write back crc
            buf[buf.Length - 4] = (byte)(crc & 0xFF);
            buf[buf.Length - 3] = (byte)((crc >> 8) & 0xFF);
            buf[buf.Length - 2] = (byte)((crc >> 16) & 0xFF);
            buf[buf.Length - 1] = (byte)((crc >> 24) & 0xFF);

            return crc;
        }

        public GPS_RESPONSE QueryVersion(int timeout, ref String kVer, ref String sVer, ref String rev)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[8];
            cmdData[0] = 0x55;
            cmdData[1] = 0x41;
            cmdData[2] = 0x08;
            cmdData[3] = 0x02;

            UInt32 crc = Crc32(ref cmdData);

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0x80, retCmd, 1000);

                kVer = retCmd[7].ToString("00") + "." + retCmd[8].ToString("00") + "." + retCmd[9].ToString("00");
                sVer = retCmd[11].ToString("00") + "." + retCmd[12].ToString("00") + "." + retCmd[13].ToString("00");
                rev = (retCmd[15] + 2000).ToString("0000") + retCmd[16].ToString("00") + retCmd[17].ToString("00");
            }

            return retval;
        }

        public GPS_RESPONSE QueryCrc(int timeout, ref uint crc)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[2];
            cmdData[0] = 0x03;
            cmdData[1] = 0x01;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0x81, retCmd, 1000);

                crc = ((uint)retCmd[6] << 8) + retCmd[7];
            }
            return retval;
        }

        public GPS_RESPONSE StartDownload(byte baudrateIdx)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[6];
            cmdData[0] = 0x0B;
            cmdData[1] = baudrateIdx;
            cmdData[2] = 0x0;
            cmdData[3] = 0x0;
            cmdData[4] = 0x0;
            cmdData[5] = 0x0;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 3000);
            return retval;
        }

        public GPS_RESPONSE SendRomBinSize(int length, byte checksum)
        {//"BINSIZE = %d Checksum = %d %lld ", promLen, mycheck, check);
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            String cmd = "BINSIZE = " + length.ToString() + " Checksum = " + checksum.ToString() +
                " " + (length + checksum).ToString() + " ";

            retval = SendStringCmdAck(cmd, cmd.Length, 15000, "OK\0");
            return retval;
        }

        public GPS_RESPONSE SendTestSrecCmd(String cmd, int timeout)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            retval = SendStringCmdAck(cmd, cmd.Length, timeout, "OK\0");
            return retval;
        }

        public GPS_RESPONSE SendTagBinSize(int length, byte checksum, int baudIdx, UInt32 tagAddress, UInt32 tagValue)
        {//("BINSIZE2 = %d %d %d %d %d %d ", promLen, mycheck, baudidx, ta, tc, check);
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            UInt32 chk = Convert.ToUInt32(length) + Convert.ToUInt32(checksum) + Convert.ToUInt32(baudIdx)
                + Convert.ToUInt32(tagAddress) + Convert.ToUInt32(tagValue);
            String cmd = "BINSIZ2 = " + length.ToString() + " " + checksum.ToString() +
                " " + baudIdx.ToString() + " " + tagAddress.ToString() + " " + tagValue.ToString() +
                " " + chk.ToString() + " ";

            retval = SendStringCmdAck(cmd, cmd.Length, 15000, "OK\0");
            return retval;
        }

        private StringBuilder debugSb = new StringBuilder(4096);
        public GPS_RESPONSE SendLoaderDownload(ref String dbgOutput)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            String cmd = "$LOADER DOWNLOAD";
            //for (int i = 0; i < 5; ++i)
            {
                dbgOutput += "send [" + cmd + "];";
                debugSb.Remove(0, debugSb.Length);
                retval = SendStringCmdAck(cmd, cmd.Length, 1000, "OK\0");
                if (GPS_RESPONSE.OK == retval)
                {
                    dbgOutput += "ack [OK];";
                }
                else
                {
                    dbgOutput += "timeout[";
                    dbgOutput += debugSb.ToString();
                    dbgOutput += "]";
                    retval = GPS_RESPONSE.OK;
                }
            }
            return retval;
        }

        public GPS_RESPONSE UploadLoader(String s)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            String[] delimiterChars = { "\r\n" };
            String[] lines = s.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            foreach (String l in lines)
            {
                String line = l + (char)0x0a;
                SendStringCmdNoAck(line, line.Length);
            }
            retval = WaitStringAck(1000, "END\0");
            return retval;
        }
        public GPS_RESPONSE InitControllerIO(UInt32 ioList, UInt32 ioDirection)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            //6f 01 d0 11 34 7e d0 00 00 00
            byte[] cmdData = new byte[10];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x6f;
            cmdData[1] = 0x01;
            cmdData[2] = (byte)(ioList >> 24 & 0xFF);
            cmdData[3] = (byte)(ioList >> 16 & 0xFF);
            cmdData[4] = (byte)(ioList >> 8 & 0xFF);
            cmdData[5] = (byte)(ioList & 0xFF);
            cmdData[6] = (byte)(ioDirection >> 24 & 0xFF);
            cmdData[7] = (byte)(ioDirection >> 16 & 0xFF);
            cmdData[8] = (byte)(ioDirection >> 8 & 0xFF);
            cmdData[9] = (byte)(ioDirection & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            return retval;
        }

        public GPS_RESPONSE SetControllerIO(UInt32 ioHigh, UInt32 ioLow)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            //6f 01 d0 11 34 7e d0 00 00 00
            byte[] cmdData = new byte[10];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x6f;
            cmdData[1] = 0x02;
            cmdData[2] = (byte)(ioHigh >> 24 & 0xFF);
            cmdData[3] = (byte)(ioHigh >> 16 & 0xFF);
            cmdData[4] = (byte)(ioHigh >> 8 & 0xFF);
            cmdData[5] = (byte)(ioHigh & 0xFF);
            cmdData[6] = (byte)(ioLow >> 24 & 0xFF);
            cmdData[7] = (byte)(ioLow >> 16 & 0xFF);
            cmdData[8] = (byte)(ioLow >> 8 & 0xFF);
            cmdData[9] = (byte)(ioLow & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            return retval;
        }

        public GPS_RESPONSE SetControllerMoto(byte function, byte homeIo, byte ccwIo, byte cwIo, byte dirIo, byte clkIo,
            UInt32 clkTimes, UInt32 clkDelay)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[16];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x6f;
            cmdData[1] = 0x03;
            cmdData[2] = function;
            cmdData[3] = homeIo;
            cmdData[4] = ccwIo;
            cmdData[5] = cwIo;
            cmdData[6] = dirIo;
            cmdData[7] = clkIo;
            cmdData[8] = (byte)(clkTimes >> 24 & 0xFF);
            cmdData[9] = (byte)(clkTimes >> 16 & 0xFF);
            cmdData[10] = (byte)(clkTimes >> 8 & 0xFF);
            cmdData[11] = (byte)(clkTimes & 0xFF);
            cmdData[12] = (byte)(clkDelay >> 24 & 0xFF);
            cmdData[13] = (byte)(clkDelay >> 16 & 0xFF);
            cmdData[14] = (byte)(clkDelay >> 8 & 0xFF);
            cmdData[15] = (byte)(clkDelay & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            return retval;
        }

        public GPS_RESPONSE SetControllerSensor(byte function, byte homeIo, byte ccwIo, byte cwIo, byte dirIo, byte clkIo,
            UInt32 clkTimes, UInt32 clkDelay)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            //6f 01 d0 11 34 7e d0 00 00 00
            byte[] cmdData = new byte[16];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x6f;
            cmdData[1] = 0x05;
            cmdData[2] = function;
            cmdData[3] = homeIo;
            cmdData[4] = ccwIo;
            cmdData[5] = cwIo;
            cmdData[6] = dirIo;
            cmdData[7] = clkIo;
            cmdData[8] = (byte)(clkTimes >> 24 & 0xFF);
            cmdData[9] = (byte)(clkTimes >> 16 & 0xFF);
            cmdData[10] = (byte)(clkTimes >> 8 & 0xFF);
            cmdData[11] = (byte)(clkTimes & 0xFF);
            cmdData[12] = (byte)(clkDelay >> 24 & 0xFF);
            cmdData[13] = (byte)(clkDelay >> 16 & 0xFF);
            cmdData[14] = (byte)(clkDelay >> 8 & 0xFF);
            cmdData[15] = (byte)(clkDelay & 0xFF);
            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 10000);
            return retval;
        }

        public GPS_RESPONSE TestControllerMotor(int timeout, Byte mode, UInt32 testIO, UInt32 duration, UInt32 testCount, ref Byte passCount, ref byte[] result)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            //6f 01 d0 11 34 7e d0 00 00 00
            byte[] cmdData = new byte[15];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x6f;
            cmdData[1] = 0x06;
            cmdData[2] = (byte)mode;    //Only support 0 now
            cmdData[3] = (byte)(testIO >> 24 & 0xFF);
            cmdData[4] = (byte)(testIO >> 16 & 0xFF);
            cmdData[5] = (byte)(testIO >> 8 & 0xFF);
            cmdData[6] = (byte)(testIO & 0xFF);
            cmdData[7] = (byte)(duration >> 24 & 0xFF);
            cmdData[8] = (byte)(duration >> 16 & 0xFF);
            cmdData[9] = (byte)(duration >> 8 & 0xFF);
            cmdData[10] = (byte)(duration & 0xFF);
            cmdData[11] = (byte)(testCount >> 24 & 0xFF);
            cmdData[12] = (byte)(testCount >> 16 & 0xFF);
            cmdData[13] = (byte)(testCount >> 8 & 0xFF);
            cmdData[14] = (byte)(testCount & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0x6f, retCmd, timeout);
                passCount = retCmd[6];
                int resultLen = (int)retCmd[3] - 3;
                for (int i = 0; i < ((resultLen > result.Length) ? resultLen : result.Length); ++i)
                {
                    result[i] = retCmd[i + 7];
                }
            } 
            return retval;
        }

        public GPS_RESPONSE QueryDrStatus(int timeout, ref UInt32 temp, ref float gyro, ref UInt32 odo_plus, ref byte odo_bw)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            byte[] cmdData = new byte[1];
            cmdData[0] = 0x7F;

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xF0, retCmd, 1000);

                temp = (UInt32)retCmd[5] << 8 | (UInt32)retCmd[6];
                UInt32 data = (UInt32)retCmd[7] << 24 | (UInt32)retCmd[8] << 16 |
                    (UInt32)retCmd[9] << 8 | (UInt32)retCmd[10];

                byte[] t = new byte[4];
                t[0] = retCmd[10]; t[1] = retCmd[9]; t[2] = retCmd[8]; t[3] = retCmd[7];
                //t[0] = retCmd[7]; t[1] = retCmd[8]; t[2] = retCmd[9]; t[3] = retCmd[10];
                gyro = System.BitConverter.ToSingle(t, 0);
                //gyro = System.BitConverter.ToSingle(retCmd, 7);

                odo_plus = (UInt32)retCmd[11] << 8 | (UInt32)retCmd[12];
                odo_bw = retCmd[13];
            }
            return retval;
        }

        public GPS_RESPONSE AntennaIO(byte type)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;

            byte[] cmdData = new byte[5];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x71;
            cmdData[1] = 0xfe;
            cmdData[2] = 0x00;
            cmdData[3] = 0x00;
            cmdData[4] = type;

            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 1000);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xc0, retCmd, 1000);
            }
            return retval;
        }

        public GPS_RESPONSE SetControllerClock(byte function, UInt32 clockLength, UInt32 ioList)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;
            //6f 01 d0 11 34 7e d0 00 00 00
            byte[] cmdData = new byte[11];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x6f;
            cmdData[1] = 0x04;
            cmdData[2] = function;
            cmdData[3] = (byte)(clockLength >> 24 & 0xFF);
            cmdData[4] = (byte)(clockLength >> 16 & 0xFF);
            cmdData[5] = (byte)(clockLength >> 8 & 0xFF);
            cmdData[6] = (byte)(clockLength & 0xFF);
            cmdData[7] = (byte)(ioList >> 24 & 0xFF);
            cmdData[8] = (byte)(ioList >> 16 & 0xFF);
            cmdData[9] = (byte)(ioList >> 8 & 0xFF);
            cmdData[10] = (byte)(ioList & 0xFF);

            BinaryCommand cmd = new BinaryCommand(cmdData);
            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 2000);
            return retval;
        }

        public GPS_RESPONSE QueryAntennaDetect(ref byte detect)
        {
            GPS_RESPONSE retval = GPS_RESPONSE.NONE;

            byte[] cmdData = new byte[1];
            byte[] recv_buff = new byte[128];

            cmdData[0] = 0x48;
            BinaryCommand cmd = new BinaryCommand(cmdData);

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), 1000);
            if (retval == GPS_RESPONSE.ACK)
            {
                byte[] retCmd = new byte[128];
                retval = WaitReturnCommand(0xbc, retCmd, 1000);
                detect = retCmd[6];
            }
            return retval;
        }

        #endregion
    }

    public enum PROVIEWS_RESPONSE
    {
        NONE,
        ACK_OK,
        ACK_FAIL,
        RETURN,
        UART_OK,
        UART_FAIL,
        TIMEOUT,
        UNKNOWN,
    };

    [Synchronization]
    public class SkytraqProViewS
    {
        private SerialPort serial = null;

        //private CultureInfo enUsCulture = CultureInfo.GetCultureInfo("en-US");

        public SkytraqProViewS()
        {
        }

        public int GetBaudRate()
        {
            return serial.BaudRate;
        }

        public int Ready()
        {
            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();

            int i = 0;
            while (serial.BytesToRead == 0)
            {
                ++i;
                Thread.Sleep(10);
            }
            return i;
        }
        public BackgroundWorker cancleWorker { get; set; }

        #region UART function
        public PROVIEWS_RESPONSE Open(string com, int baudrateIdx)
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
            }

            serial = new SerialPort(com, GpsBaudRateConverter.Index2BaudRate(baudrateIdx));
            try
            {
                serial.Open();
            }
            catch (Exception ex)
            {
                // serial port exception
                if (ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is IOException)
                {
                    // port unavailable
                    return PROVIEWS_RESPONSE.UART_FAIL;
                }
            }
            finally
            {

            }
            return PROVIEWS_RESPONSE.UART_OK;
        }

        public PROVIEWS_RESPONSE Close()
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
                serial.Dispose();
                serial = null;
                return PROVIEWS_RESPONSE.UART_OK;
            }
            return PROVIEWS_RESPONSE.NONE;
        }

        //public string ReadLineWait()
        //{
        //    serial.NewLine = "\n";
        //    return serial.ReadLine() + (Char)0x0a;
        //}

        //public int ReadLineNoWait(byte[] buff, int len, int timeOut)
        //{
        //    byte data;
        //    int crecv = 0;
        //    int read_bytes;

        //    try
        //    {
        //        Stopwatch sw = new Stopwatch();
        //        sw.Reset();
        //        sw.Start();

        //        while (sw.ElapsedMilliseconds < timeOut)
        //        {
        //            read_bytes = serial.BytesToRead;
        //            while (read_bytes > 0 && crecv < len)
        //            {

        //                data = (byte)serial.ReadByte();
        //                buff[crecv] = data;
        //                crecv++;
        //                read_bytes--;
        //                if (data == 10 && crecv > 2 && buff[crecv - 2] == 13)
        //                {
        //                    if (buff[0] == 0xa0)
        //                    {
        //                        int msg_len = buff[2];
        //                        msg_len = msg_len << 8 | buff[3];
        //                        if (crecv == msg_len + 7)
        //                            return crecv;
        //                    }
        //                    else
        //                    {
        //                        //Debug.Print(new string(Encoding.ASCII.GetChars(buff, 0, crecv)));
        //                        return crecv;
        //                    }
        //                }
        //            }
        //            Thread.Sleep(10);
        //        }
        //        return crecv;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }
        //    return 0;
        //}

        private int ReadBinLine(ref byte[] received, int timeout)
        {
            byte buffer;
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            int index = 0;
            int packetLen = 0;
            received[0] = 0;
            while (sw.ElapsedMilliseconds < timeout)
            {
                if (serial.BytesToRead > 0)
                {
                    buffer = (byte)serial.ReadByte();
                    if ((index == 0 && buffer == 0x55) || received[0] == 0x55)
                    {   //從收到0x55開始儲存
                        if (index >= received.Length)
                        {   //儲存不下就傳回Timeout
                            return index;
                        }
                        received[index] = buffer;
                        if (index == 2)
                        {
                            packetLen = received[2];
                        }
                        index++;
                        //if (buffer == 0x0A && received[index - 2] == 0x0D)
                        if (packetLen == index)
                        {   //收到packetLen長度後結束
                            return index;
                        }
                    }
                    else
                    {   //捨棄非A0開頭的資料
                        continue;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            return index;
        }
        
        public int ReadMacAddress(ref byte[] received, int timeout)
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            int index = 0;
            while (sw.ElapsedMilliseconds < timeout)
            {
                if (serial.BytesToRead > 0)
                {
                    received[index++] = (byte)serial.ReadByte();
                    if (index >= received.Length || index == 12)
                    {   //received full
                        Console.WriteLine(String.Format("A{0},{1},{2}", index, timeout, sw.ElapsedMilliseconds));
                        return index;
                    }
                    if (index == 1 && ((timeout - sw.ElapsedMilliseconds) < 800))
                    {
                        timeout += 800;
                        Console.WriteLine(String.Format("B{0},{1},{2}", index, timeout, sw.ElapsedMilliseconds));
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            if(index != 0)
                Console.WriteLine(String.Format("C{0},{1},{2}", index, timeout, sw.ElapsedMilliseconds));
            return index;
        }

        private PROVIEWS_RESPONSE WaitAck(byte id, int timeout)
        {
            //int timeout = 2000;
            const int ReceiveLength = 128;
            byte[] received = new byte[ReceiveLength];
            byte[] buffer = new byte[1];

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds < timeout)
            {
                int l = ReadBinLine(ref received, timeout);
                if (l >= 8)
                {   //最小的Ack封包會有8 bytes
                    if (received[1] == id && received[2] == 0x08 && received[3] == 0x10)
                    {
                        return PROVIEWS_RESPONSE.ACK_OK;
                    }
                    else if (received[1] == id && received[2] == 0x08 && received[3] == 0x20)
                    {
                        long spend = sw.ElapsedMilliseconds;
                        return PROVIEWS_RESPONSE.ACK_FAIL;
                    }

                    Array.Clear(received, 0, received.Length);
                    continue;
                }
            }
            return PROVIEWS_RESPONSE.TIMEOUT;
        }

        public void ClearQueue()
        {
            serial.DiscardInBuffer();
            //serial.DiscardOutBuffer();
        }

        private PROVIEWS_RESPONSE SendCmdAck(byte[] cmd, int len, int timeout)
        {
            ClearQueue();
            serial.Write(cmd, 0, len);
            return WaitAck(cmd[1], timeout);
        }
        private PROVIEWS_RESPONSE SendCmdReturn(byte[] cmd, int len, int timeout, ref byte[] ret)
        {
            ClearQueue();
            serial.Write(cmd, 0, len);
            return WaitReturnCommand(cmd[1], ref ret, timeout);
        }

        public void SendDataNoWait(byte[] cmd, int len)
        {
            //ClearQueue();
            serial.Write(cmd, 0, len);
            //return WaitAck(cmd[4]);
        }

        private PROVIEWS_RESPONSE WaitReturnCommand(byte cmdId, ref byte[] retCmd, int timeout)
        {
            PROVIEWS_RESPONSE retval = PROVIEWS_RESPONSE.TIMEOUT;
            //byte timeout = 10;
            byte[] received = new byte[128];
            //int timeout = 1000;

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds < timeout)
            {
                int l = ReadBinLine(ref received, timeout);
                if (received[1] == cmdId && received[3] == 0x04)
                {
                    received.CopyTo(retCmd, 0);
                    return PROVIEWS_RESPONSE.RETURN;
                }
            }
            return retval;
        }

        public PROVIEWS_RESPONSE GetSerialNumber(int timeout, ref byte[] serialNumber)
        {
            PROVIEWS_RESPONSE retval = PROVIEWS_RESPONSE.NONE;
            byte[] cmdData = new byte[8];
            cmdData[0] = 0x55;
            cmdData[1] = 0x41;
            cmdData[2] = 0x08;
            cmdData[3] = 0x02;

            ProViewSCommand cmd = new ProViewSCommand(cmdData);
            byte[] retData = new byte[128];

            retval = SendCmdReturn(cmd.GetBuffer(), cmd.Size(), timeout, ref retData);
            if (retval == PROVIEWS_RESPONSE.RETURN)
            {
                Buffer.BlockCopy(retData, 4, serialNumber, 0, 32);
            }

            return retval;
        }

        public PROVIEWS_RESPONSE SetSerialNumber(int timeout, byte[] serialNumber)
        {
            PROVIEWS_RESPONSE retval = PROVIEWS_RESPONSE.NONE;
            byte[] cmdData = new byte[0x28];
            cmdData[0] = 0x55;
            cmdData[1] = 0x41;
            cmdData[2] = 0x28;
            cmdData[3] = 0x01;

            Buffer.BlockCopy(serialNumber, 0, cmdData, 4, 32);

            ProViewSCommand cmd = new ProViewSCommand(cmdData);
            byte[] retData = new byte[128];

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            return retval;
        }

        public PROVIEWS_RESPONSE SetCalibration(int timeout)
        {
            PROVIEWS_RESPONSE retval = PROVIEWS_RESPONSE.NONE;
            byte[] cmdData = new byte[8];
            cmdData[0] = 0x55;
            cmdData[1] = 0x32;
            cmdData[2] = 0x08;
            cmdData[3] = 0x01;

            ProViewSCommand cmd = new ProViewSCommand(cmdData);
            byte[] retData = new byte[128];

            retval = SendCmdAck(cmd.GetBuffer(), cmd.Size(), timeout);
            return retval;
        }

        #endregion
    }

}

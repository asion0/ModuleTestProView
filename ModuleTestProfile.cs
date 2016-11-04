using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;

namespace ModuleTestProView
{
    public class ModuleTestProfileBase
    {
        protected const int MaxReadLength = 128;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        protected static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        protected static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

    }

    public class ModuleTestProfile : ModuleTestProfileBase
    {
        public static ModuleTestProfile proViewTestProfile = new ModuleTestProfile(Environment.CurrentDirectory + "\\ProViewSetting.ini");

        //Device
        public enum FunctionType
        {
            Production = 1,
            TestOnly = 2,
            Calibration = 3,
            SnReader = 4,
        }

        public FunctionType functionType = FunctionType.Production;
        public int testModeDuration = 1000;
        public int delayBeforeTestIo = 1000;
        public UInt32 ioTestHighPassPins = 0x7000;

        //public bool doesNotWriteSN = false;

        public ModuleTestProfile()
        {
        }

        public ModuleTestProfile(ModuleTestProfile r)
        {
            functionType = r.functionType;
            testModeDuration = r.testModeDuration;
            ioTestHighPassPins = r.ioTestHighPassPins;
            delayBeforeTestIo = r.delayBeforeTestIo;
        }

        public ModuleTestProfile(String s)
        {
            LoadFromIniFile(s);
        }

        public bool LoadFromIniFile(String path)
        {
            StringBuilder sb = new StringBuilder(MaxReadLength);
            GetPrivateProfileString("FunctionType", "Type", 1.ToString(), sb, MaxReadLength, path);
            functionType = (FunctionType)Convert.ToInt32(sb.ToString());

            GetPrivateProfileString("Setting", "DelayBeforeTestIo", 1000.ToString(), sb, MaxReadLength, path);
            delayBeforeTestIo = Convert.ToInt32(sb.ToString());
            
            GetPrivateProfileString("Setting", "TestModeDuration", 1000.ToString(), sb, MaxReadLength, path);
            testModeDuration = Convert.ToInt32(sb.ToString());

            GetPrivateProfileString("Setting", "IoTestHighPassPins", (0x7000).ToString(), sb, MaxReadLength, path);
            ioTestHighPassPins = Convert.ToUInt32(sb.ToString());
            return true;
        }
    }
    
    public class CountingProfile : ModuleTestProfileBase
    {
        public static CountingProfile countingProfile = null;

        //Device
        private int counter = 1;

        public CountingProfile()
        {
        }

        public CountingProfile(CountingProfile r)
        {
            counter = r.counter;
        }

        public CountingProfile(String folder, String file)
        {
            LoadFromIniFile(folder, file);
        }

        //new CountingProfile(Environment.CurrentDirectory + "\\ProViewSetting.ini")
        private String countingFile = "";
        private String backupCountingFile = "";

        public bool LoadFromIniFile(String folder, String fileName)
        {
            countingFile = String.Format("{0}\\Log", Login.loginInfo.currentPath);
            if (!Directory.Exists(countingFile))
            {
                Directory.CreateDirectory(countingFile);
            }

            countingFile = String.Format("{0}\\Log\\{1}", Login.loginInfo.currentPath, folder);
            if (!Directory.Exists(countingFile))
            {
                Directory.CreateDirectory(countingFile);
            }
            countingFile += "\\" + fileName;

            backupCountingFile = String.Format("{0}{1}", Login.loginInfo.backupDrive.Name, folder);
            if (!Directory.Exists(backupCountingFile))
            {
                Directory.CreateDirectory(backupCountingFile);
            }
            backupCountingFile += "\\" + fileName;

            StringBuilder sb = new StringBuilder(MaxReadLength);
            GetPrivateProfileString("Setting", "Counter", 0.ToString(), sb, MaxReadLength, countingFile);
            counter = Convert.ToInt32(sb.ToString());

            return true;
        }

        public bool WriteIniToFile()
        {
            StringBuilder sb = new StringBuilder(MaxReadLength);
            long r = WritePrivateProfileString("Setting", "Counter", counter.ToString(), countingFile);
            if (r != 1)
                return false;
            r = WritePrivateProfileString("Setting", "Counter", counter.ToString(), backupCountingFile);
            return (r == 1);
        }

        public int GetCounter()
        {
            return counter;
        }

        public bool IncreaseCounter()
        {
            ++counter;
            return WriteIniToFile();
        }
    }

}

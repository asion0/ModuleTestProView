using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace ModuleTestProView
{
    class Global
    {
        public static void InjectionBaudRate(ComboBox c)
        {
            c.Items.AddRange(new object[] {
                4800, 9600, 19200, 38400, 57600,
                115200, 230400, 460800, 921600 });
        }

        public static int GetTextBoxPositiveInt(TextBox t)
        {
            int value = 0;
            try
            {
                value = Convert.ToInt32(t.Text);
                t.ForeColor = (value > 0) ? Color.Black : Color.Red;
            }
            catch
            {
                t.ForeColor = Color.Red;
            }
            return value;
        }

        //public enum FunctionType
        //{
        //    ModuleTest,
        //    ResetTester,
        //    OpenPortTester,
        //    iCacheTester
        //}

        //public static FunctionType functionType = FunctionType.ModuleTest;
        //public static void Init()
        //{
        //    functionType = FunctionType.ModuleTest;
        //}
    }
}

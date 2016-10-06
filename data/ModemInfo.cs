// FIRST PR CHANGE

using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Security.Permissions;
using System.Security.AccessControl;

namespace ModemSpy
{
    public class ModemInfo
    {
        const int MAX_ENUM_COUNT = 255;

        private int _modemIndex = 0;
        string _modemRegistrySection;

        public ModemInfo() : this(0)
        {
        }

        public ModemInfo(int index)
        {
            _modemIndex = index;
            _modemRegistrySection = GetModemRegistrySection(_modemIndex);
        }

        public ModemInfo(string modelName)
        {
            _modemIndex = GetModemIndex(modelName);
            _modemRegistrySection = GetModemRegistrySection(_modemIndex);
        }


        public int ModemIndex
        {
            get { return _modemIndex; }
        }

        public string Model
        {
            get 
            {
                return GetProfileString(_modemRegistrySection, "Model", "");
            }
        }

        public string FriendlyName
        {
            get 
            {
                return GetProfileString(_modemRegistrySection, "FriendlyName", "");
            }
        }

        public string Manufacturer
        {
            get 
            {
                return GetProfileString(_modemRegistrySection, "Manufacturer", "");
            }
        }

        public string DriverDescription
        {
            get
            {
                return GetProfileString(_modemRegistrySection, "DriverDesc", "");
            }
        }

        public string LoggingPath
        {
            get
            {
                return GetProfileString(_modemRegistrySection, "LoggingPath", "");
            }
        }

        public string UserInit
        {
            get
            {
                return GetProfileString(_modemRegistrySection, "UserInit", "");
            }
            set
            {
                try
                {
                    SetProfileString(_modemRegistrySection, "UserInit", value);
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }

        public int ComPort
        {
            get
            {
                return GetModemComPortByIndex(ModemIndex);
            }
        }

        public int MaximumPortSpeed
        {
            get
            {
                return GetProfileInteger(_modemRegistrySection, "MaximumPortSpeed", 0);
            }
        }

        public override string ToString()
        {
            return this.Model;
        }

        public static ReadOnlyCollection<ModemInfo> GetModems()
        {
            List<ModemInfo> _modems = new List<ModemInfo>();

            for (int count = 0; count < MAX_ENUM_COUNT; count++)
            {
                string name = GetModemName(count);
                if (!string.IsNullOrEmpty(name) && !name.Contains("IP Virtual Modem"))
                    _modems.Add(new ModemInfo(count));
            }

            return new ReadOnlyCollection<ModemInfo>(_modems);
        }


        private static int GetModemCount()
        {
            int result = 0;
            for (int count = 0; count < MAX_ENUM_COUNT; count++)
            {
                string name = GetModemName(count);
                if (!string.IsNullOrEmpty(name) && !name.Contains("IP Virtual Modem")) result++;
            }
            return result;
        }

        private static string GetProfileString(string section, string entry, string defValue)
        {
            string strValue = defValue;

            RegistryKey sectionKey = Registry.LocalMachine.OpenSubKey(section, false);

            if (sectionKey != null)
            {
               strValue = (string)sectionKey.GetValue(entry, defValue);
            } 

            return strValue;  
        }

        private static int GetProfileInteger(string section, string entry, int defValue)
        {
            int intValue = defValue;

            RegistryKey sectionKey = Registry.LocalMachine.OpenSubKey(section, false);

            if (sectionKey != null)
            {
                intValue = (int)sectionKey.GetValue(entry, defValue);
            }

            return intValue;
        }

        private static void SetProfileString(string section, string entry, string setValue)
        {
            try
            {
                RegistryKey sectionKey = Registry.LocalMachine.OpenSubKey(section, true);

                if (sectionKey != null)                
                    sectionKey.SetValue(entry, setValue);                
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private static string GetModemName(int index)
        {
            return GetProfileString(GetModemRegistrySection(index), "Model", "");
        }

        private static string ModemClassGuid
        {
            get { return "{4D36E96D-E325-11CE-BFC1-08002BE10318}"; }
        }

        private static int GetModemIndex(string modelName)
        {
            int result = -1;

            if (!String.IsNullOrEmpty(modelName))
            {
                modelName = modelName.ToUpper();

                for (int count = 0; count < MAX_ENUM_COUNT; count++)
                {
                    string strGetModemName = GetModemName(count);
                    strGetModemName = strGetModemName.ToUpper();
                    if (modelName == strGetModemName && GetModemComPortByIndex(count) > 0)
                    {
                        result = count;
                        break;
                    }
                }
            }

            return result;
        }

        private static int GetModemComPortByIndex(int index)
        {
            int result = 0;

            string port = GetProfileString(GetModemRegistrySection(index), "AttachedTo", "");
            if (! string.IsNullOrEmpty(port))
            {
                result = Convert.ToInt16(port.Substring(3));
            }
            else
            {
                string key = FindKey("Enum", "Driver", "Modem\\" + GetPaddedIndex(index));
                if (!string.IsNullOrEmpty(key))
                {
                    port = GetProfileString(key, "PORTNAME", "");
                    if (! string.IsNullOrEmpty(port) )                   
                        result = Convert.ToInt16(port.Substring(3));                    
                }
            }

            return result;
        }

        private static int GetModemComPort(string modelName)
        {
            return GetModemComPortByIndex( GetModemIndex( modelName ) );
        }

        private static string GetPaddedIndex(int index)
        {
            string strIndex = index.ToString();
            return strIndex.PadLeft(4, '0');
        }

        private static string GetModemRegistrySection(int index)
        {
            string result;

            if (System.Environment.OSVersion.Platform == System.PlatformID.Win32Windows)
                result = "SYSTEM\\CurrentControlSet\\Services\\Class\\Modem\\" + GetPaddedIndex(index);
            else
                result = "SYSTEM\\CurrentControlSet\\Control\\Class\\" + ModemClassGuid + "\\" + GetPaddedIndex(index);

            return result;
        }

        private static string FindKey(string searchRoot, string key, string sValue)
        {
            string result = null;

            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(searchRoot, false);
            string valueFound = (string)regkey.GetValue(key);

            valueFound = valueFound.ToUpper();
            string valueWanted = sValue.ToUpper();

            if (valueFound == valueWanted)
            {
                result = searchRoot;
            }
            else
            {
                string[] keys = regkey.GetValueNames();                
                
                for (int count = 0; count < keys.Length-1 && string.IsNullOrEmpty(result); count++)
                {
                    string strKey = searchRoot + "\\" + keys[count];
                    string[] subKeys = regkey.GetValueNames();
                    if (subKeys.Length > 0)
                        result = FindKey(strKey, key, sValue);
                }
            }

            return result;
        }
    }
}

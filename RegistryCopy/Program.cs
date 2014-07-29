using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApplication1
{
    static class Program
    {
        static bool ContainsName(RegistryKey key)
        {
            string [] valueNames = key.GetValueNames();
            foreach (string valueName in valueNames)
            {
              if (valueName.Contains("Inventor") || valueName.Contains("Apprentice"))
                    return true;

                string value = key.GetValue(valueName) as string;
                if (value != null)
                {
                  if (value.Contains("Inventor") || value.Contains("Apprentice"))
                        return true;
                }
            }

            string[] subNames = key.GetSubKeyNames();
            foreach (string subName in subNames)
            {
              if (subName.Contains("Inventor") || subName.Contains("Apprentice"))
                    return true;

                RegistryKey subKey = key.OpenSubKey(subName);
                if (ContainsName(subKey))
                    return true;
            }

            return false;
        }

        static void CopyKey(RegistryKey hkcu, RegistryKey hklm)
        {
            string [] valueNames = hkcu.GetValueNames();
            foreach (string valueName in valueNames)
            {
                object value = hkcu.GetValue(valueName);
                hklm.SetValue(valueName, value); 
            }

            string[] subNames = hkcu.GetSubKeyNames();
            foreach (string subName in subNames)
            {
              try
              {
                RegistryKey hklmSubkey = hklm.CreateSubKey(subName);
                CopyKey(hkcu.OpenSubKey(subName), hklmSubkey);
              }
              catch (Exception ex)
              {
                Console.WriteLine("{0}\\{1}: {2}",
                  hklm.ToString(),
                  subName,
                  ex.Message);
              }
            }
        }

        static void IterateKeys(RegistryKey hkcu, RegistryKey hklm)
        {
            string[] subNames = hkcu.GetSubKeyNames();
            foreach (string subName in subNames)
            {
                if (subName == "CLSID" || subName == "Interface" || subName == "TypeLib")
                {
                    IterateKeys(hkcu.OpenSubKey(subName), hklm.OpenSubKey(subName, true));  
                }
                else
                {
                    RegistryKey subKey = hkcu.OpenSubKey(subName);
                    if (ContainsName(subKey))
                    {
                        try
                        {
                          RegistryKey hklmSubkey = hklm.CreateSubKey(subName);
                          CopyKey(subKey, hklmSubkey); 
                        }
                        catch (Exception ex)
                        {
                          Console.WriteLine("{0}\\{1}: {2}",
                            hklm.ToString(),
                            subName,
                            ex.Message); 
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            RegistryKey hkcu = Registry.CurrentUser.OpenSubKey("Software\\Classes");
            RegistryKey hklm = Registry.LocalMachine.OpenSubKey("Software\\Classes", true);

            IterateKeys(hkcu, hklm);

            // Wait for user to read everything
            Console.ReadKey();
        }
    }
}

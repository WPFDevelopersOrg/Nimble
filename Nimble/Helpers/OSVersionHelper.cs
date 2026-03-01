using Microsoft.Win32;

namespace Nimble.Helpers
{
    public static class OSVersionHelper
    {
        public static bool IsWindows11()
        {
            try
            {
                string registryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        object buildValue = key.GetValue("CurrentBuild");

                        if (buildValue != null)
                        {
                            int buildNumber;
                            if (int.TryParse(buildValue.ToString(), out buildNumber))
                            {
                                if (buildNumber >= 22000)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }
}

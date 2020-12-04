using System;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace SoftWareHelper.Helpers
{
    /// <summary>
    /// Themes 帮助类
    /// </summary>
    public class ThemesHelper
    {
        /// <summary>
        /// 切换Themes
        /// </summary>
        /// <param name="isDark">true:Dark false:light</param>
        public static void SetLightDark(bool isDark)
        {

            try
            {
                var existingResourceDictionary = Application.Current.Resources.MergedDictionaries
                                                    .Where(rd => rd.Source != null)
                                                    .SingleOrDefault(rd => rd.Source.OriginalString.Contains("Light") || rd.Source.OriginalString.Contains("Dark"));
                var source = $"pack://application:,,,/SoftWareHelper;component/Themes/{(isDark ? "Dark" : "Light")}.xaml";
                var newResourceDictionary = new ResourceDictionary() { Source = new Uri(source) };
                App.Current.Resources.MergedDictionaries.Remove(existingResourceDictionary);
                App.Current.Resources.MergedDictionaries.Add(newResourceDictionary);
                //节点
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Dark"].Value = isDark.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Log.Error($"MainView.SetLightDark Error：{ex.Message}");
            }

        }

        public static bool GetLightDark()
        {
            bool dark;
            if (!bool.TryParse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Dark"]) ? "false" : ConfigurationManager.AppSettings["Dark"], out dark))
            {
                dark = false;
            }
            else
            {
                dark = Convert.ToBoolean(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Dark"]) ? "false" : ConfigurationManager.AppSettings["Dark"]);
            }
            return dark;
        }

       
    }
}

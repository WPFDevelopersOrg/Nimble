using System;
using System.Configuration;

namespace SoftWareHelper.Helpers
{
    /// <summary>
    /// Config帮助类
    /// </summary>
    public class ConfigHelper
    {
        public static bool EdgeHide { get; set; }
        public static double Opacity { get; set; }
        public static bool CustomJson { get;private set; }
        public static void GetConfigHelper()
        {
            //Opacity = GetOpacity();
            //EdgeHide = GetEdgeHide();
            double opacity = 80;
            if (!double.TryParse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Opacity"]) ? "80" : ConfigurationManager.AppSettings["Opacity"], out opacity))
            {
                Opacity = 80;
            }
            else
            {
                Opacity = Convert.ToDouble(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Opacity"]) ? "80" : ConfigurationManager.AppSettings["Opacity"]);
            }
            bool edgeHide;
            if (!bool.TryParse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EdgeHide"]) ? "false" : ConfigurationManager.AppSettings["EdgeHide"], out edgeHide))
            {
                EdgeHide = false;
            }
            else
            {
                EdgeHide = Convert.ToBoolean(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EdgeHide"]) ? "false" : ConfigurationManager.AppSettings["EdgeHide"]);
            }
            bool customJson;
            if (!bool.TryParse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CustomJson"]) ? "false" : ConfigurationManager.AppSettings["CustomJson"], out customJson))
            {
                CustomJson = false;
            }
            else
            {
                CustomJson = Convert.ToBoolean(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CustomJson"]) ? "false" : ConfigurationManager.AppSettings["CustomJson"]);
            }
        }
        double GetOpacity()
        {
            double opacity = 80;
            if (!double.TryParse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Opacity"]) ? "80" : ConfigurationManager.AppSettings["Opacity"], out opacity))
            {
                opacity = 80;
            }
            else
            {
                opacity = Convert.ToDouble(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Opacity"]) ? "80" : ConfigurationManager.AppSettings["Opacity"]);
            }
            return opacity;
        }
        public static void SaveOpacity(double opacity)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Opacity"].Value = opacity.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        bool GetEdgeHide()
        {
            bool edgeHide;
            if (!bool.TryParse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EdgeHide"]) ? "false" : ConfigurationManager.AppSettings["EdgeHide"], out edgeHide))
            {
                edgeHide = false;
            }
            else
            {
                edgeHide = Convert.ToBoolean(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EdgeHide"]) ? "false" : ConfigurationManager.AppSettings["EdgeHide"]);
            }
            return edgeHide;
        }
        public static void SaveEdgeHide(bool edgeHide)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["EdgeHide"].Value = edgeHide.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}

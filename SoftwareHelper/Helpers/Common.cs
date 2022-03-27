using Microsoft.Win32;
using Newtonsoft.Json;
using SoftwareHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftwareHelper.Helpers
{
    public partial class Common
    {
        /// <summary>
        /// 路径
        /// </summary>
        public static string temporaryFile { get; set; }
        /// <summary>
        /// 存放路径图标
        /// </summary>
        public static string temporaryIconFile { get; set; }
        /// <summary>
        /// 应用程序json
        /// </summary>
        public static string temporaryApplicationJson { get; set; }
        /// <summary>
        /// 默认图标
        /// </summary>
        public static string ApplicationIcon { get; set; }
        /// <summary>
        /// 维护不可见项目
        /// </summary>
        public static List<string> SystemApplication { get; set; }
        /// <summary>
        /// 判断是否是Win10
        /// </summary>
        public static bool IsWin10 => Environment.OSVersion.Version.Major == 10;

        /// <summary>
        /// 缓存
        /// </summary>
        public static ObservableCollection<ApplicationModel> ApplicationListCache;

        public Common()
        {
            SystemApplication = new List<string>()
            {
                "Microsoft",
                "Installer",
                "Microsoft Visual Studio Installer",
                "AMD Catalyst Control Center",
                "语言包",
                "Tools for Office"
            };
        }

        #region 判断当前是否安装过此软件
        /// <summary>
        /// 判断当前是否安装过此软件
        /// </summary>
        /// <param name="p_name"></param>
        /// <returns></returns>
        public static bool IsApplictionInstalled(string p_name)
        {
            string displayName;
            RegistryKey key;

            // 当前用户
            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                Console.WriteLine($"当前用户：{displayName}");
                if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }

            // X86
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                Console.WriteLine($"当前用户32位：{displayName}");
                if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }

            // X64
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                Console.WriteLine($"当前用户64位：{displayName}");
                if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region 获取安装的软件
        //[RegistryPermissionAttribute(SecurityAction.PermitOnly, Read = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")]
        //[RegistryPermissionAttribute(SecurityAction.PermitOnly, Read = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")]
        //[RegistryPermissionAttribute(SecurityAction.PermitOnly, Read = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall")]
        public static ObservableCollection<ApplicationModel> AllApplictionInstalled()
        {
            ObservableCollection<ApplicationModel> applictionArray = new ObservableCollection<ApplicationModel>();
            ApplicationModel model;

            #region 临时注释
            //SelectQuery Sq = new SelectQuery("Win32_Product");
            //ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher(Sq);
            //ManagementObjectCollection osDetailsCollection = objOSDetails.Get();
            //foreach (ManagementObject MO in osDetailsCollection)
            //{
            //    model = new ApplicationModel
            //    { 
            //        Name= MO["Name"].ToString(),
            //        //ExePath = MO["InstallLocation"].ToString(),
            //        ExePath = MO["InstallSource"].ToString()
            //    };
            //    applictionArray.Add(model);
            //}
            #endregion


            RegistryKey key;
            // 当前用户
            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subkey = key.OpenSubKey(keyName))
                        {
                            var displayName = subkey.GetValue("DisplayName");
                            if (displayName != null)
                            {

                                if (!SystemApplication.Any(displayName.ToString().Contains))
                                {
                                    var displayPath = subkey.GetValue("DisplayIcon");
                                    if (displayPath != null)
                                    {
                                        model = new ApplicationModel
                                        {
                                            Name = displayName.ToString(),
                                            ExePath = displayPath.ToString(),
                                            IconPath = ApplicationIcon
                                        };
                                        FormModel(model, applictionArray);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }


            // X86
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subkey = key.OpenSubKey(keyName))
                        {

                            var displayName = subkey.GetValue("DisplayName");
                            if (displayName != null)
                            {
                                if (!SystemApplication.Any(displayName.ToString().Contains))
                                {
                                    var displayPath = subkey.GetValue("DisplayIcon");

                                    if (displayPath != null)
                                    {
                                        model = new ApplicationModel
                                        {
                                            Name = displayName.ToString(),
                                            ExePath = displayPath.ToString(),
                                            IconPath = ApplicationIcon
                                        };
                                        FormModel(model, applictionArray);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                   
                }
            }


            // X64
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subkey = key.OpenSubKey(keyName))
                        {
                            var displayName = subkey.GetValue("DisplayName");
                            if (displayName != null)
                            {
                                if (!SystemApplication.Any(displayName.ToString().Contains))
                                {
                                    if (displayName.ToString() == "腾讯QQ")
                                    {
                                        var installLocation = subkey.GetValue("InstallLocation");
                                        if (installLocation != null)
                                        {
                                            if (installLocation != null)
                                            {
                                                model = new ApplicationModel
                                                {
                                                    Name = displayName.ToString(),
                                                    ExePath = Path.Combine(installLocation.ToString(), "Bin/QQ.exe"),
                                                    IconPath = ApplicationIcon
                                                };

                                                FormModel(model, applictionArray);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var displayPath = subkey.GetValue("DisplayIcon");

                                        if (displayPath != null)
                                        {
                                            model = new ApplicationModel
                                            {
                                                Name = displayName.ToString(),
                                                ExePath = displayPath.ToString(),
                                                IconPath = ApplicationIcon
                                            };
                                            FormModel(model, applictionArray);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                   
                }
            }


            return applictionArray;
        }
        #endregion

        #region 读取桌面
        public static void GetDesktopAppliction(ObservableCollection<ApplicationModel> array)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            GetCommonDesktoplnk(desktop, array);
            string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            GetCommonDesktoplnk(commonDesktop, array);
            var systemAppliction = new string[] { "mspaint.exe", "mstsc.exe", "magnify.exe", "control.exe", "SnippingTool.exe", "calc.exe", "notepad.exe", "cmd.exe" };
            GetSystemAppliction(systemAppliction, array);
            //string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            //var systemAppliction = new string[] { "mspaint.exe", "mstsc.exe", "magnify.exe", "calc.exe", "notepad.exe", "cmd.exe" };
            //Action<string, ObservableCollection<ApplicationModel>> actionGetCommonDesktop = new Action<string, ObservableCollection<ApplicationModel>>(GetCommonDesktoplnk);
            //Action<string[], ObservableCollection<ApplicationModel>> actionGetSystem = new Action<string[], ObservableCollection<ApplicationModel>>(GetSystemAppliction);
            //actionGetCommonDesktop.BeginInvoke(commonDesktop, array, a => actionGetCommonDesktop.EndInvoke(a),null);
            //actionGetSystem.BeginInvoke(systemAppliction, array,a=>actionGetSystem.EndInvoke(a),null);
        }
        #region 系统
        static void GetSystemAppliction(string[] apps, ObservableCollection<ApplicationModel> array)
        {
            ApplicationModel model;
            foreach (var item in apps)
            {
                var appliction = Path.Combine(Environment.SystemDirectory, item);
                if (System.IO.File.Exists(appliction))
                {

                    FileVersionInfo fileVersion =
        FileVersionInfo.GetVersionInfo(appliction);
                    model = new ApplicationModel();
                    model.ExePath = appliction;
                    model.Name = fileVersion.FileDescription;
                    var iconPath = System.IO.Path.Combine(temporaryIconFile, model.Name);
                    iconPath = iconPath + ".png";
                    model.IconPath = iconPath;
                    string first = model.Name.Substring(0, 1);
                    if (!IsChinese(first))
                    {
                        if (char.IsUpper(first.ToCharArray()[0]))
                            model.Group = first;
                        model.Group = model.Name.Substring(0, 1).ToUpper();
                    }
                    else
                    {
                        model.Group = GetCharSpellCode(first);
                    }
                    array.Insert(0, model);
                    if (File.Exists(iconPath))
                        continue;
                    SaveImage((BitmapSource)GetIcon(appliction), iconPath);
                }
            }
        }
        #endregion

        #region 桌面
        static void GetCommonDesktoplnk(string _path, ObservableCollection<ApplicationModel> array)
        {
            ApplicationModel model;
            foreach (var lnk in Directory.GetFiles(_path))
            {
                if (Path.GetExtension(lnk) == ".lnk")
                {
                    if (System.IO.File.Exists(lnk))
                    {
                        var name = Path.GetFileNameWithoutExtension(lnk);
                        if (name == System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName))
                            continue;
                        var containsValue = array.Where(x => x.Name.Contains(name) && !x.Name.Contains("微信"));
                        if (containsValue != null && containsValue.Count() > 0)
                            continue;
                        model = new ApplicationModel();
                        model.ExePath = lnk;
                        model.Name = name;
                        var iconPath = System.IO.Path.Combine(temporaryIconFile, model.Name);
                        iconPath = iconPath + ".png";
                        model.IconPath = iconPath;
                        string first = model.Name.Substring(0, 1);
                        if (!IsChinese(first))
                        {
                            if (char.IsUpper(first.ToCharArray()[0]))
                                model.Group = first;
                            model.Group = model.Name.Substring(0, 1).ToUpper();
                        }
                        else
                        {
                            model.Group = GetCharSpellCode(first);
                        }
                        array.Insert(0, model);
                        if (File.Exists(iconPath))
                            continue;
                        SaveImage((BitmapSource)GetIcon(lnk), iconPath);
                    }
                }
            }
        }
        #endregion

        #endregion

        #region 文件夹
        public static void TemporaryFile()
        {
            try
            {
                temporaryFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                if (!System.IO.Directory.Exists(temporaryFile))
                {
                    System.IO.Directory.CreateDirectory(temporaryFile);
                }
                temporaryIconFile = System.IO.Path.Combine(temporaryFile, "Icon");
                if (!System.IO.Directory.Exists(temporaryIconFile))
                {
                    System.IO.Directory.CreateDirectory(temporaryIconFile);
                }
                temporaryApplicationJson = Path.Combine(temporaryFile, "application.json");
                ApplicationIcon = Path.Combine(temporaryIconFile, "ApplicationIcon.png");
                if (!File.Exists(ApplicationIcon))
                {
                    SaveImage((BitmapSource)GetSystemIcon(), ApplicationIcon);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("创建文件失败");
            }
        }
        #endregion


        #region 获取exe的icon 
        public static ImageSource GetSystemIcon()
        {
            System.Drawing.Icon icon = System.Drawing.SystemIcons.Application;
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
        }

        public static ImageSource GetIcon(string fileName)
        {

            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        new Int32Rect(0, 0, icon.Width, icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
        }
        public static void SaveImage(BitmapSource bitmap, string savePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
                encoder.Save(stream);
        }
        public static void IconCovertPng(string fileIco, string filePng)
        {
            //if (File.Exists(fileIco))
            //{
            //    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileIco);
            //    ImageSource source =  System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
            //            icon.Handle,
            //            new Int32Rect(0, 0, icon.Width, icon.Height),
            //            BitmapSizeOptions.FromEmptyOptions());
            //    var encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)source));
            //    using (FileStream stream = new FileStream(filePng, FileMode.Create))
            //        encoder.Save(stream);
            //}
            new System.Drawing.Icon(fileIco).ToBitmap().Save(filePng, ImageFormat.Png);//这里还可以转换成其他类型图片，详情ImageFormat
        }
        #endregion

        #region 重组model到集合
        static void FormModel(ApplicationModel model, ObservableCollection<ApplicationModel> applictionArray)
        {
            string first = model.Name.Substring(0, 1);
            if (!IsChinese(first))
            {
                if (char.IsUpper(first.ToCharArray()[0]))
                    model.Group = first;
                model.Group = model.Name.Substring(0, 1).ToUpper();
            }
            else
            {
                model.Group = GetCharSpellCode(first);
            }

            var uninsta = System.IO.Path.GetFileNameWithoutExtension(model.ExePath);
            var own = System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (uninsta.ToLower().Contains("uninstall") || uninsta.Contains("卸载") || uninsta.Contains("unins000") || uninsta.Contains("own"))
                return;
            model.ExePath = model.ExePath.Trim('"');
            var exe = Path.GetExtension(model.ExePath);
            var iconPath = System.IO.Path.Combine(temporaryIconFile, model.Name);


            switch (exe)
            {
                case ".exe":
                    iconPath = iconPath + ".png";
                    model.IconPath = iconPath;
                    applictionArray.Add(model);
                    if (File.Exists(iconPath))
                        break;
                    SaveImage((BitmapSource)GetIcon(model.ExePath), iconPath);
                    break;
                //case ".ico":
                //    iconPath = Path.Combine(Path.GetDirectoryName(model.IconPath), Path.GetFileNameWithoutExtension(model.ExePath) + ".png");
                //    SaveImage((BitmapSource)GetIcon(model.ExePath), iconPath);
                //    model.IconPath = iconPath;
                //    applictionArray.Add(model);
                //    break;
                default:
                    break;
            }
        }
        #endregion

        public static void Init()
        {
            Common.TemporaryFile();
            if(!ConfigHelper.CustomJson)
            {
                Common.ApplicationListCache = Common.AllApplictionInstalled();
                Common.GetDesktopAppliction(Common.ApplicationListCache);
                Common.ApplicationListCache = new ObservableCollection<ApplicationModel>(Common.ApplicationListCache.OrderBy(x => x.Group));
                string json = JsonHelper.Serialize(Common.ApplicationListCache);
                FileHelper.WriteFile(ConvertJsonString(json), Common.temporaryApplicationJson);
            }
            else
            {
                var json = FileHelper.ReadFile(temporaryApplicationJson);
                var applications = JsonHelper.Deserialize<ObservableCollection<ApplicationModel>>(json);
                if (applications == null && applications.Count == 0) return;
                var iconNull = applications.Where(z => string.IsNullOrWhiteSpace(z.IconPath));
                if (iconNull != null && iconNull.Count() > 0)
                {
                    foreach (var item in iconNull)
                    {
                        if (string.IsNullOrWhiteSpace(item.ExePath) && string.IsNullOrWhiteSpace(item.Name)) return;
                        var iconPath = System.IO.Path.Combine(temporaryIconFile, item.Name);
                        iconPath = $"{iconPath}.png";
                        if (applications.Any(z => z.Name == item.Name))
                        {
                            item.IconPath = iconPath;
                            string first = item.Name.Substring(0, 1);
                            if (!IsChinese(first))
                            {
                                if (char.IsUpper(first.ToCharArray()[0]))
                                    item.Group = first;
                                item.Group = item.Name.Substring(0, 1).ToUpper();
                            }
                            else
                            {
                                item.Group = GetCharSpellCode(first);
                            }
                        }
                        if (File.Exists(iconPath))
                            break;
                        SaveImage((BitmapSource)GetIcon(item.ExePath), iconPath);
                    }
                }
                Common.ApplicationListCache = new ObservableCollection<ApplicationModel>(applications.OrderBy(x => x.Group));
                var json1 = JsonHelper.Serialize(Common.ApplicationListCache);
                FileHelper.WriteFile(ConvertJsonString(json1), Common.temporaryApplicationJson);
            }
        }


        #region 格式化json字符串
        static string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        } 
        #endregion

        #region 判断首字母是否为中文,如果为中文 首字母转为英语
        static bool IsChinese(string text)
        {
            bool chinese = false;
            Regex regChina = new Regex("[^\x00-\xFF]");
            Regex regEnglish = new Regex("^[a-zA-Z]");
            if (regEnglish.IsMatch(text))
            {
                chinese = false;
            }
            else if (regChina.IsMatch(text))
            {
                chinese = true;
            }
            return chinese;
        }
        /// <summary>
        /// 得到一个汉字的拼音第一个字母
        /// </summary>
        /// <param name="CnChar">单个汉字</param>
        /// <returns>单个大写字母</returns>

        static string GetCharSpellCode(string CnChar)
        {
            long iCnChar;
            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);
            int i1 = (short)(ZW[0]);
            int i2 = (short)(ZW[1]);
            iCnChar = i1 * 256 + i2;
            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {
                return "A";
            }
            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {
                return "B";
            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {
                return "C";
            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {
                return "D";
            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {
                return "E";
            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {
                return "F";
            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {
                return "G";
            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {
                return "H";
            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {
                return "J";
            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {
                return "K";
            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {
                return "L";
            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {
                return "M";
            }
            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {
                return "N";
            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {
                return "O";
            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {
                return "P";
            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {
                return "Q";
            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {
                return "R";
            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {
                return "S";
            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {
                return "T";
            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {
                return "W";
            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {
                return "X";
            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {
                return "Y";
            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {
                return "Z";
            }
            else
                return ("?");

        }
        #endregion
    }
}

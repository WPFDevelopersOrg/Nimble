using Microsoft.Win32;
using SoftWareHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftWareHelper.Helpers
{
    public class Common
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
            foreach (String keyName in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(keyName))
                {
                    try
                    {
                        var displayName = subkey.GetValue("DisplayName");
                        var displayPath = subkey.GetValue("DisplayIcon");
                        if (displayName != null)
                        {
                            if (displayPath != null)
                            {
                                model = new ApplicationModel
                                {
                                    Name = displayName.ToString(),
                                    ExePath = displayPath.ToString(),
                                    IconPath = ApplicationIcon
                                };
                                model.ExePath = model.ExePath.Trim('"');
                                var exe = Path.GetExtension(model.ExePath);
                                var iconPath = System.IO.Path.Combine(temporaryIconFile, model.Name);

                                switch (exe)
                                {
                                    case ".exe":
                                        iconPath = iconPath + ".png";
                                        SaveImage((BitmapSource)GetIcon(model.ExePath), iconPath);
                                        model.IconPath = iconPath;
                                        break;
                                    case ".ico":
                                        iconPath = Path.Combine(Path.GetDirectoryName(model.ExePath), Path.GetFileNameWithoutExtension(model.ExePath + ".png"));
                                        IconCovertPng(model.ExePath, iconPath);
                                        model.IconPath = iconPath;
                                        break;
                                    default:
                                        break;
                                }
                                applictionArray.Add(model);
                            }


                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }

            // X86
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(keyName))
                {
                    try
                    {
                        var displayName = subkey.GetValue("DisplayName");
                        var displayPath = subkey.GetValue("DisplayIcon");
                        if (displayName != null)
                        {
                            if (displayPath != null)
                            {
                                model = new ApplicationModel
                                {
                                    Name = displayName.ToString(),
                                    ExePath = displayPath.ToString(),
                                    IconPath = ApplicationIcon
                                };
                                model.ExePath = model.ExePath.Trim('"');
                                var exe = Path.GetExtension(model.ExePath);
                                var iconPath = System.IO.Path.Combine(temporaryIconFile, model.Name);

                                switch (exe)
                                {
                                    case ".exe":
                                        iconPath = iconPath + ".png";
                                        SaveImage((BitmapSource)GetIcon(model.ExePath), iconPath);
                                        model.IconPath = iconPath;
                                        break;
                                    case ".ico":
                                        iconPath = Path.Combine(Path.GetDirectoryName(model.ExePath), Path.GetFileNameWithoutExtension(model.ExePath + ".png"));
                                        IconCovertPng(model.ExePath, iconPath);
                                        model.IconPath = iconPath;
                                        break;
                                    default:
                                        break;
                                }
                                applictionArray.Add(model);
                            }


                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }

            // X64
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(keyName))
                {
                    try
                    {
                        var displayName = subkey.GetValue("DisplayName");
                        var displayPath = subkey.GetValue("DisplayIcon");
                        if (displayName != null)
                        {
                            if (displayPath != null)
                            {
                                model = new ApplicationModel
                                {
                                    Name = displayName.ToString(),
                                    ExePath = displayPath.ToString(),
                                    IconPath = ApplicationIcon
                                };
                                model.ExePath = model.ExePath.Trim('"');
                                var exe = Path.GetExtension(model.ExePath);
                                var iconPath = System.IO.Path.Combine(temporaryIconFile, model.Name);

                                switch (exe)
                                {
                                    case ".exe":
                                        iconPath = iconPath + ".png";
                                        SaveImage((BitmapSource)GetIcon(model.ExePath), iconPath);
                                        model.IconPath = iconPath;
                                        break;
                                    case ".ico":
                                        iconPath = Path.Combine(Path.GetDirectoryName(model.ExePath), Path.GetFileNameWithoutExtension(model.ExePath + ".png"));
                                        IconCovertPng(model.ExePath, iconPath);
                                        model.IconPath = iconPath;
                                        break;
                                    default:
                                        break;
                                }
                                applictionArray.Add(model);
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


        #region 文件夹
        public static void TemporaryFile()
        {
            temporaryFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
            if (!System.IO.Directory.Exists(temporaryFile))
            {
                System.IO.Directory.CreateDirectory(temporaryFile);
            }
            temporaryIconFile = System.IO.Path.Combine(temporaryFile,"Icon");
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
        public static void SaveImage(BitmapSource bitmap,string savePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
                encoder.Save(stream);
        }
        public static void IconCovertPng(string fileIco, string filePng)
        {
            new System.Drawing.Icon(fileIco).ToBitmap().Save(filePng, ImageFormat.Png);//这里还可以转换成其他类型图片，详情ImageFormat
        }
        #endregion


    }
}

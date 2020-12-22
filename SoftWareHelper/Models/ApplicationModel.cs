using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftWareHelper.Models
{
    public class ApplicationModel
    {
        public string ID { get; set; }
        public string Group { get; set; }
        /// <summary>
        /// 软件名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 软件图标路径
        /// </summary>
        public string IconPath { get; set; }
        /// <summary>
        /// 安装路径
        /// </summary>
        public string ExePath { get; set; }
        /// <summary>
        /// 是否移除
        /// </summary>
        public bool IsRemove { get; set; }
    }
}

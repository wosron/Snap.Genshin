﻿using System.Windows.Controls;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 尚未完工，请勿引用
    /// 插件功能拓展接口
    /// 支持插件页面的额外详细链接导航
    /// 支持插件页面的右键上下文菜单
    /// </summary>
    internal interface IPlugin2
    {
        public string? DetailLink { get; set; }
        public ContextMenu? ContextMenu { get; set; }
    }
}
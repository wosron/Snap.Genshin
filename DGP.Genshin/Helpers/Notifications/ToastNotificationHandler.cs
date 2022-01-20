﻿using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Uwp.Notifications;
using Snap.Core.Logging;
using Snap.Data.Json;

namespace DGP.Genshin.Helpers.Notifications
{
    internal class ToastNotificationHandler
    {
        /// <summary>
        /// 在后台处理并响应通知
        /// </summary>
        /// <param name="toastArgs"></param>
        internal async void OnActivatedByNotification(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            this.Log(Json.Stringify(toastArgs));
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            if (args.TryGetValue("action", out string value) && value == "update")
            {
                IUpdateService updateService = App.GetService<IUpdateService>();
                if (updateService.Release is not null)
                {
                    await updateService.DownloadAndInstallPackageAsync();
                }
            }
        }
    }
}
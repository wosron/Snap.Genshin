﻿using DGP.Genshin.DataModels.GachaStatistics;
using DGP.Genshin.MiHoYoAPI.Gacha;
using DGP.Genshin.Services.Abstratcions;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.GachaStatistics
{
    /// <summary>
    /// 抽卡记录服务
    /// </summary>
    [Service(typeof(IGachaStatisticService), InjectAs.Transient)]
    public class GachaStatisticService : IGachaStatisticService
    {
        private readonly LocalGachaLogWorker localGachaLogWorker = new();

        public void LoadLocalGachaData(GachaDataCollection gachaData)
        {
            localGachaLogWorker.LoadAll(gachaData);
        }

        public async Task<IGachaLogWorker?> GetGachaLogWorkerAsync(GachaDataCollection gachaData, GachaLogUrlMode mode)
        {
            (_, string? url) = await GachaLogUrlProvider.GetUrlAsync(mode);
            return url is null ? null : (new GachaLogWorker(url, gachaData));
        }

        public async Task<(bool isOk, string? uid)> RefreshAsync(GachaDataCollection gachaData, GachaLogUrlMode mode, Action<FetchProgress> progressCallback, bool full = false)
        {
            (bool isOk, string? url) = await GachaLogUrlProvider.GetUrlAsync(mode);
            if (!isOk)
            {
                return (false, null);
            }
            if (url is null)
            {
                await new ContentDialog()
                {
                    Title = "获取祈愿记录失败",
                    Content = GetUrlFailHintByMode(mode),
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                return (false, null);
            }
            else
            {
                (bool isSuccess, string? uid) = await RefreshInternalAsync(gachaData, mode, progressCallback, full);
                if (!isSuccess)
                {
                    await new ContentDialog()
                    {
                        Title = "获取祈愿配置信息失败",
                        Content = "可能是验证密钥已过期",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
                return (isSuccess, uid);
            }
        }

        private string GetUrlFailHintByMode(GachaLogUrlMode mode)
        {
            return mode switch
            {
                GachaLogUrlMode.GameLogFile => "请在游戏中打开祈愿历史记录页面后尝试刷新",
                GachaLogUrlMode.ManualInput => "请重新输入有效的Url",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 按模式刷新
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>卡池配置是否可用</returns>
        private async Task<(bool isOk, string? uid)> RefreshInternalAsync(GachaDataCollection gachaData, GachaLogUrlMode mode, Action<FetchProgress> progressCallback, bool full = false)
        {
            IGachaLogWorker? worker = await GetGachaLogWorkerAsync(gachaData, mode);
            if (worker is null)
            {
                return (false, null);
            }
            return await FetchGachaLogsAsync(gachaData, worker, progressCallback, full);
        }

        /// <summary>
        /// 获取祈愿记录
        /// </summary>
        /// <param name="worker">工作器对象</param>
        /// <param name="full">是否增量获取</param>
        /// <returns>是否获取成功</returns>
        private async Task<(bool isOk, string? uid)> FetchGachaLogsAsync(GachaDataCollection gachaData, IGachaLogWorker worker, Action<FetchProgress> progressCallback, bool full = false)
        {
            Config? gachaConfigTypes = await worker.GetCurrentGachaConfigAsync();
            if (gachaConfigTypes?.Types != null)
            {
                string? uid = null;
                foreach (ConfigType pool in gachaConfigTypes.Types)
                {
                    if (full)
                    {
                        uid = await worker.FetchGachaLogAggressivelyAsync(pool, progressCallback);
                    }
                    else
                    {
                        uid = await worker.FetchGachaLogIncrementAsync(pool, progressCallback);
                    }
                    if (worker.IsFetchDelayEnabled)
                    {
                        await Task.Delay(worker.GetRandomDelay());
                    }
                }
                localGachaLogWorker!.SaveAll(gachaData);
                return (true, uid);
            }
            else
            {
                return (false, null);
            }
        }

        public async Task<Statistic> GetStatisticAsync(GachaDataCollection gachaData, string uid)
        {
            return await Task.Run(() => new StatisticBuilder().ToStatistic(gachaData[uid]!, uid));
        }

        #region Im/Export
        public async Task ExportDataToExcelAsync(GachaDataCollection gachaData, string uid, string path)
        {
            await Task.Run(() => localGachaLogWorker!.SaveLocalGachaDataToExcel(uid, path, gachaData));
        }

        public async Task ExportDataToJsonAsync(GachaDataCollection gachaData, string uid, string path)
        {
            await Task.Run(() => localGachaLogWorker!.ExportToUIGFJ(uid, path, gachaData));
        }

        public async Task<bool> ImportFromUIGFWAsync(GachaDataCollection gachaData, string path)
        {
            return await Task.Run(() => localGachaLogWorker!.ImportFromUIGFW(path, gachaData));
        }

        public async Task<bool> ImportFromUIGFJAsync(GachaDataCollection gachaData, string path)
        {
            return await Task.Run(() => localGachaLogWorker!.ImportFromUIGFJ(path, gachaData));
        }
        #endregion
    }
}
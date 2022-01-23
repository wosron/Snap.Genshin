﻿using DGP.Genshin.ViewModels.TitleBarButtons;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    public partial class DailyNoteTitleBarButton : TitleBarButton
    {
        public DailyNoteTitleBarButton()
        {
            DataContext = App.GetViewModel<DailyNoteViewModel>();
            InitializeComponent();
        }
    }
}
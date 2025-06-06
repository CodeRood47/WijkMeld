﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace WijkMeld.App.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        //[ObservableProperty]
        //[NotifyCanExecuteChangedFor(
        //    nameof(IsBusy), 
        //    nameof(Title) 
        //)]

        [ObservableProperty]
        private bool isBusy; 

        [ObservableProperty]
        private string title;

        public virtual Task OnAppearingAsync() => Task.CompletedTask;
        public virtual Task OnDisappearingAsync() => Task.CompletedTask;

    }
}
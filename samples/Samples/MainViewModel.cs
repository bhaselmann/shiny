﻿using System;
using System.Windows.Input;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Samples.Notifications;
using Shiny;


namespace Samples
{
    public class MainViewModel : ViewModel
    {
        readonly INavigationService navigator;
        readonly NotificationDelegate? notificationDelegate;


        public MainViewModel(INavigationService navigator, NotificationDelegate? notificationDelegate = null)
        {
            this.navigator = navigator;
            this.notificationDelegate = notificationDelegate;
            this.Navigate = ReactiveCommand.CreateFromTask<string>(async arg =>
            {
                this.IsPresented = false;
                await this.navigator.Navigate(arg);
            });
        }


        public ICommand Navigate { get; }
        [Reactive] public bool IsPresented { get; set; }


        public override async void OnAppearing()
        {
            base.OnAppearing();
            if (this.notificationDelegate != null)
                await this.notificationDelegate.TryNavigateFromNotification(this.navigator);

            //var env = DependencyService.Get<IEnvironment>();
            //Console.WriteLine($"ENV: {env?.ToString() ?? "NO"}");
        }
    }
}

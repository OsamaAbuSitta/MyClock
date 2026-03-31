using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using MyClock.App.ViewModels;
using MyClock.App.Views;
using MyClock.Core.Services;
using MyClock.Infrastructure.Services;
using ReactiveUI;

namespace MyClock.App;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = new SettingsService();
            settingsService.Load();

            var clockService = new ClockService();
            var sessionService = new SessionService(clockService);
            var notificationService = new NotificationService();

            var vm = new MainWindowViewModel(clockService, sessionService, settingsService, notificationService);
            var window = new MainWindow { DataContext = vm };

            // Register handler for the settings dialog interaction
            vm.OpenSettingsInteraction.RegisterHandler(async context =>
            {
                var settingsVm = new SettingsWindowViewModel(settingsService);
                var settingsWindow = new SettingsWindow { DataContext = settingsVm };
                await settingsWindow.ShowDialog(window);

                if (settingsVm.Saved)
                {
                    window.Opacity = settingsService.Current.Opacity;
                    vm.OnSettingsSaved();
                }

                context.SetOutput(Unit.Default);
            });

            // Wire notification manager after window is in the visual tree
            window.Opened += (_, _) =>
            {
                var topLevel = TopLevel.GetTopLevel(window);
                if (topLevel is not null)
                    notificationService.SetManager(new WindowNotificationManager(topLevel));
            };

            // Restore window position and opacity from settings
            window.Position = new PixelPoint(
                (int)settingsService.Current.WindowX,
                (int)settingsService.Current.WindowY);
            window.Opacity = settingsService.Current.Opacity;

            window.Closing += (_, _) =>
            {
                settingsService.Current.WindowX = window.Position.X;
                settingsService.Current.WindowY = window.Position.Y;
                settingsService.Save();
                vm.Dispose();
            };

            desktop.MainWindow = window;
            clockService.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

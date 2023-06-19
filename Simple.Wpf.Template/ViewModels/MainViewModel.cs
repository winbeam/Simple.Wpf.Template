using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Browser;
using JetBrains.Annotations;
using NLog;
using OpenQA.Selenium;
using Simple.Wpf.Template;
using Simple.Wpf.Template.Commands;
using Simple.Wpf.Template.Extensions;
using Simple.Wpf.Template.Models;
using Simple.Wpf.Template.Services;
using Simple.Wpf.Template.Services.Notifications;

namespace Simple.Wpf.Template.ViewModels;



//in your command: 

[UsedImplicitly]

public sealed class MainViewModel : DisposableViewModel, IMainViewModel, IRegisteredViewModel
{
    private AuthInfo _authInfo = new();
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Func<ISettingsViewModel> _settingsFunc;

    private ISettingsViewModel _settings;
    private IScrapper _scrapper;
    public AuthInfo AuthInfo
    {
        get { return _authInfo; }
        set { _authInfo = value; }
    }

    public bool IsLoggedIn { get; set; } = true;
    public bool CanIncomeTaxPage { get; set; } = false;

    public MainViewModel(Func<ISettingsViewModel> settingsFunc, 
        INotificationService notificationService,
        IScrapper scrapper,
        ISchedulers schedulers)
    {
        _settingsFunc = settingsFunc;
        _scrapper = scrapper;
        _scrapper.AuthInfo = _authInfo;

#if DEBUG
        _authInfo.Name = "신승범";
        _authInfo.Phone = "54010186";
        _authInfo.Birth = "19860514";
#endif
        var cancellationTokenSource = new CancellationTokenSource();
        Disposable.Create(() => cancellationTokenSource.Cancel())
            .DisposeWith(this);

        HomeTaxLoginPageCommand = ReactiveCommand<string>.Create()
            .DisposeWith(this);
        HomeTaxTestCommand = ReactiveCommand<string>.Create()
            .DisposeWith(this);
        HomeTaxIncomeTaxPageCommand = ReactiveCommand<string>.Create()
            .DisposeWith(this);



        SimpleNotificationCommand = ReactiveCommand<string>.Create()
            .DisposeWith(this);

        SnoozeNotificationCommand = ReactiveCommand<string>.Create()
            .DisposeWith(this);



        HomeTaxLoginPageCommand
            .ActivateGestures()
            .Subscribe(x =>
            {
                Logger.Info($"{nameof(HomeTaxLoginPageCommand)} Go Hometax Home Page ...");
                _scrapper.GoHomeTaxLogin();
            })
            .DisposeWith(this);

        HomeTaxIncomeTaxPageCommand
            .ActivateGestures()
            .Subscribe(x =>
            {
                Logger.Info($"{nameof(HomeTaxIncomeTaxPageCommand)} Global Income Tax Page and ...");
                _scrapper.GoGlobalIncomeTax();
            })
            .DisposeWith(this);

        HomeTaxTestCommand
            .ActivateGestures()
            .Subscribe(x =>
            {
                Logger.Info($"{nameof(HomeTaxTestCommand)} x...");
                _scrapper.Test();
            })
            .DisposeWith(this);


        SimpleNotificationCommand.Subscribe(text =>
                notificationService.ExecuteAsync(NotificationType.Message, new object[] { text }, cancellationTokenSource.Token))
            .DisposeWith(this);

        SnoozeNotificationCommand.Subscribe(text =>
                notificationService.ExecuteAsync(NotificationType.MessageWithSnooze, new object[] { text }, cancellationTokenSource.Token))
            .DisposeWith(this);



    }

    public IReactiveCommand<string> HomeTaxLoginPageCommand { get; }
    public IReactiveCommand<string> HomeTaxTestCommand { get; }
    public IReactiveCommand<string> HomeTaxIncomeTaxPageCommand { get; }

    public IReactiveCommand<string> ThrowFromUiThreadCommand { get; }

    public IReactiveCommand<string> ThrowFromTaskCommand { get; }

    public IReactiveCommand<string> ThrowFromRxCommand { get; }

    public IReactiveCommand<string> SimpleNotificationCommand { get; }

    public IReactiveCommand<string> SnoozeNotificationCommand { get; }

    public ISettingsViewModel Settings =>
        _settings ??= _settingsFunc()
            .DisposeWith(this);
}
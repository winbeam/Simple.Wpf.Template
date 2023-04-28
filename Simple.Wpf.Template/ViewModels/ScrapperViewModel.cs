using System.Collections.Generic;
using JetBrains.Annotations;
using NLog;
using Simple.Wpf.Template.Services;

namespace Simple.Wpf.Template.ViewModels;

[UsedImplicitly]
public sealed class ScrapperViewModel : DisposableViewModel, IScrapperViewModel, IRegisteredViewModel
{
    private readonly IConfigurationService _configurationService;

    public ScrapperViewModel(IConfigurationService configurationService) =>
        _configurationService = configurationService;

    Scrapper _scrapper;
    public ScrapperViewModel()
    {
        _scrapper = new Scrapper();
    }
    public void Go2HomeTax()
    {
        _scrapper.GoHomeTaxLogin();
    }


    public IEnumerable<LogLevel> LogLevels => LogLevel.AllLoggingLevels;

    public LogLevel LogLevel
    {
        get => _configurationService.LogLevel;
        set
        {
            _configurationService.LogLevel = value;
            LogManager.GlobalThreshold = value;
            RaisePropertyChanged(nameof(LogLevel));
        }
    }

    public bool EnableAutoShutdown
    {
        get => _configurationService.EnableAutoShutdown;
        set
        {
            _configurationService.EnableAutoShutdown = value;
            RaisePropertyChanged(nameof(EnableAutoShutdown));
        }
    }
}
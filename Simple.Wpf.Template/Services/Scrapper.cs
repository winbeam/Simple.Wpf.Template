using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;
using System.Threading;
using Simple.Wpf.Template.ViewModels;
using JetBrains.Annotations;
using NLog;
using NLog.Targets;
using System.Diagnostics;
using System.Windows;
using System.IO;
using Simple.Wpf.Template.Modules;
using System.Reactive.Disposables;
using Simple.Wpf.Template.Helpers;

namespace Simple.Wpf.Template.Services;


[UsedImplicitly]
public sealed class Scrapper : BaseModule, IScrapper, IApplicationService, IRegisteredService
{
    private IWebDriver _webDriver;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    readonly string _hometaxAddress = "https://hometax.go.kr";
    readonly string _globalIncomeAddress = "https://hometax.go.kr/websquare/websquare.wq?w2xPath=/ui/pp/index_pp.xml&tmIdx=4&tm2lIdx=0405000000&tm3lIdx=0405040000";

    public Scrapper()
    {
        _logger.Log(NLog.LogLevel.Info, "Scrapper Started");
    }
        
    public async Task Quit()
    {
        await Task.Run(() =>
        {
            if (_webDriver is null)
                return;
            _webDriver.Quit();
        });
        
    }

    void SetDriver()
    {
        if (_webDriver != null)
            return;

        var config = new ChromeConfig();
        var info = config.GetMatchingBrowserVersion();
        var manager = new DriverManager();
        manager.SetUpDriver(new ChromeConfig(), info);
        //IWebDriver _webDriver = new ChromeDriver();

        try
        {
            _webDriver = new ChromeDriver();
        }
        catch (WebDriverException ex)
        {
            _logger.Error($"{ex}");
            throw;
        }
        catch(Exception ex)
        {
            _logger.Error($"{ex}");
        }

    }
    public async Task Test()
    {
        var aa1 = _webDriver.Manage;
        var aa2 = _webDriver.PageSource;
        var aa3 = _webDriver.CurrentWindowHandle;
        var aa4 = _webDriver.Url;
        var aa5 = _webDriver.WindowHandles;
        var aa6 = _webDriver.Title;

        ClosePopups();
        await Task.Run(() =>
        {
            try
            {

                var aa1 = _webDriver.Manage;
                //var aa2 = _webDriver.PageSource;
                //var aa3 = _webDriver.CurrentWindowHandle;
                var aa4 = _webDriver.Url;
                var aa5 = _webDriver.WindowHandles;
                var aa6 = _webDriver.Title;
            }
            catch(Exception ex) 
            {
            }
        });
    }


    public async Task GoHomeTaxLogin()
    {
        ApplicationCleanup.RegisterForShutdown(Quit);
        await Task.Run(() =>
        {
            SetDriver();

            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            _webDriver.Navigate().GoToUrl(_hometaxAddress);

            GoSimpleAuth();
            FillPersonalInfo();
            ClosePopups();

        });


    }

    public async Task GoGlobalIncomeTax()
    {
        if (_webDriver is null)
            return;

        await Task.Run(() =>
        {
            AuthRequestConfirm();

            // 종합스득세 신고 페이지
            Thread.Sleep(200);
            _webDriver.Navigate().GoToUrl(_globalIncomeAddress);

            // 종합소득 클릭
            var paymentPageIframe = _webDriver.FindElement(By.Id("txppIframe"));
            _webDriver.SwitchTo().Frame(paymentPageIframe);
            _webDriver.FindElement(By.Id("group14455")).Click();
        });

    }

    public void GoSimpleAuth()
    {
        Tries(() => _webDriver.FindElement(By.Id("textbox81212912")).Click());

        Tries(() => 
        {
            var iframe = _webDriver.FindElement(By.Id("txppIframe"));
            _webDriver.SwitchTo().Frame(iframe);
        });

        Tries(() => _webDriver.FindElement(By.Id("anchor14")).Click());
    
        Tries(() => _webDriver.FindElement(By.Id("anchor23")).Click());
    }

    void Tries(Action work, int delay = 500, int tryCount = 2)
    {
        for (int count = 0; count < tryCount; count++)
        {
            try
            {
                Thread.Sleep(delay);
                work();
                return;
            }
            catch(Exception e)
            {
                _logger.Error($"{e}");
            }
        }

    }
    public void AuthRequest()
    {
        Tries(() => _webDriver.FindElement(By.Id("oacx-request-btn-pc")).Click());
    }

    public void AuthRequestConfirm()
    {
        string authSelector = "#oacxEmbededContents > div.standby > div > button.basic.sky.w70";
        Tries(() => _webDriver.FindElement(By.CssSelector(authSelector)).Click(), 200, 1);
    }
    public void FillPersonalInfo()
    {

        Tries(() =>
        {
            var iframe3 = _webDriver.FindElement(By.Id("UTECMADA02_iframe"));
            _webDriver.SwitchTo().Frame(iframe3);
            var iframe4 = _webDriver.FindElement(By.Id("simple_iframeView"));
            _webDriver.SwitchTo().Frame(iframe4);
        });
        string nameSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li:nth-child(1) > div.ul-td > input[type=text]";
        Tries(() => _webDriver.FindElement(By.CssSelector(nameSelector)).SendKeys("신승범"), 0);

        string birthSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li:nth-child(2) > div.ul-td > input";
        Tries(() => _webDriver.FindElement(By.CssSelector(birthSelector)).SendKeys("19860514"), 0);
        string phoneNumberSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li.none-telecom > div.ul-td > input";
        Tries(() => _webDriver.FindElement(By.CssSelector(phoneNumberSelector)).SendKeys("54010186"), 0);


        // todo: kakao 인증 아이콘 클릭 
        string authMethodSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.selectLayout > div > div > ul > li:nth-child(1) > label > a > span > img";
        Tries(() => _webDriver.FindElement(By.CssSelector(authMethodSelector)).Click());
        
        // 모두 동의 체크
        Tries(() =>
        {
            if (_webDriver.FindElement(By.XPath("//*[@id=\"totalAgree\"]")).Selected == false)
                _webDriver.FindElement(By.Id("totalAgree")).Click();
        }, 0);
    }
    void ClosePopups()
    {
        try
        {
            string mainWindowHandle = _webDriver.CurrentWindowHandle;
            IEnumerable<string> allWindowHandles = _webDriver.WindowHandles;
            // 현재 활성 창 핸들러를 제외한 나머지 창 핸들러를 가져옵니다.
            IEnumerable<string> popupWindowHandles = allWindowHandles.Where(handle => handle != mainWindowHandle);

            if (popupWindowHandles.Any())
            {
                foreach (string popupWindowHandle in popupWindowHandles)
                {
                    _webDriver.SwitchTo().Window(popupWindowHandle);
                    _webDriver.Close();
                }

                // 다시 기본 창으로 돌아갑니다.
                _webDriver.SwitchTo().Window(mainWindowHandle);
            }
        }
        catch(Exception ex)
        {
            _logger.Warn($"{ex}");
        }

    }


    private string _logFolder;
    public string LogFolder
    {
        get
        {
            if (!string.IsNullOrEmpty(_logFolder)) return _logFolder;

            _logFolder = GetLogFolder();
            return _logFolder;
        }
    }
    public void CopyToClipboard(string text) => Clipboard.SetText(text);

    public void Exit() => Application.Current.Shutdown();

    public void Restart()
    {
        var path = Application.ResourceAssembly.Location;
        if (path.EndsWith(".exe"))
        {
            Process.Start(path);
        }
        else if (path.EndsWith(".dll"))
        {
            var lastIndex = path.LastIndexOf(".dll", StringComparison.Ordinal);
            path = path.Remove(lastIndex) + ".exe";

            Process.Start(path);
        }

        Application.Current.Shutdown();
    }

    public void OpenFolder(string folder) => Process.Start("explorer.exe", folder);

    private static string GetLogFolder()
    {
        var fileTarget = LogManager.Configuration.AllTargets
            .OfType<FileTarget>()
            .SingleOrDefault();

        var fileName = fileTarget?.FileName.Render(new LogEventInfo { TimeStamp = DateTime.Now });
        return Path.GetDirectoryName(fileName);
    }
}



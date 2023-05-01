using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
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
using ControlzEx.Standard;
using System.Windows.Media.Animation;

namespace Simple.Wpf.Template.Services;


[UsedImplicitly]
public sealed class Scrapper : BaseModule, IScrapper, IRegisteredService //, IApplicationService
{
    private IWebDriver _webDriver;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    readonly string _hometaxAddr = "https://hometax.go.kr";
    readonly string _globalIncomeAddr = "https://hometax.go.kr/websquare/websquare.wq?w2xPath=/ui/pp/index_pp.xml&tmIdx=4&tm2lIdx=0405000000&tm3lIdx=0405040000";

    WebDriverWait _wait1;
    WebDriverWait _wait2;
    WebDriverWait _wait3;
    WebDriverWait _wait5;
    WebDriverWait _wait4;
    public Scrapper()
    {
        _logger.Log(NLog.LogLevel.Info, "Scrapper Started");
        ApplicationCleanup.RegisterForShutdown(Quit);
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

            _logger.Log(NLog.LogLevel.Info, "Web Driver Started");

            _wait1 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(300));
            _wait2 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(300));
            _wait3 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(500));
            _wait4 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(4), TimeSpan.FromMilliseconds(500));
            _wait5 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500));
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

    void IsTemporaryPageThenClick()
    {
        var element = _wait1.Until(e => e.FindElement(By.Id("RD3BOX")));
        if (element != null)
            element.Click();
    }


    // iframe 빈값( <html><head></head><body></body></html> _)
    // startCs
    // simple_iframeView
    public async Task Test()
    {
        //var aa1 = _webDriver.Manage;
        var aa2 = _webDriver.PageSource;
        //var aa3 = _webDriver.CurrentWindowHandle;
        //var aa4 = _webDriver.Url;
        //var aa5 = _webDriver.WindowHandles;
        //var aa6 = _webDriver.Title;

        await Task.Run(() =>
        {
            try
            {

                // 종합스득세 신고 페이지
                _webDriver.Navigate().GoToUrl(_globalIncomeAddr);

                IsOnServiceTime();


                // 종합소득 클릭
                var paymentPageIframe = _webDriver.FindElement(By.Id("txppIframe"));
                _webDriver.SwitchTo().Frame(paymentPageIframe);
                //Thread.Sleep(200);
                if (IsElem(By.XPath("//*[@id=\"textbox8637\"]"), out IWebElement elem))
                    elem?.Click();


            }
            catch(Exception ex)
            {
            }
            finally
            {
                _webDriver.SwitchTo().DefaultContent();
            }
        });
    }
    


    bool IsOnServiceTime()
    {
        // 5월1일부터 시작 되는..
        var elem = _webDriver.FindElement(By.PartialLinkText("서비스 이용시간"));
        return elem != null;
    }
    public async Task GoHomeTaxLogin()
    {
        await Task.Run(() =>
        {
            SetDriver();
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            _webDriver.Navigate().GoToUrl(_hometaxAddr);

            SetBrowserSizeAndPosition();

            IsTemporaryPageThenClick();

            ClosePopups();
            GoSimpleAuth();
            FillPersonalInfo("신승범", "19860514", "54010186", AuthMethod.Kakao);
            ClosePopups();

        });
    }

    void SetBrowserSizeAndPosition()
    {
        // 현재 모니터의 해상도를 가져옵니다.
        int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
        int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

        // 브라우저 창 크기를 모니터 비율에 맞게 조정합니다.
        double widthRatio = 0.4;  // 너비 비율: 80%
        double heightRatio = 0.8; // 높이 비율: 80%

        int browserWidth = (int)(screenWidth * widthRatio);
        int browserHeight = (int)(screenHeight * heightRatio);

        _webDriver.Manage().Window.Size = new System.Drawing.Size(browserWidth, browserHeight);
        // 브라우저 창 위치를 (100, 100)으로 이동합니다.
        _webDriver.Manage().Window.Position = new System.Drawing.Point(10, 10);
    }

    bool IsElem1<IWebElement>(By by, out IWebElement elem)
    {
        elem = default(IWebElement);
        try
        {
            elem = (IWebElement)_wait1.Until(drv => drv.FindElement(by));
            return elem != null;
        }
        catch (WebDriverTimeoutException e)
        {

        }
        catch (Exception e)
        {
        }
        return false;
    }

    IWebElement _logoutElem;
    bool IsElem<IWebElement>(By by, out IWebElement elem)
    {
        elem = default(IWebElement);
        try
        {
            elem = (IWebElement)_wait3.Until(drv => drv.FindElement(by));            
            return elem != null;
        }
        catch (WebDriverTimeoutException e)
        {

        }
        catch (Exception e)
        {
        }
        return false;
    }

    public async Task GoGlobalIncomeTax()
    {
        if (_webDriver is null)
            return;

        await Task.Run(() =>
        {
            try
            {

                AuthRequestConfirm();
                Thread.Sleep(500);

                if (IsElem(By.LinkText("로그아웃"), out IWebElement elem) == false)
                {
                    MessageBox.Show("로그인 상태가 아닙니다. \n 로그인 완료후 다시 시도해 주세요");
                    return;
                }
                // 종합스득세 신고 페이지
                _webDriver.Navigate().GoToUrl(_globalIncomeAddr);

                // 종합소득 클릭
                var paymentPageIframe = _webDriver.FindElement(By.Id("txppIframe"));
                _webDriver.SwitchTo().Frame(paymentPageIframe);
                Thread.Sleep(200);
                if (IsElem(By.XPath("//*[@id=\"textbox8637\"]"), out elem))
                    elem?.Click();
            }
            catch(Exception e)
            {

            }

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
                _webDriver.SwitchTo().DefaultContent();
                _logger.Error($"{e}");
            }
        }

    }

    public enum AuthMethod{
        Kakao,
        Kukmin,
        Naver,
        Pass,
        NongHyup,
        Toss,
        BankSalad,
        Hana,
        ShinHan,
        Payco,
        Samsung
    }

    void SelectAuth(AuthMethod method)
    {
        try
        {
            

        }
        catch(Exception e)
        {
            _logger.Error($"{e} CURRENT_PAGE_SOURCE:({_webDriver.PageSource})");
        }
        finally
        {
            _webDriver.SwitchTo().DefaultContent();
        }
    }
    
    public void AuthRequest()
    {
        Tries(() => _webDriver.FindElement(By.Id("oacx-request-btn-pc")).Click());
    }
    
    public void AuthRequestConfirm()
    {
        try
        {
            if (IsElem1(By.Id("txppIframe"), out IWebElement elem) == true)
                _webDriver.SwitchTo().Frame(elem);

            var iframe2 = _webDriver.FindElement(By.Id("UTECMADA02_iframe"));
            _webDriver.SwitchTo().Frame(iframe2);

            var iframe3 = _webDriver.FindElement(By.Id("simple_iframeView"));
            _webDriver.SwitchTo().Frame(iframe3);

            //string authSelector = "#oacxEmbededContents > div.standby > div > button.basic.sky.w70";
            if (IsElem1(By.Id("oacx-request-btn-pc"), out elem))
                elem.Click();

            _webDriver.SwitchTo().DefaultContent();
        }
        catch(Exception e)
        {
            _webDriver.SwitchTo().DefaultContent();
        }
    }
    public void FillPersonalInfo(string name, string birth, string phone, AuthMethod method)
    {
        try
        {
            _webDriver.SwitchTo().DefaultContent();

            if (IsElem1(By.Id("txppIframe"), out IWebElement iframe) == true)
                _webDriver.SwitchTo().Frame(iframe);
            var iframe3 = _webDriver.FindElement(By.Id("UTECMADA02_iframe"));
            _webDriver.SwitchTo().Frame(iframe3);
            var iframe4 = _webDriver.FindElement(By.Id("simple_iframeView"));
            _webDriver.SwitchTo().Frame(iframe4);

            string nameSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li:nth-child(1) > div.ul-td > input[type=text]";
            _webDriver.FindElement(By.CssSelector(nameSelector)).SendKeys(name);

            string birthSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li:nth-child(2) > div.ul-td > input";
            _webDriver.FindElement(By.CssSelector(birthSelector)).SendKeys(birth);


            string phoneNumberSelector = "#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li.none-telecom > div.ul-td > input";
            _webDriver.FindElement(By.CssSelector(phoneNumberSelector)).SendKeys(phone);

            #region AuthMethod
            string authAltName = string.Empty;
            switch (method)
            {
                case AuthMethod.Kakao:
                    authAltName = "KAKAO(카카오)"; break;
                case AuthMethod.Kukmin:
                    authAltName = "KB 국민은행(국민인증서)"; break;
                default:
                    _logger.Warn($"{method} not supported yet ");
                    return;
            }

            //var elem2 = _webDriver.FindElement(By.XPath("//img[@alt=contains(text(),'KAKAO')]"));
            var authElem = _webDriver.FindElement(By.XPath($"//img[@alt='{authAltName}']"));
            authElem.Click();
            #endregion

            if (IsElem(By.XPath("//*[@id=\"totalAgree\"]"), out IWebElement elem))
            {
                if (elem.Selected == false)
                    _webDriver.FindElement(By.Id("totalAgree")).Click();
            }
                elem?.Click();

            if (_webDriver.FindElement(By.XPath("//*[@id=\"totalAgree\"]")).Selected == false)
                _webDriver.FindElement(By.Id("totalAgree")).Click();

        }
        catch(Exception e)
        {
            _logger.Error($"{e} CURRENT_PAGE_SOURCE:({_webDriver.PageSource})");
        }
        finally
        {
            _webDriver.SwitchTo().DefaultContent();
        }
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

#if false
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
#endif

}



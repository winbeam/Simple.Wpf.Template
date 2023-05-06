using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;
using System.Threading;
using JetBrains.Annotations;
using NLog;
using System.Windows;
using Simple.Wpf.Template.Modules;
using Simple.Wpf.Template.Helpers;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;
using System.IO;
using System.Diagnostics;
using Simple.Wpf.Template.Services;

namespace Simple.Wpf.Template;


[UsedImplicitly]
public sealed class Scrapper : BaseModule, IScrapper, IRegisteredService //, IApplicationService
{
    string _downloadDirectory = @"C:\HomeTaxDownloads";
    public  const int DefaultWait = 3;

    private IWebDriver _webDriver;
    private IFrameManager _iframeManager;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    readonly string _hometaxAddr = "https://hometax.go.kr";
    readonly string _globalIncomeAddr = "https://hometax.go.kr/websquare/websquare.wq?w2xPath=/ui/pp/index_pp.xml&tmIdx=4&tm2lIdx=0405000000&tm3lIdx=0405040000";

    WebDriverWait _wait1;
    WebDriverWait _wait2;
    WebDriverWait _wait3;
    WebDriverWait _wait5;
    WebDriverWait _wait4;

    string _mainWindow = string.Empty;
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
    public AuthInfo AuthInfo { get; set; } = new();
    void SetDriver()
    {
        if (_webDriver != null)
            return;

        var config = new ChromeConfig();
        var info = config.GetMatchingBrowserVersion();
        var manager = new DriverManager();
        manager.SetUpDriver(new ChromeConfig(), info);

        try
        {
            Directory.CreateDirectory(_downloadDirectory);
            var options = new ChromeOptions();
            //options.AddArguments("--disable-extensions");
            options.AddUserProfilePreference("download.default_directory", _downloadDirectory);

            // ChromeOptions에서 설정한 값을 읽어오기
            //var arguments = options.Arguments.ToList();
            //var prefs = options.ToCapabilities().GetCapability("goog:chromeOptions");
            //string downloadPath = prefs["download"]["default_directory"].ToString();

#if false
            var options = new ChromeOptions(); 
            options.AddArgument("--no-sandbox");

            _webDriver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(5));
            _webDriver.Manage().Timeouts().PageLoad.Add(System.TimeSpan.FromSeconds(10));
#endif


            _webDriver = new ChromeDriver(options);
            _webDriver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(DefaultWait));
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(DefaultWait);

            _iframeManager = new IFrameManager(_webDriver, _logger);
            _logger.Info("Web Driver Started");

            var clock = new SystemClock();
            var interval = TimeSpan.FromMilliseconds(200);
            _wait1 = new WebDriverWait(clock, _webDriver, TimeSpan.FromSeconds(1), interval);
            _wait2 = new WebDriverWait(clock, _webDriver, TimeSpan.FromSeconds(2), interval);
            _wait3 = new WebDriverWait(clock, _webDriver, TimeSpan.FromSeconds(3), interval);
            _wait4 = new WebDriverWait(clock, _webDriver, TimeSpan.FromSeconds(4), interval);
            _wait5 = new WebDriverWait(clock, _webDriver, TimeSpan.FromSeconds(5), interval);


        }
        catch (WebDriverException ex)
        {
            _logger.Error($"{ex}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"{ex}");
        }

    }

    public void Test()
    {
        try
        {
            //var aa1 = _webDriver.Manage;
            var pageSource = _webDriver.PageSource;
            //var aa3 = _webDriver.CurrentWindowHandle;
            var url = _webDriver.Url;
            //var aa5 = _webDriver.WindowHandles;
            var title = _webDriver.Title;
            _logger.Info($"URL:{url}, TITLE:{title}, PAGE_SOURCE:{pageSource}");
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    public async Task GoHomeTaxLogin()
    {
        await Task.Run(() =>
        {
            SetDriver();

            _webDriver.Navigate().GoToUrl(_hometaxAddr);
            _mainWindow = _webDriver.CurrentWindowHandle;

            SetBrowserSizeAndPosition();

            ClickIfTemporaryPageExist();

            GoSimpleAuth();
            ClosePopups();
            FillPersonalInfo(AuthInfo.Name, AuthInfo.Birth, AuthInfo.Phone, AuthMethod.Kakao);

        });
    }

    public async Task GoGlobalIncomeTax()
    {
        if (_webDriver is null)
            return;

        await Task.Run(() =>
        {
            try
            {
                AuthRequestConfirmIfNotClicked();

                if (IsLoggedIn() == false)
                {
                    MsgBox("로그인 상태가 아닙니다. \n 로그인 완료후 다시 시도해 주세요");
                    return;
                }

                if (GoToUrl(_globalIncomeAddr) == false || IsOnServiceTime() == false)
                {
                    MsgBox("서비스 시간이 아닙니다");
                    return;
                }
                 

                // 금융소득 조회 버튼 있으면 클릭
                using (new IFramer(_iframeManager, new() { "txppIframe" }))
                {
                    // or By.XPath("//*[@id=\"textbox8637\"]")
                    if (IsClickable(By.XPath("//span[contains(text(),'금융소득 조회')]"), out var elem))
                    {
                        elem.Click();
                    }
                    else
                    {
                        MsgBox("금융소소득 조회 버튼 오류");
                        return;
                    }    
                }


                // popup 
                _mainWindow = _webDriver.CurrentWindowHandle;
                var allWindowHandles = _webDriver.WindowHandles;
                // 현재 활성 창 핸들러를 제외한 나머지 창 핸들러를 가져옵니다.
                var popups = allWindowHandles.Where(handle => handle != _mainWindow);

                if (popups.Any() == false)
                {
                    MsgBox("금융소득 조회 팝업 없음");
                    return;
                }
                    
                foreach (string popup in popups)
                {
                    try
                    {
                        string message = string.Empty;
                        _webDriver.SwitchTo().Window(popup);
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex);
                    }

                    if (DismissIfAlertExist(popup, "없습니다")) // alert: 조회된 데이터가 없습니다.
                    {
                        MsgBox("자료가 없습니다");
                        //return; // for test, temporary
                    }

                    if (DownloadGlobalIncomeIfExist(popup))
                    {
                        if (MessageBox.Show("자료 다운로드 완료\n 다운로드 폴더를 Open 하시겠습니까?", 
                                            "HomeTaxAuto", 
                                            MessageBoxButton.YesNo, 
                                            MessageBoxImage.Information) == MessageBoxResult.Yes)
                            Process.Start(_downloadDirectory);
                    }
                    else
                    {
                        MsgBox("내려 받기할 명세가 없습니다");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                MsgBox("GoGlobalIncomeTax 오류");
            }
            finally
            {
                _webDriver.SwitchTo().Window(_mainWindow);
            }
        });
    }

    void MsgBox(string msg, MessageBoxImage image = MessageBoxImage.Error)
    {
        MessageBox.Show(msg, "HomeTaxAuto", MessageBoxButton.OK, image);
    }

    bool GoToUrl(string url)
    {
        var task = Task.Run(() =>
        {
            _webDriver.Navigate().GoToUrl(url);
        });
        return task.Wait(3000);
    }

    bool DownloadGlobalIncomeIfExist(string window)
    {
        try
        {
            if (IsClickable(By.Id("btnExcelDwld"), out var elem))
            {
                elem.Click(); // downloading...
            }
            else
            {
                _logger.Error("not found : btnExcelDwld");
                return false;
            }

            if (DismissIfAlertExist(window, "없습니다"))
            {
                //MsgBox("내려 받기할 명세가 없습니다");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e);
            return false;
        }
    }

    void DefaultContent()
    {
        try
        {
            _webDriver.SwitchTo().DefaultContent();
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
        }
    }


    void ClickIfTemporaryPageExist()
    {
        try
        {
            var elem = _wait1.Until(ExpectedConditions.ElementToBeClickable(By.Id("RD3BOX")));
            if (elem != null)
                elem.Click();
        }
        catch (Exception ex)
        {
            _logger.Info($"ClickIfTemporaryPageExist ({ex})");
        }
    }

    bool DismissIfAlertExist(string windowsHandle, string message)
    {
        if (DismissIfAlertExist(windowsHandle, ref message))
            if (message.Contains(message))
                return true;
        return false;
    }

    bool DismissIfAlertExist(string windowsHandle, ref string message)
    {
        try
        {
            //Wait for the alert to be displayed
            _wait1.Until(ExpectedConditions.AlertIsPresent());
            //Store the alert in a variable
            IAlert alert = _webDriver.SwitchTo().Alert();
            //Store the alert in a variable for reuse
            string text = alert.Text;
            //Press the Cancel button
            alert.Dismiss();

            return true;
        }
        catch
        {
            return false;
        }
    }
    bool IsOnServiceTime()
    {
        return IsExist(By.PartialLinkText("서비스 이용시간"), out var elem) == false;
    }
    bool IsClickable(By by, out IWebElement elem, int timeout = 2, int sleep = 0)
    {
        elem = null;
        var wait = new WebDriverWait(new SystemClock(),
                                    _webDriver,
                                    TimeSpan.FromSeconds(timeout),
                                    TimeSpan.FromMilliseconds(200));
        try
        {
            elem = wait.Until(ExpectedConditions.ElementToBeClickable(by));
            Thread.Sleep(sleep);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            return false;
        }
    }
    bool IsExist(By by, out IWebElement elem, int timeout = 1)
    {
        elem = null;
        var wait = new WebDriverWait(new SystemClock(),
                                    _webDriver,
                                    TimeSpan.FromSeconds(timeout),
                                    TimeSpan.FromMilliseconds(200));
        try
        {
            elem = wait.Until(ExpectedConditions.ElementExists(by));
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            return false;
        }
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

    bool IsElem1<IWebElement>(By by, out IWebElement elem, int delay = 0)
    {
        return IsElem(_wait1, by, out elem, delay);
    }
    bool IsElem2<IWebElement>(By by, out IWebElement elem, int delay = 0)
    {
        return IsElem(_wait2, by, out elem, delay);
    }
    bool IsElem3<IWebElement>(By by, out IWebElement elem, int delay = 0)
    {
        return IsElem(_wait3, by, out elem, delay);
    }

    IWebElement _logoutElem; // todo log 
    bool IsElem<IWebElement>(WebDriverWait wait, By by, out IWebElement elem, int delay = 0)
    {
        elem = default(IWebElement);
        try
        {
            elem = (IWebElement)wait.Until(drv => drv.FindElement(by));
            Thread.Sleep(delay);
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

    public bool GoSimpleAuth()
    {
        try
        {
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            if (IsClickable(By.Id("textbox81212912"), out var elem, 3, 500))
                elem.Click();

            using (new IFramer(_iframeManager, new() { "txppIframe" }))
            {
                if (IsClickable(By.Id("anchor14"), out var elem2, 3, 500))
                    elem2.Click();
                if (IsClickable(By.Id("anchor23"), out var elem3, 3, 500))
                    elem3.Click();
            }
            return true;

        }
        catch (Exception e)
        {
            _logger.Error(e);
            return false;
        }
        finally
        {
            SetWait();
        }
    }

    void SetWait(int wait = DefaultWait)
    {
        _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(DefaultWait);
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

    public bool IsLoggedIn()
    {
        return IsClickable(By.XPath("//span[contains(text(),'로그아웃')]"), out var elem);
    }
    public bool AuthRequestConfirmIfNotClicked()
    {
        try
        {
            using (new IFramer(_iframeManager, new() { "txppIframe", "UTECMADA02_iframe", "simple_iframeView" }))
            {
                if (IsClickable(By.XPath("//button[contains(text(),'인증 완료')]"), out IWebElement elem, 1))
                {
                    elem.Click();
                    return true;
                }
            }
        }
        catch(Exception e)
        {
            _logger.Error(e);
        }
        return false;
    }
    public void FillPersonalInfo(string name, string birth, string phone, AuthMethod method)
    {
        try
        {
            using (new IFramer(_iframeManager, new() { "txppIframe", "UTECMADA02_iframe", "simple_iframeView" }))
            {
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

                if (IsElem3(By.XPath("//*[@id=\"totalAgree\"]"), out IWebElement elem))
                {
                    if (elem.Selected == false)
                        _webDriver.FindElement(By.Id("totalAgree")).Click();
                }
                if (_webDriver.FindElement(By.XPath("//*[@id=\"totalAgree\"]")).Selected == false)
                    _webDriver.FindElement(By.Id("totalAgree")).Click();

                // 인증 요청
                if (IsElem1(By.Id("oacx-request-btn-pc"), out IWebElement authReq))
                    authReq.Click();

            }
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



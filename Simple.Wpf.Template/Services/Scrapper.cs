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

namespace Simple.Wpf.Template.Services;


[UsedImplicitly]
public class Scrapper : IScrapper, IRegisteredService
{
    private IWebDriver _webDriver;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public Scrapper()
    {
        _logger.Log(NLog.LogLevel.Info, "Scrapper Started");


    }

    ~Scrapper()
    {
        Quit();
    }
    public void Quit()
    {
        _webDriver.Quit();
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
            throw;
        }

    }
    public void GoGoogle()
    {
        SetDriver();

        _webDriver.Navigate().GoToUrl("https://www.google.com");
        var aa = _webDriver.Title.Contains("Google");
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
            catch
            {
            }
        }

    }
    public void AuthRequest()
    {


        Tries(() =>
        {
            _webDriver.FindElement(By.Id("oacx-request-btn-pc")).Click();

        });
    }

    public void AuthRequestConfirmed()
    {

        Tries(() =>
        {
            // 인증 완료
            var ok = _webDriver.FindElement(By.CssSelector("#oacxEmbededContents > div.standby > div > button.basic.sky.w70"));
            ok.Click();
        }, 200, 1);
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

        await Task.Run(() =>
        {
            SetDriver();

            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            _webDriver.Navigate().GoToUrl("https://hometax.go.kr");

            GoSimpleAuth();
            FillPersonalInfo();

        });

    }

    public async Task GoGlobalIncomeTax()
    {
        await Task.Run(() =>
        {
            AuthRequestConfirmed();

#if false
            Tries(() =>
            {
                var paymentPageIframe = _webDriver.FindElement(By.Id("txppIframe"));
                _webDriver.SwitchTo().Frame(paymentPageIframe);
            });
            Tries(() =>
            {
                _webDriver.FindElement(By.CssSelector("#grpMenuAtag_04_0405040000")).Click();
            });

            Tries(() =>
            {
                _webDriver.FindElement(By.Id("sub_a_0405040000")).Click();
            });

            Tries(() =>
            {
                _webDriver.FindElement(By.Id("textbox8637")).Click();
            });
        
#endif
            // 종합스득세 신고 페이지
            Thread.Sleep(200);
            _webDriver.Navigate().GoToUrl("https://hometax.go.kr/websquare/websquare.wq?w2xPath=/ui/pp/index_pp.xml&tmIdx=4&tm2lIdx=0405000000&tm3lIdx=0405040000");

            var paymentPageIframe = _webDriver.FindElement(By.Id("txppIframe"));
            _webDriver.SwitchTo().Frame(paymentPageIframe);

            _webDriver.FindElement(By.Id("group14455")).Click();
        });

    }

    public void GoSimpleAuth()
    {

        Tries(() =>
        {
            var elem = _webDriver.FindElement(By.Id("textbox81212912"));
            elem.Click();
        });


        Tries(() =>
        {
            var iframe = _webDriver.FindElement(By.Id("txppIframe"));
            _webDriver.SwitchTo().Frame(iframe);
        });

        Tries(() =>
        {
            var item1 = _webDriver.FindElement(By.Id("anchor14"));
            item1.Click();
        });

        Tries(() =>
        {
            var item2 = _webDriver.FindElement(By.Id("anchor23"));
            item2.Click();
        });
        var aa1 = _webDriver.Manage;
        var aa2 = _webDriver.PageSource;
        var aa3 = _webDriver.CurrentWindowHandle;
        var aa4 = _webDriver.Url;
        var aa5 = _webDriver.WindowHandles;
        var aa6 = _webDriver.Title;
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
        Tries(() =>
        {
            var name = _webDriver.FindElement(By.CssSelector("#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li:nth-child(1) > div.ul-td > input[type=text]"));
            name.SendKeys("신승범");
        }, 0);

        Tries(() =>
        {
            var birth = _webDriver.FindElement(By.CssSelector("#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li:nth-child(2) > div.ul-td > input"));
            birth.SendKeys("19860514");
        }, 0);

        Tries(() =>
        {
            var phoneNumberEditBox = _webDriver.FindElement(By.CssSelector("#oacxEmbededContents > div:nth-child(2) > div > div.formLayout > section > form > div.tab-content > div:nth-child(1) > ul > li.none-telecom > div.ul-td > input"));
            phoneNumberEditBox.SendKeys("54010186");
        }, 0);

        Tries(() =>
        {
            // todo: kakao 인증 아이콘 클릭 
            //var aa = _webDriver.FindElement(By.XPath("네이버"));
            var kakao = _webDriver.FindElement(By.CssSelector("#oacxEmbededContents > div:nth-child(2) > div > div.selectLayout > div > div > ul > li:nth-child(1) > label > a > span > img"));
            //var kakao = _webDriver.FindElement(By.CssSelector("#oacxEmbededContents > div:nth-child(2) > div > div.selectLayout > div > div > ul > li.selected > label > a > span > img"));
            kakao.Click();
        });
        Tries(() =>
        {
            bool selected = _webDriver.FindElement(By.XPath("//*[@id=\"totalAgree\"]")).Selected;
            if (selected == false)
            {
                // 모두 동의 체크
                _webDriver.FindElement(By.Id("totalAgree")).Click();
            }
        }, 0);
    }
    public void GoHomeTaxLogin2()
    {
        SetDriver();

        _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
        _webDriver.Navigate().GoToUrl("https://hometax.go.kr");


        GoSimpleAuth();
        FillPersonalInfo();
        ClosePopups();
        return;

        //_webDriver.SwitchTo().ParentFrame();
        //AuthRequest(); // manually 
        AuthRequestConfirmed();
        GoGlobalIncomeTax();

        Tries(() =>
        {
            var aa = _webDriver.Manage;
        });

        Tries(() =>
        {
            var aa = _webDriver.PageSource;
        });

        Tries(() =>
        {
            var aa = _webDriver.CurrentWindowHandle;
        });

        Tries(() =>
        {
            var aa = _webDriver.Url;
        });

        Tries(() =>
        {
            var aa = _webDriver.WindowHandles;
        });

        Tries(() =>
        {
            var aa = _webDriver.Title;
        });

        Quit();


    }

    void ClosePopups()
    {
        // 현재 활성 창의 핸들러를 저장합니다.
        string mainWindowHandle = _webDriver.CurrentWindowHandle;

        // 모든 창 핸들러를 가져옵니다.
        IEnumerable<string> allWindowHandles = _webDriver.WindowHandles;

        // 현재 활성 창 핸들러를 제외한 나머지 창 핸들러를 가져옵니다.
        IEnumerable<string> popupWindowHandles = allWindowHandles.Where(handle => handle != mainWindowHandle);

        // 팝업 창이 있으면 닫습니다.
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
}



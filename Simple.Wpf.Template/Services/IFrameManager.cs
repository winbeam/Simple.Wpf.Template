using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Wpf.Template.Services;
public class IFramer : IDisposable
{
    IFrameManager _manager;
    bool _setResult = false;
    public bool IsSet
    {
        get => _setResult;
    }
    
    public IFramer(IFrameManager manager, List<string> iframes)
    {
        _manager = manager;
        _setResult = _manager.Set(iframes);
    }
   
    public void Dispose()
    {
        _manager.SwitchToDefault();
    }
}
public class IFrameManager
{
    List<string> _iframes = new();

    WebDriverWait _wait;
    IWebDriver _webDriver;
    ILogger _logger;
    public IFrameManager(IWebDriver webDriver, ILogger logger)
    {
        _webDriver = webDriver;
        _logger = logger;
        _wait = new WebDriverWait(new SystemClock(),
                                    _webDriver,
                                    TimeSpan.FromSeconds(3),
                                    TimeSpan.FromMilliseconds(200));
    }
    public bool Set(List<string> iframes)
    {
        if (IsAlready(iframes))
            return true;

        try
        {
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            SwitchToDefault();
            foreach (var iframe in iframes)
            {
                _webDriver.SwitchTo().Frame(iframe);
            }
            return true;
        }
        catch(Exception e)
        {
            _logger.Error(e);
            SwitchToDefault();
            return false;
        }
        finally
        {
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Scrapper.DefaultWait);
        }
    }
    public void SwitchToDefault()
    {
        _webDriver.SwitchTo().DefaultContent();
    }
    bool IsExist(By by, out IWebElement elem, int timeout = 1)
    {
        elem = null;
        try
        {
            elem = _wait.Until(ExpectedConditions.ElementExists(by));
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            return false;
        }
    }

    bool IsAlready(List<string> iframes)
    {
        return Enumerable.SequenceEqual(_iframes, iframes);
    }

}

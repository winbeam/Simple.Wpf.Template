using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Wpf.Template.Services;
public class IFrameState : IDisposable
{
    IFrameManager _manager;
    public IFrameState(IFrameManager manager, List<string> iframes)
    {
        _manager = manager;
        _manager.Set(iframes);
    }

    public void Dispose()
    {
        _manager.SwitchToDefault();
    }
}
public class IFrameManager
{
    List<string> _iframes = new();

    WebDriverWait _wait1;
    IWebDriver _webDriver;
    ILogger _logger;
    public IFrameManager(IWebDriver webDriver, ILogger logger)
    {
        _webDriver = webDriver;
        _logger = logger;
        _wait1 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(200));
    }
    public bool Set(List<string> iframes)
    {
        if (IsAlready(iframes))
            return true;

        SwitchToDefault();

        try
        {
            foreach (var iframe in iframes)
            {
                //if (IsElem1(By.Id(iframe), out IWebElement iframeElem) == false)
                //    return false;
                _webDriver.SwitchTo().Frame(iframe);
            }
        }
        catch(Exception e)
        {
            SwitchToDefault();
            _logger.Error(e);
            return false;
        }

        return true;
    }
    public void SwitchToDefault()
    {
        _webDriver.SwitchTo().DefaultContent();
    }
    bool IsAlready(List<string> iframes)
    {
        return Enumerable.SequenceEqual(_iframes, iframes);
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
            _logger.Error(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
        return false;
    }

}

using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Wpf.Template.Services;

internal class IFrameState
{
    List<string> _iframes = new();

    WebDriverWait _wait1;
    IWebDriver _webDriver;
    ILogger _logger;
    public IFrameState(IWebDriver webDriver, ILogger logger)
    {
        _webDriver = webDriver;
        _logger = logger;
        _wait1 = new WebDriverWait(new SystemClock(), _webDriver, timeout: TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(200));
    }
    public bool Set(List<string> iframes)
    {
        if (IsAlready(iframes))
            return true;

        _webDriver.SwitchTo().DefaultContent(); // from start

        try
        {
            foreach (var iframe in iframes)
            {
                if (IsElem1(By.Id(iframe), out IWebElement iframeElem) == false)
                    return false;

                _webDriver.SwitchTo().Frame(iframeElem);
            }
        }
        catch(Exception e)
        {
            _logger.Error(e);
        }

        return false;
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

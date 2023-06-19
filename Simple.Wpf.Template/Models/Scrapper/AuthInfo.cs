using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Simple.Wpf.Template;

public enum MobileCarrier
{
    SKT,
    KT,
    LG
}

public class AuthInfo
{
    public string Name { get; set; } = "홍길동";
    string _phone = "12341234";
    public string Phone
    {
        get { return _phone; }
        set 
        {
            _phone = Regex.Replace(value, @"[^0-9]", "");
            if (_phone.StartsWith("010"))
                _phone = _phone[3..];
        }
    }

    public string Birth { get; set; } = "19900101";
    public MobileCarrier MobileCarrier { get; set; }  //  for PASS

}

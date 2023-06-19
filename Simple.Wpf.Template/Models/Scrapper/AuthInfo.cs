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
        get => _phone;
        set
        {
            if (value.Length > 8)
                value = value.Substring(0, 8);
            _phone = Regex.Replace(value, @"[^0-9]", "");
            if (_phone.StartsWith("010"))
                _phone = _phone[3..];
        }
    }

    string _birth = "19900101";
    public string Birth
    {
        get => _birth;
        set
        {
            if (value.Length > 8)
                value = value.Substring(0, 8);
            _birth = Regex.Replace(value, @"[^0-9]", "");
        }
    }
    public MobileCarrier MobileCarrier { get; set; }  //  for PASS

}

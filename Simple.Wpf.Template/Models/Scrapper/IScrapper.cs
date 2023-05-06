using System.Threading.Tasks;

namespace Simple.Wpf.Template
{
    public interface IScrapper
    {
        AuthInfo AuthInfo { get; set; }
        Task GoHomeTaxLogin();
        Task GoGlobalIncomeTax();
        void Test();
        Task Quit();
    }
}
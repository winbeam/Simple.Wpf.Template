using System.Threading.Tasks;

namespace Simple.Wpf.Template.Services
{
    public interface IScrapper
    {
        Task GoHomeTaxLogin();
        Task GoGlobalIncomeTax();
        void Test();
        Task Quit();
    }
}
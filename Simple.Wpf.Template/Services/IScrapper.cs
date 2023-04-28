using System.Threading.Tasks;

namespace Simple.Wpf.Template.Services
{
    public interface IScrapper
    {
        void GoGoogle();
        Task GoHomeTaxLogin();
        Task GoGlobalIncomeTax();
        Task Test();
        void Quit();
    }
}
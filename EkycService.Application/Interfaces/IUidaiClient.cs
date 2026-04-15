
namespace EkycService.Application.Interfaces
{
        public interface IUidaiClient
        {
            Task<string> GetKycXmlAsync(string uid);
        }
}

using Web_API.Security.Common;
using System.Threading.Tasks;

namespace Web_API.Security.Service
{
    public interface IUserService
    {
        Task<ApiResult<string>> Authencate(LoginRequest request);

    }
    public class LoginRequest
    {
        public string Key { get; set; }
    }
}

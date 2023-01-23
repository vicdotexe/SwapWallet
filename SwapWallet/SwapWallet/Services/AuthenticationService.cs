using System.Threading.Tasks;
using SwapWallet.Models;

namespace SwapWallet.Services
{
    public interface IAuthenticationService
    {
        User AuthenticatedUser { get; }
        bool IsAuthenticated { get; }
        Task<bool> Login(User user, string password);
        Task Logout();
    }

    public class AuthenticationService : IAuthenticationService
    {
        public User AuthenticatedUser { get; set; }
        public bool IsAuthenticated { get; set; }

        public Task<bool> Login(User user, string password)
        {
            var success = user.TryInitialize(password);
            if (success)
                AuthenticatedUser = user;
            IsAuthenticated = true;
            return Task.FromResult(success);
        }

        public Task Logout()
        {
            AuthenticatedUser = null;
            IsAuthenticated = false;
            return Task.FromResult(false);
        }
    }
}

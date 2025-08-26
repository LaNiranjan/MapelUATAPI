using MapelRestAPI.Interfaces;

namespace MapelRestAPI.Services
{
    public class UserAuthHandler
    {
        private readonly IUserService _userService;

        public UserAuthHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<string> HandleAsync(string user)
        {
            return await _userService.GetUserDetailsAsync(user);
        }
    }
}

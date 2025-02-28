using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.Service;

namespace UserRegistration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserRegistrationController : ControllerBase
    {

        UserRegistrationBL _userRegistrationBL;

        string username = "root";
        string password = "root";

        public UserRegistrationController(UserRegistrationBL userRegistrationBL)
        {
            _userRegistrationBL = userRegistrationBL;

        }

        [HttpGet]
        public string registration()
        {
            return _userRegistrationBL.registrationBL(this.username, this.password);
        }
    }
}

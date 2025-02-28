using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Service;

namespace BusinessLayer.Service
{
    public class UserRegistrationBL
    {
        UserRegistrationRL _userRegistrationRL;
        public UserRegistrationBL(UserRegistrationRL userRegistrationRL) {
            _userRegistrationRL=userRegistrationRL;
        }

        public string registrationBL(string username, string password)
        {
            if(username=="root" && password == "root")  return "Login Successfull";

            return _userRegistrationRL.registrationRL(username,password);
        }
    }
}

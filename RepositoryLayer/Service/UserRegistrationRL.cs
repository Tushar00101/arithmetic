using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Service
{
    public class UserRegistrationRL
    {
        string username = "tushar";
        string password = "tushar";

        public UserRegistrationRL() { }

        public string registrationRL(string username,string password)
        {
            if(this.username==username && this.password==password)
                    return "Login Successfull";

            return "Ivalid Username or Password";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public class ApiCredentials
    {
        public string Url { get; }
        public string Company { get; }
        public string UserName { get; }
        public string Password { get; }

        public ApiCredentials(string url, string company, string userName, string password)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (company == null)
                throw new ArgumentNullException(nameof(company));
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            Url = url;
            Company = company;
            UserName = userName;
            Password = password;
        }
    }
}

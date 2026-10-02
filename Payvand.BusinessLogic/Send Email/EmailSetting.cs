using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.BusinessLogic.Send_Email
{
    public class EmailSetting
    {
        public string Port { get; set; } = "";
        public string Host { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool EnableSsl { get; set; }
    }
}

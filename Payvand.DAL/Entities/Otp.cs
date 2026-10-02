using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.DAL.Entities
{
    public class Otp
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
    }

}

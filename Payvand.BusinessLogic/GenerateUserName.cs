using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.BusinessLogic
{
    public class GenerateUserName
    {
        public string GenerateNumber()
        {
            Random random = new Random();
            return random.Next(100000,1000000).ToString();
        }

        public string GenerateName()
        {
            var name = "us" + "_" + GenerateNumber();
            return name;
        }
    }
}

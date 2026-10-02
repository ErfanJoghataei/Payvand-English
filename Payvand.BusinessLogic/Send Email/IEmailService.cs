using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Payvand.BusinessLogic.Send_Email
{
    public interface IEmailService
    {
       Task<bool> Send(string to); 
    }
}

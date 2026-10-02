using Payvand.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.BusinessLogic.Link
{
    public interface ILinkServise
    {
        public  Task< ICollection<ShortenedLink>> GetLinks(int userid);
        public Task RemoveLink(int linkid);

    }
}

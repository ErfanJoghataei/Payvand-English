using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.DAL.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public string FullName { get; set; }
      
        public string? Email { get; set; }
        public string Topic { get; set; }
        public string Text { get; set; }
        public DateTime SendedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }

    }
}

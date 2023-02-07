using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class UserEntity : BaseEntity
    {

        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastPasswordChangedDate { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public int? Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool? IsAdmin { get; set; }
         
    }
}

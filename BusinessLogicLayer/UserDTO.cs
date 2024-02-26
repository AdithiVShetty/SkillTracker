using DataAccessLayer;
using System;
using System.Collections.Generic;

namespace BusinessLogicLayer
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string ContactNo { get; set; }
        public string Gender { get; set; }
        public bool IsAdmin { get; set; }
        public string HashedPassword { get; set; }
    }
}
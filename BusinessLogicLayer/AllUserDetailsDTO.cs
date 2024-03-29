﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer
{
    public class AllUserDetailsDTO
    {
        [Key]
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public string FullName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string ContactNo { get; set; }
        public string Gender { get; set; }
        public List<UpdateUserSkillDTO> Skills { get; set; }
    }
}

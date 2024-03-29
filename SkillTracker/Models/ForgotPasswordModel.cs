﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SkillTracker.Models
{
    public class ForgotPasswordModel
    {
        public string EmailId { get; set; }
        public DateTime DateOfBirth { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Invalid Password")]
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
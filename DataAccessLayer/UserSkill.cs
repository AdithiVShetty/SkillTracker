//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserSkill
    {
        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> SkillId { get; set; }
        public string Proficiency { get; set; }
    
        public virtual Skill Skill { get; set; }
        public virtual User User { get; set; }
    }
}

using DataAccessLayer;
using System;

namespace BusinessLogicLayer
{
    public class UserSkillDTO
    {
        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> SkillId { get; set; }
        public string Proficiency { get; set; }

        public virtual Skill Skill { get; set; }
        public virtual User User { get; set; }
    }
}

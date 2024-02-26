using DataAccessLayer;
using System.Collections.Generic;

namespace BusinessLogicLayer
{
    public class SkillDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<UserSkill> UserSkills { get; set; }
    }
}
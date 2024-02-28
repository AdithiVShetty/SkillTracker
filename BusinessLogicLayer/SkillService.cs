using DataAccessLayer;
using System;
using System.Linq;

namespace BusinessLogicLayer
{
    public class SkillService
    {
        DataAccessLayer.SkillTrackerDBEntities db = new SkillTrackerDBEntities();
        public int AddSkillToSkillTable(string skillName)
        {
            Skill skill = new Skill();
            skill.Name = skillName;
            db.Skills.Add(skill);
            db.SaveChanges();

            Skill newSkill = db.Skills.FirstOrDefault(s => s.Name == skillName);
            int newSkillId = newSkill.Id;
            return newSkillId;
        }
        public bool AddSkillToUserSkill(int userId, UpdateUserSkillDTO updateUserSkillDTO)
        {
            if (IsUserSkillExists(userId, updateUserSkillDTO.Name))
            {
                throw new ArgumentException("User already has this skill");
            }
            int newSkillId;
            Skill skill = db.Skills.FirstOrDefault(s => s.Name == updateUserSkillDTO.Name);
            if (skill == null)
            {
                newSkillId = AddSkillToSkillTable(updateUserSkillDTO.Name);
            }
            else
            {
                newSkillId = skill.Id;
            }
            UserSkill userSkill = new UserSkill();
            userSkill.UserId = userId;
            userSkill.SkillId = newSkillId;
            userSkill.Proficiency = updateUserSkillDTO.Proficiency;

            db.UserSkills.Add(userSkill);
            db.SaveChanges();
            return true;
        }
        private bool IsUserSkillExists(int userId, string skillName)
        {
            return db.UserSkills.Any(us => us.UserId == userId && us.Skill.Name == skillName);
        }
        public bool UpdateSkill(int userId, int skillId, UpdateUserSkillDTO updateUserSkillDTO)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }
            UserSkill userSkill = db.UserSkills.FirstOrDefault(us => us.UserId == userId && us.SkillId == skillId);
            if (userSkill != null)
            {
                Skill skill = db.Skills.FirstOrDefault(s => s.Id == skillId);
                if (skill != null)
                {
                    skill.Name = updateUserSkillDTO.Name;
                }
                else
                {
                    return false;
                }
                userSkill.Proficiency = updateUserSkillDTO.Proficiency;
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public string DeleteUserSkill(int userSkillId)
        {
            UserSkill userSkill = db.UserSkills.Find(userSkillId);
            int userSkill_SkillId = (int)userSkill.SkillId;
            Skill skill = db.Skills.FirstOrDefault(s => s.Id == userSkill_SkillId);
            string userSkillName = skill.Name;

            db.UserSkills.Remove(userSkill);
            db.SaveChanges();
            return userSkillName;
        }
    }
}
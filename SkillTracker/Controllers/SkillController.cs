using BusinessLogicLayer;
using SkillTracker.Models;
using System;
using System.Web.Http;

namespace SkillTracker.Controllers
{
    public class SkillController : ApiController
    {
        SkillService userSkillService = new SkillService();

        [HttpPut]
        [Route("api/Skill/{id}/{skillId}/updateskill")]
        public IHttpActionResult UpdateUserSkill(int id, int skillId, UpdateUserSkillModel updateUserSkillsModel)
        {
            UpdateUserSkillDTO updateUserSkillsDTO = new UpdateUserSkillDTO();

            updateUserSkillsDTO.Name = updateUserSkillsModel.Name;
            updateUserSkillsDTO.Proficiency = updateUserSkillsModel.Proficiency;

            bool result = userSkillService.UpdateSkill(id, skillId, updateUserSkillsDTO);
            if (result)
            {
                return Ok<string>("Skills Updated Successfully!");
            }
            else
            {
                return Ok<string>("Error occured during updating Try Again..");
            }
        }

        [HttpPost]
        [Route("api/Skill/{id}/AddSkill")]
        public IHttpActionResult PostNewSkill(int id, UpdateUserSkillModel updateUserSkillModel)
        {
            try
            {
                UpdateUserSkillDTO updateUserSkillDTO = new UpdateUserSkillDTO();

                updateUserSkillDTO.Name = updateUserSkillModel.Name;
                updateUserSkillDTO.Proficiency = updateUserSkillModel.Proficiency;

                bool result = userSkillService.AddSkillToUserSkill(id, updateUserSkillDTO);
                if (result)
                {
                    return Ok<string>($"Successfully added {updateUserSkillDTO.Name} as a skill!");
                }
                else
                {
                    return Ok<string>("Error occured during adding Try Again..");
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/Skill/{skillid}/DeleteSkill/{userSkillId}")]
        public IHttpActionResult DeleteUserSkill(int id, int userSkillId)
        {
            string skillName = userSkillService.DeleteUserSkill(userSkillId);
            return Ok<string>($"Deleted {skillName} from your skills");
        }
    }
}
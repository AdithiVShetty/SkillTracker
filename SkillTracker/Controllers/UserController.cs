using AutoMapper;
using BusinessLogicLayer;
using SkillTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SkillTracker.Controllers
{
    [EnableCors(origins: "*",headers: "*",methods:"*")]
    public class UserController : ApiController
    {
        private readonly IMapper mapper;
        public UserController()
        {
            var mapConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDTO, UserModel>();
                cfg.CreateMap<AllUserDetailsDTO, AllUserDetailsModel>()
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills));
                cfg.CreateMap<UpdateUserSkillDTO, UpdateUserSkillModel>();
                cfg.CreateMap<UserModel, DisplayModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.EmailId, opt => opt.MapFrom(src => src.EmailId));
            });
            mapper = mapConfig.CreateMapper();
        }

        UserService userBusiness = new UserService();

        [Route("api/user")]
        public List<DisplayModel> GetAllUsers()        //View -> Admin
        {
            List<UserDTO> users = userBusiness.GetListOfUsers();
            List<UserModel> userModelList = mapper.Map<List<UserModel>>(users);
            List<DisplayModel> displayUsers = mapper.Map<List<DisplayModel>>(userModelList);

            return displayUsers;
        }

        [Route("api/user/getalladmins")]
        public List<DisplayModel> GetAllAdmins()       //View -> Admin
        {
            List<UserDTO> admins = userBusiness.GetListOfAdmins();
            List<UserModel> adminModelList = mapper.Map<List<UserModel>>(admins);
            List<DisplayModel> displayAdmins = mapper.Map<List<DisplayModel>>(adminModelList);

            return displayAdmins;
        }

        [Route("api/user/getuserbyidorname")]
        public IHttpActionResult GetUserByIdOrName(string searchUser)       //View -> Admin
        {
            List<UserDTO> foundUsers = userBusiness.GetUserByIdOrName(searchUser);

            if (foundUsers.Any())
            {
                List<UserModel> users = mapper.Map<List<UserModel>>(foundUsers);
                List<DisplayModel> displayUsers = mapper.Map<List<DisplayModel>>(users);

                return Ok(displayUsers);
            }
            else
            {
                return Ok("User not found");
            }
        }

        [Route("api/user/{id}")]
        public IHttpActionResult GetUserDetails(int id)         //View -> Admin & User(Id)
        {
            AllUserDetailsDTO getUserDetailsDTOs = userBusiness.GetUserDetails(id);
            AllUserDetailsModel getUserDetailsModel = mapper.Map<AllUserDetailsModel>(getUserDetailsDTOs);
            return Ok(getUserDetailsModel);
        }

        [HttpPost]
        [Route("api/user/postnewuser")]
        public IHttpActionResult PostNewUser([FromBody] UserModel newUser)      //View -> Admin
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(string.Join(" ", errors));
            }
            UserDTO userDTO = new UserDTO
            {
                EmailId = newUser.EmailId,
                Password = newUser.Password,
            };
            userBusiness.AddUser(userDTO);
            return Ok($"User with UserID: {userDTO.Id} added successfully.");
        }

        [HttpDelete]
        [Route("api/user/{id}/Delete")]
        public IHttpActionResult DeleteUser(int id)         //View -> Admin
        {
            try
            {
                UserService userBusiness = new UserService();
                userBusiness.DeleteUser(id);

                return Ok($"User with UserID: {id} is deleted.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/user/{id}/update")]
        public IHttpActionResult PutUpdatedUserDetails(int id, UserModel userModel)         //View -> Admin & User(Id)
        {
            UserDTO userDTO = new UserDTO();
            userDTO.FullName = userModel.FullName;
            userDTO.ContactNo = userModel.ContactNo;
            userDTO.DateOfBirth = userModel.DateOfBirth;
            userDTO.Gender = userModel.Gender;

            bool result = userBusiness.UpdateUserDetails(id, userDTO);
            if (result)
            {
                return Ok($"Details of User with UserID: {id} has been successfully Updated!");
            }
            else
            {
                return Ok("Error occured during updating Try Again..");
            }
        }

        [HttpGet]
        [Route("api/user/getusersbyskill")]
        public IHttpActionResult GetUsersBySkill(string skillName)
        {
            List<UserDTO> users = userBusiness.GetUsersBySkill(skillName);

            if (users.Any())
            {
                List<UserModel> userModelList = mapper.Map<List<UserModel>>(users);
                List<DisplayModel> displayUsers = mapper.Map<List<DisplayModel>>(userModelList);

                return Ok(displayUsers);
            }
            else
            {
                return Ok("No users found with the specified skill.");
            }
        }

        [HttpPost]
        [Route("api/user/postloginuser")]
        public IHttpActionResult PostLoginUser(UserModel loginUser)         //View -> Admin & User
        {
            string email = loginUser.EmailId;
            string password = loginUser.Password;

            UserDTO authenticatedUser = userBusiness.AuthenticateUser(email, password);

            if (authenticatedUser != null)
            {
                if (authenticatedUser.IsAdmin)
                {
                    //List<UserDTO> allUsers = userBusiness.GetListOfUsers();
                    //List<UserModel> userModelList = mapper.Map<List<UserModel>>(allUsers);
                    //List<DisplayModel> displayUsers = mapper.Map<List<DisplayModel>>(userModelList);
                    //return Ok(displayUsers);
                    return Ok(new { userType = "admin" });
                }
                else
                {
                    //UserModel userModel = mapper.Map<UserModel>(authenticatedUser);
                    //return Ok(userModel);
                    return Ok(new { userType = "user", userId = authenticatedUser.Id, name = authenticatedUser.FullName});
                }
            }
            else
            {
                return BadRequest("Invalid EmailId or Password");
            }
        }

        [HttpPost]
        [Route("api/user/forgotpassword")]
        public IHttpActionResult ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            string email = forgotPasswordModel.EmailId;
            DateTime dob = forgotPasswordModel.DateOfBirth;
            string newPassword = forgotPasswordModel.NewPassword;
            string confirmPassword = forgotPasswordModel.ConfirmPassword;

            if (newPassword != confirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            if (userBusiness.VerifyUserEmailAndDOB(email, dob))
            {
                if (userBusiness.UpdatePassword(email, newPassword))
                {
                    return Ok("Password updated successfully.");
                }
                else
                {
                    return InternalServerError();
                }
            }
            else
            {
                return BadRequest("Invalid email or date of birth.");
            }
        }
    }
}
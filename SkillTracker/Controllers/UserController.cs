using AutoMapper;
using BusinessLogicLayer;
using SkillTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SkillTracker.Controllers
{
    public class UserController : ApiController
    {
        private readonly IMapper mapper;
        public UserController()
        {
            var mapConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDTO, UserModel>();
                cfg.CreateMap<UserModel, DisplayModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.EmailId, opt => opt.MapFrom(src => src.EmailId));
            });
            mapper = mapConfig.CreateMapper();
        }

        UserService userBusiness = new UserService();
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
            UserDTO user = userBusiness.GetUserDetails(id);

            if (user == null)
            {
                return Ok($"User with User ID: {id} could not be found");
            }
            UserModel userModel = mapper.Map<UserModel>(user);
            return Ok(userModel);
        }

        [HttpPost]
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
            return Ok($"User with User ID: {userDTO.Id} added successfully.");
        }

        [HttpDelete]
        [Route("api/User/{id}")]
        public IHttpActionResult DeleteUser(int id)         //View -> Admin
        {
            UserService userBusiness = new UserService();
            userBusiness.DeleteUser(id);

            return Ok($"User with User ID: {id} is deleted.");
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
                    List<UserDTO> allUsers = userBusiness.GetListOfUsers();
                    List<UserModel> userModelList = mapper.Map<List<UserModel>>(allUsers);
                    List<DisplayModel> displayUsers = mapper.Map<List<DisplayModel>>(userModelList);
                    return Ok(displayUsers);
                }
                else
                {
                    UserModel userModel = mapper.Map<UserModel>(authenticatedUser);
                    return Ok(userModel);
                }
            }
            else
            {
                return BadRequest("Invalid EmailId or Password");
            }
        }
    }
}
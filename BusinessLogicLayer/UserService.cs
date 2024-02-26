using AutoMapper;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogicLayer
{
    public class UserService
    {
        private readonly IMapper mapper;
        public UserService()
        {
            var mapConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => HashPassword(src.Password)));
            });
            mapper = mapConfig.CreateMapper();
        }

        DataAccessLayer.SkillTrackerDBEntities db = new SkillTrackerDBEntities();
        public List<UserDTO> GetListOfUsers()
        {
            DbSet<User> userDb = db.Users;

            List<UserDTO> users = new List<UserDTO>();
            foreach (var user in userDb)
            {
                if (user.IsAdmin != true)
                {
                    users.Add(MapUserToUserDTO(user));
                }
            }
            return users;
        }
        public List<UserDTO> GetListOfAdmins()
        {
            DbSet<User> userDb = db.Users;

            List<UserDTO> users = new List<UserDTO>();
            foreach (var user in userDb)
            {
                if (user.IsAdmin == true)
                {
                    users.Add(MapUserToUserDTO(user));
                }
            }
            return users;
        }
        public List<UserDTO> GetUserByIdOrName(string searchUser)
        {
            DbSet<User> userDb = db.Users;
            List<UserDTO> matchingUsers = new List<UserDTO>();

            if (int.TryParse(searchUser, out int userId))
            {
                var userById = userDb.FirstOrDefault(u => u.Id == userId);
                if (userById != null)
                {
                    matchingUsers.Add(MapUserToUserDTO(userById));
                    return matchingUsers;
                }
            }
            var userByName = userDb.Where(u => u.FullName.StartsWith(searchUser)).ToList();
            if (userByName.Any())
            {
                matchingUsers.AddRange(userByName.Select(u => MapUserToUserDTO(u)));
            }
            return matchingUsers;
        }
        public UserDTO GetUserDetails(int userId)
        {
            User user = db.Users.Find(userId);
            UserDTO userDTO = mapper.Map<UserDTO>(user);
            return userDTO;
        }
        public void AddUser(UserDTO newUser)
        {
            DbSet<User> userDb = db.Users;

            if (userDb.Any(u => u.EmailId == newUser.EmailId))
            {
                throw new InvalidOperationException("User with the same EmailId already exists.");
            }
            User user = new User();
            user.EmailId = newUser.EmailId;
            user.Password = newUser.Password;

            userDb.Add(user);
            db.SaveChanges();

            newUser.Id = user.Id;
        }
        public void DeleteUser(int userId)
        {
            User user = db.Users.Find(userId);
            db.Users.Remove(user);

            db.SaveChanges();
        }
        public bool UpdateUserDetails(int id, UserDTO userDTO)
        {
            var existingUser = db.Users.FirstOrDefault(u => u.Id == id);
            if (existingUser != null)
            {
                existingUser.FullName = userDTO.FullName;
                existingUser.ContactNo = userDTO.ContactNo;
                existingUser.DateOfBirth = userDTO.DateOfBirth;
                existingUser.Gender = userDTO.Gender;
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public UserDTO AuthenticateUser(string email, string password)
        {
            DbSet<User> userDb = db.Users;
            var authenticatedUser = userDb.FirstOrDefault(u => u.EmailId == email && u.Password == password);

            if (authenticatedUser != null)
            {
                UserDTO userDTO = mapper.Map<UserDTO>(authenticatedUser);
                return userDTO;
            }
            return null;
        }
        public string HashPassword(string password)
        {
            using (SHA512 sha256Hash = SHA512.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private UserDTO MapUserToUserDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                EmailId = user.EmailId,
            };
        }
    }
}
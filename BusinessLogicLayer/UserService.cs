﻿using AutoMapper;
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
                cfg.CreateMap<User, UserDTO>();
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
                var userById = userDb.FirstOrDefault(user => user.Id == userId);
                if (userById != null)
                {
                    matchingUsers.Add(MapUserToUserDTO(userById));
                    return matchingUsers;
                }
            }
            var userByName = userDb.Where(user => user.FullName.StartsWith(searchUser)).ToList();
            if (userByName.Any())
            {
                matchingUsers.AddRange(userByName.Select(user => MapUserToUserDTO(user)));
            }
            return matchingUsers;
        }
        public AllUserDetailsDTO GetUserDetails(int userId)
        {
            User user = db.Users.Find(userId);
            List<UserSkill> userSkills = db.UserSkills
                                        .Where(skill => skill.UserId == userId)
                                        .ToList();
            var userDetails = userSkills
            .Join(db.Skills, us => us.SkillId, skill => skill.Id, (us, skill) => new UpdateUserSkillDTO
            {
                Name = skill.Name,
                Proficiency = us.Proficiency
            }).ToList();

            List<UpdateUserSkillDTO> updateUserSkillsDTOs = new List<UpdateUserSkillDTO>();
            foreach (var item in userDetails)
            {
                UpdateUserSkillDTO updateUserSkillsDTO = new UpdateUserSkillDTO
                {
                    Name = item.Name,
                    Proficiency = item.Proficiency
                };
                updateUserSkillsDTOs.Add(updateUserSkillsDTO);
            }

            AllUserDetailsDTO getUserDetailsDTO = new AllUserDetailsDTO();
            getUserDetailsDTO.Id = user.Id;
            getUserDetailsDTO.EmailId = user.EmailId;
            getUserDetailsDTO.Password = user.Password;
            getUserDetailsDTO.FullName = user.FullName;
            getUserDetailsDTO.DateOfBirth = user.DateOfBirth;
            getUserDetailsDTO.ContactNo = user.ContactNo; 
            getUserDetailsDTO.Gender = user.Gender;

            getUserDetailsDTO.Skills = new List<UpdateUserSkillDTO>();
            foreach (var skill in updateUserSkillsDTOs)
            {
                UpdateUserSkillDTO skillDTO = new UpdateUserSkillDTO
                {
                    Name = skill.Name,
                    Proficiency = skill.Proficiency
                };
                getUserDetailsDTO.Skills.Add(skillDTO);
            }
            return getUserDetailsDTO;
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
            user.Password = HashPassword(newUser.Password);

            userDb.Add(user);
            db.SaveChanges();

            newUser.Id = user.Id;
        }
        public void DeleteUser(int userId)
        {
            using (var db = new SkillTrackerDBEntities())
            {
                User user = db.Users.Find(userId);

                if (user != null)
                {
                    var userSkills = db.UserSkills.Where(us => us.UserId == userId);
                    foreach (var userSkill in userSkills)
                    {
                        db.UserSkills.Remove(userSkill);
                    }
                    db.Users.Remove(user);
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception($"User with ID {userId} not found.");
                }
            }
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

        public List<UserDTO> GetUsersBySkill(string skillName)
        {
            DbSet<UserSkill> userSkillDb = db.UserSkills;

            var usersWithSkill = userSkillDb
                .Where(us => us.Skill.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))
                .Select(us => us.User).ToList();

            return usersWithSkill.Select(user => MapUserToUserDTO(user)).ToList();
        }
        public UserDTO AuthenticateUser(string email, string password)
        {
            DbSet<User> userDb = db.Users;
            string normalPassword = password;
            string hashedPassword = HashPassword(password);

            var authenticatedUser = userDb.FirstOrDefault(u => u.EmailId == email && (u.Password == hashedPassword || u.Password == normalPassword));

            if (authenticatedUser != null)
            {
                UserDTO userDTO = mapper.Map<UserDTO>(authenticatedUser);
                return userDTO;
            }
            return null;
        }
        public bool VerifyUserEmailAndDOB(string email, DateTime dob)
        {
            DbSet<User> userDb = db.Users;
            return userDb.Any(user => user.EmailId == email && user.DateOfBirth == dob);
        }
        public bool UpdatePassword(string email, string newPassword)
        {
            DbSet<User> userDb = db.Users;
            var userToUpdate = userDb.FirstOrDefault(user => user.EmailId == email);

            if (userToUpdate != null)
            {
                userToUpdate.Password = HashPassword(newPassword);
                db.SaveChanges();
                return true;
            }
            return false;
        }
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                byte[] truncatedBytes = new byte[10];
                Array.Copy(bytes, truncatedBytes, 10);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < truncatedBytes.Length; i++)
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
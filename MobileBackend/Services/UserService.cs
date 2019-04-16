﻿using AutoMapper;
using MobileBackend.DTO;
using MobileBackend.Models.Domain;
using MobileBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IEncrypter encrypter;

        public UserService(IUserRepository userRepository, IMapper mapper, IEncrypter encrypter)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.encrypter = encrypter;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var allUsers = await userRepository.GetAllAsync();
            return allUsers.Select(n => mapper.Map<User, UserDto>(n)).ToList();
        }

        public async Task<UserDto> GetUserAsync(string email)
        {
            var user = await userRepository.GetUserAsync(email.Trim().ToLower());

            return mapper.Map<User, UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(string username)
        {
            var users = await userRepository.GetUsersAsync(username);
            return users.Select(n => mapper.Map<User, UserDto>(n)).ToList();
        }

        public async Task RegisterAsync(string email, string username, string password)
        {
            var user = await userRepository.GetUserAsync(email);
            if(user != null)
            {
                throw new Exception($"User {email} already exists");
            }

            var salt = encrypter.GetSalt(password);
            var hash = encrypter.GetHash(password, salt);
            user = new User(email, username, hash, salt);
            await userRepository.AddAsync(user);
        }

        public async Task LoginAsync(string email, string password)
        {
            var user = await userRepository.GetUserAsync(email.Trim().ToLower());

            if(user == null)
            {
                throw new ArgumentNullException("Invalid email or password");
            }

            var salt = encrypter.GetSalt(password);
            var hash = encrypter.GetHash(password, salt);

            if (password.Equals(hash))
            {
                return;
            }
            else
            {
                throw new Exception("Invalid email or password");
            }
        }
    }
}

﻿using gcapi.Enums;
using System.ComponentModel.DataAnnotations;

namespace gcapi.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string SecondName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? TelegramId { get; set; }
        public List<EventModel>? UserEvents { get; set; } = new List<EventModel>();


        //Аккаунт
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string? Salt { get; set; }
        public Roles? Role { get; set; } = Roles.User;
    }
}

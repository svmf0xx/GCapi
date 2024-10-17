﻿using gcapi.DataBase;
using gcapi.Models;
using Microsoft.EntityFrameworkCore;
using gcapi.Dto;
using gcapi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace gcapi.Realizations
{
    public class GroupService(gContext gContext, IUserService userService) : IGroupService
    {
        private readonly gContext _context = gContext;
        private readonly IUserService _userService = userService;

        public async Task<IActionResult> AddGroup(GroupDto gr)
        {
            try
            {
                var users = await _context.UserTable.Where(u => gr.GroupUsers.Contains(u.Username)).ToListAsync();

                var newGroup = new GroupModel
                {
                    Name = gr.Name,
                    GroupUsers = users
                };
                _context.Add(newGroup);
                await _context.SaveChangesAsync();
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        public async Task<IActionResult> EditGroup(GroupDto gr)
        {
            var users = await _context.UserTable.Where(u => gr.GroupUsers.Contains(u.Username)).ToListAsync();

            var theGroup = await _context.GroupTable.Where(g => g.Id == gr.Id).FirstOrDefaultAsync();
            if (theGroup != null)
            {
                theGroup.Name = gr.Name;
                theGroup.GroupUsers = users;
                _context.Update(theGroup);
                await _context.SaveChangesAsync();
                return new OkResult();
            }
            return new BadRequestObjectResult("Группы не существует");
        }

        public async Task<List<GroupModel>> GetAllGroups()
        {
            return await _context.GroupTable.Include(g => g.GroupUsers).ToListAsync();
        }

        public async Task<List<GroupModel>> GetUserGroups(Guid userId)
        {
            var theUser = await _context.UserTable.FindAsync(userId);

            if (theUser != null)
                return await _context.GroupTable.Where(g => g.GroupUsers.Contains(theUser)).ToListAsync();

            else throw new NullReferenceException();
        }

        public async Task<List<UserModel>> GetUsersFromGroup(Guid id)
        {
            var theGroup = await _context.GroupTable.FindAsync(id);
            if (theGroup != null)
                return theGroup.GroupUsers;

            else throw new NullReferenceException();
        }

        public async Task<IActionResult> RemoveGroup(Guid id)
        {
            var theGroup = await _context.GroupTable.FindAsync(id);
            if (theGroup != null)
            {
                var theEvents = await _context.EventTable.Where(ev => theGroup.GroupEvents.Contains(ev)).ToListAsync();
                var thePlans = await _context.PlanTable.Where(p => theGroup.GroupPlans.Contains(p)).ToListAsync();
                var theUsers = await _context.UserTable.Where(u => theGroup.GroupUsers.Contains(u)).ToListAsync();

                foreach (var theUser in theUsers)
                {
                    foreach (var theEvent in theEvents)
                    {
                        theUser.Events.Remove(theEvent);
                        _context.Remove(theEvent);
                        _context.Update(theUser);
                    }
                    foreach (var thePlan in thePlans)
                    {
                        theUser.Plans.Remove(thePlan);
                        _context.Remove(thePlan);
                        _context.Update(theUser);
                    }
                    theUser.Groups.Remove(theGroup);
                }
                await _context.SaveChangesAsync();
                return new OkResult();
            }
            else return new BadRequestObjectResult("Группы не существует");

        }





        //invite code
        static string GenerateInvite(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        public async Task<string> GetInvite(Guid grId, Guid userId)
        {
            string rcode;
            try
            {
                var gr = await _context.GroupTable.FindAsync(grId);
                var user = await _context.UserTable.FindAsync(userId);

                var inv = await _context.InviteCodeTable.FindAsync(gr);

                if (user == null || inv == null)
                {
                    return String.Empty;
                }
                if (inv == null)
                {
                    var code = GenerateInvite();
                    var newInv = new InviteCodeModel
                    {
                        Code = code,
                        Owner = user,
                        Group = gr,
                        CreatedAt = DateTime.Now,
                        ExpiredAt = DateTime.Now.AddDays(3)
                    };
                    _context.InviteCodeTable.Add(newInv);
                    rcode = code;
                }
                else
                {
                    inv.ExpiredAt = DateTime.Now.AddDays(3);
                    _context.InviteCodeTable.Update(inv);
                    rcode = inv.Code;
                }
                await _context.SaveChangesAsync();
                return rcode;
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public async Task<Guid> CheckInvite(string code)
        {
            var inv = await _context.InviteCodeTable.FindAsync(code);
            if (inv != null)
            {
                return inv.Group.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}

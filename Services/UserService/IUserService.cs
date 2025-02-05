using DowntimeTracker.Data;
using DowntimeTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace DowntimeTracker.Services
{
    public interface IUserService
    {
        Task UpdateUserLoginAsync(string userAd);
    }

    public class UserService : IUserService
    {
        private readonly TCZNT5000 _context;

        public UserService(TCZNT5000 context)
        {
            _context = context;
        }

        public async Task UpdateUserLoginAsync(string userAd)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserAd == userAd);
            if (user != null)
            {
                var userLog = await _context.UserLogs.FirstOrDefaultAsync(ul => ul.UserId == user.UserId);
                var currentTime = DateTime.Now;

                if (userLog == null)
                {
                    userLog = new UserLog
                    {
                        UserId = user.UserId,
                        LastLogin = currentTime,
                        CurrentLogin = currentTime
                    };
                    _context.UserLogs.Add(userLog);
                }
                else
                {
                    userLog.LastLogin = userLog.CurrentLogin;
                    userLog.CurrentLogin = currentTime;
                    _context.UserLogs.Update(userLog);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
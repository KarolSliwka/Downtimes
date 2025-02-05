using DiscrepancyReport.Services.MessageService;
using DowntimeTracker.Data;
using Microsoft.EntityFrameworkCore;
using static DowntimeTracker.Services.AccessService;

namespace DowntimeTracker.Services
{
    public interface IAccessService
    {
        Task<(List<UserAccessDto> AdminUsers, List<UserAccessDto> NormalUsers)> CheckUserAccessAsync();
        Task ChangeUserAccessAsync();
    }

    public class AccessService : IAccessService
    {
        private readonly TCZNT5000 _context;
        private readonly MessageServices _message; // Ensure this is the correct class name

        public AccessService(TCZNT5000 context, MessageServices message)
        {
            _context = context;
            _message = message;
        }

        public async Task<(List<UserAccessDto> AdminUsers, List<UserAccessDto> NormalUsers)> CheckUserAccessAsync()
        {
            var usersWithAccess = await _context.Users
                .Include(u => u.UserLogs)
                .Where(u => u.AccessLevel != "noaccess")
                .OrderBy(u => u.NameSurname)
                .Select(u => new UserAccessDto
                {
                    UserId = u.UserId,
                    NameSurname = u.NameSurname,
                    UserAd = u.UserAd,
                    AccessLevel = u.AccessLevel,
                    DaysSinceLastLogin = u.UserLogs
                        .OrderByDescending(ul => ul.LastLogin)
                        .Select(ul => (DateTime.Now - ul.LastLogin).Days)
                        .FirstOrDefault() // This will return 0 if there are no logs
                })
                .ToListAsync();

            // Separate admin users and normal users
            var adminUsers = usersWithAccess.Where(uwa => uwa.AccessLevel != "user").ToList();
            var normalUsers = usersWithAccess.Where(uwa => uwa.AccessLevel == "user" && uwa.DaysSinceLastLogin >= 30).ToList();

            // Combine the lists into a single message
            var message = _message.AccessReview(adminUsers.Any() ? adminUsers : null, normalUsers.Any() ? normalUsers : null);
            await _message.SendMessage(message);

            return (adminUsers, normalUsers);
        }

        public async Task ChangeUserAccessAsync()
        {
            var usersToChange = await _context.Users
                .Include(u => u.UserLogs)
                .Where(u => u.AccessLevel != "noaccess")
                .OrderBy(u => u.NameSurname)
                .Select(u => new UserAccessDto
                {
                    UserId = u.UserId,
                    NameSurname = u.NameSurname,
                    UserAd = u.UserAd,
                    AccessLevel = u.AccessLevel,
                    DaysSinceLastLogin = u.UserLogs
                        .OrderByDescending(ul => ul.LastLogin)
                        .Select(ul => (DateTime.Now - ul.LastLogin).Days)
                        .FirstOrDefault() // This will return 0 if there are no logs
                })
                .ToListAsync();

            // Get all users with access who haven't logged in for 70 days
            var allUsers = usersToChange.Where(uwa => uwa.DaysSinceLastLogin >= 70).ToList();

            // Change users' access
            if (allUsers.Count() > 0)
            {
                foreach (var user in allUsers)
                {
                    // Update the access level for each user
                    var userToUpdate = await _context.Users.FindAsync(user.UserId);
                    if (userToUpdate != null)
                    {
                        userToUpdate.AccessLevel = "noaccess";
                    }
                }
                await _context.SaveChangesAsync();
            }

            // No return statement needed, method will complete without returning anything
        }

        public class UserAccessDto
        {
            public int UserId { get; set; }
            public string NameSurname { get; set; }
            public string UserAd { get; set; }
            public string AccessLevel { get; set; }
            public int DaysSinceLastLogin { get; set; }
        }
    }
}
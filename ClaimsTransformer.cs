using DowntimeTracker.Data;
using DowntimeTracker.Models;
using Microsoft.AspNetCore.Authentication;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;

namespace DowntimeTracker
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly TCZNT5000 _context;

        public ClaimsTransformer(TCZNT5000 context)
        {
            _context = context;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var userName = identity.Name;
            var user = _context.Users.SingleOrDefault(t => t.UserAd == userName);
            if (user == null)
            {
                TryGetUserFullName(userName, out string name, out string surname);
                user = new User
                {
                    UserAd = userName,
                    NameSurname = name + " " + surname,
                    AccessLevel = "noaccess",
                    HrmAvailable = false
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                //_context.Entry(user).Reference(t => t.Role).Load();
            }

            var claim = new Claim(identity.RoleClaimType, user.AccessLevel);
            var claimHrm = new Claim(identity.RoleClaimType, user.HrmAvailable ? "isHrm" : "isNotHrm");

            identity.AddClaim(claim);
            identity.AddClaim(claimHrm);

            string currentUser = userName?.Substring(7);
            string position = _context?.Personel?.FirstOrDefault((Personel o) => o.EmpAd == currentUser)?.EmpPosition;
            string userRole = _context?.Users?.FirstOrDefault((User o) => o.UserAd == userName).AccessLevel;
            bool hrmAccess = false;
            if (position != null)
            {
                if (position != null && !position.Contains("TEAM LEADER"))
                {
                    hrmAccess = true;
                }
                Claim claimAccess2 = new Claim(identity.RoleClaimType, hrmAccess ? "isAccess" : "isNotAccess");
                identity.AddClaim(claimAccess2);
            }
            else if (userRole == "super")
            {
                hrmAccess = true;
                Claim claimAccess = new Claim(identity.RoleClaimType, hrmAccess ? "isAccess" : "isNotAccess");
                identity.AddClaim(claimAccess);
            }

            return Task.FromResult(principal);
        }

        private void TryGetUserFullName(string user, out string name, out string surname)
        {
            try
            {
                using var contextP = new PrincipalContext(ContextType.Domain);
                var principal = UserPrincipal.FindByIdentity(contextP, user);
                name = principal.GivenName;
                surname = principal.Surname;
            }
            catch (Exception e)
            {
                name = "-";
                surname = "-";
            }
        }
    }
}

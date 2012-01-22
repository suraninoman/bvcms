using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace CmsData
{
    public class MembershipService
    {
        private MembershipProvider _provider;

        public MembershipService()
            : this(null)
        {
        }

        public MembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            return _provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }
        public static User CreateUser(int PeopleId, string username, string password)
        {
            CMSMembershipProvider.provider.AdminOverride = true;
            var user = CMSMembershipProvider.provider.NewUser(
                username,
                password,
                null,
                true,
                PeopleId);
            CMSMembershipProvider.provider.AdminOverride = false;
            return user;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
            return currentUser.ChangePassword(oldPassword, newPassword);
        }
        public static bool ChangePassword(string userName, string newPassword)
        {
            CMSMembershipProvider.provider.AdminOverride = true;
            var mu = CMSMembershipProvider.provider.GetUser(userName, false);
            mu.UnlockUser();
            var ret = mu.ChangePassword(mu.ResetPassword(), newPassword);
            CMSMembershipProvider.provider.AdminOverride = false;
            return ret;
        }
        public static string FetchUsername(CMSDataContext Db, string first, string last)
        {
            var username = first.Trim().ToLower()[0] + last.Trim().ToLower();
            var uname = username;
            var i = 1;
            while (Db.Users.SingleOrDefault(u => u.Username == uname) != null)
                uname = username + i++;
            return uname;
        }
        public static string FetchUsernameNoCheck(string first, string last)
        {
            return first.ToLower()[0] + last.ToLower();
        }
        public static string FetchPassword(CMSDataContext Db)
        {
            var rnd = new Random();
            var n = Db.Words.Count();
            var r1 = rnd.Next(1, n);
            var r2 = rnd.Next(1, n);
            var q = from w in Db.Words
                    where w.N == r1 || w.N == r2
                    select w.WordX;
            var a = q.ToArray();
            if (a.Length == 2)
                return a[0] + "." + a[1];
            return a[0] + "." + a[0];
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Web;
using UtilityExtensions;
using CmsData;
using System.Web.Mvc;
using System.Data.Linq.SqlClient;


namespace CmsWeb.Models
{
    public class CheckInModel
    {
        public List<FamilyInfo> Match(string id, int campus, int thisday)
        {
            var ph = Util.GetDigits(id).PadLeft(10, '0');
            var p7 = ph.Substring(3);
            var ac = ph.Substring(0, 3);
            var q1 = from f in DbUtil.Db.Families
                     where f.HeadOfHousehold.DeceasedDate == null
                     where f.HomePhoneLU.StartsWith(p7)
                        || f.People.Any(p => p.CellPhoneLU.StartsWith(p7))
                     let flock = f.FamilyCheckinLocks
                        .SingleOrDefault(l => SqlMethods.DateDiffSecond(l.Created, DateTime.Now) < 60)
                     orderby f.FamilyId
                     select new FamilyInfo
                     {
                         FamilyId = f.FamilyId,
                         AreaCode = f.HomePhoneAC,
                         Name = f.HeadOfHousehold.Name,
                         Phone = id,
                         Locked = flock == null ? false : flock.Locked,
                     };
            var matches = q1.ToList();
            if (matches.Count > 1)
                matches = matches.Where(m => m.AreaCode == ac || ac == "000").ToList();
            return matches;
        }
        public List<Attendee> FamilyMembers(int id, int campus, int thisday)
        {
            var now = Util.Now;
            // get org members first
            var members =
                from om in DbUtil.Db.OrganizationMembers
                let Hour = DbUtil.Db.GetTodaysMeetingHour(om.OrganizationId, thisday)
                let CheckedIn = DbUtil.Db.GetAttendedTodaysMeeting(om.OrganizationId, thisday, om.PeopleId)
                let recreg = om.Person.RecRegs.FirstOrDefault()
                where om.Organization.CanSelfCheckin.Value
                where (om.Pending ?? false) == false
                where om.Organization.CampusId == campus || om.Organization.CampusId == null
                where om.Organization.OrganizationStatusId == (int)Organization.OrgStatusCode.Active
                where om.Person.FamilyId == id
                where om.Person.DeceasedDate == null
                where Hour != null
                select new Attendee
                {
                    Id = om.PeopleId,
                    Position = om.Person.PositionInFamilyId,
                    MemberVisitor = "M",
                    Name = om.Person.Name,
                    First = om.Person.PreferredName,
                    Last = om.Person.LastName,
                    BYear = om.Person.BirthYear,
                    BMon = om.Person.BirthMonth,
                    BDay = om.Person.BirthDay,
                    Class = om.Organization.OrganizationName,
                    Leader = om.Organization.LeaderName,
                    OrgId = om.OrganizationId,
                    Location = om.Organization.Location,
                    Age = om.Person.Age ?? 0,
                    Gender = om.Person.Gender.Code,
                    NumLabels = om.MemberTypeId ==
                        (int)CmsData.OrganizationMember.MemberTypeCode.Member ?
                            (om.Organization.NumCheckInLabels ?? 1)
                            : (om.Organization.NumWorkerCheckInLabels ?? 0),
                    Hour = Hour,
                    CheckedIn = CheckedIn ?? false,

                    goesby = om.Person.NickName,
                    email = om.Person.EmailAddress,
                    addr = om.Person.Family.AddressLineOne,
                    zip = om.Person.Family.ZipCode,
                    home = om.Person.Family.HomePhone,
                    cell = om.Person.CellPhone,
                    marital = om.Person.MaritalStatusId,
                    gender = om.Person.GenderId,
                    CampusId = om.Person.CampusId,
                    allergies = recreg.MedicalDescription,
                    emfriend = recreg.Emcontact,
                    emphone = recreg.Emphone,
                    activeother = recreg.ActiveInAnotherChurch ?? false,
                    parent = recreg.Mname ?? recreg.Fname,
                    grade = om.Person.Grade,
                    HasPicture = om.Person.PictureId != null,
                    Custody = (om.Person.CustodyIssue ?? false) == true,
                    Transport = (om.Person.OkTransport ?? false) == true,
                    RequiresSecurityLabel = (om.MemberTypeId == 220) && (om.Person.Age ?? 0) < 18 && (om.Organization.NoSecurityLabel ?? false) == false,
                    church = om.Person.OtherNewChurch,
                };

            // now get recent visitors

            var visitors =
                from a in DbUtil.Db.Attends
                let Hour1 = DbUtil.Db.GetTodaysMeetingHour(a.OrganizationId, thisday)
                where a.Person.FamilyId == id
                where a.Person.DeceasedDate == null
                where a.Organization.CanSelfCheckin.Value
                where a.Organization.CampusId == campus || a.Organization.CampusId == null
                where a.AttendanceFlag && a.MeetingDate >= a.Organization.VisitorDate.Value.Date
                where Attend.VisitAttendTypes.Contains(a.AttendanceTypeId.Value)
                where !a.Organization.OrganizationMembers.Any(om => om.PeopleId == a.PeopleId)
                where Hour1 != null
                group a by new { a.PeopleId, a.OrganizationId } into g
                let a = g.OrderByDescending(att => att.MeetingDate).First()
                let recreg = a.Person.RecRegs.FirstOrDefault()
                select new Attendee
                {
                    Id = a.PeopleId,
                    Position = a.Person.PositionInFamilyId,
                    MemberVisitor = "V",
                    Name = a.Person.Name,
                    First = a.Person.PreferredName,
                    Last = a.Person.LastName,
                    BYear = a.Person.BirthYear,
                    BMon = a.Person.BirthMonth,
                    BDay = a.Person.BirthDay,
                    Class = a.Organization.OrganizationName,
                    Leader = a.Organization.LeaderName,
                    OrgId = a.OrganizationId,
                    //OrgId = (a.Organization.CanSelfCheckin ?? false) ? a.OrganizationId : 0,
                    Location = a.Organization.Location,
                    Age = a.Person.Age ?? 0,
                    Gender = a.Person.Gender.Code,
                    NumLabels = a.Organization.NumCheckInLabels ?? 1,
                    Hour = DbUtil.Db.GetTodaysMeetingHour(a.OrganizationId, thisday),
                    CheckedIn = DbUtil.Db.GetAttendedTodaysMeeting(a.OrganizationId, thisday, a.PeopleId) ?? false,

                    goesby = a.Person.NickName,
                    email = a.Person.EmailAddress,
                    addr = a.Person.Family.AddressLineOne,
                    zip = a.Person.Family.ZipCode,
                    home = a.Person.Family.HomePhone,
                    cell = a.Person.CellPhone,
                    marital = a.Person.MaritalStatusId,
                    gender = a.Person.GenderId,
                    CampusId = a.Person.CampusId,
                    allergies = recreg.MedicalDescription,
                    emfriend = recreg.Emcontact,
                    emphone = recreg.Emphone,
                    activeother = recreg.ActiveInAnotherChurch ?? false,
                    parent = recreg.Mname ?? recreg.Fname,
                    grade = a.Person.Grade,
                    HasPicture = a.Person.PictureId != null,
                    Custody = (a.Person.CustodyIssue ?? false) == true,
                    Transport = (a.Person.OkTransport ?? false) == true,
                    RequiresSecurityLabel = ((a.Person.Age ?? 0) < 18) && (a.Organization.NoSecurityLabel ?? false) == false,
                    church = a.Person.OtherNewChurch,
                };

            var list = members.ToList();
            list.AddRange(visitors.ToList());

            // now get rest of family
            const string PleaseVisit = "No self check-in meetings available";
            var VisitorOrgName = PleaseVisit;
            var VisitorOrgId = 0;
            var VisitorOrgHour = (DateTime?)null;
            // find a org on campus that allows an older, new visitor to check in to
            var qv = from o in DbUtil.Db.Organizations
                     where o.CampusId == campus || o.CampusId == null
                     where o.CanSelfCheckin == true
                     where o.AllowNonCampusCheckIn == true
                     where o.SchedDay == thisday
                     select o;
            var vo = qv.FirstOrDefault();
            if (vo != null)
            {
                VisitorOrgName = vo.OrganizationName;
                VisitorOrgId = vo.OrganizationId;
                VisitorOrgHour = vo.SchedTime;
            }
            var otherfamily =
                from p in DbUtil.Db.People
                where p.FamilyId == id
                where p.DeceasedDate == null
                where !list.Select(a => a.Id).Contains(p.PeopleId)
                let oldervisitor = (p.CampusId != campus || p.CampusId == null) && p.Age > 12
                let recreg = p.RecRegs.FirstOrDefault()
                select new Attendee
                {
                    Id = p.PeopleId,
                    Position = p.PositionInFamilyId,
                    Name = p.Name,
                    First = p.PreferredName,
                    Last = p.LastName,
                    BYear = p.BirthYear,
                    BMon = p.BirthMonth,
                    BDay = p.BirthDay,
                    Class = oldervisitor ? VisitorOrgName : PleaseVisit,
                    OrgId = oldervisitor ? VisitorOrgId : 0,
                    Leader = "",
                    Age = p.Age ?? 0,
                    Gender = p.Gender.Code,
                    NumLabels = 1,
                    Hour = VisitorOrgHour,

                    goesby = p.NickName,
                    email = p.EmailAddress,
                    addr = p.Family.AddressLineOne,
                    zip = p.Family.ZipCode,
                    home = p.Family.HomePhone,
                    cell = p.CellPhone,
                    marital = p.MaritalStatusId,
                    gender = p.GenderId,
                    CampusId = p.CampusId,
                    allergies = recreg.MedicalDescription,
                    emfriend = recreg.Emcontact,
                    emphone = recreg.Emphone,
                    activeother = recreg.ActiveInAnotherChurch ?? false,
                    parent = recreg.Mname ?? recreg.Fname,
                    grade = p.Grade,
                    HasPicture = p.PictureId != null,
                    Custody = p.CustodyIssue ?? false,
                    Transport = p.OkTransport ?? false,
                    RequiresSecurityLabel = false,
                    church = p.OtherNewChurch,
                };
            list.AddRange(otherfamily.ToList());
            var list2 = list.OrderBy(a => a.Position)
                .ThenByDescending(a => a.Position == 10 ? a.Gender : "U")
                .ThenBy(a => a.Age).ToList();
            return list2;
        }
        private bool HasImage(int? imageid)
        {
            var q = from i in ImageData.DbUtil.Db.Images
                    where i.Id == imageid
                    select i.Length;
            var len = q.SingleOrDefault();
            return len > 0;
        }
        public List<Attendee> FamilyMembersKiosk(int id, int campus)
        {
            DbUtil.Db.SetNoLock();
            var now = Util.Now;
            // get org members first
            var members =
                from om in DbUtil.Db.OrganizationMembers
                where om.Organization.AllowKioskRegister == true
                where om.Organization.CampusId == campus || campus == 0
                where om.Person.FamilyId == id
                where (om.Pending ?? false) == false
                where om.Person.DeceasedDate == null
                let recreg = om.Person.RecRegs.FirstOrDefault()
                select new Attendee
                {
                    Id = om.PeopleId,
                    Position = om.Person.PositionInFamilyId,
                    MemberVisitor = "M",
                    Name = om.Person.Name,
                    First = om.Person.PreferredName,
                    Last = om.Person.LastName,
                    BYear = om.Person.BirthYear,
                    BMon = om.Person.BirthMonth,
                    BDay = om.Person.BirthDay,
                    Class = om.Organization.OrganizationName,
                    Leader = om.Organization.LeaderName,
                    OrgId = om.OrganizationId,
                    Location = om.Organization.Location,
                    Age = om.Person.Age ?? 0,
                    Gender = om.Person.Gender.Code,
                    NumLabels = om.MemberTypeId ==
                        (int)CmsData.OrganizationMember.MemberTypeCode.Member ?
                            (om.Organization.NumCheckInLabels ?? 1)
                            : (om.Organization.NumWorkerCheckInLabels ?? 0),
                    CheckedIn = true,

                    goesby = om.Person.NickName,
                    email = om.Person.EmailAddress,
                    addr = om.Person.Family.AddressLineOne,
                    zip = om.Person.Family.ZipCode,
                    home = om.Person.Family.HomePhone,
                    cell = om.Person.CellPhone,
                    marital = om.Person.MaritalStatusId,
                    gender = om.Person.GenderId,
                    CampusId = om.Person.CampusId,
                    allergies = recreg.MedicalDescription,
                    emfriend = recreg.Emcontact,
                    emphone = recreg.Emphone,
                    activeother = recreg.ActiveInAnotherChurch ?? false,
                    parent = recreg.Mname ?? recreg.Fname,
                    grade = om.Person.Grade,
                    church = om.Person.OtherNewChurch,
                    HasPicture = om.Person.PictureId != null,
                };

            var list = members.ToList();

            // now get rest of family
            const string PleaseVisit = "No class assigned yet";
            var otherfamily =
                from p in DbUtil.Db.People
                where p.FamilyId == id
                where p.DeceasedDate == null
                where !list.Select(a => a.Id).Contains(p.PeopleId)
                let recreg = p.RecRegs.FirstOrDefault()
                select new Attendee
                {
                    Id = p.PeopleId,
                    Position = p.PositionInFamilyId,
                    Name = p.Name,
                    First = p.PreferredName,
                    Last = p.LastName,
                    BYear = p.BirthYear,
                    BMon = p.BirthMonth,
                    BDay = p.BirthDay,
                    Class = PleaseVisit,
                    OrgId = 0,
                    Age = p.Age ?? 0,
                    Gender = p.Gender.Code,
                    NumLabels = 1,

                    goesby = p.NickName,
                    email = p.EmailAddress,
                    addr = p.Family.AddressLineOne,
                    zip = p.Family.ZipCode,
                    home = p.Family.HomePhone,
                    cell = p.CellPhone,
                    marital = p.MaritalStatusId,
                    gender = p.GenderId,
                    CampusId = p.CampusId,
                    allergies = recreg.MedicalDescription,
                    emfriend = recreg.Emcontact,
                    emphone = recreg.Emphone,
                    activeother = recreg.ActiveInAnotherChurch ?? false,
                    parent = recreg.Mname ?? recreg.Fname,
                    grade = p.Grade,
                    church = p.OtherNewChurch,
                    HasPicture = p.PictureId != null,
                };
            list.AddRange(otherfamily.ToList());
            var list2 = list.OrderBy(a => a.Position)
                .ThenByDescending(a => a.Position == 10 ? a.Gender : "U")
                .ThenBy(a => a.Age).ToList();
            return list2;
        }
        public IEnumerable<Campu> Campuses()
        {
            var q = from c in DbUtil.Db.Campus
                    where c.Organizations.Any(o => o.CanSelfCheckin == true)
                    orderby c.Id
                    select c;
            return q;
        }
        public void RecordAttend(int PeopleId, int OrgId, bool Present, int thisday)
        {
            var q = from o in DbUtil.Db.Organizations
                    where o.OrganizationId == OrgId
                    let p = DbUtil.Db.People.Single(pp => pp.PeopleId == PeopleId)
                    select new
                    {
                        MeetingId = DbUtil.Db.GetTodaysMeetingId(OrgId, thisday),
                        MeetingTime = DbUtil.Db.GetTodaysMeetingHour(OrgId, thisday),
                        o.AttendTrkLevelId,
                        o.Location,
                        OrgEntryPoint = o.EntryPointId,
                        p.EntryPointId,
                    };
            var info = q.Single();
            var meeting = DbUtil.Db.Meetings.SingleOrDefault(m => m.MeetingId == info.MeetingId);
            if (info.EntryPointId == null)
            {
                var p = DbUtil.Db.LoadPersonById(PeopleId);
                if (info.OrgEntryPoint > 0)
                    p.EntryPointId = info.OrgEntryPoint;
            }
            if (meeting == null)
            {
                meeting = new CmsData.Meeting
                {
                    OrganizationId = OrgId,
                    MeetingDate = info.MeetingTime,
                    CreatedDate = Util.Now,
                    CreatedBy = Util.UserId1,
                    GroupMeetingFlag = info.AttendTrkLevelId
                        == (int)CmsData.Organization.AttendTrackLevelCode.Headcount,
                    Location = info.Location,
                };
                DbUtil.Db.Meetings.InsertOnSubmit(meeting);
                DbUtil.Db.SubmitChanges();
            }
            Attend.RecordAttendance(PeopleId, meeting.MeetingId, Present);
            DbUtil.Db.UpdateMeetingCounters(meeting.MeetingId);
        }
        public void JoinUnJoinOrg(int PeopleId, int OrgId, bool Member)
        {
            var om = DbUtil.Db.OrganizationMembers.SingleOrDefault(m => m.PeopleId == PeopleId && m.OrganizationId == OrgId);
            if (om == null && Member)
                om = OrganizationMember.InsertOrgMembers(OrgId, PeopleId, (int)OrganizationMember.MemberTypeCode.Member, DateTime.Now, null, false);
            else if (om != null && !Member)
                om.Drop(DbUtil.Db);
            DbUtil.Db.SubmitChanges();

            var org = DbUtil.Db.LoadOrganizationById(OrgId);
            if (org != null && org.EmailAddresses.HasValue())
            {
                var p = DbUtil.Db.LoadPersonById(PeopleId);
                var what = Member? "joined" : "dropped";
                var smtp = Util.Smtp();
                Util.Email(smtp, DbUtil.AdminMail, org.EmailAddresses,
                    "cms check-in, {0} class on ".Fmt(what) + Util.CmsHost, 
                    "<a href='{0}/Person/Index/{1}'>{2}</a> {3} {4}".Fmt( 
                        Util.ServerLink("/Person/Index/" + PeopleId), 
                        PeopleId, p.Name, what, org.OrganizationName));
            }
        }
    }
}

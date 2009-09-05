﻿/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church 
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license 
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using UtilityExtensions;
using CmsData;
using System.Collections;
using System.Text.RegularExpressions;
using System.Data.Linq.SqlClient;
using System.Threading;

namespace CMSPresenter
{
    public class PersonDialogSearchInfo
    {
        public int PeopleId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime? JoinDate { get; set; }
        public string BirthDate { get; set; }
        public string Address { get; set; }
        public string CityStateZip { get; set; }
        public string HomePhone { get; set; }
        public string CellPhone { get; set; }
        public string WorkPhone { get; set; }
        public int? Age { get; set; }
        public string MemberStatus { get; set; }
        public bool HasTag { get; set; }
        internal IEnumerable<string> _Groups { get; set; }
        public string Groups
        {
            get { return string.Join(",", _Groups.ToArray()); }
        }
        
        public string ToolTip
        {
            get
            {
                return "ID: {0}\nMobile Phone: {1}\nWork Phone: {2}\nHome Phone: {3}\nBirthDate: {4:d}\nJoin Date: {5:d}\nStatus: {6}"
                    .Fmt(PeopleId, CellPhone, WorkPhone, HomePhone, BirthDate, JoinDate, MemberStatus);
            }
        }
    }
    public interface SearchParameters
    {
        string DOB {get; set;}
        string Comm {get; set;}
        int OrgId {get; set;}
        string Name {get; set;}
        string Addr {get; set;}
        int Gender {get; set;}
        int Member {get; set;}
        int Campus { get; set; }
        int Tag {get; set;}
    }
    [DataObject]
    public class PersonSearchDialogController
    {
        static int TagTypeId_AddSelected = DbUtil.TagTypeId_AddSelected;
        public static void ResetSearchTags()
        {
            var t = DbUtil.Db.FetchOrCreateTag(Util.SessionId, Util.UserPeopleId, TagTypeId_AddSelected);
            DbUtil.Db.TagPeople.DeleteAllOnSubmit(t.PersonTags);
            DbUtil.Db.SubmitChanges();
        }

        public IEnumerable<PersonDialogSearchInfo> FetchSearchList(SearchParameters p, bool usersonly)
        {
            return FetchSearchList(p.Name, p.Comm, p.Addr, p.Member, p.Tag, p.DOB, p.Gender, p.OrgId, p.Campus, usersonly);
        }
        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<PersonDialogSearchInfo> FetchSearchList(
            string namesearch, string commsearch, string addrsearch, int memstatus, int tag, string dob, int gender, int orgid, int campus, bool usersonly)
        {
            var t = DbUtil.Db.FetchOrCreateTag(Util.SessionId, Util.UserPeopleId, TagTypeId_AddSelected);
            var n = t.People().Count();
            var list = FetchPeopleList(t.People()).ToList();
            var ids = list.Select(p => p.PeopleId).ToArray();

            var query = PersonSearchController.ApplySearch(namesearch, addrsearch, commsearch, memstatus, tag, dob, gender, orgid, campus, usersonly);
            query = query.Where(p => !ids.Contains(p.PeopleId));
            int maxitems = 100;
            list.AddRange(FetchPeopleList(query).Take(maxitems));
            maxitems += n;
            n += query.Count();
            if (n > maxitems)
                list.Add(new PersonDialogSearchInfo { Name = "(showing top {1} of {0:N0}".Fmt(n,maxitems) });
            return list;
        }
        public int count;
        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<PersonDialogSearchInfo> FetchOrgMemberList(int startRowIndex, int maximumRows, string sortExpression,
            int memtype, int tag, DateTime? inactivedt, int orgid, bool noinactive, bool pending)
        {
//'MemberData' could not find a non-generic method 'FetchOrgMemberList' that has parameters: 
            //startRowIndex, maximumRows, sortExpression, 
            //memtype, tag, inactive, orgid, noinactive."

            var q0 = SearchMembers(memtype, tag, inactivedt, orgid, noinactive, pending);
            count = q0.Count();
            var q1 = q0.OrderBy(m => m.Person.Name2).Skip(startRowIndex).Take(maximumRows);
            return FetchMemberList(q1);
        }
        public int Count(int startRowIndex, int maximumRows, string sortExpression,
                int memtype, int tag, DateTime? inactivedt, int orgid, bool noinactive, bool pending)
        {
            return count;
        }
        public static IQueryable<OrganizationMember> SearchMembers(int memtype, int tag, 
            DateTime? inactive, int orgid, bool? noinactive, bool pending)
        {
            var q0 = from om in DbUtil.Db.OrganizationMembers
                     where om.OrganizationId == orgid
                     where (om.Pending ?? false) == pending
                     where om.MemberTypeId != (int)OrganizationMember.MemberTypeCode.InActive || noinactive == false
                     select om;
            if (memtype != 0)
                q0 = q0.Where(om => om.MemberTypeId == memtype);
            if (tag > 0)
                q0 = q0.Where(om => om.Person.Tags.Any(t => t.Id == tag));
            if (inactive.HasValue)
                q0 = q0.Where(om => om.InactiveDate == inactive);
            return q0;
        }
        public IQueryable<PersonDialogSearchInfo> FetchMemberList(IQueryable<OrganizationMember> query)
        {
            var TagName = Util.SessionId;
            var TagOwner = Util.UserPeopleId;
            var q = from m in query
                    select new PersonDialogSearchInfo
                    {
                        PeopleId = m.PeopleId,
                        Name = m.Person.Name,
                        LastName = m.Person.LastName,
                        JoinDate = m.Person.JoinDate,
                        BirthDate = m.Person.BirthMonth + "/" + m.Person.BirthDay + "/" + m.Person.BirthYear,
                        Address = m.Person.PrimaryAddress,
                        CityStateZip = m.Person.PrimaryCity + ", " + m.Person.PrimaryState + " " + m.Person.PrimaryZip.Substring(0, 5),
                        HomePhone = m.Person.HomePhone,
                        CellPhone = m.Person.CellPhone,
                        WorkPhone = m.Person.WorkPhone,
                        Age = m.Person.Age,
                        MemberStatus = m.Person.MemberStatus.Description,
                        _Groups = m.OrgMemMemTags.Select(mt => mt.MemberTag.Name),
                    };
            return q;
        }

        public IQueryable<PersonDialogSearchInfo> FetchPeopleList(IQueryable<Person> query)
        {
            var TagName = Util.SessionId;
            var TagOwner = Util.UserPeopleId;
            var q = from p in query
                    select new PersonDialogSearchInfo
                    {
                        PeopleId = p.PeopleId,
                        Name = p.Name,
                        LastName = p.LastName,
                        JoinDate = p.JoinDate,
                        BirthDate = p.BirthMonth + "/" + p.BirthDay + "/" + p.BirthYear,
                        Address = p.PrimaryAddress,
                        CityStateZip = p.PrimaryCity + ", " + p.PrimaryState + " " + p.PrimaryZip.Substring(0, 5),
                        HomePhone = p.HomePhone,
                        CellPhone = p.CellPhone,
                        WorkPhone = p.WorkPhone,
                        Age = p.Age,
                        MemberStatus = p.MemberStatus.Description,
                        HasTag = p.Tags.Any(t => t.Tag.Name == TagName && t.Tag.PeopleId == TagOwner && t.Tag.TypeId == TagTypeId_AddSelected),
                    };
            return q;
        }
        public int Count(
            string namesearch, string commsearch, string addrsearch, int memstatus, int tag, string dob, int gender, int orgid, int campus, bool usersonly)
        {
            return count;
        }

        public enum AddFamilyType
        {
            ExistingFamily = 0,
            NewFamily = 1,
            Couple = 2,
        }
        private static AddFamilyType ParseFamilyType(object value)
        {
            return (AddFamilyType)Enum.Parse(typeof(AddFamilyType), value.ToString());
        }
        public static bool AddNewPerson(string name, string dob, int FamilyId, int GenderId, int OriginId, int? EntryPointId, int? CampusId)
        {
            var f = DbUtil.Db.Families.Single(fa => fa.FamilyId == FamilyId);
            var tag = DbUtil.Db.FetchOrCreateTag(Util.SessionId, Util.UserPeopleId, TagTypeId_AddSelected);
            DbUtil.Db.TagPeople.DeleteAllOnSubmit(tag.PersonTags); // only return the new people we are adding
            var p = Person.Add(f, 20, tag, name, dob, false, GenderId, OriginId, EntryPointId);
            DbUtil.Db.SubmitChanges();
            Task.AddNewPerson(Util.UserPeopleId.Value, p.PeopleId);
            return true;
        }
        public static bool AddNewPerson(string name, string dob, string selectedValue, int GenderId, int OriginId, int? EntryPointId, int? CampusId)
        {
            return AddNewPerson(name, dob, ParseFamilyType(selectedValue), GenderId, OriginId, EntryPointId, CampusId);
        }
        public static bool AddNewPerson(string potentialName, string dob, AddFamilyType famtype, int GenderId, int OriginId, int? EntryPointId, int? CampusId)
        {
            var tag = DbUtil.Db.FetchOrCreateTag(Util.SessionId, Util.UserPeopleId, TagTypeId_AddSelected);

            string name;
            if (potentialName.HasValue())
                name = potentialName.Trim();
            else
                name = famtype == AddFamilyType.Couple ? "NewCouple" : "NewPerson";

            Family fam;
            int position;
            if (famtype == AddFamilyType.ExistingFamily)
            {
                if (tag.People().Count() != 1)
                    return false;
                fam = tag.People().First().Family;
                var cnt = fam.People.Where(c => c.PositionInFamilyId == 10).Count();
                if (cnt < 2) // room for primary adult?
                    position = 10; // primary adult
                else
                    position = 30; // child
                if (name.StartsWith("New"))
                    name = fam.People.First().LastName;
            }
            else // new couple or single family
            {
                fam = new Family();
                DbUtil.Db.Families.InsertOnSubmit(fam);
                position = 10; // primary adult
            }
            DbUtil.Db.TagPeople.DeleteAllOnSubmit(tag.PersonTags); // only return the new people we are adding
            Person p1, p2 = null;
            if (famtype == AddFamilyType.Couple)
            {
                p1 = Person.Add(fam, position, tag, name, dob, true, 1, OriginId, EntryPointId); // male
                p2 = Person.Add(fam, position, tag, name, dob, true, 2, OriginId, EntryPointId); // female
                p2.CampusId = CampusId;
            }
            else
                p1 = Person.Add(fam, position, tag, name, dob, false, GenderId, OriginId, EntryPointId); // unknown gender
            p1.CampusId = CampusId;
            DbUtil.Db.SubmitChanges();
            Task.AddNewPerson(Util.UserPeopleId.Value, p1.PeopleId);
            if (p2 != null)
                Task.AddNewPerson(Util.UserPeopleId.Value, p2.PeopleId);
            return true;
        }
    }
}

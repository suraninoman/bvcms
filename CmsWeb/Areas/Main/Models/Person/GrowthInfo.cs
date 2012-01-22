using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsData;
using System.Web.Mvc;
using UtilityExtensions;
using CMSPresenter;

namespace CmsWeb.Models.PersonPage
{
    public class GrowthInfo
    {
        private static CodeValueController cv = new CodeValueController();

        public int PeopleId { get; set; }

        public int? InterestPointId { get; set; }
        public string InterestPoint { get { return cv.InterestPoints().ItemValue(InterestPointId ?? 0); } }
        public int? OriginId { get; set; }
        public string Origin { get { return cv.Origins().ItemValue(OriginId ?? 0); } }
        public int? EntryPointId { get; set; }
        public string EntryPoint { get { return cv.EntryPoints().ItemValue(EntryPointId ?? 0); } }
        public bool? MemberAnyChurch { get; set; }
        public bool ChristAsSavior { get; set; }
        public bool PleaseVisit { get; set; }
        public bool InterestedInJoining { get; set; }
        public bool SendInfo { get; set; }
        public string Comments { get; set; }

        public static GrowthInfo GetGrowthInfo(int? id)
        {
            var q = from p in DbUtil.Db.People
                    where p.PeopleId == id
                    select new GrowthInfo
                    {
                        PeopleId = p.PeopleId,

                        InterestPointId = p.InterestPointId ?? 0,
                        OriginId = p.OriginId ?? 0,
                        EntryPointId = p.EntryPointId ?? 0,
                        ChristAsSavior = p.ChristAsSavior,
                        Comments = p.Comments,
                        InterestedInJoining = p.InterestedInJoining,
                        MemberAnyChurch = p.MemberAnyChurch,
                        PleaseVisit = p.PleaseVisit,
                        SendInfo = p.InfoBecomeAChristian,
                    };
            return q.Single();
        }
        public void UpdateGrowth()
        {
            var p = DbUtil.Db.LoadPersonById(PeopleId);


            if (InterestPointId == 0)
                InterestPointId = null;
            if (OriginId == 0)
                OriginId = null;
            if (EntryPointId == 0)
                EntryPointId = null;

            p.InterestPointId = InterestPointId;
            p.OriginId = OriginId;
            p.EntryPointId = EntryPointId;
            p.ChristAsSavior = ChristAsSavior;
            p.Comments = Comments;
            p.InterestedInJoining = InterestedInJoining;
            p.MemberAnyChurch = MemberAnyChurch;
            p.PleaseVisit = PleaseVisit;
            p.InfoBecomeAChristian = SendInfo;

            DbUtil.Db.SubmitChanges();
            DbUtil.LogActivity("Updated Growth: {0}".Fmt(p.Name));
        }
        public static IEnumerable<SelectListItem> InterestPoints()
        {
            return QueryModel.ConvertToSelect(cv.InterestPoints(), "Id");
        }
        public static IEnumerable<SelectListItem> Origins()
        {
            return QueryModel.ConvertToSelect(cv.Origins(), "Id");
        }
        public static IEnumerable<SelectListItem> EntryPoints()
        {
            return QueryModel.ConvertToSelect(cv.EntryPoints(), "Id");
        }
    }
}

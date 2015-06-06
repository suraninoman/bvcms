using System.Linq;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Areas.OnlineReg.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.OnlineReg.Controllers
{
    public partial class OnlineRegController
    {
        [HttpGet]
        public ActionResult Continue(int id)
        {
            var m = OnlineRegModel.GetRegistrationFromDatum(id);
            if (m == null)
                return Message("no existing registration available");
            var n = m.List.Count - 1;
            m.HistoryAdd("continue");
            m.UpdateDatum();
            SetHeaders(m);
            return View("Index", m);
        }
        
        [HttpGet]
        public ActionResult StartOver(int id)
        {
            var pid = (int)TempData["PeopleId"];
            if (pid == 0)
                return Message("not logged in");
            var m = OnlineRegModel.GetRegistrationFromDatum(id);
            if (m == null)
                return Message("no existing registration available");
            m.HistoryAdd("startover");
            m.UpdateDatum(abandoned: true);
            return Redirect(m.URL);
        }

        [HttpPost]
        public ActionResult AutoSaveProgress(OnlineRegModel m)
        {
            try { m.UpdateDatum(); } 
            catch { }
            return Content("saved");
        }

        [HttpPost]
        public ActionResult SaveProgress(OnlineRegModel m)
        {
            m.HistoryAdd("saveprogress");
            if (m.UserPeopleId == null)
                m.UserPeopleId = Util.UserPeopleId;
            m.UpdateDatum();
            var p = m.UserPeopleId.HasValue ? DbUtil.Db.LoadPersonById(m.UserPeopleId.Value) : m.List[0].person;

            if (p == null)
                return Content("We have not found your record yet, cannot save progress, sorry");
            if (m.masterorgid == null && m.Orgid == null)
                return Content("Registration is not far enough along to save, sorry.");

            var registerLink = EmailReplacements.CreateRegisterLink(m.masterorgid ?? m.Orgid, "Resume registration for {0}".Fmt(m.Header));
            var msg = "<p>Hi {first},</p>\n<p>Here is the link to continue your registration:</p>\n" + registerLink;
            var notifyids = DbUtil.Db.NotifyIds((m.masterorg ?? m.org).NotifyIds);
            DbUtil.Db.Email(notifyids[0].FromEmail, p, "Continue your registration for {0}".Fmt(m.Header), msg);

            /* We use Content as an ActionResult instead of Message because we want plain text sent back
             * This is an HttpPost ajax call and will have a SiteLayout wrapping this.
             */
            return Content("We have saved your progress. An email with a link to finish this registration will come to you shortly.");
        }

        [HttpGet]
        public ActionResult Existing(int id)
        {
            if(!TempData.ContainsKey("PeopleId"))
                return Message("not logged in");
            var pid = (int?)TempData["PeopleId"];
            if (!pid.HasValue || pid == 0)
                return Message("not logged in");
            var m = OnlineRegModel.GetRegistrationFromDatum(id);
            if (m == null)
                return Message("no existing registration available");
            if (m.UserPeopleId != m.Datum.UserPeopleId)
                return Message("incorrect user");
            TempData["PeopleId"] = pid;
            return View(m);
        }

        [HttpPost]
        public ActionResult SaveProgressPayment(int id)
        {
            var ed = DbUtil.Db.RegistrationDatas.SingleOrDefault(e => e.Id == id);
            if (ed != null)
            {
                var m = Util.DeSerialize<OnlineRegModel>(ed.Data);
                m.HistoryAdd("saveprogress");
                if (m.UserPeopleId == null)
                    m.UserPeopleId = Util.UserPeopleId;
                m.UpdateDatum();
                return Json(new { confirm = "/OnlineReg/FinishLater/" + id });
            }
            return Json(new { confirm = "/OnlineReg/Unknown" });
        }

        [HttpGet]
        public ActionResult FinishLater(int id)
        {
            var ed = DbUtil.Db.RegistrationDatas.SingleOrDefault(e => e.Id == id);
            if (ed == null) 
                return View("Unknown");
            var m = Util.DeSerialize<OnlineRegModel>(ed.Data);
            m.FinishLaterNotice();
            return View(m);
        }
    }
}

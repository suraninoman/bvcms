﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CmsData;
using UtilityExtensions;
using System.Configuration;
using CMSWeb;
using System.Web.Security;
using System.Web.DynamicData;
using System.Data.Linq.Mapping;
using System.Collections;
using System.Web.SessionState;
using System.Xml.Linq;
using System.Net.Mail;
using System.Web.Configuration;
using CMSPresenter;

namespace CMSWeb2
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
            //RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{myWebForms}.aspx/{*pathInfo}");
            routes.IgnoreRoute("{myWebForms}.ashx/{*pathInfo}");
            routes.IgnoreRoute("{myWebForms}.asmx/{*pathInfo}");
            routes.IgnoreRoute("{myWebForms}.js/{*pathInfo}");
            routes.IgnoreRoute("{name}.ico");
            routes.IgnoreRoute("{name}.png");
            routes.IgnoreRoute("Admin/{*pathInfo}");
            routes.IgnoreRoute("AppReview/{*pathInfo}");
            routes.IgnoreRoute("CustomErrors/{*pathInfo}");
            routes.IgnoreRoute("Contributions/{*pathInfo}");
            routes.IgnoreRoute("Report/{*pathInfo}");
            routes.IgnoreRoute("fckeditor/{*pathInfo}");
            routes.IgnoreRoute("StaffOnly/{*pathInfo}");
            routes.IgnoreRoute("images/{*pathInfo}");
            routes.IgnoreRoute("App_Themes/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("Upload/{*pathInfo}");
            routes.IgnoreRoute("{myWebPage}.htm");
            routes.IgnoreRoute("{myReport}.rdlc");
            routes.IgnoreRoute("ttt/{*pathInfo}");
            routes.IgnoreRoute("{dir1}/{dir2}/{file}.js");
            routes.IgnoreRoute("{dir1}/{dir2}/{file}.css");

            routes.RouteExistingFiles = true;

            routes.MapRoute("Cache",
                "cache/{action}/{key}/{version}",
                new { controller = "Cache", action = "Content", key = "", version = "" });
            routes.MapRoute("Task",
                "Task/{action}/{id}",
                new { controller = "Task", action = "List", id = "" });
            routes.MapRoute("QB",
                "QueryBuilder/{action}/{id}",
                new { controller = "QueryBuilder", action = "Main", id = "" });
            routes.MapRoute("Display",
                "Display/{action}/{page}",
                new { controller = "Display", action = "Page", page = "" });
            routes.MapRoute("StepClass",
                "StepClass/{action}",
                new { controller = "StepClass", action = "Step1" });
            routes.MapRoute("VolunteerConfirm",
                "Volunteer/Confirm",
                new { controller = "Volunteer", action = "confirm", id = "" });
            routes.MapRoute("VolunteerHome",
                "Volunteer/{id}",
                new { controller = "Volunteer", action = "Start", id = "" });
            routes.MapRoute("TaskDetailRow",
                "Task/Detail/{id}/Row/{rowid}",
                new { controller = "Task", action = "Detail", id = "" });
            routes.MapRoute("Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" });
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (Util.UserId == 0 && Util.UserName.HasValue())
            {
                var u = DbUtil.Db.Users.SingleOrDefault(us => us.Username == Util.UserName);
                if (u != null)
                {
                    Util.UserId = u.UserId;
                    Util.UserPeopleId = u.PeopleId;
                }
            }
            Util.SessionStarting = true;
        }
        //protected void Application_BeginRequest(object sender, EventArgs e)
        //{
        //}
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current != null)
                DbUtil.DbDispose();
            if (Response.Status.StartsWith("401")
                    && Request.Url.AbsolutePath.EndsWith(".aspx"))
                Login.CheckStaffRole(User.Identity.Name);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            var InDebug = false;
#if DEBUG
            InDebug = true;
#endif
            if (InDebug)
            {
                Response.Write("<html><body><pre>\n");
                Response.Write(ex.ToString());
                Response.Write("</pre></body></html>\n");
                Server.ClearError();
                return;
            }
            var u = DbUtil.Db.CurrentUser;
            var smtp = new SmtpClient();
            var msg = new MailMessage();
            msg.Subject = "bvcms error on " + Request.Url.Authority;
            if (u != null)
            {
                msg.From = new MailAddress(u.EmailAddress, u.Name);
                msg.Body = "\n{0} ({1}, {2})\n".Fmt(u.EmailAddress, u.UserId, u.Name) + ex.ToString();
            }
            else
            {
                msg.From = new MailAddress(WebConfigurationManager.AppSettings["sysfromemail"]);
                msg.Body = ex.ToString();
            }
            foreach (var a in CMSRoleProvider.provider.GetRoleUsers("Developer"))
                msg.To.Add(new MailAddress(a.Person.EmailAddress, a.Name));
            smtp.Send(msg);
        }
    }
}
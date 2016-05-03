using System;
using System.Collections.Generic;
using System.Linq;
using ImageData;
using IronPython.Modules;
using UtilityExtensions;
using System.Text;
using System.Web;
using CmsData.Codes;

namespace CmsData.View
{
    public partial class OrgPerson
    {
        public string CityStateZip
        {
            get { return Util.FormatCSZ4(City,St,Zip); }
        }

        public HtmlString AddressBlock
        {
            get
            {
                var sb = new StringBuilder();
                if (Address.HasValue())
                    sb.Append(Address);
                if (Address2.HasValue())
                {
                    if (sb.Length > 0)
                        sb.Append("<br>");
                    sb.Append(Address2);
                }
                var csz = CityStateZip;
                if (csz.HasValue())
                {
                    if (sb.Length > 0)
                        sb.Append("<br>");
                    sb.Append(csz);
                }
                if (sb.Length > 0)
                {
                    sb.Insert(0, "<div>");
                    sb.Append("</div>");
                }
                return new HtmlString(sb.ToString());
            } 
        }

        public string Group
        {
            get 
            { 
                return GroupCode == GroupSelectCode.Member ? "Member" 
                    : GroupCode == GroupSelectCode.Inactive ? "Inactive"
                    : "NonMember"; 
            }
        }

        private static bool? _hideBirthYearForOrgLeaders;
        public string BirthDate
        {
            get
            {
                if(!_hideBirthYearForOrgLeaders.HasValue)
                    _hideBirthYearForOrgLeaders = DbUtil.Db.Setting("HideBirthYearForOrgLeaders", "false").ToLower() == "true";

                if (_hideBirthYearForOrgLeaders.Value && Util.IsInRole("OrgLeadersOnly"))
                    return Util.FormatBirthday(null, BirthMonth, BirthDay);

                return Util.FormatBirthday(BirthYear, BirthMonth, BirthDay);
            }
        }

        public IEnumerable<string> Phones
        {
            get
            {
                var phones = new List<string>();
                if(CellPhone.HasValue())
                    phones.Add(CellPhone.FmtFone("C"));
                if(HomePhone.HasValue())
                    phones.Add(HomePhone.FmtFone("H"));
                if(WorkPhone.HasValue())
                    phones.Add(WorkPhone.FmtFone("W"));
                return phones;
            }
        }

        public long? LastAttendedTicks
        {
            get
            {
                if(LastAttended.HasValue)
                    return LastAttended.Value.Ticks;
                return null;
            }
        }
    }
}

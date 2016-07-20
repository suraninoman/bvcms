﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using CmsData;

namespace CmsWeb.Areas.Manage.Models
{
    public class ResourceModel
    {
        public int ResourceId { get; set; }
        public Resource Resource { get; set; }
        public List<ResourceAttachment> Attachments { get; set; }

        public ResourceModel(int resourceId)
        {
            ResourceId = resourceId;
            Resource = DbUtil.Db.Resources.First(x => x.ResourceId == resourceId);
            Attachments = DbUtil.Db.ResourceAttachments.Where(x => x.ResourceId == resourceId).ToList();
        }

        public ResourceModel(Resource resource)
        {
            Resource = resource;
            ResourceId = resource.ResourceId;
            Attachments = DbUtil.Db.ResourceAttachments.Where(x => x.ResourceId == ResourceId).ToList();
        }

        [DisplayName("Organization")]
        public string OrganizationName
        {
            get { return Resource.Organization?.OrganizationName; }
        }

        [DisplayName("Organization Type")]
        public string OrganizationTypeName
        {
            get { return Resource.OrganizationType?.Description; }
        }

        [DisplayName("Congregation")]
        public string CampusName
        {
            get { return Resource.Campu?.Description; }
        }

        [DisplayName("Member Types")]
        public string MemberTypes
        {
            get {
                if (string.IsNullOrWhiteSpace(Resource.MemberTypeIds))
                    return string.Empty;

                var memberTypeIds = Resource.MemberTypeIds.Split(',').Select(int.Parse) ;
                return string.Join(", ", DbUtil.Db.MemberTypes.Where(x => memberTypeIds.Contains(x.Id)).Select(x => x.Description));
            }
        }

        [DisplayName("Resource Type")]
        public string ResourceTypeName
        {
            get { return Resource.ResourceType.Name; }
        }

        [DisplayName("Resource Category")]
        public string ResourceCategoryName
        {
            get { return Resource.ResourceCategory.Name; }
        }
    }
}
﻿using System.Linq;
using System.Web.Http;
using System.Web.OData;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CmsData;
using CmsWeb.Models.Api.Lookup;

namespace CmsWeb.Controllers.Api.Lookup
{
    public class BundleHeaderTypesController : ODataController
    {
        public BundleHeaderTypesController()
        {
            Mapper.CreateMap<BundleHeaderType, ApiLookup>();
        }

        public IHttpActionResult Get()
        {
            return Ok(DbUtil.Db.BundleHeaderTypes.Project().To<ApiLookup>().AsQueryable());
        }
    }
}

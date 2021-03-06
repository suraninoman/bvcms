﻿using System.Linq;
using System.Web.Http;
using System.Web.OData;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CmsData;
using CmsWeb.Models.Api.Lookup;

namespace CmsWeb.Controllers.Api.Lookup
{
    public class CampusesController : ODataController
    {
        public CampusesController()
        {
            Mapper.CreateMap<Campu, ApiLookup>();
        }

        public IHttpActionResult Get()
        {
            return Ok(DbUtil.Db.Campus.Project().To<ApiLookup>().AsQueryable());
        }
    }
}

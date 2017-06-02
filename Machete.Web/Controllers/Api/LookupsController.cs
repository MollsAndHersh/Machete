﻿using AutoMapper;
using Machete.Service;
using Machete.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Machete.Web.Controllers.Api
{
    [ElmahHandleError]
    [Authorize]
    public class LookupsController : ApiController
    {
        private readonly ILookupService serv;
        private readonly IMapper map;

        public LookupsController(ILookupService serv, IMapper map)
        {
            this.serv = serv;
            this.map = map;
        }

        // GET: api/Lookups
        public IHttpActionResult Get()
        {
            var result = serv.GetAll();
            return Json(new { data = result });
        }

        public IHttpActionResult Get(string category)
        {
            var result = serv.GetMany(w => w.category == category);
            return Json(new { data = result });
        }

        // GET: api/Lookups/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Lookups
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Lookups/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Lookups/5
        public void Delete(int id)
        {
        }
    }
}

using Microsoft.Practices.Unity;
using Sabio.Web.Domain;
using Sabio.Web.Exceptions;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests.Users;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/admin/roles")]
    public class AdminRoleApiController : ApiController
    {
        [Dependency]
        public IAdminUserService _AdminService { get; set; }


        [Route(), HttpGet]
        public HttpResponseMessage GetAllRoles()
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemsResponse<UserRole> response = new ItemsResponse<UserRole>();

            List<UserRole> roleList = _AdminService.GetAllRoles();

            response.Items = roleList;

            return Request.CreateResponse(HttpStatusCode.OK, response);


        }

        [Route("{jobGuid:guid}"), HttpGet]
        public HttpResponseMessage GetUserRoleById(Guid jobGuid)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<UserProfile> response = new ItemResponse<UserProfile>();

            UserProfile userRoleList = _AdminService.GetUserRoleById(jobGuid);

            response.Item = userRoleList;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

    }

}

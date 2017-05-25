using Microsoft.Practices.Unity;
using Sabio.Web.Classes.Tasks.Bringg;
using Sabio.Web.Domain;
using Sabio.Web.Exceptions;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Requests.Bringg;
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
    [RoutePrefix("api/admin/users")]
    [Authorize(Roles = "Administrator, CustomerService")]
    public class AdminUserApiController : ApiController
    {
        [Dependency]
        public IAdminUserService _AdminService { get; set; }
        [Dependency]
        public IUserEmailService _EmailService { get; set; }
        [Dependency]
        public IUserProfileService _UserProfileService { get; set; }
        [Dependency]
        public IBringgUserService _BringgUserService { get; set; }


        [Route("create"), HttpPost]
        public HttpResponseMessage CreateUser(UserProfile model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ApplicationUser newUser = null;
            try
            {
                newUser = (ApplicationUser)UserService.CreateUser(model.Email, model.Password, model.Phone);
            }
            catch (IdentityResultException ex)
            {
                string validatedData = UtilityService.ValidateData(ex);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, validatedData);

            }
            _AdminService.CreateUserProfile(newUser.Id, model);
            _AdminService.CreateUserRole(newUser.Id, model);


            Guid? userTokenGuid = null;

            userTokenGuid = TokenService.tokenInsert(newUser.Id);

            ItemResponse<UserProfile> response = new ItemResponse<UserProfile>();

            response.Item = (UserProfile)_UserProfileService.GetUserById(newUser.Id);

            string UserEmail = model.Email;
            if (UserEmail != null) //sends the new employee an email to confirm their account.
            {
                _EmailService.SendAdminProfileEmail(userTokenGuid, UserEmail);
            }

            return Request.CreateResponse(response);
        }


        [Route(), HttpGet]
        public HttpResponseMessage GetAllUsers([FromUri]PaginatedRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            PaginatedItemsResponse<UserProfile> response = _AdminService.GetAllUsers(model);

            return Request.CreateResponse(HttpStatusCode.OK, response);


        }

        [Route("{jobGuid:guid}"), HttpGet]
        public HttpResponseMessage GetUserById(Guid jobGuid)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<UserProfile> response = new ItemResponse<UserProfile>();

            UserProfile userList = _AdminService.GetUserById(jobGuid);

            response.Item = userList;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("{websiteId:int}"), HttpGet]
        public HttpResponseMessage GetUsersByWebsiteId (int websiteId, [FromUri]PaginatedRequest model)
        {
            if(!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            PaginatedItemsResponse<UserProfile> response = _AdminService.GetUserByWebsiteId(websiteId, model);

            return Request.CreateResponse(HttpStatusCode.OK, response);



        }

        [Route("update/{jobGuid:guid}"), HttpPut]
        public HttpResponseMessage AdminUpdateUser(AdminUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _BringgUserService.AdminUpdateUser(model);
                ItemResponse<bool> response = new ItemResponse<bool>();

                response.Item = true;
                
                return Request.CreateResponse(HttpStatusCode.OK, response);


        }

        [Route("delete/{UserId:guid}"), HttpDelete]
        public HttpResponseMessage Delete(Guid UserId)
        {

            _BringgUserService.AdminDeleteUser(UserId);

            SuccessResponse response = new SuccessResponse();

            return Request.CreateResponse(response);

        }

    }

}
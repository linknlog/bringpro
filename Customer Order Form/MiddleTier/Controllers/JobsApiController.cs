using Microsoft.Practices.Unity;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/jobs")]
    public class JobsApiController : ApiController

    {
        [Dependency]
        public IJobsService _JobsService { get; set; }
        [Dependency]
        public IUserProfileService _UserProfileService { get; set; }
        [Dependency]
        public IActivityLogService _ActivityLogService { get; set; }
        [Dependency]
        public IAdminJobScheduleService _ScheduleService { get; set; }

        [Dependency]
        public IBrainTreeService _BrainTreeService { get; set; }

        [Route(), HttpGet]
        public HttpResponseMessage GetAllJobsWithFilter([FromUri] PaginatedRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            PaginatedItemsResponse<Job> response = _JobsService.GetAllJobsWithFilter(model);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("{jobId:int}"), HttpGet]
        public HttpResponseMessage GetJobById(int jobId)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<Job> response = new ItemResponse<Job>();

            Job Job = _JobsService.GetJobById(jobId);

            response.Item = Job;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("activity/{jobId:int}"), HttpGet]
        public HttpResponseMessage GetActivityByJobId(int jobId)
        {
            ItemsResponse<ActivityLog> response = new ItemsResponse<ActivityLog>();

            string gotUserId = UserService.GetCurrentUserId();

            if (gotUserId != null)
            {
                List<ActivityLog> list = _ActivityLogService.GetByJobId(jobId);
                response.Items = list;
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        [Route("create"), HttpPost]
        public HttpResponseMessage JobInsert(JobInsertRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> response = new ItemResponse<int>();
            UserProfile user = null;
            // Check to see if user is logged use userservice.isloggedin
            if (UserService.IsLoggedIn())
            {
               
                user = _UserProfileService.GetUserById(UserService.GetCurrentUserId());

                model.ExternalCustomerId = Int32.Parse(user.ExternalUserId);
                model.UserId = user.UserId;
                model.Phone = user.Phone;
         
            }

            int tempJobId = _JobsService.InsertJob(model);

            response.Item = tempJobId;

            //Activity Log Service

            if (user != null && user.UserId != null)
            {
                ActivityLogRequest Activity = new ActivityLogRequest();

                Activity.ActivityType = ActivityTypeId.CreatedJob;

                Activity.JobId = tempJobId;
                Activity.TargetValue = (int)JobStatus.BringgCreated;

                _ActivityLogService.InsertActivityToLog(model.UserId, Activity);
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage JobEdit(JobUpdateRequest model, int id)
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            model.Id = id;

            bool isSuccessful = _JobsService.UpdateJob(model);

            ItemResponse<bool> response = new ItemResponse<bool>();

            response.Item = isSuccessful;

            ActivityLogRequest Activity = new ActivityLogRequest();

            Activity.ActivityType = ActivityTypeId.JobUpdated;
            Activity.JobId = id;
            Activity.TargetValue = (int)JobStatus.BringgCreated;

            _ActivityLogService.InsertActivityToLog(model.UserId, Activity);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("update/{id:int}"), HttpPut]
        public HttpResponseMessage PaymentNonce(JobUpdateRequest model, int id)
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            model.Id = id;
            bool isSuccessful = JobsService.UpdatePaymentNonce(model);

            ItemResponse<bool> response = new ItemResponse<bool>();
            response.Item = isSuccessful;
            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("guestcheckout"), HttpPost]
        public HttpResponseMessage GuestPayment(CustomerPaymentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            string paymentNonce;

            try
            {
                paymentNonce = _BrainTreeService.GuestPayment(model);
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }


            JobUpdateRequest jobUpdate = new JobUpdateRequest
            {
                Id = model.JobId,
                Phone = model.Phone,
                ContactName = string.Format("{0} {1}", model.FirstName, model.LastName),
                PaymentNonce = paymentNonce
            };

            ActivityLogRequest Activity = new ActivityLogRequest
            {
                ActivityType = ActivityTypeId.CreatedJob,
                JobId = model.JobId,
                TargetValue = (int)JobStatus.BringgCreated
            };

            //For guest check out we use the phone number as the userID
            _ActivityLogService.InsertActivityToLog(model.Phone, Activity);

            bool isSuccessful = _JobsService.UpdateJob(jobUpdate);


            ItemResponse<bool> response = new ItemResponse<bool>();
            response.Item = isSuccessful;



            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("{id:int}"), HttpDelete]
        public HttpResponseMessage JobDelete(int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            JobDeleteRequest model = new JobDeleteRequest();

            model.Id = Id;

            Job JobList = new Job();
            
            JobList = _JobsService.GetJobById(Id);

            bool isSuccessful = _JobsService.DeleteJob(model);

            ItemResponse<bool> response = new ItemResponse<bool>();

            response.Item = isSuccessful;

            //Activity Log Service

            ActivityLogRequest Activity = new ActivityLogRequest();

            Activity.ActivityType = ActivityTypeId.JobDeleted;
            Activity.JobId = Id;

            _ActivityLogService.InsertActivityToLog(JobList.UserId, Activity);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }


        [Route("waypoints"), HttpPost]
        public HttpResponseMessage JobWaypointInsert([FromBody] JobOrderUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            //Edmund's Section
            int JobScheduleId = 0;

            JobSchedule Sched = new JobSchedule();
            Sched.JobId = model.JobId;
            Sched.ScheduleId = model.ScheduleId;
            Sched.Date = model.Date;
            if (model.Date != null && model.JobId != null)
            {
                try
                {
                    JobScheduleId = _ScheduleService.JobScheduleUpsert(Sched);
                }
                catch (System.Exception ex)
                {

                    throw (ex);
                }
            }
            JobScheduleItemsResponse<int> response = new JobScheduleItemsResponse<int>();

            //Original Service from this controller
            response.Items = _JobsService.SaveJob(model);

            response.JobScheduleId = JobScheduleId;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("submit/{JobId:int}"), HttpPost]
        public HttpResponseMessage SubmitJob(int JobId)

        {
            _JobsService.BringgCreateTask(JobId);

            ItemResponse<bool> response = new ItemResponse<bool>();
            response.Item = true;
            return Request.CreateResponse(response);
        }

        [Route("items/{ItemId:int}"), HttpDelete]
        public HttpResponseMessage Delete(int ItemId)
        {
            SuccessResponse response = new SuccessResponse();
            _JobsService.DeleteJobItem(ItemId);
            return Request.CreateResponse(response);
        }

    }
}






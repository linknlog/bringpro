using Microsoft.Practices.Unity;
using Sabio.Web.Domain;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/schedule")]
    public class JobScheduleAPIController : ApiController
    {
        [Dependency]
        public IAdminJobScheduleService _ScheduleService { get; set; }

        [Route("get/{WebsiteId:int}"), HttpPost]
        public HttpResponseMessage GetByWebsiteId(int WebsiteId, JobTimeSlotsQueryRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            
            TimeSlotsItemsResponse<JobTimeSlots> response = new TimeSlotsItemsResponse<JobTimeSlots>();

            response = _ScheduleService.GetAllTimeSlotsByTeam(WebsiteId, model);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("edit/{Id:int}"), HttpGet]
        public HttpResponseMessage GetTimeSlotById(int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<JobTimeSlots> response = new ItemResponse<JobTimeSlots>();

            response.Item = _ScheduleService.GetTimeSlotById(Id);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("edit/{Id:int}"), HttpPut]
        public HttpResponseMessage TimeSlotUpdate(JobTimeSlots model, int Id)
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            model.Id = Id;

            bool isSuccessful = _ScheduleService.UpdateTimeSlot(model);

            ItemResponse<bool> response = new ItemResponse<bool>();

            response.Item = isSuccessful;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route(), HttpPost]
        public HttpResponseMessage InsertNewTimeSlot(JobTimeSlots model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> response = new ItemResponse<int>();

            try
            {
                response.Item = _ScheduleService.InsertNewTimeSlot(model);
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }


            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("edit"), HttpPost]
        public HttpResponseMessage GetTimeSlotByDate(JobTimeSlotsQueryRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemsResponse<JobTimeSlots> response = new ItemsResponse<JobTimeSlots>();

            response.Items = _ScheduleService.GetAllTimeSlotByDate(model);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("{Id:int}"), HttpDelete]
        public HttpResponseMessage TimeSlotDelete(int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            bool isSuccessful = _ScheduleService.DeleteTimeSlot(Id);

            ItemResponse<bool> response = new ItemResponse<bool>();

            response.Item = isSuccessful;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("available"), HttpPost]
        public HttpResponseMessage GetAvailableTimeByDate(JobTimeSlotsQueryRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemsResponse<JobTimeSlots> response = new ItemsResponse<JobTimeSlots>();

            response.Items = _ScheduleService.GetAllAvailableByDate(model);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("booking"), HttpPost]
        public HttpResponseMessage InsertJobSchedule(JobSchedule model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> response = new ItemResponse<int>();

            response.Item = _ScheduleService.InsertNewJobSchedule(model);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }
    }


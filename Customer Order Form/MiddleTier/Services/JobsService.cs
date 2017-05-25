//this beast was a collaboration effort between myself and four other developers.

using Microsoft.Practices.Unity;
using Sabio.Data;
using Sabio.Web.Classes.Tasks.Bringg.Interfaces;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace Sabio.Web.Services
{
    
    public class JobsService : BaseService, IJobsService

    {
        [Dependency]
        public IAddressService _AddressService { get; set; }
        [Dependency]
        public IJobsWaypointService _JobsWaypointService { get; set; }

        [Dependency]
        public IJobItemOptionsService _JobItemOptionsService { get; set; }
        [Dependency("CreateTask")]
        public IBringgTask<Job> _CreateTask { get; set; }
        [Dependency("CreateTaskWithWaypoints")]
        public IBringgTask<Job> _CreateTaskWithWaypoints { get; set; }
        [Dependency("CreateWaypoint")]
        public IBringgTask<JobWaypoint> _CreateWaypoint { get; set; }
        [Dependency]
        public IUserAddressService _UserAddressService { get; set; }
        [Dependency]
        public IWebsiteTeamService _WebsiteTeamService { get; set; }

        public List<int> SaveJob(JobOrderUpdateRequest model)
        {
            List<int> list = new List<int>(); // create a list of WaypointIds, which are ints
            foreach (var waypoint in model.Waypoints) // goes through every Waypoint submitted in the payload
            {
                int waypointId = SaveWaypoint(waypoint); // Notice how this gets a waypointId after calling Savewaypoint()
                list.Add(waypointId); // This line gets exeuted after calling SaveWaypoint(), which calls another function SaveItem()

            }

            return list; // this line executes last in this 'upsert' function

        }

        public int SaveWaypoint(WaypointRequest model)
        {
            // we call this so we can attach an AddressId to each Waypoint in WaypointInsert() or WaypointUpdate()
            int addressId = _AddressService.SaveWaypointAddress(model);

            if (model.Id == 0) // insert Waypoint
            {
                int waypointId = _JobsWaypointService.WaypointInsert(model, addressId);

                List<int> PickupList = new List<int>(); // every Waypoint has a list of items
                foreach (var items in model.JobWaypointItemsPickup) //loops through all items in the model
                {
                    int itemId = SaveItem(items, model.JobId, waypointId); // see comment directly above SaveItem() that explains why we needs this parameters passed in
                    PickupList.Add(itemId);
                }
                List<int> DropoffList = new List<int>(); // every Waypoint has a list of items
                foreach (var items in model.JobWaypointItemsDropOff) //loops through all items in the model
                {
                    int itemId = SaveItem(items, model.JobId, waypointId); // see comment directly above SaveItem() that explains why we needs this parameters passed in
                    DropoffList.Add(itemId);
                }

                return waypointId;
            }
            else // update Waypoint
            {
                _JobsWaypointService.WaypointUpdate(model, addressId);

                int waypointId = model.Id; // we need this to be returned for the list of Waypoints

                List<int> PickupList = new List<int>(); // every Waypoint, even an updated one, needs a new list because we loop through each waypoint's items after inserting or updating a waypoint
                foreach (var items in model.JobWaypointItemsPickup)
                {
                    int itemId = SaveItem(items, model.JobId, waypointId);
                    PickupList.Add(itemId);
                }
                List<int> DropoffList = new List<int>(); // every Waypoint, even an updated one, needs a new list because we loop through each waypoint's items after inserting or updating a waypoint
                foreach (var items in model.JobWaypointItemsDropOff)
                {
                    int itemId = SaveItem(items, model.JobId, waypointId);
                    DropoffList.Add(itemId);
                }

                return waypointId;
            }
        }

        // We need JobId and waypointId in order to put them in database columns for each Item.
        // This will make it organized for us to extract the approriate items for each Waypoint during the Get. 
        public int SaveItem(WaypointItemPIckupRequest model, int? jobId, int waypointId)
        {
            int waypointItemId = 0;

            // This if statement assigns a default MediaId to an item that does not have an user image attached. The default MediaIds establish default stock images.
            if (model.MediaId == null && model.ItemTypeId > 0)
            {
                JobItemOptionsDomain JobItemOptions = _JobItemOptionsService.getIdJobItemOptions(model.ItemTypeId);
                model.MediaId = JobItemOptions.MediaId;
            }
            else
            {
                model.MediaId = model.MediaId;
            }

            if (model.Id == 0) // insert Item into database
            {
                _JobsWaypointService.WaypointItemInsert(model, jobId, waypointId); // this function returns waypointItemId
                return waypointItemId;
            }
            else  // update Item in database
            {
                int wayPointItemId = 0;
                _JobsWaypointService.WaypointItemUpdate(model, jobId, waypointId);
                wayPointItemId = model.Id; // this line is needed because SaveItem() needs to return waypointItemId, so it can be added to a list of items in each waypoint 
                return wayPointItemId;
            }
        }
        ///////// The above code -- SaveJob(), SaveWaypoint(), SaveItem() -- make up the 'upsert' that creates the massive job object

        public int insertWaypointItems(JobsItemsInsertRequest model)
        {
            int id = 0;
            int waypointId = 33;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_WaypointItemsInsert"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@JobId", model.JobId);
                          paramCollection.AddWithValue("@WaypointId", waypointId);
                          paramCollection.AddWithValue("@ItemType", model.ItemTypeId);
                          paramCollection.AddWithValue("@ItemNote", model.ItemNote);
                          paramCollection.AddWithValue("@Quantity", model.Quantity);
                          paramCollection.AddWithValue("@MediaId", model.MediaId);


                          SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                          p.Direction = System.Data.ParameterDirection.Output;

                          paramCollection.Add(p);

                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                          int.TryParse(param["@Id"].Value.ToString(), out id);
                      });


            return id;
        }

        public bool UpdateWaypointItems(JobItemsUpdateRequest model)
        {
            bool success = false;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_WaypointItemsUpdate"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@Id", model.Id);
                          paramCollection.AddWithValue("@JobId", model.JobId);
                          paramCollection.AddWithValue("@ItemTypeId", model.ItemTypeId);
                          paramCollection.AddWithValue("@ItemNote", model.ItemNote);
                          paramCollection.AddWithValue("@Quantity", model.Quantity);
                          paramCollection.AddWithValue("@MediaId", model.MediaId);


                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                          success = true;
                      });


            return success;
        }

        public static bool UpdateJobPrice(int JobId, double price)
        {
            bool success = false;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_UpdatePrice"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", JobId);
                    paramCollection.AddWithValue("@Price", price);


                }, returnParameters: delegate (SqlParameterCollection param)
                {
                    success = true;
                });


            return success;

        }



        //filter version
        public PaginatedItemsResponse<Job> GetAllJobsWithFilter(PaginatedRequest model)
        {
            List<Job> JobList = null;
            PaginatedItemsResponse<Job> response = null;


            DataProvider.ExecuteCmd(GetConnection, "dbo.Jobs_SelectAll_v2"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@CurrentPage", model.CurrentPage);
                  paramCollection.AddWithValue("@ItemsPerPage", model.ItemsPerPage);
                  paramCollection.AddWithValue("@Query", model.Query);
                  paramCollection.AddWithValue("@QueryWebsiteId", model.QueryWebsiteId);
                  paramCollection.AddWithValue("@QueryStatus", model.QueryStatus);
                  paramCollection.AddWithValue("@QueryJobType", model.QueryJobType);
                  paramCollection.AddWithValue("@QueryStartDate", model.QueryStartDate);
                  paramCollection.AddWithValue("@QueryEndDate", model.QueryEndDate);

              }, map: delegate (IDataReader reader, short set)

              {

                  if (set == 0)
                  {
                      Job SingleJob = new Job();
                      int startingIndex = 0; //startingOrdinal

                      SingleJob.Id = reader.GetSafeInt32(startingIndex++);
                      SingleJob.UserId = reader.GetSafeString(startingIndex++);
                      SingleJob.JobStatus = reader.GetSafeEnum<JobStatus>(startingIndex++);
                      SingleJob.JobType = reader.GetSafeEnum<JobsType>(startingIndex++);
                      SingleJob.Price = reader.GetSafeDecimal(startingIndex++);
                      SingleJob.Phone = reader.GetSafeString(startingIndex++);
                      SingleJob.JobRequest = reader.GetSafeInt32(startingIndex++);
                      SingleJob.SpecialInstructions = reader.GetSafeString(startingIndex++);
                      SingleJob.Created = reader.GetSafeDateTime(startingIndex++);
                      SingleJob.Modified = reader.GetSafeDateTime(startingIndex++);
                      SingleJob.WebsiteId = reader.GetSafeInt32(startingIndex++);

                      if (JobList == null)
                      {
                          JobList = new List<Job>();
                      }
                      JobList.Add(SingleJob);

                  }
                  else if (set == 1)

                  {
                      response = new PaginatedItemsResponse<Job>();
                      response.TotalItems = reader.GetSafeInt32(0);

                  }
              }

           );

            response.Items = JobList;
            response.CurrentPage = model.CurrentPage;
            response.ItemsPerPage = model.ItemsPerPage;

            return response;
        }

        
        public Job GetJobById(int Id)
        {

            Job Job = null;
            List<JobWaypoint> Waypoints = null;
            List<JobWaypointItem> Items = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Jobs_SelectJobByJobId"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@Id", Id);

              }, map: delegate (IDataReader reader, short set)
              {
                  if (set == 0)
                  {
                      int startingIndex = 0;
                      Job = new Job();
                      Job.Id = reader.GetSafeInt32(startingIndex++);
                      Job.UserId = reader.GetSafeString(startingIndex++);
                      Job.JobStatus = reader.GetSafeEnum<JobStatus>(startingIndex++);
                      Job.JobType = reader.GetSafeEnum<JobsType>(startingIndex++);
                      Job.Price = reader.GetSafeDecimal(startingIndex++);
                      Job.JobRequest = reader.GetSafeInt32(startingIndex++);
                      Job.ContactName = reader.GetSafeString(startingIndex++);
                      Job.Phone = reader.GetSafeString(startingIndex++);
                      Job.SpecialInstructions = reader.GetSafeString(startingIndex++);
                      Job.Created = reader.GetSafeDateTime(startingIndex++);
                      Job.Modified = reader.GetSafeDateTime(startingIndex++);
                      Job.WebsiteId = reader.GetSafeInt32(startingIndex++);
                      Job.ExternalJobId = reader.GetSafeInt32(startingIndex++);
                      Job.ExternalCustomerId = reader.GetSafeInt32(startingIndex++);


                      Job.User = new UserProfile();
                      Job.User.UserId = reader.GetSafeString(startingIndex++);
                      Job.User.FirstName = reader.GetSafeString(startingIndex++);
                      Job.User.LastName = reader.GetSafeString(startingIndex++);
                      Job.User.ExternalUserId = reader.GetSafeString(startingIndex++);
                      Job.User.DateCreated = reader.GetSafeDateTime(startingIndex++);
                      Job.User.DateModified = reader.GetSafeDateTime(startingIndex++);
                      Job.User.MediaId = reader.GetSafeInt32(startingIndex++);
                      Job.User.TokenHash = reader.GetSafeGuid(startingIndex++).ToString(); // Unable to cast object of type 'System.Guid' to type 'System.String

                      Job.Website = new Website();
                      Job.Website.Id = reader.GetSafeInt32(startingIndex++);
                      Job.Website.Name = reader.GetSafeString(startingIndex++);
                      Job.Website.Slug = reader.GetSafeString(startingIndex++);
                      Job.Website.Description = reader.GetSafeString(startingIndex++);
                      Job.Website.Url = reader.GetSafeString(startingIndex++);
                      Job.Website.MediaId = reader.GetSafeInt32Nullable(startingIndex++);
                      Job.Website.DateCreated = reader.GetSafeUtcDateTime(startingIndex++);
                      Job.Website.DateModified = reader.GetSafeDateTime(startingIndex++);
                      Job.Website.Phone = reader.GetSafeString(startingIndex++);
                      Job.Website.AddressId = reader.GetSafeInt32Nullable(startingIndex++);

                      Job.Website.Media = new Media();
                      Job.Website.Media.Id = reader.GetSafeInt32(startingIndex++);
                      Job.Website.Media.Url = reader.GetSafeString(startingIndex++);

                      Job.User.CreditCards = new UserCreditCards();
                      Job.User.CreditCards.Id = reader.GetSafeInt32(startingIndex++);
                      Job.User.CreditCards.UserId = reader.GetSafeString(startingIndex++);
                      Job.User.CreditCards.ExternalCardIdNonce = reader.GetSafeString(startingIndex++);
                      Job.User.CreditCards.Last4DigitsCC = reader.GetSafeString(startingIndex++);
                      Job.User.CreditCards.CardType = reader.GetSafeString(startingIndex++);

                      Job.User.Email = reader.GetSafeString(startingIndex++);
                      Job.User.Phone = reader.GetSafeString(startingIndex++);

                  }
                  else if (set == 1)
                  {
                      int startingIndex = 0;

                      JobWaypoint Waypoint = new JobWaypoint();

                      Waypoint.Id = reader.GetSafeInt32(startingIndex++);
                      Waypoint.JobId = reader.GetSafeInt32(startingIndex++);
                      Waypoint.AddressId = reader.GetSafeInt32(startingIndex++);
                      Waypoint.SuiteNo = reader.GetSafeString(startingIndex++);
                      Waypoint.ContactName = reader.GetSafeString(startingIndex++);
                      Waypoint.Phone = reader.GetSafeString(startingIndex++);
                      Waypoint.SpecialInstructions = reader.GetSafeString(startingIndex++);
                      Waypoint.ServiceNote = reader.GetSafeString(startingIndex++);
                      Waypoint.Created = reader.GetSafeDateTime(startingIndex++);
                      Waypoint.Modified = reader.GetSafeDateTime(startingIndex++);
                      Waypoint.ExternalWaypointId = reader.GetSafeInt32(startingIndex++);
                      Waypoint.ExternalCustomerId = reader.GetSafeInt32(startingIndex++);


                      Waypoint.Address = new Address();
                      Waypoint.Address.AddressId = reader.GetSafeInt32(startingIndex++);
                      Waypoint.Address.DateCreated = reader.GetSafeDateTime(startingIndex++);
                      Waypoint.Address.DateModified = reader.GetSafeDateTime(startingIndex++);
                      Waypoint.Address.UserId = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.Name = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.ExternalPlaceId = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.Line1 = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.Line2 = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.City = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.State = reader.GetSafeString(startingIndex++);
                      Waypoint.Address.StateId = reader.GetSafeInt32(startingIndex++);
                      Waypoint.Address.ZipCode = reader.GetSafeInt32(startingIndex++);
                      Waypoint.Address.Latitude = reader.GetSafeDecimal(startingIndex++);
                      Waypoint.Address.Longitude = reader.GetSafeDecimal(startingIndex++);



                      if (Waypoints == null)
                      {
                          Waypoints = new List<JobWaypoint>();
                      }
                      Waypoints.Add(Waypoint);

                  }
                  else if (set == 2)
                  {
                      int startingIndex = 0;
                      JobWaypointItem Item = new JobWaypointItem();

                      Item.Id = reader.GetSafeInt32(startingIndex++);
                      Item.JobId = reader.GetSafeInt32(startingIndex++);
                      Item.WaypointId = reader.GetSafeInt32(startingIndex++);
                      Item.ItemTypeId = reader.GetSafeInt32(startingIndex++);
                      Item.ItemNote = reader.GetSafeString(startingIndex++);
                      Item.Quantity = reader.GetSafeInt32(startingIndex++);
                      Item.MediaId = reader.GetSafeInt32(startingIndex++);
                      Item.Created = reader.GetSafeDateTime(startingIndex++);
                      Item.Modified = reader.GetSafeDateTime(startingIndex++);
                      Item.Operation = reader.GetSafeInt32(startingIndex++);
                      Item.ParentItemId = reader.GetSafeInt32(startingIndex++);


                      Item.JobItem = new JobItemOptionsDomain();
                      Item.JobItem.Id = reader.GetSafeInt32(startingIndex++);
                      Item.JobItem.dateCreated = reader.GetSafeDateTime(startingIndex++);
                      Item.JobItem.dateModified = reader.GetSafeDateTime(startingIndex++);
                      Item.JobItem.Name = reader.GetSafeString(startingIndex++);
                      Item.JobItem.MinTime = reader.GetSafeInt32(startingIndex++);
                      Item.JobItem.websiteId = reader.GetSafeInt32(startingIndex++);
                      Item.JobItem.MediaId = reader.GetSafeInt32(startingIndex++);
                      Item.JobItem.MaxTime = reader.GetSafeInt32(startingIndex++);

                      Item.JobItem.Media = new Media();
                      Item.JobItem.Media.Id = reader.GetSafeInt32(startingIndex++);
                      Item.JobItem.Media.Url = reader.GetSafeString(startingIndex++);
                      Item.JobItem.Media.MediaType = reader.GetSafeInt32(startingIndex++);
                      Item.JobItem.Media.UserId = reader.GetSafeString(startingIndex++);
                      Item.JobItem.Media.Title = reader.GetSafeString(startingIndex++);
                      Item.JobItem.Media.Description = reader.GetSafeString(startingIndex++);

                      if (Item.MediaId != 0 && Item.MediaId != 1) //will table is trunicated the $$ Item.Media! = 1 can be removed from this line
                      {
                          Item.Media = new Media();
                          Item.Media.DateModified = reader.GetSafeDateTime(startingIndex++);
                          Item.Media.DateCreated = reader.GetSafeDateTime(startingIndex++);
                          Item.Media.Url = reader.GetSafeString(startingIndex++);
                          Item.Media.MediaType = reader.GetSafeInt32(startingIndex++);
                          Item.Media.UserId = reader.GetSafeString(startingIndex++);
                          Item.Media.Title = reader.GetSafeString(startingIndex++);
                          Item.Media.Description = reader.GetSafeString(startingIndex++);
                          Item.Media.ExternalMediaId = reader.GetSafeInt32(startingIndex++);
                          Item.Media.FileType = reader.GetSafeString(startingIndex++);
                      }
                      if (Items == null)
                      {
                          Items = new List<JobWaypointItem>();
                      }
                      Items.Add(Item);

                  }
              }
              );

            if (Waypoints != null)
            {

                foreach (var waypoint in Waypoints) // we loop through every Waypoint in our list we created in the beginning of our Get...
                {
                    foreach (var item in Items) // ... at the same time, we loop through every Item in our list we created in the beginning of our Get...
                    {
                        if (item.WaypointId == waypoint.Id) // ... and when an Item has a WaypointId that matches an Id in Waypoint...
                        {
                            if (waypoint.JobWaypointItemsPickup == null) // (null check, then instantiate a list to put Items in -- this goes in JobWaypoint Domain)
                            {
                                waypoint.JobWaypointItemsPickup = new List<JobWaypointItem>();
                            }
                            if (waypoint.JobWaypointItemsDropOff == null)
                            {
                                waypoint.JobWaypointItemsDropOff = new List<JobWaypointItem>();
                            }
                            if (item.Operation == 1) // ... if Item is for pick up, then ...
                            {
                                waypoint.JobWaypointItemsPickup.Add(item); // ... we add the particular Item to this list.
                            }
                            else // ... if Item is for drop off, then ...
                            {
                                waypoint.JobWaypointItemsDropOff.Add(item); // ... we add the particular Item to this list.
                            }
                        }
                    }
                }
                Job.JobWaypoints = Waypoints; //Job Domain already has a key value for a list of Waypoints -- we set that to our list that we created in beginning of this Get service.
            }

            //Edmund's Zipcode area
            if (Waypoints != null)
            {
                if (Job.JobWaypoints[0].Address.ZipCode != 0)
                {
                    int TeamId = 0;
                    string queryZipCode = (Job.JobWaypoints[0].Address.ZipCode).ToString();
                    TeamId = _WebsiteTeamService.GetTeamIdByZipcode(queryZipCode);
                    Job.TeamId = TeamId;

                }
            }

            return Job;


        }

        public void BringgCreateTaksWithWayPoints(int JobId)  //**This function calls CreateTaskWithWaypoints.Execute(Job) which does not work
        {
            Job Job = GetJobById(JobId);
            //CreateTaskWithWaypoints<Job> CreateTaskWithWaypoints = new CreateTaskWithWaypoints<Job>();
            _CreateTaskWithWaypoints.Execute(Job);
        }

        public void BringgCreateTask(int JobId)
        {
            // the next three lines of code submits a task (aka job) to Bringg
            Job Job = GetJobById(JobId);
            //CreateTask<Job> CreateTask = new CreateTask<Job>();
            _CreateTask.Execute(Job);

            // after creating a task in Bringg and getting back an ExternalJobId, we call GetJobById because it has the updated ExternalJobId
            Job = GetJobById(JobId); // job.userid != null call my function

            if (Job.UserId != null)
            {
                // pass in job to UserAddressService process job address function 

                _UserAddressService.ProcessJobAddresses(Job, Job.UserId);
            }
            // now that the taks/job has been created in Bringg (along with one Waypoint), we want add all additional Waypoints to Bringg
            // we do not get the first waypoint because it was already sent to bring in CreateTask.Execute(Job)

            //Daniel
            int counter = 0;
            foreach (var waypoint in Job.JobWaypoints)
            {
                if (counter > 0)
                {

                    waypoint.ExternalJobId = Job.ExternalJobId;
                   
                    _CreateWaypoint.Execute(waypoint);
                }
                counter++;
            }
        }

        public static void UpdateExternalJobId(int JobId, int ExternalJobId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_UpdateExternalJobId",
                inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", JobId);
                    paramCollection.AddWithValue("@ExternalJobId", ExternalJobId);
                });
        }

        public static void UpdateExternalWaypointId(int WaypointId, int ExternalWaypointId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_WaypointUpdateExternalWaypointId",
                inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@Id", WaypointId);
                    paramCollection.AddWithValue("@ExternalWaypointId", ExternalWaypointId);
                });
        }



        public List<JobWaypointItem> GetAllJobsItems()
        {
            List<JobWaypointItem> JobsItemList = new List<JobWaypointItem>();

            DataProvider.ExecuteCmd(GetConnection, "dbo.Jobs_SelectAllJobsItems"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {

              }, map: delegate (IDataReader reader, short set)
              {
                  JobWaypointItem SingleJobItem = new JobWaypointItem();
                  int startingIndex = 0; //startingOrdinal

                  SingleJobItem.Id = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.JobId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.ItemTypeId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.ItemNote = reader.GetSafeString(startingIndex++);
                  SingleJobItem.Quantity = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.MediaId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.AddressId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.Created = reader.GetSafeDateTime(startingIndex++);
                  SingleJobItem.Modified = reader.GetSafeDateTime(startingIndex++);

                  JobsItemList.Add(SingleJobItem);

              }

           );

            return JobsItemList;
        }

        public JobWaypointItem GetJobsItemsById(int Id)
        {
            JobWaypointItem SingleJobItem = new JobWaypointItem();

            DataProvider.ExecuteCmd(GetConnection, "dbo.Jobs_SelectJobsItemsById"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@Id", Id);

              }, map: delegate (IDataReader reader, short set)
              {
                  int startingIndex = 0;

                  SingleJobItem.Id = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.JobId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.ItemTypeId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.ItemNote = reader.GetSafeString(startingIndex++);
                  SingleJobItem.Quantity = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.MediaId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.AddressId = reader.GetSafeInt32(startingIndex++);
                  SingleJobItem.Created = reader.GetSafeDateTime(startingIndex++);
                  SingleJobItem.Modified = reader.GetSafeDateTime(startingIndex++);
              }

              );

            return SingleJobItem;
        }

        public static Job GetByExternalJobId(int id)
        {

            Job model = null;

            DataProvider.ExecuteCmd(GetConnection, "Jobs_SelectAllByExternalJobId",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@ExternalJobId", id);
                }, map: delegate (IDataReader reader, short set)
                {

                    model = new Job();
                    int startingIndex = 0;

                    model.Id = reader.GetSafeInt32(startingIndex++);
                    model.UserId = reader.GetSafeString(startingIndex++);
                    model.JobStatus = reader.GetSafeEnum<JobStatus>(startingIndex++);
                    model.JobType = reader.GetSafeEnum<JobsType>(startingIndex++);
                    model.Price = reader.GetSafeDecimal(startingIndex++);
                    model.JobRequest = reader.GetSafeInt32(startingIndex++);
                    model.ContactName = reader.GetSafeString(startingIndex++);
                    model.Phone = reader.GetSafeString(startingIndex++);
                    model.SpecialInstructions = reader.GetSafeString(startingIndex++);
                    model.Created = reader.GetSafeDateTime(startingIndex++);
                    model.Modified = reader.GetSafeDateTime(startingIndex++);
                    model.WebsiteId = reader.GetSafeInt32(startingIndex++);
                    model.ExternalJobId = reader.GetSafeInt32(startingIndex++);
                    model.ExternalCustomerId = reader.GetSafeInt32(startingIndex++);
                    model.PaymentNonce = reader.GetSafeString(startingIndex++);
                    model.BillingId = reader.GetSafeInt32(startingIndex++);


                });

            return model;


        }

        public int GetWebsiteIdByJobId(int JobId)
        {
            int WebsiteId = 0;
            DataProvider.ExecuteCmd(GetConnection, "Jobs_SelectWebsiteIdByJobId",
                inputParamMapper: delegate(SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@JobId", JobId);

                }, map: delegate(IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    WebsiteId = reader.GetSafeInt32(startingIndex++);
                });
            return WebsiteId;
        }

        public string GetUserIdByJobId(int JobId)
        {
            string UserId = null;
            DataProvider.ExecuteCmd(GetConnection, "Jobs_SelectUserIdByJobId",
                inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@JobId", JobId);

                }, map: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    UserId = reader.GetSafeString(startingIndex++);
                });
            return UserId;
        }

        public int InsertJob(JobInsertRequest model)
        {
            int id = 0;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_Insert"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@UserId", model.UserId);
                          paramCollection.AddWithValue("@Status", model.Status);
                          paramCollection.AddWithValue("@JobType", model.JobType);
                          paramCollection.AddWithValue("@Price", model.Price);
                          paramCollection.AddWithValue("@Phone", model.Phone);
                          paramCollection.AddWithValue("@JobRequest", model.JobRequest);
                          paramCollection.AddWithValue("@SpecialInstructions", model.SpecialInstructions);
                          paramCollection.AddWithValue("@WebsiteId", model.WebsiteId);
                          paramCollection.AddWithValue("@ExternalCustomerId", model.ExternalCustomerId);

                          SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                          p.Direction = System.Data.ParameterDirection.Output;

                          paramCollection.Add(p);

                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                          int.TryParse(param["@Id"].Value.ToString(), out id);
                      });


            return id;
        }

        public bool UpdateJob(JobUpdateRequest model)
        {
            bool success = false;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_Update_Test"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)

                      {
                          paramCollection.AddWithValue("@Id", model.Id);
                          paramCollection.AddWithValue("@UserId", model.UserId);
                          paramCollection.AddWithValue("@Status", model.Status);
                          paramCollection.AddWithValue("@JobType", model.JobType);
                          paramCollection.AddWithValue("@Price", model.Price);
                          paramCollection.AddWithValue("@JobRequest", model.JobRequest);
                          paramCollection.AddWithValue("@ContactName",model.ContactName);
                          paramCollection.AddWithValue("@Phone", model.Phone);
                          paramCollection.AddWithValue("@SpecialInstructions", model.SpecialInstructions);
                          paramCollection.AddWithValue("@WebsiteId", model.WebsiteId);
                          paramCollection.AddWithValue("@ExternalJobId", model.ExternalJobId);
                          paramCollection.AddWithValue("@ExternalCustomerId", model.ExternalCustomerId);
                          paramCollection.AddWithValue("@PaymentNonce", model.PaymentNonce);
                          paramCollection.AddWithValue("@BillingId", model.BillingId);

                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                          success = true;
                      });


            return success;
        }


        public bool DeleteJob(JobDeleteRequest model)
        {
            bool success = false;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_DeleteById"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@Id", model.Id);

                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                          success = true;
                      });

            return success;
        }


        public bool DeleteJobItem(int ItemId)
        {
            bool success = false;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_WaypointItemsDelete"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@Id", ItemId);

                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                          success = true;
                      });

            return success;
        }

        public static void UpdateJobStatus(JobStatus jobStatus, int externalJobId)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Jobs_UpdateJobStatus",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@JobStatus", jobStatus);
                    parameterCollection.AddWithValue("@ExternalJobId", externalJobId);


                });


        }

    }
}
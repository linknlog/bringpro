using Sabio.Data;
using Sabio.Web.Domain;
using Sabio.Web.Models.Requests;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Sabio.Web.Services
{
    public class JobItemOptionsServices : BaseService, IJobItemOptionsService
    {

        //postJobItemOptions: sends a new entry of Job Item options to the database.
        public int postJobItemOptions(JobItemOptionsInsertRequest model)
        {

            int Id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.JobItemOptions_Insert"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@Name", model.Name);
                     paramCollection.AddWithValue("@Weight", model.MinTime);
                     paramCollection.AddWithValue("@WebsiteId", model.WebsiteId);
                     paramCollection.AddWithValue("@MediaId", model.MediaId);
                     paramCollection.AddWithValue("@WeightMax", model.MaxTime);

                     SqlParameter jobitemoptionID = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                     jobitemoptionID.Direction = System.Data.ParameterDirection.Output;

                     paramCollection.Add(jobitemoptionID);

                 }, returnParameters: delegate (SqlParameterCollection param)
                 {
                     int.TryParse(param["@Id"].Value.ToString(), out Id);
                 }
                 );
            return Id;
        }

        //getIdJobItemOptions: retrieves an entry of Job Item options by its Id.
        public JobItemOptionsDomain getIdJobItemOptions(int id)
        {
            JobItemOptionsDomain jobInfo = new JobItemOptionsDomain();
            try
            {

                DataProvider.ExecuteCmd(GetConnection, "dbo.JobItemOptions_SelectById"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@Id", id);
                 }, map: delegate (IDataReader reader, short set)
                 {
                     int startingIndex = 0;

                     jobInfo.Id = reader.GetSafeInt32(startingIndex++);
                     jobInfo.dateCreated = reader.GetSafeUtcDateTime(startingIndex++);
                     jobInfo.dateModified = reader.GetSafeUtcDateTime(startingIndex++);
                     jobInfo.Name = reader.GetSafeString(startingIndex++);
                     jobInfo.MinTime = reader.GetSafeInt32(startingIndex++);
                     jobInfo.websiteId = reader.GetSafeInt32(startingIndex++);
                     jobInfo.MediaId = reader.GetSafeInt32(startingIndex++);
                     jobInfo.MaxTime = reader.GetSafeInt32(startingIndex++);

                     Media m = new Media();

                     m.Id = reader.GetSafeInt32(startingIndex++);
                     m.DateModified = reader.GetSafeDateTime(startingIndex++);
                     m.DateCreated = reader.GetSafeDateTime(startingIndex++);
                     m.Url = reader.GetSafeString(startingIndex++);
                     m.MediaType = reader.GetSafeInt32(startingIndex++);
                     m.UserId = reader.GetSafeString(startingIndex++);
                     m.Title = reader.GetSafeString(startingIndex++);
                     m.Description = reader.GetSafeString(startingIndex++);
                     m.ExternalMediaId = reader.GetSafeInt32(startingIndex++);
                     m.FileType = reader.GetSafeString(startingIndex++);

                     if (m.Id != 0)
                     {
                         jobInfo.Media = m;
                     }
                 });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return jobInfo;
        }

        //getAllJobItemOptions: retrieves all entries of Job Item options.
        public List<JobItemOptionsDomain> getAllJobItemOptions()
        {
            List<JobItemOptionsDomain> jobList = new List<JobItemOptionsDomain>();

            try
            {

                DataProvider.ExecuteCmd(GetConnection, "dbo.JobItemOptions_SelectAll"
                    , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                    {

                    }, map: delegate (IDataReader reader, short set)
                    {
                        JobItemOptionsDomain jobInfo = new JobItemOptionsDomain();
                        int startingIndex = 0;

                        jobInfo.Id = reader.GetSafeInt32(startingIndex++);
                        jobInfo.dateCreated = reader.GetSafeUtcDateTime(startingIndex++);
                        jobInfo.dateModified = reader.GetSafeUtcDateTime(startingIndex++);
                        jobInfo.Name = reader.GetSafeString(startingIndex++);
                        jobInfo.MinTime = reader.GetSafeInt32(startingIndex++);
                        jobInfo.websiteId = reader.GetSafeInt32(startingIndex++);
                        jobInfo.MediaId = reader.GetSafeInt32(startingIndex++);
                        jobInfo.MaxTime = reader.GetSafeInt32(startingIndex++);

                        Media m = new Media();

                        m.Id = reader.GetSafeInt32(startingIndex++);
                        m.DateModified = reader.GetSafeDateTime(startingIndex++);
                        m.DateCreated = reader.GetSafeDateTime(startingIndex++);
                        m.Url = reader.GetSafeString(startingIndex++);
                        m.MediaType = reader.GetSafeInt32(startingIndex++);
                        m.UserId = reader.GetSafeString(startingIndex++);
                        m.Title = reader.GetSafeString(startingIndex++);
                        m.Description = reader.GetSafeString(startingIndex++);
                        m.ExternalMediaId = reader.GetSafeInt32(startingIndex++);
                        m.FileType = reader.GetSafeString(startingIndex++);

                        if (m.Id != 0)
                        {
                            jobInfo.Media = m;
                        }
                        jobList.Add(jobInfo);
                    });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return jobList;
        }

        //updateJobItemOptions: updates an entry of Job Item options in the database.
        public bool updateJobItemOptions(JobItemOptionsUpdateRequest model)
        {

            bool isSuccess = false;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.JobItemOptions_Update"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@Id", model.Id);
                     paramCollection.AddWithValue("@Name", model.Name);
                     paramCollection.AddWithValue("@Weight", model.MinTime);
                     paramCollection.AddWithValue("@WebsiteId", model.WebsiteId);
                     paramCollection.AddWithValue("@MediaId", model.MediaId);
                     paramCollection.AddWithValue("@WeightMax", model.MaxTime);

                 }, returnParameters: delegate (SqlParameterCollection param)
                 {
                     isSuccess = true;
                 });
            return isSuccess;
        }

        //deleteJobItemOptions: deletes an entry of Job Item options by its Id.
        public bool deleteJobItemOptions(JobItemOptionsDeleteRequest model)
        {
            bool isSuccess = false;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.JobItemOptions_Delete"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@Id", model.Id);

                 }, returnParameters: delegate (SqlParameterCollection param)
                 {
                     isSuccess = true;
                 }
                );
            return isSuccess;
        }
    }
}
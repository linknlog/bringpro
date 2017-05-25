using Sabio.Data;
using Sabio.Web.Domain;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sabio.Web.Services
{
    public class AdminRoleService : BaseService,IAdminRoleService
    {

        public void AddRoleToUser(string UserId, string RoleId)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.AspNetUserRoles_Insert"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@UserId", UserId);
                          paramCollection.AddWithValue("@RoleId", RoleId);

                      }, returnParameters: delegate (SqlParameterCollection param)
                      {
                      });
        }

        public List<UserRole> GetAllRoles()
        {
            List<UserRole> roleList = new List<UserRole>();

            DataProvider.ExecuteCmd(GetConnection, "dbo.AdminUserRoles_GetRoles"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {


              }, map: delegate (IDataReader reader, short set)
              {
                  UserRole SingleRole = new UserRole();
                  int startingIndex = 0;

                  SingleRole.RoleId = reader.GetSafeString(startingIndex++);
                  SingleRole.Name = reader.GetSafeString(startingIndex++);

                  roleList.Add(SingleRole);

              }

           );
            return roleList;

        }

        public UserProfile GetUserRoleById(Guid roleGuid)
        {
            UserProfile userById = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AspUserRole_GetId"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@userId", roleGuid);


              }, map: delegate (IDataReader reader, short set)
              {

                  userById = new UserProfile();
                  int startingIndex = 0; //startingOrdinal

                  userById.UserId = reader.GetSafeString(startingIndex++);
                  userById.RoleId = reader.GetSafeString(startingIndex++);

              }

           );
            return userById;

        }


    }
}


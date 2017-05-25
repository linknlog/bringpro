using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using Sabio.Data;
using Sabio.Web.Classes.Tasks.Bringg;
using Sabio.Web.Classes.Tasks.Bringg.Interfaces;
using Sabio.Web.Domain;
using Sabio.Web.Exceptions;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Requests.Bringg;
using Sabio.Web.Models.Requests.Users;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Sabio.Web.Services
{
    public class AdminUserService : BaseService, IAdminUserService
    {
        [Dependency]
        public IUserEmailService _EmailService { get; set; }

        [Dependency("CreateUser")]
        public IBringgTask<BringgCreateUserRequest> _BringgTask { get; set; }



        public void CreateUserProfile(string userId, UserProfile model)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UserProfiles_Insert"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@UserId", userId);
                    paramCollection.AddWithValue("@FirstName", model.FirstName);
                    paramCollection.AddWithValue("@LastName", model.LastName);
                    paramCollection.AddWithValue("@ExternalUserId", model.ExternalUserId);

                }
                );
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UserWebsite_Insert"
                     , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                     {
                         paramCollection.AddWithValue("@UserId", userId);

                         SqlParameter s = new SqlParameter("@WebsiteIds", SqlDbType.Structured);
                         if (model.WebsiteIds != null && model.WebsiteIds.Any())
                         {
                             s.Value = new IntIdTable(model.WebsiteIds);
                         }
                         paramCollection.Add(s);

                     });

            BringgCreateUserRequest bringgRequest = new BringgCreateUserRequest();
            bringgRequest.Name = model.FirstName + " " + model.LastName;
            bringgRequest.Phone = model.Phone;
            bringgRequest.Email = model.Email;
            bringgRequest.Password = model.Password;
            bringgRequest.userId = userId;

            bool isDriver = false;
            if (model.RoleId == "46D45B66-AE30-4F99-A7C4-9C7B697E0BB4") //driver role ID
            {
                isDriver = true;
            }
            bringgRequest.Driver = isDriver;
            _BringgTask.Execute(bringgRequest);

        }

        public void CreateUserRole(string userId, UserProfile model)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.AspNetUserRoles_Insert"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@UserId", userId);
                    paramCollection.AddWithValue("@RoleId", model.RoleId);


                }
                );
        }

        private ApplicationUserManager GetUserManager()
        {
            throw new NotImplementedException();
        }

        public PaginatedItemsResponse<UserProfile> GetAllUsers(PaginatedRequest model)
        {

            List<UserProfile> userList = null;
            PaginatedItemsResponse<UserProfile> response = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AdminUserProfiles_GetUsers_v2"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@CurrentPage", model.CurrentPage);
                  paramCollection.AddWithValue("@ItemsPerPage", model.ItemsPerPage);
                  paramCollection.AddWithValue("@Query", model.Query);


              }, map: delegate (IDataReader reader, short set)
              {

                  if (set == 0)
                  {
                      UserProfile SingleUser = new UserProfile();
                      int startingIndex = 0;

                      SingleUser.UserId = reader.GetSafeString(startingIndex++);
                      SingleUser.FirstName = reader.GetSafeString(startingIndex++);
                      SingleUser.LastName = reader.GetSafeString(startingIndex++);
                      SingleUser.DateCreated = reader.GetSafeDateTime(startingIndex++);
                      SingleUser.DateModified = reader.GetSafeDateTime(startingIndex++);
                      SingleUser.Email = reader.GetSafeString(startingIndex++);
                      SingleUser.Phone = reader.GetSafeString(startingIndex++);
                      SingleUser.Role.RoleId = reader.GetSafeString(startingIndex++);
                      SingleUser.Role.Name = reader.GetSafeString(startingIndex++);

                      if (userList == null)
                      {
                          userList = new List<UserProfile>();
                      }
                      userList.Add(SingleUser);

                  }
                  else if (set == 1)
                  {

                      response = new PaginatedItemsResponse<UserProfile>();

                      response.TotalItems = reader.GetSafeInt32(0);

                  }
              }

           );
            response.Items = userList;
            response.CurrentPage = model.CurrentPage;
            response.ItemsPerPage = model.ItemsPerPage;
            return response;

        }

        public UserProfile GetUserById(Guid jobGuid)
        {

            UserProfile userById = null;
            List<int> Websites = new List<int>();
            DataProvider.ExecuteCmd(GetConnection, "dbo.AdminUserBy_Id_v4"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@userId", jobGuid);


              }, map: delegate (IDataReader reader, short set)
              {
                  if (set == 0)
                  {
                      userById = new UserProfile();
                      int startingIndex = 0; //startingOrdinal

                      userById.UserId = reader.GetSafeString(startingIndex++);
                      userById.FirstName = reader.GetSafeString(startingIndex++);
                      userById.LastName = reader.GetSafeString(startingIndex++);
                      userById.DateCreated = reader.GetSafeDateTime(startingIndex++);
                      userById.DateModified = reader.GetSafeDateTime(startingIndex++);
                      userById.ExternalUserId = reader.GetSafeString(startingIndex++);
                      userById.Email = reader.GetSafeString(startingIndex++);
                      userById.Phone = reader.GetSafeString(startingIndex++);
                      userById.Role.RoleId = reader.GetSafeString(startingIndex++);
                      userById.Role.Name = reader.GetSafeString(startingIndex++);

                  }
                  else if (set == 1)
                  {
                      int startingIndex = 0;
                      Websites.Add(reader.GetSafeInt32(startingIndex++));
                  }


              }

           );

            userById.WebsiteIds = Websites.ToArray();
            return userById;

        }
        public static UserProfile GetUserByIdStatic(Guid jobGuid)
        {

            UserProfile userById = null;
            List<int> Websites = new List<int>();
            DataProvider.ExecuteCmd(GetConnection, "dbo.AdminUserBy_Id_v4"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@userId", jobGuid);


              }, map: delegate (IDataReader reader, short set)
              {
                  if (set == 0)
                  {
                      userById = new UserProfile();
                      int startingIndex = 0; //startingOrdinal

                      userById.UserId = reader.GetSafeString(startingIndex++);
                      userById.FirstName = reader.GetSafeString(startingIndex++);
                      userById.LastName = reader.GetSafeString(startingIndex++);
                      userById.DateCreated = reader.GetSafeDateTime(startingIndex++);
                      userById.DateModified = reader.GetSafeDateTime(startingIndex++);
                      userById.ExternalUserId = reader.GetSafeString(startingIndex++);
                      userById.Email = reader.GetSafeString(startingIndex++);
                      userById.Phone = reader.GetSafeString(startingIndex++);
                      userById.Role.RoleId = reader.GetSafeString(startingIndex++);
                      userById.Role.Name = reader.GetSafeString(startingIndex++);

                  }
                  else if (set == 1)
                  {
                      int startingIndex = 0;
                      Websites.Add(reader.GetSafeInt32(startingIndex++));
                  }


              }

           );

            userById.WebsiteIds = Websites.ToArray();
            return userById;

        }
        public PaginatedItemsResponse<UserProfile> GetUserByWebsiteId(int WebsiteId, PaginatedRequest model)
        {
            List<UserProfile> List = new List<UserProfile>();
            PaginatedItemsResponse<UserProfile> response = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AdminUserProfiles_GetUsersByWebsiteId"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@WebsiteId", WebsiteId);
                  paramCollection.AddWithValue("@CurrentPage", model.CurrentPage);
                  paramCollection.AddWithValue("@ItemsPerPage", model.ItemsPerPage);
                  paramCollection.AddWithValue("@Query", model.Query);


              }, map: delegate (IDataReader reader, short set)
              {
                  if (set == 0)
                  {
                      UserProfile up = new UserProfile();
                      int startingIndex = 0; //startingOrdinal

                      up.UserId = reader.GetSafeString(startingIndex++);
                      up.FirstName = reader.GetSafeString(startingIndex++);
                      up.LastName = reader.GetSafeString(startingIndex++);
                      up.DateCreated = reader.GetSafeDateTime(startingIndex++);
                      up.DateModified = reader.GetSafeDateTime(startingIndex++);
                      up.WebsiteId = reader.GetSafeInt32(startingIndex++);
                      up.Email = reader.GetSafeString(startingIndex++);
                      up.Phone = reader.GetSafeString(startingIndex++);
                      up.Role.RoleId = reader.GetSafeString(startingIndex++);
                      up.Role.Name = reader.GetSafeString(startingIndex++);

                      if (List == null)
                      {
                          List = new List<UserProfile>();
                      }
                      List.Add(up);

                  }

                  else if (set == 1)
                  {

                      response = new PaginatedItemsResponse<UserProfile>();

                      response.TotalItems = reader.GetSafeInt32(0);

                  }

              }

              );

            response.Items = List;
            response.CurrentPage = model.CurrentPage;
            response.ItemsPerPage = model.ItemsPerPage;
            return response;
        }

        public void AdminUpdateUserRole(string UserId, string RoleId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "[dbo].[AspNetRoles_Update]"
                      , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                      {
                          paramCollection.AddWithValue("@UserId", UserId);
                          paramCollection.AddWithValue("@RoleId", RoleId);

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


        public UserProfile GetUserRoleById(Guid jobGuid)
        {

            UserProfile userById = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AspUserRole_GetId"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@userId", jobGuid);


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


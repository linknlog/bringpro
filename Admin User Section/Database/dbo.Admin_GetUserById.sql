
ALTER Proc [dbo].[AdminUserBy_Id_v2]

 @userId nvarchar(128)


AS

/* TEST CODE

  Declare @userId nvarchar(128) = '71344e50-3470-4662-a6df-3488ab5fc064'
  exec AdminUserBy_Id @userId

*/

BEGIN

SELECT [uP].[UserId]
      ,[uP].[FirstName]
      ,[uP].[LastName]
      ,[uP].[DateAdded]
      ,[uP].[DateModified]
      ,[nU].[Email]
      ,[nU].[PhoneNumber]
      ,[nR].[Id]
      ,[nR].[Name]

  FROM [dbo].[UserProfiles] as uP
       left join [dbo].[AspNetUsers] as nU
       on [uP].[UserId] = [nU].[Id]
       left join [dbo].[AspNetUserRoles] as uR
       on [uP].[UserId] = [uR].[UserId]
       left join [dbo].[AspNetRoles] as nR
       on [uR].[RoleId] = [nR].[Id]

  WHERE uP.[UserId] = @userId


END





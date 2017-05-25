

/*includes pagination and table joins*/

ALTER Proc [dbo].[AdminUserProfiles_GetUsers_v2]

        @CurrentPage int = 1
      , @ItemsPerPage int = 50
      , @Query nvarchar(128) = null

AS

/*-----TEST CODE-----


exec [dbo].[AdminUserProfiles_GetUsers_v2]

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
  
    WHERE
        (@Query  IS NULL OR
        (uP.FirstName LIKE '%'+@Query+'%') OR
        (uP.LastName LIKE '%'+@Query+'%') OR
        (nU.Email LIKE '%'+@Query+'%'))
        

  ORDER BY UserId 
    OFFSET ((@CurrentPage - 1) * @ItemsPerPage) ROWS
        FETCH NEXT  @ItemsPerPage ROWS ONLY

    SELECT COUNT('UserId')

      FROM [dbo].[UserProfiles] as uP
           left join [dbo].[AspNetUsers] as nU
           on [uP].[UserId] = [nU].[Id]

    WHERE
        (@Query  IS NULL OR
        (uP.FirstName LIKE '%'+@Query+'%') OR
        (uP.LastName LIKE '%'+@Query+'%') OR
        (nU.Email LIKE '%'+@Query+'%'))


END





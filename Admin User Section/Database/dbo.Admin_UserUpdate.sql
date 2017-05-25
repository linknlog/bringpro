
/*updating multiple tables with one stored proc*/

USE [C28]
GO
/****** Object:  StoredProcedure [dbo].[AdminUser_Update]    Script Date: 5/24/2017 11:37:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER proc [dbo].[AdminUser_Update]
        @UserId nvarchar(128)
        ,@FirstName nvarchar(100)
        ,@LastName nvarchar(100)
        ,@Email nvarchar(256)
        ,@PhoneNumber nvarchar(MAX)
        ,@RoleId nvarchar (128)


as


/*-----TEST CODE-----

Declare @UserId nvarchar(128) = '5aac3baf-8822-4f04-9565-44d4fe460305'

SELECT *
FROM  [dbo].[UserProfiles]
Where [UserId] = @UserId

SELECT *
FROM  [dbo].[AspNetUsers]
Where Id = @UserId



SELECT *
FROM  [dbo].[AspNetUserRoles]
Where userid = @UserId

Exec [dbo].[AdminUser_Update] 
@UserId




*/


BEGIN

UPDATE [dbo].[UserProfiles]

    SET [FirstName] = @FirstName
       ,[LastName] = @LastName
       ,[DateModified] = GETUTCDATE()

    WHERE UserId = @UserId


UPDATE [dbo].[AspNetusers]

    SET [Email] = @Email
       ,[PhoneNumber] = @PhoneNumber

    WHERE Id = @UserId

UPDATE [dbo].[AspNetuserRoles]

    SET [RoleId] = @RoleId

    WHERE UserId = @UserId

END
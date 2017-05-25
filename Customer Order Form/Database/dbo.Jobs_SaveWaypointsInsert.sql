USE [C28]
GO
/****** Object:  StoredProcedure [dbo].[Jobs_SaveWaypointsInsert]    Script Date: 5/24/2017 10:38:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Proc [dbo].[Jobs_SaveWaypointsInsert]
         @Id int Output
        ,@JobId int 
        ,@AddressId int
        ,@SuiteNo nvarchar(50) = null
        ,@ContactName nvarchar(150) = null
        ,@Phone varchar(30) = null
        ,@SpecialInstructions nvarchar(MAX) = null
        ,@ServiceNote nvarchar(150) = null
        ,@ExternalCustomerId int = null


as
/*TEST CODE
Declare @Id int = 0;

Declare  @JobId int = 1
        ,@AddressId int = 2
        ,@SuiteNo nvarchar(50) = null
        ,@ContactName nvarchar(150) = null
        ,@Phone varchar(30) = null
        ,@SpecialInstructions nvarchar(MAX) = null
        ,@ServiceNote nvarchar(150) = null
        ,@ExternalCustomerId int = null

INSERT INTO [dbo].[JobWaypoints]
           ([JobId]
           ,[AddressId]
           ,[SuiteNo]
           ,[ContactName]
           ,[Phone]
           ,[SpecialInstructions]
           ,[ServiceNote]
           ,[ExternalCustomerId])

     VALUES
           (@JobId
           ,@AddressId
           ,@SuiteNo
           ,@ContactName
           ,@Phone
           ,@SpecialInstructions
           ,@ServiceNote
           ,@ExternalCustomerId)

    Set @Id = SCOPE_IDENTITY()

    Select*
    From dbo.JobWaypoints


*/
Begin

    INSERT INTO [dbo].JobWaypoints
           ([JobId]
           ,[AddressId]
           ,[SuiteNo]
           ,[ContactName]
           ,[Phone]
           ,[SpecialInstructions]
           ,[ServiceNote]
           ,[ExternalCustomerId])

    Values
           (@JobId
           ,@AddressId
           ,@SuiteNo
           ,@ContactName
           ,@Phone
           ,@SpecialInstructions
           ,@ServiceNote
           ,@ExternalCustomerId)

    Set @Id = SCOPE_IDENTITY()

End
USE [C28]
GO
/****** Object:  StoredProcedure [dbo].[Jobs_WaypointItemsInsert]    Script Date: 5/24/2017 10:42:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Proc [dbo].[Jobs_WaypointItemsInsert]
     
    @JobId int
    ,@WaypointId int
    ,@ItemTypeId int
    ,@ItemNote nvarchar(150) = null
    ,@Quantity int
    ,@MediaId int = null
    ,@Operation int = null
    ,@ParentItemId int = null
    ,@Id int Output

  
as
/*TEST CODE
Declare @Id int = 36;
Declare  @JobId int = 5
    ,@WaypointId int = 88623487
    ,@ItemTypeId int = 2
    ,@ItemNote nvarchar(150) = null
    ,@Quantity int = 3
    ,@MediaId int = null
    ,@Operation int = 1
    ,@ParentItemId int = null

Exec [dbo].[Jobs_WaypointItemsInsert]
           @JobId
          ,@WaypointId 
          ,@ItemTypeId
          ,@ItemNote
          ,@Quantity
          ,@MediaId
          ,@Operation
          ,@ParentItemId
          ,@Id OUTPUT

  Select*
  From dbo.JobWaypointsItems



*/
Begin

  INSERT INTO [dbo].[JobWaypointsItems]
           ([JobId]
       ,[WaypointId]
           ,[ItemTypeId]
           ,[ItemNote]
           ,[Quantity]
           ,[MediaId]
       ,[Operation]
       ,[ParentItemId])


  VALUES
       (@JobId
       ,@WaypointId
       ,@ItemTypeId
       ,@ItemNote
       ,@Quantity
       ,@MediaId
       ,@Operation
       ,@ParentItemId)


  Set @Id = SCOPE_IDENTITY();

End


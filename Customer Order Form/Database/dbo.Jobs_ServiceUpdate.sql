USE [C28]
GO
/****** Object:  StoredProcedure [dbo].[Jobs_ServiceUpdate]    Script Date: 5/24/2017 10:43:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[Jobs_ServiceUpdate]
     @Id int
    ,@JobId int 
    ,@Service int

as

/*
DECLARE @Id int = 1

DECLARE  @JobId int = 5
    ,@Service int = 5


Update dbo.JobsService
  Set 
     [Modified] = getutcdate()
    ,[JobId] = @JobId
        ,[ServiceId] = @Service

    
  Where Id = @Id

Select * 
From dbo.JobsService
where @Id = id
*/

Begin
  
  Update dbo.JobsService
  Set 
     [Modified] = getutcdate()
    ,[JobId] = @JobId
        ,[Service] = @Service
    
  Where Id = @Id
    

End

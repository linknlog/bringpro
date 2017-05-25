USE [C28]
GO
/****** Object:  StoredProcedure [dbo].[Jobs_SelectAll]    Script Date: 5/24/2017 10:47:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[Jobs_SelectAll]
    @CurrentPage int = 1
     ,@ItemsPerPage int = 5
     ,@Query nvarchar(50) = null


As
/*
Declare @CurrentPage int = 1
Exec dbo.Jobs_SelectAll

*/

Begin

  SELECT [Id]
      ,[UserId]
      ,[Status]
      ,[JobType]
      ,[Price]
      ,[Phone]
      ,[JobRequest]
      ,[SpecialInstructions]
      ,[Created]
      ,[Modified]
    ,[WebsiteId]

  From dbo.Jobs

    Where @Query IS NULL OR 
    [UserId] Like ('%'+@Query+'%') OR
    [Status] Like ('%'+@Query+'%') OR
    [JobType] Like ('%'+@Query+'%') OR
    [Price] Like ('%'+@Query+'%') OR
    [Phone] Like ('%'+@Query+'%')
  

  ORDER BY [Created] DESC

  OFFSET ((@CurrentPage - 1) * @ItemsPerPage) ROWS
             FETCH NEXT  @ItemsPerPage ROWS ONLY 
        
  SELECT COUNT('Id')

  FROM [dbo].[Jobs]

      Where @Query IS NULL OR 
    [UserId] Like ('%'+@Query+'%') OR
    [Status] Like ('%'+@Query+'%') OR
    [JobType] Like ('%'+@Query+'%') OR
    [Price] Like ('%'+@Query+'%') OR
    [Phone] Like ('%'+@Query+'%')
  


End
USE [C28]
GO
/****** Object:  StoredProcedure [dbo].[JobItemOptions_Delete]    Script Date: 5/24/2017 10:44:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[JobItemOptions_Delete]
        @Id int
as

/* --- TEST CODE ----
Declare @Id int = 11

Select * from [dbo].[JobItemOptions]

Execute [dbo].[JobItemOptions_Delete] @Id

Select * from [dbo].[JobItemOptions]

--- END TEST CODE ----*/

BEGIN

DELETE FROM [dbo].[JobItemOptions]
      WHERE @Id = Id
END

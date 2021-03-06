/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP 1000 [ID]
      ,[StartTime]
      ,[EndTime]
	  ,DateDiff(second, Starttime, Endtime) as TotalSeconds
      ,[AppName]
      ,[WindowTitle]
      ,[DevProjectName]
      ,[ModuleName]
      ,[ITProjectID]
      ,[UserName]
      ,[MachineName]
      ,[UserDisplayName]
  FROM [DevTrkr].[dbo].[WindowEvents]
  order by StartTime desc

 -- 	select 
	--  sum(TotalSeconds) / 3600 as Hours, 
	--  (sum(TotalSeconds) % 3600) / 60 as Minutes, 
	--  sum(TotalSeconds) % 60 as Seconds
	--from
	--(
	--	select WindowTitle, DateDiff(second, Starttime, Endtime) as TotalSeconds 
	--	from DevTrkr..WindowEvents with (nolock)
	--	where AppName='ComputerLocked'
	--) x

	select newid()

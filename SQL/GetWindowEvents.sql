/****** Script for SelectTopNRows command from SSMS  ******/
SELECT * 
  FROM [DevTrkr].[dbo].[WindowEvents]
  order by StartTime desc

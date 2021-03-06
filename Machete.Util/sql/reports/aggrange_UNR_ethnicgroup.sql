declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'
select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByEthnicGroup-' + convert(varchar(5), min(WW.raceID)) as id,
L.text_EN as [Ethnic group], 
count(*) as [Count]
FROM (
  select W.ID, W.raceID
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.raceID
) as WW
JOIN dbo.Lookups L ON L.ID = WW.raceID
group by L.text_EN

union 

select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByEthnicGroup-NULL' as id,
'NULL' as [Ethnic group], 
count(*) as [Count]
from (
   select W.ID, min(dateforsignin) firstsignin
   from workers W
   JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
   WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
   and W.raceID is null
   group by W.ID
) as WWW

union
select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByEthnicGroup-TOTAL'  as id,
'Total' as [Ethnic group], 
count(*) as [Count]
from (
   select W.ID, min(dateforsignin) firstsignin
   from workers W
   JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
   WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
   group by W.ID
) as WWW
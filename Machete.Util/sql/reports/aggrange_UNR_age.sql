declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'



select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) + '-' + convert(varchar(5), min(WW.incomeID)) as id,
L.text_EN as label, 
count(*) as value

FROM (

)
FROM (
  select W.ID,(CONVERT(int,CONVERT(char(8),GETDATE(),112))-CONVERT(char(8),min(W.dateOfBirth),112))/10000 AS AgeIntYears
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by w.ID 

) as WW

group by

union 

select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) + '-NULL' as id,
'NULL' as label, 
count(*) as value
from (
   select W.ID, min(dateforsignin) firstsignin
   from workers W
   JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
   WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
   and W.incomeID is null
   group by W.ID
) as WWW

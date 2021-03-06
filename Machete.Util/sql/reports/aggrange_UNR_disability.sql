declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'

select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByDisability-' + min(disabled) as id,
disabled as [Disabled?], 
count(*) as [Count]
FROM (
  select W.ID, 
  CASE 
	WHEN W.disabled = 1 then 'yes'
	when W.disabled = 0 then 'no'
	when W.disabled is null then 'NULL'
  END as disabled
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.disabled
) as WW
group by disabled
union 
select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByDisability-TOTAL' as id,
'Total' as [Disabled?], 
count(distinct(w.id)) as [Count]
from Workers W
JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
WHERE dateforsignin >= @startDate and dateforsignin <= @endDate



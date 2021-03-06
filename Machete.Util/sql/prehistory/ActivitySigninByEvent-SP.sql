USE [machete]
GO
/****** Object:  StoredProcedure [dbo].[ActivitySignins_Events_Pivot]    Script Date: 06/22/2012 20:42:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[ActivitySignins_Events_Pivot]
as
set nocount on
declare @execsql nvarchar(4000)  
set @execsql = 'Create table ActivitySigninByEvent(wid int'    
select @execsql=    
COALESCE(@execsql + ',['+ cast(groupingint as varchar) + '-min] int,[' + cast(groupingint as varchar)+ '-date] date,[' + cast(groupingint as varchar)+ '-name] varchar(60)', '['+ cast(groupingint as varchar) + '-min] int,[' + cast(groupingint as varchar)+ '-date] date,[' + cast(groupingint as varchar)+ '-name] varchar(60)')  
FROM (select * from (
	select top 20 groupingint 
	from [ClassHoursGroupedByTest] 
	union all select '1'
union all select '2'
union all select '3'
union all select '4'
union all select '5'
union all select '6'
union all select '7'
) goo
group by groupingint) as foo2  set @execsql = @execsql + ')'  
--select @execsql
drop table ActivitySigninByEvent
exec (@execsql)      
DECLARE @numHdr VARCHAR(MAX)  
SELECT @numHdr = COALESCE(@numHdr + ',[' + cast(groupingint as varchar) + ']','[' + cast(groupingint as varchar)+ ']')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo    
DECLARE @minHdr VARCHAR(MAX),@minValue VARCHAR(MAX)  
SELECT @minHdr = COALESCE(@minHdr + ',[' + cast(groupingint as varchar) + '] as [' + cast(groupingint as varchar)+ '-min]','[' + cast(groupingint as varchar) + '] as [' + cast(groupingint as varchar)+ '-min]')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo  
SELECT @minValue = COALESCE(@minValue + ',['+ cast(groupingint as varchar)+ '-min]','[' + cast(groupingint as varchar)+ '-min]')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo    
--select @minValue
DECLARE @dateHdr VARCHAR(MAX),@dateValue VARCHAR(MAX)  
SELECT @dateHdr = COALESCE(@dateHdr + ',[' + cast(groupingint as varchar) + '] as [' + cast(groupingint as varchar)+ '-date]','[' + cast(groupingint as varchar) + '] as [' + cast(groupingint as varchar)+ '-date]')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo  
SELECT @dateValue = COALESCE(@dateValue + ',['+ cast(groupingint as varchar)+ '-date]=pd.['+ cast(groupingint as varchar)+ '-date]','[' + cast(groupingint as varchar)+ '-date]=pd.['+ cast(groupingint as varchar)+ '-date]')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo  
--select @dateValue    
DECLARE @nameHdr VARCHAR(MAX),@nameValue VARCHAR(MAX)  SELECT @nameHdr = COALESCE(@nameHdr + ',[' + cast(groupingint as varchar) + '] as [' + cast(groupingint as varchar)+ '-name]','[' + cast(groupingint as varchar) + '] as [' + cast(groupingint as varchar)+ '-name]')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo  
SELECT @nameValue = COALESCE(@nameValue + ',['+ cast(groupingint as varchar)+ '-name]=pd.['+ cast(groupingint as varchar)+ '-name]','[' + cast(groupingint as varchar)+ '-name]=pd.['+ cast(groupingint as varchar)+ '-name]')  
FROM (select top 20 groupingint from [ClassHoursGroupedByTest] group by groupingint order by groupingint) as foo  
--select @nameValue    
DECLARE @PivotMinutes NVARCHAR(MAX)  
SET @PivotMinutes = N'    
insert into ActivitySigninByEvent ([wid],'+@minValue+')    
SELECT wid, ' + @minHdr+ '    
FROM 
(      
	SELECT wid, minutes, groupingint      
	FROM [ClassHoursGroupedByTest]
) AS PivotData    
PIVOT (      
	SUM(minutes)      
	FOR groupingint 
	IN (' + @numHdr + ')   
) AS PivotTable    
order by wid'  
DECLARE @PivotDate NVARCHAR(MAX)  
SET @PivotDate = N'
update ActivitySigninByEvent
set '+@dateValue+'    
from ActivitySigninByEvent    
inner join    
(
	SELECT wid, ' + @dateHdr+ '
	FROM
	(
		SELECT wid, datefrom, groupingint              
		FROM [ClassHoursGroupedByTest]
	)AS pd2         
	PIVOT 
	(               
		max(datefrom)               
		FOR groupingint          
		IN (' + @numHdr + ')
	) as pd1   
) pd
on (ActivitySigninByEvent.wid = pd.wid)  '  


DECLARE @PivotName NVARCHAR(MAX)  SET @PivotName = N'
update ActivitySigninByEvent           
set '+@nameValue+'    
from ActivitySigninByEvent    
inner join   
(        
	SELECT wid, ' + @nameHdr+ '           
	FROM   
	(              
		SELECT wid, Grouping, groupingint              
		FROM [ClassHoursGroupedByTest]
	)AS pd2         
	PIVOT   
	(              
		max(Grouping)               
		FOR groupingint         
		IN (' + @numHdr + ') 
	) as pd1
) pd
on (ActivitySigninByEvent.wid = pd.wid)'  
                  
--select @PivotDate  
EXECUTE(@PivotMinutes)  
EXECUTE(@PivotDate)  
EXECUTE(@PivotName)
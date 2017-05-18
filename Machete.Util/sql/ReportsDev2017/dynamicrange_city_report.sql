declare @startDate DateTime = '1/1/2011'
declare @endDate DateTime = '1/1/2017'

select
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-CityReport-NewEnrolls-' + convert(varchar(4), [year]) as id, 
    'Newly enrolled in program (within time range)' as label, 
    cast(year as int) as year, 
	cast([1] as int) as 'Jan', 
	cast([2] as int) as 'Feb', 
	cast([3] as int) as 'Mar', 
	cast([4] as int) as 'Apr',
	cast([5] as int) as 'May', 
	cast([6] as int) as 'Jun', 
	cast([7] as int) as 'Jul', 
	cast([8] as int) as 'Aug',
	cast([9] as int) as 'Sep', 
	cast([10] as int) as 'Oct', 
	cast([11] as int) as 'Nov', 
	cast([12] as int) as 'Dec'
from
(
	select min(year(signindate)) as year, min(month(signindate)) as month, cardnum
	from 
	(
		SELECT MIN(dateforsignin) AS signindate, dwccardnum as cardnum
		FROM dbo.WorkerSignins
		WHERE dateforsignin >= @startdate AND
		dateforsignin < @EnDdate
		GROUP BY dwccardnum

		union 

		select min(dateforsignin) as singindate, dwccardnum as cardnum
		from activitysignins asi
		where dateforsignin >= @startdate
		and dateforsignin < @enddate
		group by dwccardnum
	) 
	as result_set
	group by  cardnum
) as foo
PIVOT  
(  
count (cardnum)  
FOR month IN  
( [1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12] )  
) AS pvt 

union 

select
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-CityReport-FinEd-' + convert(varchar(4), [year]) as id, 
    'Counts of members who accessed finanacial literacy' as label, 
    cast(year as int) as year, 
	cast([1] as int) as 'Jan', 
	cast([2] as int) as 'Feb', 
	cast([3] as int) as 'Mar', 
	cast([4] as int) as 'Apr',
	cast([5] as int) as 'May', 
	cast([6] as int) as 'Jun', 
	cast([7] as int) as 'Jul', 
	cast([8] as int) as 'Aug',
	cast([9] as int) as 'Sep', 
	cast([10] as int) as 'Oct', 
	cast([11] as int) as 'Nov', 
	cast([12] as int) as 'Dec'
from
(
	select min(year(signindate)) as year, min(month(signindate)) as month, cardnum
	from 
	(
		SELECT ASIs.dwccardnum as cardnum, MIN(dateStart) as signindate
		FROM dbo.Activities Acts
		JOIN dbo.ActivitySignins ASIs ON Acts.ID = ASIs.ActivityID
		JOIN dbo.Lookups Ls ON Acts.name = Ls.ID
		WHERE Ls.ID = 179 AND dateStart >= @startDate AND dateStart <= @endDate

		GROUP BY ASIs.dwccardnum
	) 
	as result_set
	group by  cardnum
) as foo
PIVOT  
(  
count (cardnum)  
FOR month IN  
( [1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12] )  
) AS pvt 

union 

select
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-CityReport-Training-' + convert(varchar(4), [year]) as id, 
    'Counts of members who participated in job skill training or workshops' as label, 
    cast(year as int) as year, 
	cast([1] as int) as 'Jan', 
	cast([2] as int) as 'Feb', 
	cast([3] as int) as 'Mar', 
	cast([4] as int) as 'Apr',
	cast([5] as int) as 'May', 
	cast([6] as int) as 'Jun', 
	cast([7] as int) as 'Jul', 
	cast([8] as int) as 'Aug',
	cast([9] as int) as 'Sep', 
	cast([10] as int) as 'Oct', 
	cast([11] as int) as 'Nov', 
	cast([12] as int) as 'Dec'
from
(
	select min(year(signindate)) as year, min(month(signindate)) as month, cardnum
	from 
	(
		SELECT ASIs.dwccardnum as cardnum, MIN(dateStart) as signindate
		FROM dbo.Activities Acts
		JOIN dbo.ActivitySignins ASIs ON Acts.ID = ASIs.ActivityID
		JOIN dbo.Lookups Ls ON Acts.name = Ls.ID
		WHERE dateStart >= @startdate AND dateStart <= @enddate AND
		(Ls.ID = 182 OR Ls.ID = 181 OR Ls.ID = 180
		OR Ls.ID = 179 OR Ls.ID = 178 OR Ls.ID = 134
		OR Ls.ID = 168 OR Ls.ID = 156 OR Ls.ID = 152
		OR Ls.ID = 145 OR Ls.ID = 135 OR Ls.ID = 104)

GROUP BY dwccardnum
	) 
	as result_set
	group by  cardnum
) as foo
PIVOT  
(  
count (cardnum)  
FOR month IN  
( [1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12] )  
) AS pvt 

union 

select
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-CityReport-ESL-' + convert(varchar(4), [year]) as id, 
    'Counts of members who accessed at least 12 hours of ESL classes' as label, 
    cast(year as int) as year, 
	cast([1] as int) as 'Jan', 
	cast([2] as int) as 'Feb', 
	cast([3] as int) as 'Mar', 
	cast([4] as int) as 'Apr',
	cast([5] as int) as 'May', 
	cast([6] as int) as 'Jun', 
	cast([7] as int) as 'Jul', 
	cast([8] as int) as 'Aug',
	cast([9] as int) as 'Sep', 
	cast([10] as int) as 'Oct', 
	cast([11] as int) as 'Nov', 
	cast([12] as int) as 'Dec'
from
(
	select year, month, cardnum
	from 
	(
			SELECT  year(dateStart) as year, month(datestart) as month, dwccardnum as cardnum,
			sum(DATEDIFF( minute, dateStart, dateEnd )) as Minutes
			from dbo.Activities Acts

			JOIN dbo.Lookups Ls ON Acts.name = Ls.ID
			JOIN dbo.ActivitySignins ASIs ON Acts.ID = ASIs.ActivityID
			WHERE text_en LIKE '%English%'
			AND dateStart >= @startdate AND dateend <= @EnDdate
			group by year(datestart), month(datestart), dwccardnum
	) as foo
	where foo.minutes >= 720

) as foo
PIVOT  
(  
count (cardnum)  
FOR month IN  
( [1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12] )  
) AS pvt

union 

select
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-CityReport-UndupCount-' + convert(varchar(4), [year]) as id, 

    'A2H1-0 count of unduplicated individuals securing day labor employment this month' as label, 
    cast(year as int) as year, 
	cast([1] as int) as 'Jan', 
	cast([2] as int) as 'Feb', 
	cast([3] as int) as 'Mar', 
	cast([4] as int) as 'Apr',
	cast([5] as int) as 'May', 
	cast([6] as int) as 'Jun', 
	cast([7] as int) as 'Jul', 
	cast([8] as int) as 'Aug',
	cast([9] as int) as 'Sep', 
	cast([10] as int) as 'Oct', 
	cast([11] as int) as 'Nov', 
	cast([12] as int) as 'Dec'
from
(
	SELECT count(distinct(dwccardnum)) AS cardnum, year(dateTimeofWork) as year, month(dateTimeofWork) AS month
	from dbo.WorkAssignments WAs
	JOIN dbo.WorkOrders WOs ON WAs.workOrderID = WOs.ID
	JOIN dbo.Workers Ws on WAs.workerAssignedID = Ws.ID
	join dbo.lookups l on l.id = wos.status
	WHERE dateTimeofWork >= @startdate 
	and dateTimeofWork <= @EnDdate
	and l.text_EN = 'Completed'
	group by year(dateTimeofWork), month(dateTimeofWork)

) as foo
PIVOT  
(  
sum (cardnum)  
FOR month IN  
( [1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12] )  
) AS pvt

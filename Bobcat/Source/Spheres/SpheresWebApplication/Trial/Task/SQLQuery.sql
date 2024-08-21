/*
drop table TEST1
drop table TEST2
drop table TEST3

create TABLE TEST1 (ID int, DT datetime )
create TABLE TEST2 (ID int, DT datetime)
create TABLE TEST3 (ID int, DT datetime )
*/

/*
delete from TEST1
delete from TEST2
delete from TEST3
*/

select  DT, ID, TBL 
from
(
select 'TEST1' TBL, DT, ID   from dbo.TEST1 WITH (NOLOCK)
union all
select 'TEST2' TBL, DT, ID  from dbo.TEST2 WITH (NOLOCK)
union all
select 'TEST3' TBL, DT, ID  from dbo.TEST3 WITH (NOLOCK)
) t 
order by DT

select  DT, ID, TBL 
from
(
select 'TEST1' TBL, DT, ID   from dbo.TEST1
union all
select 'TEST2' TBL, DT, ID  from dbo.TEST2
union all
select 'TEST3' TBL, DT, ID  from dbo.TEST3
) t 
order by DT
declare @tmp table (
	Id bigint not null primary key
)
;

insert into @tmp
select Id
from es.EventStore
where StreamId = 'bd49834e-2293-44b2-8de9-7ec33e9d69aa' and SequenceNumber between 100 and 200
order by SequenceNumber
;

select es.* 
from es.EventStore es
where Id in (select Id from @tmp)
order by Id
;

----------------

select es.* 
from es.EventStore es
where Id in (
	select Id
	from es.EventStore
	where StreamId = 'bd49834e-2293-44b2-8de9-7ec33e9d69aa' and SequenceNumber between 100 and 200
)
order by es.Id
;

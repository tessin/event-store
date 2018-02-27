
if schema_id(N'es') is null
exec sys.sp_executesql N'create schema es'
;

go

if object_id(N'es.EventStore', N'U') is null
begin

create table es.EventStore ( 
	Id bigint not null identity(1,1) primary key,
	StreamId uniqueidentifier not null,
	SequenceNumber int not null,
	TypeId uniqueidentifier not null,
	Payload varbinary(max) not null,
	UncompressedSize int not null,
	Created datetimeoffset(2) not null
)
;

create unique index IX_EventStore_Stream on es.EventStore (StreamId, SequenceNumber)
;

end
;

go

if type_id(N'es.UncommittedEvent') is null
create type es.UncommittedEvent as table (
	StreamId uniqueidentifier not null,
	SequenceNumber int not null,
	TypeId uniqueidentifier not null,
	Payload varbinary(max) not null,
	UncompressedSize int not null,
	Created datetimeoffset(2) not null,
	primary key (StreamId, SequenceNumber)
)
;

go

if object_id(N'es.Append', 'P') is null
exec sys.sp_executesql N'
create procedure es.[Append] (
	@uncommitted es.UncommittedEvent readonly
) 
as
insert into es.EventStore (
    StreamId, SequenceNumber, TypeId, Payload, UncompressedSize, Created
)
select StreamId, SequenceNumber, TypeId, Payload, UncompressedSize, Created
from @uncommitted
'
;

if object_id(N'es.GetEnumerable', 'P') is null
exec sys.sp_executesql N'
create procedure es.GetEnumerable (
	@min bigint,
	@max bigint
) 
as
select Id, StreamId, SequenceNumber, TypeId, Payload, UncompressedSize, Created
from es.EventStore 
where Id between @min and @max
order by Id
option (optimize for unknown)
'
;

if object_id(N'es.GetEnumerableStream', 'P') is null
exec sys.sp_executesql N'
create procedure es.GetEnumerableStream (
	@streamId uniqueidentifier,
	@min int,
	@max int
) 
as
select Id, StreamId, SequenceNumber, TypeId, Payload, UncompressedSize, Created
from es.EventStore 
where StreamId = @streamId
	and SequenceNumber between @min and @max
order by StreamId, SequenceNumber
option (optimize for unknown)
'
;

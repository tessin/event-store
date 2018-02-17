
if schema_id(N'es') is null
exec sys.sp_executesql N'create schema es'
;

go

if object_id(N'es.EventStore', N'U') is null
begin
;

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

if type_id('es.UncommittedEvent') is null
begin
;

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

end
;

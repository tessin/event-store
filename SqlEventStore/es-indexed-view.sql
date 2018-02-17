create view es.EventStream 
with schemabinding
as
select es.Id, 
	es.StreamId, 
	es.SequenceNumber, 
	es.TypeId, 
	es.Payload, 
	es.UncompressedSize, 
	es.Created
from es.EventStore es
;

go

create unique clustered index IX_EventStream on es.EventStream (StreamId, SequenceNumber)
;

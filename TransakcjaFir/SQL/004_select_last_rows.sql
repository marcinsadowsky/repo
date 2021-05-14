use TransactionFirDb
go

set transaction isolation level read uncommitted
go

select top 20 * from REP.[Transaction] order by 1 desc
select top 20 * from REP.TransactionCore order by 1 desc
select top 20 * from REP.TransactionAml order by 1 desc
select top 20 * from REP.TransactionStir order by 1 desc
select top 20 * from REP.TransactionPersonsList order by 1 desc
select top 20 * from REP.TransactionPerson order by 1 desc

use master
go

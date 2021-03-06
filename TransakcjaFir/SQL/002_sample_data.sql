set nocount on
go

use TransactionFirDb
go

declare @i int, @j int
declare @ref varchar(40)

set @i = 1 

while @i <= 20
begin
	set @ref = 'abc ' + convert(varchar, @i)

	insert REP.TransactionCore ( TransactionReference, VersionNumber, IsLastVersion, Amount, Currency, PaymentDetails1, PaymentDetails2, PaymentDetails3, PaymentDetails4 )
		select @ref, 1, 1, 1000 + @i, 'PLN', @ref + ' details 1', @ref + ' details 2', @ref + ' details 3', @ref + ' details 4' 

	insert REP.TransactionAml ( TransactionReference, VersionNumber, IsLastVersion, ProcessingStatus, AmlRelatedAttribure )
		select @ref, 1, 1, 'Not sent', 'Aml attr ' + @ref + convert(varchar, @i)

	insert REP.TransactionStir ( TransactionReference, VersionNumber, IsLastVersion, ProcessingStatus, StirRelatedAttribure )
		select @ref, 1, 1, 'Not sent', 'Stir attr ' + @ref + convert(varchar, @i)

	insert REP.TransactionDisposersList ( TransactionReference, VersionNumber, IsLastVersion )
		select @ref, 1, 1

	set @j = 1

	while @j <= 10 
	begin

		insert REP.TransactionDisposer ( TransactionDisposersListId, PersonId, IsAmlMainDisposer, IsAmlAdditionalDisposer, IsStirDisposer, PersonName )
			select p.TransactionDisposersListId, @i * 10000 + @j, 1, 0, 1, @ref + ' person name'
			from REP.TransactionDisposersList p
			where TransactionReference = @ref and VersionNumber = 1

		set @j = @j + 1
	end
	set @i = @i + 1
end


insert REP.[Transaction] ( CreationDate, CreationUserAccountId, LastModificationDate, LastModificationUserAccountId, ValidationCounter, Status,
							TransactionReference, VersionNumber, IsLastVersion, TransactionCoreId, TransactionAmlId, TransactionStirId, TransactionDisposersListId )
	select getdate(), 1, getdate(), 1, 0, 'C',
			c.TransactionReference, 1, 1, c.TransactionCoreId, a.TransactionAmlId, s.TransactionStirId, p.TransactionDisposersListId
		from REP.TransactionCore c
		inner join REP.TransactionAml a on c.TransactionReference = a.TransactionReference
		inner join REP.TransactionStir s on c.TransactionReference = s.TransactionReference
		inner join REP.TransactionDisposersList p on c.TransactionReference = p.TransactionReference
go

use master
go

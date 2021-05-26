use master
go

drop database if exists TransactionFirDb
go

create database TransactionFirDb
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TransactionFirDb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TransactionFirDb.mdf' , SIZE = 512000KB , MAXSIZE = UNLIMITED, FILEGROWTH = 0)
 LOG ON 
( NAME = N'TransactionFirDb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TransactionFirDb_log.ldf' , SIZE = 204800KB , MAXSIZE = 2048GB , FILEGROWTH = 0)
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

use TransactionFirDb
go

create schema REP
go

create table REP.[Transaction] (
	TransactionId bigint identity (103, 1) not null primary key,
	CreationDate datetime2 not null,
	CreationUserAccountId bigint not null,
	LastModificationDate datetime2 not null,
	LastModificationUserAccountId bigint not null,
	ValidationCounter tinyint not null,
	Status char(1) not null,
	TransactionReference varchar(40) not null,
	VersionNumber int not null,
	IsLastVersion bit not null,
	TransactionCoreId bigint not null,
	TransactionAmlId bigint not null,
	TransactionStirId bigint not null,
	TransactionDisposersListId bigint not null
)
go

create unique index IdxTransactionNk on REP.[Transaction] ( TransactionReference, VersionNumber )
go

create unique index IdxTransaction_CurrentVersion on REP.[Transaction] ( TransactionReference, IsLastVersion ) where IsLastVersion = 1
go

create table REP.TransactionCore (
	TransactionCoreId bigint identity (21, 1) not null primary key,
	TransactionReference varchar(40) not null,
	VersionNumber int not null,
	IsLastVersion bit not null,
	Amount money,
	Currency char(3),
	PaymentDetails1 varchar(35) null,
	PaymentDetails2 varchar(35) null,
	PaymentDetails3 varchar(35) null,
	PaymentDetails4 varchar(35) null,
)
go

create unique index IdxTransactionCoreNk on REP.[TransactionCore] ( TransactionReference, VersionNumber )
go

create unique index IdxTransactionCore_CurrentVersion on REP.[TransactionCore] ( TransactionReference, IsLastVersion ) where IsLastVersion = 1
go

create table REP.TransactionAml (
	TransactionAmlId bigint identity (54, 1) not null primary key,
	TransactionReference varchar(40) not null,
	VersionNumber int not null,
	IsLastVersion bit not null,
	ProcessingStatus varchar(20) not null,
	SendDate datetime2 null,
	StatusDate datetime2 null,
	AmlRelatedAttribure varchar(100) not null,
)
go

create unique index idxTransactionAmlNk on REP.[TransactionAml] ( TransactionReference, VersionNumber )
go

create unique index IdxTransactionAml_CurrentVersion on REP.[TransactionAml] ( TransactionReference, IsLastVersion ) where IsLastVersion = 1
go

create table REP.TransactionStir (
    TransactionStirId bigint identity (17, 1) not null primary key,
	TransactionReference varchar(40) not null,
	VersionNumber int not null,
	IsLastVersion bit not null,
	ProcessingStatus varchar(20) not null,
	SendDate datetime2 null,
	StatuseDate datetime2 null,
	StirRelatedAttribure varchar(100) not null,
)
go

create unique index idxTransactionStirNk on REP.[TransactionStir] ( TransactionReference, VersionNumber )
go

create unique index IdxTransactionStir_CurrentVersion on REP.[TransactionStir] ( TransactionReference, IsLastVersion ) where IsLastVersion = 1
go

create table REP.TransactionDisposersList (
	TransactionDisposersListId bigint identity (561, 1) not null primary key,
	TransactionReference varchar(40) not null,
	VersionNumber int not null,
	IsLastVersion bit not null,
)

create unique index idxTransactionDisposersListNk on REP.[TransactionDisposersList] ( TransactionReference, VersionNumber )
go

create unique index IdxTransactionDisposersList_CurrentVersion on REP.[TransactionDisposersList] ( TransactionReference, IsLastVersion ) where IsLastVersion = 1
go

create table REP.TransactionDisposer (
	TransactionDisposerId bigint identity (31, 1) not null primary key,
	TransactionDisposersListId bigint not null,
	PersonId bigint not null,
	IsAmlMainDisposer bit not null,
	IsAmlAdditionalDisposer bit not null,
	IsStirDisposer bit not null,
	PersonName varchar(100) not null,
)
go

alter table REP.[Transaction] add constraint fk_transaction_TransactionCore foreign key (TransactionCoreId) references REP.TransactionCore (TransactionCoreId)
go

alter table REP.[Transaction] add constraint fk_transaction_TransactionAml foreign key (TransactionAmlId) references REP.TransactionAml (TransactionAmlId)
go

alter table REP.[Transaction] add constraint fk_transaction_TransactionStir foreign key (TransactionStirId) references REP.TransactionStir (TransactionStirId)
go

alter table REP.[Transaction] add constraint fk_transaction_TransactionDisposersList foreign key (TransactionDisposersListId) references REP.TransactionDisposersList (TransactionDisposersListId)
go

alter table REP.TransactionDisposer add constraint fk_TransactionDisposer_TransactionDisposersList foreign key (TransactionDisposersListId) references REP.TransactionDisposersList (TransactionDisposersListId)
go

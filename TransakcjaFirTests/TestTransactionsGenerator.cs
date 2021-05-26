using System;
using System.Collections.Generic;
using TransakcjaFir.BLL;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;


namespace TransakcjaFirTests
{
    internal class TestTransactionsGenerator
    {
        private readonly TransactionService _transactionService;
        private readonly TransactionVersioningService _transactionVersioningService;
        private readonly FirContext _dbContext;

        public TestTransactionsGenerator()
        {
            _dbContext = new FirContext();
            _transactionService = new TransactionService(_dbContext);
            _transactionVersioningService = new TransactionVersioningService(_dbContext, _transactionService);
        }

        private Transaction CreateTransaction(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds)
        {
            var transaction = _transactionService.CreateTransaction(reference);

            transaction.CreationDate = DateTime.Now;
            transaction.CreationUserAccountId = 3;
            transaction.LastModificationDate = DateTime.Now;
            transaction.LastModificationUserAccountId = 3;
            transaction.ValidationCounter = 0;
            transaction.Status = "C";

            transaction.Core.Amount = 100;
            transaction.Core.Currency = "ZMW";
            transaction.Core.PaymentDetails1 = "detail line 1";
            transaction.Core.PaymentDetails2 = "detail line 2";
            transaction.Core.PaymentDetails3 = "detail line 3";
            transaction.Core.PaymentDetails4 = "detail line 4";

            transaction.Aml.AmlRelatedAttribure = "aml attributes";
            transaction.Aml.ProcessingStatus = amlExportStatus;

            transaction.Stir.StirRelatedAttribure = "stir attributes";
            transaction.Stir.ProcessingStatus = stirExportStatus;

            foreach (int id in personsIds)
            {
                _transactionService.AddPerson(transaction, CreatePerson(id, $"person {id} name", BaseTransactionTest.AmlPersonsRole.MainDisposer, BaseTransactionTest.StirPersonsRole.Disposer));
            }
            return transaction;
        }

        internal Transaction CreateTransactionWithCoreData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string paymentDetails1)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, personsIds);
            transaction.Core.PaymentDetails1 = paymentDetails1;
            return transaction;
        }

        internal void PrepareTransactionWithCoreData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string paymentDetails1)
        {
            _dbContext.Transactions.Add(CreateTransactionWithCoreData(reference, amlExportStatus, stirExportStatus, personsIds, paymentDetails1));
            _dbContext.SaveChanges();
        }

        internal Transaction CreateTransactionWithAmlData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string amlData)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, personsIds);
            transaction.Aml.AmlRelatedAttribure = amlData;
            return transaction;
        }

        internal void PrepareTransactionWithAmlData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string amlData)
        {
            _dbContext.Transactions.Add(CreateTransactionWithAmlData(reference, amlExportStatus, stirExportStatus, personsIds, amlData));
            _dbContext.SaveChanges();
        }

        internal Transaction CreateTransactionWithStirData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string stirData)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, personsIds);
            transaction.Stir.StirRelatedAttribure = stirData;
            return transaction;
        }

        internal void PrepareTransactionWithStirData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string stirData)
        {
            _dbContext.Transactions.Add(CreateTransactionWithStirData(reference, amlExportStatus, stirExportStatus, personsIds, stirData));
            _dbContext.SaveChanges();
        }

        internal Transaction CreateTransactionWithPersonsData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, new int[] { });
            var personAml = CreatePerson(4, "aml", BaseTransactionTest.AmlPersonsRole.MainDisposer, BaseTransactionTest.StirPersonsRole.None);
            var personStir = CreatePerson(6, "stir", BaseTransactionTest.AmlPersonsRole.None, BaseTransactionTest.StirPersonsRole.Disposer);
            var personAmlStir = CreatePerson(8, "aml_stir", BaseTransactionTest.AmlPersonsRole.AdditionalDisposer, BaseTransactionTest.StirPersonsRole.Disposer);
            var persons = new List<TransactionDisposer>() { personAml, personStir, personAmlStir };
            foreach (var person in persons)
            {
                transaction.Disposers.List.Add(person);
            }
            return transaction;
        }

        internal void PrepareTransactionWithPersonsData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus)
        {
            _dbContext.Transactions.Add(CreateTransactionWithPersonsData(reference, amlExportStatus, stirExportStatus));
            _dbContext.SaveChanges();
        }

        internal TransactionDisposer CreatePerson(int personId, string name, BaseTransactionTest.AmlPersonsRole amlRole, BaseTransactionTest.StirPersonsRole stirRole)
        {
            var person = new TransactionDisposer()
            {
                PersonId = personId,
                PersonName = name,
                IsAmlMainDisposer = false,
                IsAmlAdditionalDisposer = false,
                IsStirDisposer = false,
            };
            switch (amlRole)
            {
                case BaseTransactionTest.AmlPersonsRole.MainDisposer:
                    person.IsAmlMainDisposer = true;
                    break;
                case BaseTransactionTest.AmlPersonsRole.AdditionalDisposer:
                    person.IsAmlAdditionalDisposer = true;
                    break;
            }
            switch (stirRole)
            {
                case BaseTransactionTest.StirPersonsRole.Disposer:
                    person.IsStirDisposer = true;
                    break;
            }
            return person;
        }
    }
}

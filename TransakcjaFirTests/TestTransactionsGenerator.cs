using System;
using System.Collections.Generic;
using TransakcjaFir.BLL;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;
using static TransakcjaFirTests.BaseTest;

namespace TransakcjaFirTests
{
    internal class TestTransactionsGenerator
    {
        private readonly FirTransactionService _service;
        private readonly FirContext _dbContext;

        public TestTransactionsGenerator()
        {
            _dbContext = new FirContext();
            _service = new FirTransactionService(_dbContext);
        }

        private Transaction CreateTransaction(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds)
        {
            var transaction = _service.CreateTransaction(reference);

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
                _service.AddPerson(transaction, CreatePerson(id, $"person {id} name", AmlPersonsRole.MainDisposer, StirPersonsRole.MainDisposer));
            }
            return transaction;
        }

        internal void CreateTransactionWithCoreData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string paymentDetails1)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, personsIds);
            transaction.Core.PaymentDetails1 = paymentDetails1;
            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();
        }

        internal void CreateTransactionWithAmlData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string amlData)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, personsIds);
            transaction.Aml.AmlRelatedAttribure = amlData;
            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();
        }

        internal void CreateTransactionWithStirData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus, int[] personsIds, string stirData)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, personsIds);
            transaction.Stir.StirRelatedAttribure = stirData;
            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();
        }

        internal void CreateTransactionWithPersonsData(string reference, AmlExportStatusEnum amlExportStatus, StirExportStatusEnum stirExportStatus)
        {
            var transaction = CreateTransaction(reference, amlExportStatus, stirExportStatus, new int[] { });
            var personAml = CreatePerson(4, "aml", AmlPersonsRole.MainDisposer, StirPersonsRole.None);
            var personStir = CreatePerson(6, "stir", AmlPersonsRole.None, StirPersonsRole.Disposer);
            var personAmlStir = CreatePerson(8, "aml_stir", AmlPersonsRole.Ubo, StirPersonsRole.MainDisposer);
            var persons = new List<TransactionPerson>() { personAml, personStir, personAmlStir };
            foreach (var person in persons)
            {
                transaction.Persons.List.Add(person);
            }
            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();
        }

        internal TransactionPerson CreatePerson(int personId, string name, BaseTest.AmlPersonsRole amlRole, BaseTest.StirPersonsRole stirRole)
        {
            var person = new TransactionPerson()
            {
                PersonId = personId,
                PersonName = name,
                IsAmlMainDisposer = false,
                IsAmlAdditionalDisposer = false,
                IsAmlUbo = false,
                IsStirMainDisposer = false,
                IsStirDisposer = false,
            };
            switch (amlRole)
            {
                case BaseTest.AmlPersonsRole.MainDisposer:
                    person.IsAmlMainDisposer = true;
                    break;
                case BaseTest.AmlPersonsRole.AdditionalDisposer:
                    person.IsAmlAdditionalDisposer = true;
                    break;
                case BaseTest.AmlPersonsRole.Ubo:
                    person.IsAmlUbo = true;
                    break;
            }
            switch (stirRole)
            {
                case BaseTest.StirPersonsRole.MainDisposer:
                    person.IsStirMainDisposer = true;
                    break;
                case BaseTest.StirPersonsRole.Disposer:
                    person.IsStirDisposer = true;
                    break;
            }
            return person;
        }
    }
}

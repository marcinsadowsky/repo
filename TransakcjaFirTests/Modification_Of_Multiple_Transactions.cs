using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using TransakcjaFir.BLL;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;
using Xunit;
using Xunit.Abstractions;

namespace TransakcjaFirTests
{
    public class Modification_Of_Multiple_Transactions : BaseTransactionTest
    {
        const int numberOfCoreChanges = 50;
        const int numberOfPersonsChanges = 60;
        const int numberOfAmlChanges = 70;
        const int numberOfStirChanges = 80;

        private readonly string referencePrefix;

        private readonly TestTransactionsGenerator generator = new TestTransactionsGenerator();

        private readonly ITestOutputHelper _testOutputHelper;

        public Modification_Of_Multiple_Transactions(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            _testOutputHelper.WriteLine($"Preparing test transaction {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");
            _testOutputHelper.WriteLine($"-> {numberOfCoreChanges} transactions with modification of CORE data");
            _testOutputHelper.WriteLine($"-> {numberOfPersonsChanges} transactions with modification of DISPOSERS data");
            _testOutputHelper.WriteLine($"-> {numberOfAmlChanges} transactions with modification of AML related data");
            _testOutputHelper.WriteLine($"-> {numberOfStirChanges} transactions with modification of STIR related data");

            // prepare test transactions
            referencePrefix = $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.ffff}";

            List<Transaction> transactions = new List<Transaction>();
            for (int i = 0; i < numberOfCoreChanges; i++)
            {
                var referenceCore = $"{referencePrefix} Core {Guid.NewGuid()}".Substring(0, 40);
                var transaction = generator.CreateTransactionWithCoreData(referenceCore, AmlExportStatusEnum.Sent, StirExportStatusEnum.Sent, new int[] { 4, 1, 2, 3 }, "xxxx");
                transactions.Add(transaction);
            }
            for (int i = 0; i < numberOfPersonsChanges; i++)
            {
                var referencePersons = $"{referencePrefix} Persons {Guid.NewGuid()}".Substring(0, 40);
                var transaction = generator.CreateTransactionWithPersonsData(referencePersons, AmlExportStatusEnum.Sent, StirExportStatusEnum.Sent);
                transactions.Add(transaction);
            }
            for (int i = 0; i < numberOfAmlChanges; i++)
            {
                var referenceAml = $"{referencePrefix} Aml {Guid.NewGuid()}".Substring(0, 40);
                var transaction = generator.CreateTransactionWithAmlData(referenceAml, AmlExportStatusEnum.Sent, StirExportStatusEnum.Sent, new int[] { 4, 1, 3 }, "xxxx");
                transactions.Add(transaction);
            }
            for (int i = 0; i < numberOfStirChanges; i++)
            {
                var referenceStir = $"{referencePrefix} Stir {Guid.NewGuid()}".Substring(0, 40);
                var transaction = generator.CreateTransactionWithStirData(referenceStir, AmlExportStatusEnum.Sent, StirExportStatusEnum.Sent, new int[] { 4, 1, 3 }, "xxxx");
                transactions.Add(transaction);
            }

            _testOutputHelper.WriteLine($"Saving test transaction [= START =] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");
            using (var context = new FirContext())
            {
                context.AddRange(transactions);
                context.SaveChanges();
            }
            _testOutputHelper.WriteLine($"Saving test transaction [== END ==] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");
        }

        [Fact]
        public async void Should_Modify_Set_Of_Transactions()
        {
            using (var context = new FirContext())
            {
                // run test
                _testOutputHelper.WriteLine($"Performing modifications on transactions [= START =] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transactions = await readService.LoadTransactionsWithRefereneMask(referencePrefix);
                foreach (var transaction in transactions)
                {
                    if (transaction.TransactionReference.Contains("Core"))
                    {
                        transaction.Core.PaymentDetails1 = "yyyy";
                    }
                    if (transaction.TransactionReference.Contains("Persons"))
                    {
                        // add new person
                        transaction.Disposers.List.Add(generator.CreatePerson(20, "created", AmlPersonsRole.AdditionalDisposer, StirPersonsRole.Disposer));
                        // upodate person
                        var modifiedPerson = transaction.Disposers.List.Single(p => p.PersonId == 8).PersonName = "new_aml_stir";
                        // delete persons
                        transaction.Disposers.List.RemoveAll(p => p.PersonId == 4 || p.PersonId == 6);
                    }
                    if (transaction.TransactionReference.Contains("Aml"))
                    {
                        transaction.Aml.AmlRelatedAttribure = "aMl#aMl";
                    }
                    if (transaction.TransactionReference.Contains("Stir"))
                    {
                        transaction.Stir.StirRelatedAttribure = "StiR";
                    }
                }
                _testOutputHelper.WriteLine($"Performing modifications on transactions [== END ==] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");

                _testOutputHelper.WriteLine($"Creating new transactions' versions and saving modified transactions [= START =] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");
                await versioningService.SaveTransactionsVersionsAsync();
                _testOutputHelper.WriteLine($"Creating new transactions' versions and saving modified transactions [== END ==] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");

                _testOutputHelper.WriteLine($"Checking test results [= START =] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");

                transactions.Select(t => t.VersionNumber).Distinct().ToList()
                    .Should().BeEquivalentTo(new[] { 2 }, "version number should change from 1 to 2 for every modified transaction");

                transactions.Where(t => t.TransactionReference.Contains("Core"))
                    .Select(t => t.Core.PaymentDetails1).Distinct().ToList().Should().BeEquivalentTo(new[] { "yyyy" }, "core data should be modified in updated transactions");

                transactions.Where(t => t.TransactionReference.Contains("Person"))
                    .SelectMany(t => t.Disposers.List.Select(l => l.PersonId)).Distinct().OrderBy(id => id).ToList().Should().BeEquivalentTo(new long[] { 8, 20 });

                transactions.Where(t => t.TransactionReference.Contains("Aml"))
                    .Select(t => t.Aml.AmlRelatedAttribure).Distinct().ToList().Should().BeEquivalentTo(new[] { "aMl#aMl" }, "AML data should be modified in updated transactions");

                transactions.Where(t => t.TransactionReference.Contains("Stir"))
                    .Select(t => t.Stir.StirRelatedAttribure).Distinct().ToList().Should().BeEquivalentTo(new[] { "StiR" }, "STIR data should be modified in updated transactions");

                _testOutputHelper.WriteLine($"Checking test results [== END ==] {DateTime.Now:dd.MM.yyyy HH:mm:ss.ffffff}");
            }
        }
    }
}

using FluentAssertions;
using System.Linq;
using TransakcjaFir.BLL;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;
using Xunit;

namespace TransakcjaFirTests
{
    public class Modification_Of_Transaction_Sent_To_Stir_Only : BaseTransactionTest
    {

        [Fact]
        public async void Should_Create_New_Version_For_Stir_When_Changing_Core_Data()
        {
            string reference = GenerateReference("SentStirCore");

            // prepare test transaction
            new TestTransactionsGenerator().PrepareTransactionWithCoreData(reference, AmlExportStatusEnum.NotSent, StirExportStatusEnum.Sent, new int[] { 4, 1, 3 }, "xxxx");
            using (var context = new FirContext())
            {
                // run test
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transaction = await readService.LoadTransaction(reference, 1);
                transaction.Core.PaymentDetails1 = "yyyy";
                await versioningService.SaveTransactionsVersionsAsync();

                // check results kept in memory
                CheckModifiedTransaction(transaction);
            }

            // check data saved in DB
            using (var context = new FirContext())
            {
                TransactionService readService = new TransactionService(context);
                var previous = await readService.LoadTransaction(reference, 1);
                CheckTransactionData(previous, 1, false);
                CheckTransactionCoreData(previous.Core, 1, false);
                CheckPersonsData(previous.Disposers, 1, true, 3);
                CheckAmlData(previous.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(previous.Stir, 1, false, StirExportStatusEnum.Sent);
                previous.Core.PaymentDetails1.Should().Be("xxxx");
                var saved = await readService.LoadTransaction(reference, 2);
                CheckModifiedTransaction(saved);
            }

            void CheckModifiedTransaction(Transaction result)
            {
                CheckTransactionData(result, 2, true);
                CheckTransactionCoreData(result.Core, 2, true);
                CheckPersonsData(result.Disposers, 1, true, 3);
                CheckAmlData(result.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(result.Stir, 2, true, StirExportStatusEnum.NotSent);
                result.Core.PaymentDetails1.Should().Be("yyyy");
            }
        }

        [Fact]
        public async void Should_Not_Create_New_Version_When_Changing_Aml_Data()
        {
            string reference = GenerateReference("SentStirAml");

            // prepare test
            new TestTransactionsGenerator().PrepareTransactionWithAmlData(reference, AmlExportStatusEnum.NotSent, StirExportStatusEnum.Sent, new int[] { 3, 4 }, "xxxx");

            using (var context = new FirContext())
            {
                // run test
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transaction = await readService.LoadTransaction(reference, 1);
                transaction.Aml.AmlRelatedAttribure = "yyyy";
                await versioningService.SaveTransactionsVersionsAsync();
                // check results kept in memory
                CheckModifiedTransaction(transaction);
            }

            // check data saved in DB
            using (var context = new FirContext())
            {
                TransactionService service = new TransactionService(context);
                var saved = await service.LoadTransaction(reference, 1);
                CheckModifiedTransaction(saved);
            }

            void CheckModifiedTransaction(Transaction result)
            {
                CheckTransactionData(result, 1, true);
                CheckTransactionCoreData(result.Core, 1, true);
                CheckPersonsData(result.Disposers, 1, true, 2);
                CheckAmlData(result.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(result.Stir, 1, true, StirExportStatusEnum.Sent);
                result.Aml.AmlRelatedAttribure.Should().Be("yyyy");
            }
        }

        [Fact]
        public async void Should_Create_New_Version_For_Stir_When_Changing_Stir_Data()
        {
            string reference = GenerateReference("SentStirStir");

            // prepare test
            new TestTransactionsGenerator().PrepareTransactionWithStirData(reference, AmlExportStatusEnum.NotSent, StirExportStatusEnum.Sent, new int[] { 44, 55, 66 }, "xxxx");

            using (var context = new FirContext())
            {
                // run test
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transaction = await readService.LoadTransaction(reference, 1);
                transaction.Stir.StirRelatedAttribure = "yyyy";
                await versioningService.SaveTransactionsVersionsAsync();
                // check results kept in memory
                CheckModifiedTransaction(transaction);
            }

            // check data saved in DB
            using (var context = new FirContext())
            {
                TransactionService service = new TransactionService(context);
                // check data saved in DB
                var previous = await service.LoadTransaction(reference, 1);
                CheckTransactionData(previous, 1, false);
                CheckTransactionCoreData(previous.Core, 1, true);
                CheckPersonsData(previous.Disposers, 1, true, 3);
                CheckAmlData(previous.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(previous.Stir, 1, false, StirExportStatusEnum.Sent);
                previous.Stir.StirRelatedAttribure.Should().Be("xxxx");

                var saved = await service.LoadTransaction(reference, 2);
                CheckModifiedTransaction(saved);
            }

            void CheckModifiedTransaction(Transaction result)
            {
                CheckTransactionData(result, 2, true);
                CheckTransactionCoreData(result.Core, 1, true);
                CheckPersonsData(result.Disposers, 1, true, 3);
                CheckAmlData(result.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(result.Stir, 2, true, StirExportStatusEnum.NotSent);
                result.Stir.StirRelatedAttribure.Should().Be("yyyy");
            }
        }

        [Theory]
        [InlineData(PersonsListOperation.Add)]
        [InlineData(PersonsListOperation.Modify)]
        [InlineData(PersonsListOperation.Delete)]
        public async void Should_Not_Create_New_Version_When_Changing_Aml_Person_Data(PersonsListOperation operation)
        {
            string reference = GenerateReference("SentStirPerAml");

            var generator = new TestTransactionsGenerator();
            generator.PrepareTransactionWithPersonsData(reference, AmlExportStatusEnum.NotSent, StirExportStatusEnum.Sent);

            using (var context = new FirContext())
            {
                // run test
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transaction = await readService.LoadTransaction(reference, 1);

                switch (operation)
                {
                    case PersonsListOperation.Add:
                        transaction.Disposers.List.Add(generator.CreatePerson(20, "created", AmlPersonsRole.AdditionalDisposer, StirPersonsRole.None));
                        break;
                    case PersonsListOperation.Modify:
                        transaction.Disposers.List.Single(p => p.PersonId == 4).PersonName = "new_aml";
                        break;
                    case PersonsListOperation.Delete:
                        transaction.Disposers.List.RemoveAll(p => p.PersonId == 4);
                        break;
                }
                await versioningService.SaveTransactionsVersionsAsync();
                // check results kept in memory
                CheckModifiedTransaction(transaction);
            }

            // check data saved in DB
            using (var context = new FirContext())
            {
                TransactionService service = new TransactionService(context);
                var saved = await service.LoadTransaction(reference, 1);
                CheckModifiedTransaction(saved);
            }

            void CheckModifiedTransaction(Transaction result)
            {
                CheckTransactionData(result, 1, true);
                CheckTransactionCoreData(result.Core, 1, true);
                CheckAmlData(result.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(result.Stir, 1, true, StirExportStatusEnum.Sent);

                switch (operation)
                {
                    case PersonsListOperation.Add:
                        CheckPersonsData(result.Disposers, 1, true, 4);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8, 20 });
                        break;
                    case PersonsListOperation.Modify:
                        CheckPersonsData(result.Disposers, 1, true, 3);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8 });
                        result.Disposers.List.Single(p => p.PersonId == 4).PersonName.Should().Be("new_aml");
                        break;
                    case PersonsListOperation.Delete:
                        CheckPersonsData(result.Disposers, 1, true, 2);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 6, 8 });
                        break;
                }
            }
        }

        [Theory]
        [InlineData(PersonsListOperation.Add)]
        [InlineData(PersonsListOperation.Modify)]
        [InlineData(PersonsListOperation.Delete)]
        public async void Should_Create_New_Version_For_Stir_When_Changing_Stir_Person_Data(PersonsListOperation operation)
        {
            string reference = GenerateReference("SentBothPerBoth");

            var generator = new TestTransactionsGenerator();
            generator.PrepareTransactionWithPersonsData(reference, AmlExportStatusEnum.NotSent, StirExportStatusEnum.Sent);

            using (var context = new FirContext())
            {
                // run test
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transaction = await readService.LoadTransaction(reference, 1);

                switch (operation)
                {
                    case PersonsListOperation.Add:
                        transaction.Disposers.List.Add(generator.CreatePerson(20, "created", AmlPersonsRole.None, StirPersonsRole.Disposer));
                        break;
                    case PersonsListOperation.Modify:
                        var modifiedPerson = transaction.Disposers.List.Single(p => p.PersonId == 6).PersonName = "new_stir";
                        break;
                    case PersonsListOperation.Delete:
                        transaction.Disposers.List.RemoveAll(p => p.PersonId == 6);
                        break;
                }
                await versioningService.SaveTransactionsVersionsAsync();
                // check results kept in memory
                CheckModifiedTransaction(transaction);
            }

            // check data saved in DB
            using (var context = new FirContext())
            {
                TransactionService service = new TransactionService(context);
                var previous = await service.LoadTransaction(reference, 1);
                CheckTransactionData(previous, 1, false);
                CheckTransactionCoreData(previous.Core, 1, true);
                CheckAmlData(previous.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(previous.Stir, 1, false, StirExportStatusEnum.Sent);
                CheckPersonsData(previous.Disposers, 1, false, 3);
                previous.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8 });
                previous.Disposers.List.Single(p => p.PersonId == 6).PersonName.Should().Be("stir");
                var saved = await service.LoadTransaction(reference, 2);
                CheckModifiedTransaction(saved);
            }

            void CheckModifiedTransaction(Transaction result)
            {
                CheckTransactionData(result, 2, true);
                CheckTransactionCoreData(result.Core, 1, true);
                CheckAmlData(result.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(result.Stir, 2, true, StirExportStatusEnum.NotSent);

                switch (operation)
                {
                    case PersonsListOperation.Add:
                        CheckPersonsData(result.Disposers, 2, true, 4);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8, 20 });
                        break;
                    case PersonsListOperation.Modify:
                        CheckPersonsData(result.Disposers, 2, true, 3);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8 });
                        result.Disposers.List.Single(p => p.PersonId == 6).PersonName.Should().Be("new_stir");
                        break;
                    case PersonsListOperation.Delete:
                        CheckPersonsData(result.Disposers, 2, true, 2);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 8 });
                        break;
                }
            }

        }

        [Theory]
        [InlineData(PersonsListOperation.Add)]
        [InlineData(PersonsListOperation.Modify)]
        [InlineData(PersonsListOperation.Delete)]
        public async void Should_Create_New_Version_For_Stir_When_Changing_Aml_And_Stir_Person_Data(PersonsListOperation operation)
        {
            string reference = GenerateReference("SentBothPerBoth");

            var generator = new TestTransactionsGenerator();
            generator.PrepareTransactionWithPersonsData(reference, AmlExportStatusEnum.NotSent, StirExportStatusEnum.Sent);

            using (var context = new FirContext())
            {
                // run test
                TransactionService readService = new TransactionService(context);
                TransactionVersioningService versioningService = new TransactionVersioningService(context, readService);
                var transaction = await readService.LoadTransaction(reference, 1);

                switch (operation)
                {
                    case PersonsListOperation.Add:
                        transaction.Disposers.List.Add(generator.CreatePerson(20, "created", AmlPersonsRole.AdditionalDisposer, StirPersonsRole.Disposer));
                        break;
                    case PersonsListOperation.Modify:
                        transaction.Disposers.List.Single(p => p.PersonId == 8).PersonName = "new_aml_stir";
                        break;
                    case PersonsListOperation.Delete:
                        transaction.Disposers.List.RemoveAll(p => p.PersonId == 8);
                        break;
                }
                await versioningService.SaveTransactionsVersionsAsync();
                // check results kept in memory
                CheckModifiedTransaction(transaction);
            }

            // check data saved in DB
            using (var context = new FirContext())
            {
                TransactionService service = new TransactionService(context);
                var previous = await service.LoadTransaction(reference, 1);
                CheckTransactionData(previous, 1, false);
                CheckTransactionCoreData(previous.Core, 1, true);
                CheckAmlData(previous.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(previous.Stir, 1, false, StirExportStatusEnum.Sent);
                CheckPersonsData(previous.Disposers, 1, false, 3);
                previous.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8 });
                previous.Disposers.List.Single(p => p.PersonId == 8).PersonName.Should().Be("aml_stir");
                var saved = await service.LoadTransaction(reference, 2);
                CheckModifiedTransaction(saved);
            }

            void CheckModifiedTransaction(Transaction result)
            {
                CheckTransactionData(result, 2, true);
                CheckTransactionCoreData(result.Core, 1, true);
                CheckAmlData(result.Aml, 1, true, AmlExportStatusEnum.NotSent);
                CheckStirData(result.Stir, 2, true, StirExportStatusEnum.NotSent);

                switch (operation)
                {
                    case PersonsListOperation.Add:
                        CheckPersonsData(result.Disposers, 2, true, 4);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8, 20 });
                        break;
                    case PersonsListOperation.Modify:
                        CheckPersonsData(result.Disposers, 2, true, 3);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6, 8 });
                        result.Disposers.List.Single(p => p.PersonId == 8).PersonName.Should().Be("new_aml_stir");
                        break;
                    case PersonsListOperation.Delete:
                        CheckPersonsData(result.Disposers, 2, true, 2);
                        result.Disposers.List.Select(p => p.PersonId).ToList().Should().BeEquivalentTo(new[] { 4, 6 });
                        break;
                }
            }
        }
    }
}

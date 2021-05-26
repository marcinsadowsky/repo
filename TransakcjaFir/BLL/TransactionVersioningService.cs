using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;

namespace TransakcjaFir.BLL
{
    public class TransactionVersioningService
    {
        readonly TransactionService _transactionService;
        readonly FirContext _dbContext;

        public TransactionVersioningService(FirContext dbContext, TransactionService transactionService)
        {
            _dbContext = dbContext;
            _transactionService = transactionService;
        }

        public async Task SaveTransactionsVersionsAsync()
        {
            Dictionary<long, Transaction> newTransactionsVersions = new Dictionary<long, Transaction>();

            var allTransactions = _dbContext.ChangeTracker.Entries<Transaction>().ToList();
            var allCore = _dbContext.ChangeTracker.Entries<TransactionCore>().ToList();
            var allPersonsLists = _dbContext.ChangeTracker.Entries<TransactionDisposersList>().ToList();
            var allPersons = _dbContext.ChangeTracker.Entries<TransactionDisposer>().ToList();
            var allAml = _dbContext.ChangeTracker.Entries<TransactionAml>().ToList();
            var allStir = _dbContext.ChangeTracker.Entries<TransactionStir>().ToList();

            foreach (var entry in allTransactions)
            {
                if ((entry.State == EntityState.Modified) || (entry.State == EntityState.Unchanged))
                {
                    PrepareNewVersion(newTransactionsVersions, entry, allCore, allPersonsLists, allPersons, allAml, allStir);
                }
            }

            var ids = newTransactionsVersions.Keys.ToList();
            var previousTransactionsVersions = _transactionService.GetTransactionSelectQuery().Where(t => ids.Contains(t.Id)).ToList();

            if (previousTransactionsVersions.Count != ids.Count)
            {
                throw new Exception($"Incorrect number of previous transactions' versions loaded [expected: {ids.Count}, loaded: {previousTransactionsVersions.Count}]");
            }

            foreach (var previousVersion in previousTransactionsVersions)
            {
                var newVersion = newTransactionsVersions[previousVersion.Id];
                previousVersion.Core.IsLastVersion = newVersion.CoreId != 0;
                previousVersion.Aml.IsLastVersion = newVersion.AmlId != 0;
                previousVersion.Stir.IsLastVersion = newVersion.StirId != 0;
                previousVersion.Disposers.IsLastVersion = newVersion.DisposersId != 0;
                if ((newVersion.CoreId == 0) || (newVersion.DisposersId == 0) || (newVersion.AmlId == 0) || (newVersion.StirId == 0))
                {
                    previousVersion.IsLastVersion = false;
                }
            }

            _dbContext.Transactions.AddRange(newTransactionsVersions.Values);

            // todo: do ustalenia czy SavChanges tutaj docelowo zostaje (plusy i minusy rozwiązania)
            await _dbContext.SaveChangesAsync();
        }

        private void PrepareNewVersion(
            Dictionary<long, Transaction> newTransactionsVersions,
            EntityEntry<Transaction> transactionEntry,
            List<EntityEntry<TransactionCore>> allCore,
            List<EntityEntry<TransactionDisposersList>> allPersonsLists,
            List<EntityEntry<TransactionDisposer>> allPersons,
            List<EntityEntry<TransactionAml>> allAml,
            List<EntityEntry<TransactionStir>> allStir)
        {
            var transaction = transactionEntry.Entity;
            var isAmlExported = transaction.Aml.IsExported();
            var isStirExported = transaction.Stir.IsExported();

            if (isAmlExported || isStirExported)
            {
                var coreEntity = allCore.First(t => t.Entity.TransactionReference == transaction.TransactionReference);
                if (coreEntity.State == EntityState.Modified)
                {
                    Clone(coreEntity);
                }
                var isCoreModified = transaction.Core.Id == 0;

                (var isAmlPersonsListModified, var isStirPersonsListModified) = ClonePersonsList(isAmlExported, isStirExported, transaction.Disposers, allPersonsLists, allPersons);

                if (isAmlExported)
                {
                    var amlEntity = allAml.First(t => t.Entity.TransactionReference == transaction.TransactionReference);
                    if (isCoreModified || isAmlPersonsListModified || amlEntity.State == EntityState.Modified)
                    {
                        Clone(amlEntity);
                        amlEntity.Entity.ProcessingStatus = AmlExportStatusEnum.NotSent;
                    }
                }

                if (isStirExported)
                {
                    var stirEntity = allStir.First(t => t.Entity.TransactionReference == transaction.TransactionReference);
                    if (isCoreModified || isStirPersonsListModified || stirEntity.State == EntityState.Modified)
                    {
                        Clone(stirEntity);
                        stirEntity.Entity.ProcessingStatus = StirExportStatusEnum.NotSent;
                    }
                }

                if ((transaction.Core.Id == 0) || (transaction.Disposers.Id == 0) || (transaction.Aml.Id == 0) || (transaction.Stir.Id == 0))
                {
                    newTransactionsVersions.Add(transaction.Id, transaction);
                    Clone(transactionEntry);
                    transaction.CoreId = transaction.Core.Id;
                    transaction.AmlId = transaction.Aml.Id;
                    transaction.StirId = transaction.Stir.Id;
                    transaction.DisposersId = transaction.Disposers.Id;
                }
            }
        }

        private void Clone<T>(EntityEntry<T> entry) where T : class, ITransactionPartVersion
        {
            entry.State = EntityState.Detached;
            entry.Entity.VersionNumber++;
            entry.Entity.IsLastVersion = true;
            entry.Entity.Id = 0;
        }

        private (bool, bool) ClonePersonsList(bool isAmlExported, bool isStirExported,
            TransactionDisposersList list, List<EntityEntry<TransactionDisposersList>> allPersonsLists, List<EntityEntry<TransactionDisposer>> allPersons)
        {
            var personsList = allPersonsLists.First(l => l.Entity.TransactionReference == list.TransactionReference);

            var originalPersons = allPersons
               .Where(p => p.Entity.TransactionDisposersListId == list.Id)
               .ToList();

            bool isAmlPersonsListModified = originalPersons.Any(p => p.Entity.IsAmlPerson() && (p.State == EntityState.Added || p.State == EntityState.Modified || p.State == EntityState.Deleted));
            bool isStirPersonsListModified = originalPersons.Any(p => p.Entity.IsStirPerson() && (p.State == EntityState.Added || p.State == EntityState.Modified || p.State == EntityState.Deleted));

            if ((isAmlExported && isAmlPersonsListModified) || (isStirExported && isStirPersonsListModified))
            {
                Clone(personsList);
                foreach (var person in originalPersons)
                {
                    person.State = EntityState.Detached;
                    person.Entity.Id = 0;
                }
            }
            return (isAmlPersonsListModified, isStirPersonsListModified);
        }
    }
}

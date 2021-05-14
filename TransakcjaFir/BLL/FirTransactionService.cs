using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;

namespace TransakcjaFir.BLL
{
    public class FirTransactionService
    {
        readonly FirContext _dbContext;

        public FirTransactionService(FirContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Transaction> GetTransactionSelectQuery()
        {
            var query = _dbContext
                .Transactions
                .Include(t => t.Core)
                .Include(t => t.Aml)
                .Include(t => t.Stir)
                .Include(t => t.Persons).ThenInclude(l => l.List);
            return query;
        }

        public Transaction CreateTransaction(string reference)
        {
            var transaction = Transaction.Create(reference, 1);
            transaction.Core = TransactionCore.Create(transaction, reference, 1);
            transaction.Aml = TransactionAml.Create(transaction, reference, 1);
            transaction.Stir = TransactionStir.Create(transaction, reference, 1);
            transaction.Persons = TransactionPersonsList.Create(transaction, reference, 1);
            return transaction;
        }

        public void AddPerson(Transaction transaction, TransactionPerson person)
        {
            transaction.Persons.List.Add(person);
            person.TransactionPersonsList = transaction.Persons;
            person.TransactionPersonsListId = transaction.Persons.Id;
        }

        public async Task<Transaction> LoadTransaction(string reference, int version)
        {
            var query = GetTransactionSelectQuery();
            var transaction = await query
                .Where(t => t.TransactionReference == reference && t.VersionNumber == version)
                .FirstOrDefaultAsync();
            return transaction;
        }

        public async Task SaveTransactionAsync(Transaction transaction)
        {
            var state = _dbContext.Entry(transaction).State;
            if (state == EntityState.Detached)
            {
                _dbContext.Transactions.Add(transaction);
                await _dbContext.SaveChangesAsync();
                return;
            }
            await PerformUpdate(transaction);
        }

        private async Task PerformUpdate(Transaction transaction)
        {
            var isAmlExported = transaction.Aml.IsExported();
            var isStirExported = transaction.Stir.IsExported();

            if (isAmlExported || isStirExported)
            {
                bool isAmlPersonsListModified;
                bool isStirPersonsListModified;
                List<TransactionPerson> originalPersonsList;

                (isAmlPersonsListModified, isStirPersonsListModified, originalPersonsList) = IsPersonsListModified(transaction.Persons);
                var personsCloneNeeded = ((isAmlExported && isAmlPersonsListModified) || (isStirExported && isStirPersonsListModified));

                var newCoreData = CloneTransactionCore(transaction.Core);
                var newPersonsList = personsCloneNeeded ? ClonePersonsList(transaction.Persons, originalPersonsList) : null;
                var newAmlData = isAmlExported ? CloneAmlData(transaction.Aml, newCoreData != null, isAmlPersonsListModified) : null;
                var newStirData = isStirExported ? CloneStirData(transaction.Stir, newCoreData != null, isStirPersonsListModified) : null;

                await PrepareNewTransactionVersion(transaction, newCoreData, newAmlData, newStirData, newPersonsList);
            }

            await _dbContext.SaveChangesAsync();
        }

        private TransactionCore CloneTransactionCore(TransactionCore core)
        {
            var originalCoreEntity = _dbContext.Entry(core);
            if (originalCoreEntity.State == EntityState.Modified)
            {
                originalCoreEntity.State = EntityState.Detached;
                return TransactionCore.CloneAsNewVersion(core);
            }
            return null;
        }

        private TransactionAml CloneAmlData(TransactionAml aml, bool isTransactionCoreModified, bool isAmlPersonsListModified)
        {
            var originaAmlEntity = _dbContext.Entry(aml);
            if (isTransactionCoreModified || isAmlPersonsListModified || originaAmlEntity.State == EntityState.Modified)
            {
                originaAmlEntity.State = EntityState.Detached;
                return TransactionAml.CloneAsNewVersion(aml);
            }
            return null;
        }

        private TransactionStir CloneStirData(TransactionStir stir, bool isTransactionCoreModified, bool isStirPersonsListModified)
        {
            var originalStirEntity = _dbContext.Entry(stir);
            if (isTransactionCoreModified || isStirPersonsListModified || originalStirEntity.State == EntityState.Modified)
            {
                originalStirEntity.State = EntityState.Detached;
                return TransactionStir.CloneAsNewVersion(stir);
            }
            return null;
        }

        private (bool, bool, List<TransactionPerson>) IsPersonsListModified(TransactionPersonsList list)
        {
            bool aml = false;
            bool stir = false;
            var persons = _dbContext.ChangeTracker.Entries()
                .Where(e => e.Entity.GetType() == typeof(TransactionPerson))
                .Select(e => (TransactionPerson)e.Entity)
                .Where(p => p.TransactionPersonsListId == list.Id)
                .ToList();

            foreach (var person in persons)
            {
                var state = _dbContext.Entry(person).State;
                if ((state == EntityState.Added) || (state == EntityState.Modified) || (state == EntityState.Deleted))
                {
                    if (person.IsAmlPerson())
                    {
                        aml = true;
                    }
                    if (person.IsStirPerson())
                    {
                        stir = true;
                    }
                }
            }
            return (aml, stir, persons);
        }

        private TransactionPersonsList ClonePersonsList(TransactionPersonsList list, List<TransactionPerson> originalPersons)
        {
            _dbContext.Entry(list).State = EntityState.Detached;
            foreach (var person in originalPersons)
            {
                _dbContext.Entry(person).State = EntityState.Detached;
            }
            return TransactionPersonsList.CloneAsNewVersion(list);
        }

        private async Task PrepareNewTransactionVersion(Transaction transaction, TransactionCore newCoreData, TransactionAml newAmlData, TransactionStir newStirData, TransactionPersonsList newPersonsList)
        {
            if ((newCoreData != null) || (newAmlData != null) || (newStirData != null) || (newPersonsList != null))
            {
                _dbContext.Entry(transaction).State = EntityState.Detached;
                var existing = await LoadTransaction(transaction.TransactionReference, transaction.VersionNumber);
                existing.IsLastVersion = false;
                if (newCoreData != null)
                {
                    existing.Core.IsLastVersion = false;
                    transaction.Core = newCoreData;
                    transaction.CoreId = 0;
                }
                if (newAmlData != null)
                {
                    existing.Aml.IsLastVersion = false;
                    transaction.Aml = newAmlData;
                    transaction.AmlId = 0;
                }
                if (newStirData != null)
                {
                    existing.Stir.IsLastVersion = false;
                    transaction.Stir = newStirData;
                    transaction.StirId = 0;
                }
                if (newPersonsList != null)
                {
                    existing.Persons.IsLastVersion = false;
                    transaction.Persons = newPersonsList;
                    transaction.PersonsId = 0;
                }

                // await _dbContext.SaveChangesAsync();
                // _dbContext.Entry(existing).State = EntityState.Detached;
                transaction.Id = 0;
                transaction.VersionNumber = existing.VersionNumber + 1;
                _dbContext.Transactions.Add(transaction);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransakcjaFir.DAL;
using TransakcjaFir.Model;

namespace TransakcjaFir.BLL
{
    public class TransactionService
    {
        readonly FirContext _dbContext;

        public TransactionService(FirContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Transaction> GetTransactionSelectQuery()
        {
            var query = _dbContext
                .Transactions
                .Include(t => t.Core)
                .Include(t => t.Aml)
                .Include(t => t.Stir)
                .Include(t => t.Disposers).ThenInclude(l => l.List);
            return query;
        }

        public Transaction CreateTransaction(string reference)
        {
            var transaction = new Transaction()
            {
                TransactionReference = reference,
                VersionNumber = 1,
                IsLastVersion = true
            };

            transaction.Core = new TransactionCore()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = 1,
                IsLastVersion = true
            };

            transaction.Aml = new TransactionAml()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = 1,
                IsLastVersion = true,
                ProcessingStatus = AmlExportStatusEnum.NotSent,
            };

            transaction.Stir = new TransactionStir()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = 1,
                IsLastVersion = true,
                ProcessingStatus = StirExportStatusEnum.NotSent,
            };

            transaction.Disposers = new TransactionDisposersList()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = 1,
                IsLastVersion = true,
                List = new List<TransactionDisposer>()
            };

            return transaction;
        }

        public void AddPerson(Transaction transaction, TransactionDisposer person)
        {
            transaction.Disposers.List.Add(person);
            person.TransactionDisposersList = transaction.Disposers;
            person.TransactionDisposersListId = transaction.Disposers.Id;
        }

        public async Task<Transaction> LoadTransaction(string reference, int version)
        {
            var query = GetTransactionSelectQuery();
            var transaction = await query
                .Where(t => t.TransactionReference == reference && t.VersionNumber == version)
                .FirstOrDefaultAsync();
            return transaction;
        }

        public async Task<List<Transaction>> LoadTransactionsWithRefereneMask(string referencePrefix)
        {
            var query = GetTransactionSelectQuery();
            var transactions = await query
                .Where(t => t.TransactionReference.StartsWith(referencePrefix)
                    && t.VersionNumber == 1
                )
                .ToListAsync();
            return transactions;
        }
    }
}

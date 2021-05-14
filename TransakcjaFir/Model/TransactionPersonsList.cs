using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace TransakcjaFir.Model
{
    public class TransactionPersonsList
    {
        public long Id { get; set; }
        public List<Transaction> Transaction { get; set; }
        public string TransactionReference { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLastVersion { get; set; }
        public List<TransactionPerson> List { get; set; }

        public static TransactionPersonsList Create(Transaction transaction, string reference, int version) =>
            new TransactionPersonsList()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = version,
                IsLastVersion = true,
                List = new List<TransactionPerson>()
            };

        public static TransactionPersonsList CloneAsNewVersion(TransactionPersonsList original)
        {
            var list = new TransactionPersonsList()
            {
                TransactionReference = original.TransactionReference,
                VersionNumber = original.VersionNumber + 1,
                IsLastVersion = true,
                List = new List<TransactionPerson>()
            };
            var persons = original.List ?? new List<TransactionPerson>();
            foreach (var person in persons)
            {
                list.List.Add(TransactionPerson.CloneAsNewVersion(person));
            }
            return list;
        }
    }

    internal class TransactionPersonsListEntityTypeConfiguration : IEntityTypeConfiguration<TransactionPersonsList>
    {
        public void Configure(EntityTypeBuilder<TransactionPersonsList> builder)
        {
            builder.ToTable("TransactionPersonsList", "REP");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("TransactionPersonsListId");
        }
    }
}

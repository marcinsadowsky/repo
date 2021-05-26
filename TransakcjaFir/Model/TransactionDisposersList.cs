using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace TransakcjaFir.Model
{
    public class TransactionDisposersList : ITransactionPartVersion
    {
        public long Id { get; set; }
        public List<Transaction> Transaction { get; set; }
        public string TransactionReference { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLastVersion { get; set; }
        public List<TransactionDisposer> List { get; set; }
    }

    internal class TransactionPersonsListEntityTypeConfiguration : IEntityTypeConfiguration<TransactionDisposersList>
    {
        public void Configure(EntityTypeBuilder<TransactionDisposersList> builder)
        {
            builder.ToTable("TransactionDisposersList", "REP");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("TransactionDisposersListId");
        }
    }
}

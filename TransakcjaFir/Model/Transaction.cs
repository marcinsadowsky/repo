using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace TransakcjaFir.Model
{
    public class Transaction : ITransactionPartVersion
    {
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreationUserAccountId { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUserAccountId { get; set; }
        public byte ValidationCounter { get; set; }
        public string Status { get; set; }
        public string TransactionReference { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLastVersion { get; set; }
        public TransactionCore Core { get; set; }
        public long CoreId { get; set; }
        public TransactionAml Aml { get; set; }
        public long AmlId { get; set; }
        public TransactionStir Stir { get; set; }
        public long StirId { get; set; }
        public TransactionDisposersList Disposers { get; set; }
        public long DisposersId { get; set; }
    }

    internal class TransactionEntityTypeConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transaction", "REP");
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Id).HasColumnName("TransactionId");
            builder.Property(c => c.CoreId).HasColumnName("TransactionCoreId");
            builder.Property(c => c.AmlId).HasColumnName("TransactionAmlId");
            builder.Property(c => c.StirId).HasColumnName("TransactionStirId");
            builder.Property(c => c.DisposersId).HasColumnName("TransactionDisposersListId");

            builder.HasOne(c => c.Core).WithMany(c => c.Transaction);
            builder.HasOne(c => c.Aml).WithMany(c => c.Transaction);
            builder.HasOne(c => c.Stir).WithMany(c => c.Transaction);
            builder.HasOne(c => c.Disposers).WithMany(c => c.Transaction);
        }
    }
}

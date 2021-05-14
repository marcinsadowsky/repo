using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using TransakcjaFir.DAL.ValueConverters;

namespace TransakcjaFir.Model
{
    public class TransactionStir
    {
        public long Id { get; set; }
        public List<Transaction> Transaction { get; set; }
        public string TransactionReference { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLastVersion { get; set; }
        public StirExportStatusEnum ProcessingStatus { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? StatuseDate { get; set; }
        public string StirRelatedAttribure { get; set; }
        public bool IsExported()
        {
            return ProcessingStatus != StirExportStatusEnum.NotSent;
        }

        public static TransactionStir Create(Transaction transaction, string reference, int version) =>
            new TransactionStir()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = version,
                IsLastVersion = true,
                ProcessingStatus = StirExportStatusEnum.NotSent,
            };

        public static TransactionStir CloneAsNewVersion(TransactionStir stir) =>
            new TransactionStir()
            {
                TransactionReference = stir.TransactionReference,
                VersionNumber = stir.VersionNumber + 1,
                IsLastVersion = true,
                ProcessingStatus = StirExportStatusEnum.NotSent,
                StirRelatedAttribure = stir.StirRelatedAttribure,
            };
    }

    internal class TransactionStirEntityTypeConfiguration : IEntityTypeConfiguration<TransactionStir>
    {
        public void Configure(EntityTypeBuilder<TransactionStir> builder)
        {
            builder.ToTable("TransactionStir", "REP");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("TransactionStirId");
            builder.Property(c => c.ProcessingStatus).HasConversion(new StirExportStatusValueConverter());
        }
    }
}

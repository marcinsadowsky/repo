using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using TransakcjaFir.DAL.ValueConverters;

namespace TransakcjaFir.Model
{
    public class TransactionAml : ITransactionPartVersion
    {
        public long Id { get; set; }
        public List<Transaction> Transaction { get; set; }
        public string TransactionReference { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLastVersion { get; set; }
        public AmlExportStatusEnum ProcessingStatus { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? StatusDate { get; set; }
        public string AmlRelatedAttribure { get; set; }

        public bool IsExported()
        {
            return (ProcessingStatus != AmlExportStatusEnum.NotApplicable) && (ProcessingStatus != AmlExportStatusEnum.NotSent);
        }
    }

    internal class TransactionAmlEntityTypeConfiguration : IEntityTypeConfiguration<TransactionAml>
    {
        public void Configure(EntityTypeBuilder<TransactionAml> builder)
        {
            builder.ToTable("TransactionAMl", "REP");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("TransactionAMlId");
            builder.Property(c => c.ProcessingStatus).HasConversion(new AmlExportStatusValueConverter());
        }
    }
}
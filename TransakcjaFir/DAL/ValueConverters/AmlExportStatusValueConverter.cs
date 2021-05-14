using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using TransakcjaFir.Model;

namespace TransakcjaFir.DAL.ValueConverters
{
    public class AmlExportStatusValueConverter : ValueConverter<AmlExportStatusEnum, string>
    {
        private static readonly IReadOnlyDictionary<string, AmlExportStatusEnum> _values = new Dictionary<string, AmlExportStatusEnum>
        {
            { "Not sent", AmlExportStatusEnum.NotSent },
            { "In progress", AmlExportStatusEnum.InProgress },
            { "Sent", AmlExportStatusEnum.Sent },
            { "Error", AmlExportStatusEnum.Error }
        };

        public AmlExportStatusValueConverter() : base(status => ToDbValue(status), status => ToEnum(status)) { }

        public static string ToDbValue(AmlExportStatusEnum enumValue)
        {
            return _values.Single(v => v.Value == enumValue).Key;
        }

        public static AmlExportStatusEnum ToEnum(string dbValue)
        {
            return _values[dbValue];
        }
    }
}

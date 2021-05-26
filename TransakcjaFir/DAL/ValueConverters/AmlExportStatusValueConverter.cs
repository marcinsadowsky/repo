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
            { "n/a", AmlExportStatusEnum.NotApplicable },
            { "NotSent", AmlExportStatusEnum.NotSent },
            { "Sending", AmlExportStatusEnum.InProgress },
            { "Sent", AmlExportStatusEnum.Sent },
            { "Delivered", AmlExportStatusEnum.Delivered },
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

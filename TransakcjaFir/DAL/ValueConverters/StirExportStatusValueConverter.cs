﻿using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Linq;
using TransakcjaFir.Model;

namespace TransakcjaFir.DAL.ValueConverters
{
    public class StirExportStatusValueConverter : ValueConverter<StirExportStatusEnum, string>
    {
        private static readonly IReadOnlyDictionary<string, StirExportStatusEnum> _values = new Dictionary<string, StirExportStatusEnum>
        {
            { "Not sent", StirExportStatusEnum.NotSent },
            { "In progress", StirExportStatusEnum.InProgress },
            { "Sent", StirExportStatusEnum.Sent },
            { "Error", StirExportStatusEnum.Error }
        };

        public StirExportStatusValueConverter() : base(status => ToDbValue(status), status => ToEnum(status)) { }

        public static string ToDbValue(StirExportStatusEnum enumValue)
        {
            return _values.Single(v => v.Value == enumValue).Key;
        }

        public static StirExportStatusEnum ToEnum(string dbValue)
        {
            return _values[dbValue];
        }
    }
}

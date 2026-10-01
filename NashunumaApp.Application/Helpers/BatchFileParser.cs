using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NashunumaApp.Application.Helpers
{
    public static class BatchFileParser
    {
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls", ".csv", ".txt" };

        public static bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        public static List<string> Parse(Stream stream, string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";

            return ext switch
            {
                ".xlsx" or ".xls" => ParseExcel(stream),
                ".csv" or ".txt" => ParseCsv(stream),
                _ => throw new NotSupportedException(
                    $"File type '{ext}' is not supported. Allowed: .xlsx, .xls, .csv, .txt")
            };
        }

        private static List<string> ParseExcel(Stream stream)
        {
            var batches = new List<string>();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Excel file contains no worksheets.");

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null) return batches;

            var firstRow = usedRange.FirstRow();
            var firstCellValue = firstRow.Cell(1).GetString()?.Trim() ?? "";

            bool firstRowIsHeader = !IsPlausibleBatchNumber(firstCellValue);
            int startRow = firstRowIsHeader ? firstRow.RowNumber() + 1 : firstRow.RowNumber();

            foreach (var row in usedRange.RowsUsed().Where(r => r.RowNumber() >= startRow))
            {
                var value = row.Cell(1).GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    batches.Add(value);
            }

            return batches
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> ParseCsv(Stream stream)
        {
            var batches = new List<string>();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            string? line;
            bool isFirstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var tokens = line
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().Trim('"'))
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                foreach (var token in tokens)
                {
                    if (isFirstLine && !IsPlausibleBatchNumber(token))
                        continue;

                    batches.Add(token);
                }

                isFirstLine = false;
            }

            return batches
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsPlausibleBatchNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var lower = value.ToLowerInvariant();
            if (lower.Contains("batch") || lower.Contains("number") || lower.Contains("no."))
                return false;
            return true;
        }
    }
}
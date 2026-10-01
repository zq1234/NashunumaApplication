using ClosedXML.Excel;
using NashunumaApp.Domain.Common.DTOs;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Helpers
{
    public static class BatchExportWriter
    {
        private static readonly string[] Headers =
        {
            "BatchNumber", "MotherCnic", "VisitDate", "TrimisterNo", "SiteName",
            "UpdatedBy", "PhoneNo", "Address", "Province", "District", "Tehsil", "Email"
        };

        // ---------------- CSV ----------------
        public static async Task WriteCsvAsync( Stream output, IAsyncEnumerable<MotherTrimisterDto> rows,CancellationToken cancellationToken = default)
        {
            await using var writer = new StreamWriter(output, new UTF8Encoding(false), leaveOpen: true);
            // BOM so Excel opens UTF-8 correctly: use new UTF8Encoding(true) if you want BOM.

            await writer.WriteLineAsync(string.Join(",", Headers));

            await foreach (var r in rows.WithCancellation(cancellationToken))
            {
                var line = string.Join(",",
                    Csv(r.BatchNumber),
                    Csv(r.MotherCnic),
                    //Csv(r.VisitDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    Csv(r.VisitDate),
                    Csv(r.TrimisterNo?.ToString(CultureInfo.InvariantCulture)),
                    Csv(r.SiteName),
                    Csv(r.UpdatedBy),
                    Csv(r.PhoneNo),
                    Csv(r.Address),
                    Csv(r.Province),
                    Csv(r.District),
                    Csv(r.Tehsil),
                    Csv(r.Email));

                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync();
        }

        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!needsQuotes) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        // ---------------- XLSX ----------------
        public static async Task WriteXlsxAsync( Stream output, IAsyncEnumerable<MotherTrimisterDto> rows, CancellationToken cancellationToken = default)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("MotherTrimister");

            for (int c = 0; c < Headers.Length; c++)
                ws.Cell(1, c + 1).Value = Headers[c];

            ws.Row(1).Style.Font.Bold = true;

            int row = 2;
            await foreach (var r in rows.WithCancellation(cancellationToken))
            {
                ws.Cell(row, 1).Value = r.BatchNumber ?? "";
                ws.Cell(row, 2).Value = r.MotherCnic ?? "";
                ws.Cell(row, 3).Value = r.VisitDate;
                ws.Cell(row, 3).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(row, 4).Value = r.TrimisterNo;
                ws.Cell(row, 5).Value = r.SiteName ?? "";
                ws.Cell(row, 6).Value = r.UpdatedBy ?? "";
                ws.Cell(row, 7).Value = r.PhoneNo ?? "";
                ws.Cell(row, 8).Value = r.Address ?? "";
                ws.Cell(row, 9).Value = r.Province ?? "";
                ws.Cell(row, 10).Value = r.District ?? "";
                ws.Cell(row, 11).Value = r.Tehsil ?? "";
                ws.Cell(row, 12).Value = r.Email ?? "";
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(output);
            await output.FlushAsync(cancellationToken);
        }
    }
}
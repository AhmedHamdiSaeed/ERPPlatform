using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ERPPlatform.Imports;
using NPOI.SS.UserModel;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Application.Imports;

/// <summary>
/// One parsed data row. <see cref="RowNumber"/> is the 1-based spreadsheet row
/// (header included) so an administrator can jump straight to it in Excel;
/// <see cref="DataRowNumber"/> is the 1-based ordinal used for chunk boundaries.
/// </summary>
public class EmployeeImportRow
{
    public int DataRowNumber { get; set; }

    public int RowNumber { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Salary { get; set; } = string.Empty;
    public string JoiningDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string LeaveBalance { get; set; } = string.Empty;

    public string? Get(string canonicalColumn) => canonicalColumn switch
    {
        EmployeeImportConsts.ColumnEmployeeCode => EmployeeCode,
        EmployeeImportConsts.ColumnName => Name,
        EmployeeImportConsts.ColumnEmail => Email,
        EmployeeImportConsts.ColumnPhone => Phone,
        EmployeeImportConsts.ColumnPosition => Position,
        EmployeeImportConsts.ColumnDepartmentName => DepartmentName,
        EmployeeImportConsts.ColumnSalary => Salary,
        EmployeeImportConsts.ColumnJoiningDate => JoiningDate,
        EmployeeImportConsts.ColumnStatus => Status,
        EmployeeImportConsts.ColumnLocation => Location,
        EmployeeImportConsts.ColumnManagerName => ManagerName,
        EmployeeImportConsts.ColumnLeaveBalance => LeaveBalance,
        _ => null
    };
}

/// <summary>Result of the up-front structural validation pass.</summary>
public class EmployeeImportScanResult
{
    public bool IsValid => Errors.Count == 0;

    public int HeaderRowIndex { get; set; }

    public int TotalRows { get; set; }

    public IReadOnlyDictionary<string, int> ColumnMap { get; set; } = new Dictionary<string, int>();

    /// <summary>Fatal problems (missing columns) — the import cannot start at all.</summary>
    public List<EmployeeImportRowError> Errors { get; set; } = new();

    /// <summary>
    /// In-file duplicates (same employee code or email on two different rows).
    /// Recorded up front because the offending rows can live in different chunks.
    /// </summary>
    public List<EmployeeImportRowError> DuplicateErrors { get; set; } = new();
}

public class EmployeeImportRowError
{
    public int RowNumber { get; set; }
    public string? ColumnName { get; set; }
    public string? Value { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Reads .xlsx and .xls with NPOI. Never trusts the file: headers are matched
/// leniently, every cell is size-clamped and every value is re-validated by
/// <see cref="EmployeeImportRowValidator"/> before it touches the database.
/// </summary>
public interface IEmployeeImportExcelReader
{
    EmployeeImportScanResult Scan(Stream stream, int maxRows);

    /// <summary>
    /// Reads the inclusive 1-based data-row range <paramref name="startDataRow"/>..<paramref name="endDataRow"/>.
    /// Each Hangfire chunk job re-opens the retained file and reads only its own slice.
    /// </summary>
    List<EmployeeImportRow> ReadRange(Stream stream, int startDataRow, int endDataRow);

    byte[] BuildTemplate();
}

public class EmployeeImportExcelReader : IEmployeeImportExcelReader, ITransientDependency
{
    private const int MaxCellLength = 512;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public EmployeeImportScanResult Scan(Stream stream, int maxRows)
    {
        var result = new EmployeeImportScanResult();
        stream.Position = 0;

        using var workbook = WorkbookFactory.Create(stream);
        var sheet = GetFirstSheet(workbook);
        if (sheet == null)
        {
            result.Errors.Add(new EmployeeImportRowError { RowNumber = 1, ErrorMessage = "The workbook contains no worksheets." });
            return result;
        }

        var headerRowIndex = FindHeaderRowIndex(sheet, maxRows);
        if (headerRowIndex < 0)
        {
            result.Errors.Add(new EmployeeImportRowError
            {
                RowNumber = 1,
                ErrorMessage = "Could not find a header row. The first row must contain column names such as EmployeeCode, Name and Email."
            });
            return result;
        }

        result.HeaderRowIndex = headerRowIndex;

        var headerRow = sheet.GetRow(headerRowIndex);
        var columnMap = BuildColumnMap(sheet, headerRow);
        result.ColumnMap = columnMap;

        var missing = EmployeeImportConsts.RequiredColumns
            .Where(c => !columnMap.ContainsKey(c))
            .ToList();

        foreach (var column in missing)
        {
            result.Errors.Add(new EmployeeImportRowError
            {
                RowNumber = headerRowIndex + 1,
                ColumnName = column,
                ErrorMessage = $"Missing required column: {column}"
            });
        }

        // Count data rows and, in the same pass, detect duplicates that span chunks.
        var codeToRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var emailToRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var formatter = new DataFormatter(Invariant);
        var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

        var dataRowNumber = 0;
        for (var r = headerRowIndex + 1; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null || IsEmptyRow(row, formatter, evaluator)) continue;

            dataRowNumber++;
            if (dataRowNumber > maxRows)
            {
                result.Errors.Add(new EmployeeImportRowError
                {
                    RowNumber = r + 1,
                    ErrorMessage = $"The file contains more than the maximum of {maxRows:N0} data rows."
                });
                break;
            }

            var spreadsheetRow = r + 1;

            if (columnMap.TryGetValue(EmployeeImportConsts.ColumnEmployeeCode, out var codeIndex))
            {
                var code = CellText(row, codeIndex, formatter, evaluator);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    if (codeToRow.TryGetValue(code, out var firstRow))
                    {
                        result.DuplicateErrors.Add(new EmployeeImportRowError
                        {
                            RowNumber = spreadsheetRow,
                            ColumnName = EmployeeImportConsts.ColumnEmployeeCode,
                            Value = code,
                            ErrorMessage = $"Duplicate employee number at rows {firstRow} and {spreadsheetRow}"
                        });
                    }
                    else
                    {
                        codeToRow[code] = spreadsheetRow;
                    }
                }
            }

            if (columnMap.TryGetValue(EmployeeImportConsts.ColumnEmail, out var emailIndex))
            {
                var email = CellText(row, emailIndex, formatter, evaluator);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (emailToRow.TryGetValue(email, out var firstRow))
                    {
                        result.DuplicateErrors.Add(new EmployeeImportRowError
                        {
                            RowNumber = spreadsheetRow,
                            ColumnName = EmployeeImportConsts.ColumnEmail,
                            Value = email,
                            ErrorMessage = $"Duplicate email at rows {firstRow} and {spreadsheetRow}"
                        });
                    }
                    else
                    {
                        emailToRow[email] = spreadsheetRow;
                    }
                }
            }
        }

        result.TotalRows = dataRowNumber;
        if (dataRowNumber == 0)
        {
            result.Errors.Add(new EmployeeImportRowError
            {
                RowNumber = headerRowIndex + 1,
                ErrorMessage = "The worksheet contains a header row but no data rows."
            });
        }

        return result;
    }

    public List<EmployeeImportRow> ReadRange(Stream stream, int startDataRow, int endDataRow)
    {
        var rows = new List<EmployeeImportRow>();
        stream.Position = 0;

        using var workbook = WorkbookFactory.Create(stream);
        var sheet = GetFirstSheet(workbook);
        if (sheet == null) return rows;

        var headerRowIndex = FindHeaderRowIndex(sheet, int.MaxValue);
        if (headerRowIndex < 0) return rows;

        var columnMap = BuildColumnMap(sheet, sheet.GetRow(headerRowIndex));
        var formatter = new DataFormatter(Invariant);
        var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

        var dataRowNumber = 0;
        for (var r = headerRowIndex + 1; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null || IsEmptyRow(row, formatter, evaluator)) continue;

            dataRowNumber++;
            if (dataRowNumber < startDataRow) continue;
            if (dataRowNumber > endDataRow) break;

            rows.Add(new EmployeeImportRow
            {
                DataRowNumber = dataRowNumber,
                RowNumber = r + 1,
                EmployeeCode = Value(columnMap, row, EmployeeImportConsts.ColumnEmployeeCode, formatter, evaluator),
                Name = Value(columnMap, row, EmployeeImportConsts.ColumnName, formatter, evaluator),
                Email = Value(columnMap, row, EmployeeImportConsts.ColumnEmail, formatter, evaluator),
                Phone = Value(columnMap, row, EmployeeImportConsts.ColumnPhone, formatter, evaluator),
                Position = Value(columnMap, row, EmployeeImportConsts.ColumnPosition, formatter, evaluator),
                DepartmentName = Value(columnMap, row, EmployeeImportConsts.ColumnDepartmentName, formatter, evaluator),
                Salary = Value(columnMap, row, EmployeeImportConsts.ColumnSalary, formatter, evaluator),
                JoiningDate = Value(columnMap, row, EmployeeImportConsts.ColumnJoiningDate, formatter, evaluator),
                Status = Value(columnMap, row, EmployeeImportConsts.ColumnStatus, formatter, evaluator),
                Location = Value(columnMap, row, EmployeeImportConsts.ColumnLocation, formatter, evaluator),
                ManagerName = Value(columnMap, row, EmployeeImportConsts.ColumnManagerName, formatter, evaluator),
                LeaveBalance = Value(columnMap, row, EmployeeImportConsts.ColumnLeaveBalance, formatter, evaluator)
            });
        }

        return rows;
    }

    public byte[] BuildTemplate()
    {
        IWorkbook workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
        var sheet = workbook.CreateSheet("Employees");

        var headerStyle = workbook.CreateCellStyle();
        var font = workbook.CreateFont();
        font.IsBold = true;
        headerStyle.SetFont(font);

        var header = sheet.CreateRow(0);
        for (var i = 0; i < EmployeeImportConsts.AllColumns.Length; i++)
        {
            var cell = header.CreateCell(i);
            cell.SetCellValue(EmployeeImportConsts.AllColumns[i]);
            cell.CellStyle = headerStyle;
        }

        var sample = sheet.CreateRow(1);
        sample.CreateCell(0).SetCellValue("EMP-0001");
        sample.CreateCell(1).SetCellValue("Sara Mahmoud");
        sample.CreateCell(2).SetCellValue("sara.mahmoud@erpplatform.com");
        sample.CreateCell(3).SetCellValue("+201000000000");
        sample.CreateCell(4).SetCellValue("HR Manager");
        sample.CreateCell(5).SetCellValue("Human Resources");
        sample.CreateCell(6).SetCellValue(18000);
        sample.CreateCell(7).SetCellValue(DateTime.Today.ToString("yyyy-MM-dd", Invariant));
        sample.CreateCell(8).SetCellValue("Active");
        sample.CreateCell(9).SetCellValue("Cairo HQ");
        sample.CreateCell(10).SetCellValue("Ahmed Hamdi");
        sample.CreateCell(11).SetCellValue(21);

        for (var i = 0; i < EmployeeImportConsts.AllColumns.Length; i++)
        {
            sheet.AutoSizeColumn(i);
        }

        using var ms = new MemoryStream();
        workbook.Write(ms, leaveOpen: false);
        return ms.ToArray();
    }

    // ── Helpers ─────────────────────────────────────────────────

    private static ISheet? GetFirstSheet(IWorkbook workbook)
    {
        if (workbook.NumberOfSheets == 0) return null;

        // Prefer a worksheet whose name mentions employees, else the first one.
        for (var i = 0; i < workbook.NumberOfSheets; i++)
        {
            var name = workbook.GetSheetName(i) ?? string.Empty;
            if (name.Contains("employee", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("staff", StringComparison.OrdinalIgnoreCase))
            {
                return workbook.GetSheetAt(i);
            }
        }

        return workbook.GetSheetAt(0);
    }

    /// <summary>
    /// The header is the first row that contains at least one recognised column name,
    /// which tolerates title/logo rows above the real header.
    /// </summary>
    private static int FindHeaderRowIndex(ISheet sheet, int maxRows)
    {
        var limit = Math.Min(sheet.LastRowNum, Math.Max(maxRows, 50));
        for (var r = 0; r <= limit; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;

            var formatter = new DataFormatter(Invariant);
            var texts = new List<string>();
            for (var c = row.FirstCellNum; c < row.LastCellNum; c++)
            {
                var cell = row.GetCell(c);
                if (cell == null) continue;
                var text = formatter.FormatCellValue(cell);
                if (!string.IsNullOrWhiteSpace(text)) texts.Add(Normalise(text));
            }

            if (texts.Count == 0) continue;

            var matches = EmployeeImportConsts.RequiredColumns.Count(required =>
                EmployeeImportConsts.ColumnAliases.TryGetValue(required, out var aliases) &&
                aliases.Any(alias => texts.Contains(Normalise(alias))));

            // Require two of the three required columns so a stray title row cannot win.
            if (matches >= 2) return r;
        }

        return -1;
    }

    /// <summary>Maps canonical column name -> zero-based cell index.</summary>
    private static Dictionary<string, int> BuildColumnMap(ISheet sheet, IRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (headerRow == null) return map;

        var formatter = new DataFormatter(Invariant);
        for (var c = headerRow.FirstCellNum; c < headerRow.LastCellNum; c++)
        {
            var cell = headerRow.GetCell(c);
            if (cell == null) continue;

            var text = Normalise(formatter.FormatCellValue(cell));
            if (string.IsNullOrEmpty(text)) continue;

            foreach (var pair in EmployeeImportConsts.ColumnAliases)
            {
                if (map.ContainsKey(pair.Key)) continue;
                if (pair.Value.Any(alias => text.Equals(Normalise(alias), StringComparison.Ordinal)))
                {
                    map[pair.Key] = c;
                }
            }
        }

        return map;
    }

    private static string Value(
        IReadOnlyDictionary<string, int> columnMap,
        IRow row,
        string canonicalColumn,
        DataFormatter formatter,
        IFormulaEvaluator evaluator)
    {
        return columnMap.TryGetValue(canonicalColumn, out var index)
            ? CellText(row, index, formatter, evaluator)
            : string.Empty;
    }

    private static string CellText(IRow row, int columnIndex, DataFormatter formatter, IFormulaEvaluator evaluator)
    {
        var cell = row.GetCell(columnIndex);
        if (cell == null) return string.Empty;

        string text;
        try
        {
            text = cell.CellType == CellType.Formula
                ? formatter.FormatCellValue(cell, evaluator)
                : formatter.FormatCellValue(cell);
        }
        catch
        {
            text = cell.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        text = text.Trim();
        return text.Length <= MaxCellLength ? text : text.Substring(0, MaxCellLength);
    }

    private static bool IsEmptyRow(IRow row, DataFormatter formatter, IFormulaEvaluator evaluator)
    {
        // NPOI reports hidden/blank cells as present; check the actual text.
        for (var c = row.FirstCellNum; c < row.LastCellNum; c++)
        {
            if (!string.IsNullOrWhiteSpace(CellText(row, c, formatter, evaluator))) return false;
        }

        return true;
    }

    /// <summary>Case/space/underscore/hyphen insensitive comparison key.</summary>
    private static string Normalise(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch == ' ' || ch == '_' || ch == '-' || ch == '\t') continue;
            sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }
}

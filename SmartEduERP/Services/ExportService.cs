using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartEduERP.Data.Models;
using SmartEduERP.Services;

namespace SmartEduERP.Services;

public class ExportService
{
    private readonly byte[]? _logoBytes;
    private const string LogoAbsolutePath = @"C:\\Users\\Dandy\\source\\repos\\SmartEduERP\\SmartEduERP - Copy 2\\SmartEduERP\\wwwroot\\images\\logo.png";

    public ExportService()
    {
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
        _logoBytes = TryLoadLogo();
    }

    #region Excel Exports

    public byte[] ExportStudentsToExcel(List<Student> students)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "First Name";
        worksheet.Cell(1, 3).Value = "Last Name";
        worksheet.Cell(1, 4).Value = "Email";
        worksheet.Cell(1, 5).Value = "Contact Number";
        worksheet.Cell(1, 6).Value = "Status";
        worksheet.Cell(1, 7).Value = "Date of Birth";
        worksheet.Cell(1, 8).Value = "Address";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data
        for (int i = 0; i < students.Count; i++)
        {
            var row = i + 2;
            worksheet.Cell(row, 1).Value = students[i].StudentId;
            worksheet.Cell(row, 2).Value = students[i].FirstName;
            worksheet.Cell(row, 3).Value = students[i].LastName;
            worksheet.Cell(row, 4).Value = students[i].Email;
            worksheet.Cell(row, 5).Value = students[i].ContactNumber;
            worksheet.Cell(row, 6).Value = students[i].EnrollmentStatus;
            worksheet.Cell(row, 7).Value = students[i].DateOfBirth?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 8).Value = students[i].Address;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportTeachersToExcel(List<Teacher> teachers)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Teachers");

        var generatedAt = DateTime.Now;
        worksheet.Cell(1, 1).Value = $"Generated: {generatedAt:yyyy-MM-dd HH:mm}";
        var generatedRange = worksheet.Range(1, 1, 1, 6);
        generatedRange.Merge();
        generatedRange.Style.Font.Italic = true;
        generatedRange.Style.Font.FontColor = XLColor.Gray;
        generatedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Cell(2, 1).Value = "ID";
        worksheet.Cell(2, 2).Value = "First Name";
        worksheet.Cell(2, 3).Value = "Last Name";
        worksheet.Cell(2, 4).Value = "Email";
        worksheet.Cell(2, 5).Value = "Department";
        worksheet.Cell(2, 6).Value = "Position";

        var headerRange = worksheet.Range(2, 1, 2, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

        for (int i = 0; i < teachers.Count; i++)
        {
            var row = i + 3;
            worksheet.Cell(row, 1).Value = teachers[i].TeacherId;
            worksheet.Cell(row, 2).Value = teachers[i].FirstName;
            worksheet.Cell(row, 3).Value = teachers[i].LastName;
            worksheet.Cell(row, 4).Value = teachers[i].Email;
            worksheet.Cell(row, 5).Value = teachers[i].Department;
            worksheet.Cell(row, 6).Value = teachers[i].Position;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportGradesToExcel(List<Grade> grades)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Grades");

        worksheet.Cell(1, 1).Value = "Grade ID";
        worksheet.Cell(1, 2).Value = "Student";
        worksheet.Cell(1, 3).Value = "Subject";
        worksheet.Cell(1, 4).Value = "Teacher";
        worksheet.Cell(1, 5).Value = "Grade";
        worksheet.Cell(1, 6).Value = "Remarks";

        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightYellow;

        for (int i = 0; i < grades.Count; i++)
        {
            var row = i + 2;
            worksheet.Cell(row, 1).Value = grades[i].GradeId;
            worksheet.Cell(row, 2).Value = $"{grades[i].Student?.FirstName} {grades[i].Student?.LastName}";
            worksheet.Cell(row, 3).Value = grades[i].Subject?.SubjectName;
            worksheet.Cell(row, 4).Value = $"{grades[i].Teacher?.FirstName} {grades[i].Teacher?.LastName}";
            worksheet.Cell(row, 5).Value = grades[i].GradeValue?.ToString() ?? "";
            worksheet.Cell(row, 6).Value = grades[i].Remarks;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportSubjectsToExcel(List<Subject> subjects)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Subjects");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Code";
        worksheet.Cell(1, 3).Value = "Name";
        worksheet.Cell(1, 4).Value = "Grade Level";
        worksheet.Cell(1, 5).Value = "Section";
        worksheet.Cell(1, 6).Value = "Teacher";
        worksheet.Cell(1, 7).Value = "Department";

        var header = worksheet.Range(1, 1, 1, 7);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < subjects.Count; i++)
        {
            var row = i + 2;
            var s = subjects[i];
            worksheet.Cell(row, 1).Value = s.SubjectId;
            worksheet.Cell(row, 2).Value = s.SubjectCode;
            worksheet.Cell(row, 3).Value = s.SubjectName;
            worksheet.Cell(row, 4).Value = s.GradeLevel;
            worksheet.Cell(row, 5).Value = s.Section;
            worksheet.Cell(row, 6).Value = s.Teacher != null ? $"{s.Teacher.FirstName} {s.Teacher.LastName}" : "Unassigned";
            worksheet.Cell(row, 7).Value = s.Teacher?.Department ?? "";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportPaymentsToExcel(List<Payment> payments)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payments");

        // Headers
        worksheet.Cell(1, 1).Value = "Payment ID";
        worksheet.Cell(1, 2).Value = "Student";
        worksheet.Cell(1, 3).Value = "Amount";
        worksheet.Cell(1, 4).Value = "Date";
        worksheet.Cell(1, 5).Value = "Method";
        worksheet.Cell(1, 6).Value = "Status";

        var header = worksheet.Range(1, 1, 1, 6);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightCyan;

        for (int i = 0; i < payments.Count; i++)
        {
            var row = i + 2;
            var p = payments[i];
            worksheet.Cell(row, 1).Value = p.PaymentId;
            worksheet.Cell(row, 2).Value = p.Student != null ? $"{p.Student.FirstName} {p.Student.LastName}" : "Unknown";
            worksheet.Cell(row, 3).Value = p.Amount;
            worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 4).Value = p.PaymentDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 5).Value = p.PaymentMethod;
            worksheet.Cell(row, 6).Value = p.PaymentStatus;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportTeacherIncomeToExcel(List<TeacherIncome> incomes, List<Allowance> allowances, List<Tax> taxes)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Teacher Income");

        var generatedAt = DateTime.Now;
        worksheet.Cell(1, 1).Value = $"Generated: {generatedAt:yyyy-MM-dd HH:mm}";
        var generatedRange = worksheet.Range(1, 1, 1, 12);
        generatedRange.Merge();
        generatedRange.Style.Font.Italic = true;
        generatedRange.Style.Font.FontColor = XLColor.Gray;
        generatedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Cell(2, 1).Value = "Teacher ID";
        worksheet.Cell(2, 2).Value = "Teacher Name";
        worksheet.Cell(2, 3).Value = "Period Start";
        worksheet.Cell(2, 4).Value = "Period End";
        worksheet.Cell(2, 5).Value = "Basic";
        worksheet.Cell(2, 6).Value = "Overtime";
        worksheet.Cell(2, 7).Value = "Other";
        worksheet.Cell(2, 8).Value = "Gross";
        worksheet.Cell(2, 9).Value = "Allowances";
        worksheet.Cell(2, 10).Value = "Taxable Base";
        worksheet.Cell(2, 11).Value = "Total Tax";
        worksheet.Cell(2, 12).Value = "Net";

        var headerRange = worksheet.Range(2, 1, 2, 12);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < incomes.Count; i++)
        {
            var income = incomes[i];
            var row = i + 3;

            var calc = CalculateNetIncome(income, allowances, taxes);

            worksheet.Cell(row, 1).Value = income.TeacherId;
            worksheet.Cell(row, 2).Value = income.TeacherName;
            worksheet.Cell(row, 3).Value = income.PeriodStartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 4).Value = income.PeriodEndDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 5).Value = income.BasicSalary;
            worksheet.Cell(row, 6).Value = income.OvertimePay;
            worksheet.Cell(row, 7).Value = income.OtherIncome;
            worksheet.Cell(row, 8).Value = calc.GrossIncome;
            worksheet.Cell(row, 9).Value = calc.TotalAllowances;
            worksheet.Cell(row, 10).Value = calc.TaxableBase;
            worksheet.Cell(row, 11).Value = calc.TotalTax;
            worksheet.Cell(row, 12).Value = calc.NetIncome;

            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 12).Style.NumberFormat.Format = "#,##0.00";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportTaxesToExcel(List<Tax> taxes)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Taxes");

        var generatedAt = DateTime.Now;
        worksheet.Cell(1, 1).Value = $"Generated: {generatedAt:yyyy-MM-dd HH:mm}";
        var generatedRange = worksheet.Range(1, 1, 1, 4);
        generatedRange.Merge();
        generatedRange.Style.Font.Italic = true;
        generatedRange.Style.Font.FontColor = XLColor.Gray;
        generatedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Cell(2, 1).Value = "Name";
        worksheet.Cell(2, 2).Value = "Description";
        worksheet.Cell(2, 3).Value = "Rate";
        worksheet.Cell(2, 4).Value = "Type";

        var headerRange = worksheet.Range(2, 1, 2, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < taxes.Count; i++)
        {
            var row = i + 3;
            worksheet.Cell(row, 1).Value = taxes[i].Name;
            worksheet.Cell(row, 2).Value = taxes[i].Description;
            worksheet.Cell(row, 3).Value = taxes[i].Rate;
            worksheet.Cell(row, 4).Value = taxes[i].IsPercentage ? "Percentage" : "Fixed";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportAllowancesToExcel(List<Allowance> allowances)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Allowances");

        var generatedAt = DateTime.Now;
        worksheet.Cell(1, 1).Value = $"Generated: {generatedAt:yyyy-MM-dd HH:mm}";
        var generatedRange = worksheet.Range(1, 1, 1, 5);
        generatedRange.Merge();
        generatedRange.Style.Font.Italic = true;
        generatedRange.Style.Font.FontColor = XLColor.Gray;
        generatedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Cell(2, 1).Value = "Name";
        worksheet.Cell(2, 2).Value = "Description";
        worksheet.Cell(2, 3).Value = "Amount";
        worksheet.Cell(2, 4).Value = "Type";
        worksheet.Cell(2, 5).Value = "Taxable";

        var headerRange = worksheet.Range(2, 1, 2, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < allowances.Count; i++)
        {
            var row = i + 3;
            worksheet.Cell(row, 1).Value = allowances[i].Name;
            worksheet.Cell(row, 2).Value = allowances[i].Description;
            worksheet.Cell(row, 3).Value = allowances[i].Amount;
            worksheet.Cell(row, 4).Value = allowances[i].IsPercentage ? "Percentage" : "Fixed";
            worksheet.Cell(row, 5).Value = allowances[i].IsTaxable ? "Yes" : "No";
            worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportPayrollAttendanceToExcel(List<HrService.PayrollAttendanceExportDto> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");

        var generatedAt = DateTime.Now;
        worksheet.Cell(1, 1).Value = $"Generated: {generatedAt:yyyy-MM-dd HH:mm}";
        var generatedRange = worksheet.Range(1, 1, 1, 8);
        generatedRange.Merge();
        generatedRange.Style.Font.Italic = true;
        generatedRange.Style.Font.FontColor = XLColor.Gray;
        generatedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Cell(2, 1).Value = "Employee ID";
        worksheet.Cell(2, 2).Value = "Employee";
        worksheet.Cell(2, 3).Value = "Period Start";
        worksheet.Cell(2, 4).Value = "Period End";
        worksheet.Cell(2, 5).Value = "Total Days";
        worksheet.Cell(2, 6).Value = "Present/Late";
        worksheet.Cell(2, 7).Value = "Absent";
        worksheet.Cell(2, 8).Value = "Minutes Late";

        var headerRange = worksheet.Range(2, 1, 2, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = i + 3;
            worksheet.Cell(row, 1).Value = rows[i].EmployeeId;
            worksheet.Cell(row, 2).Value = rows[i].EmployeeName;
            worksheet.Cell(row, 3).Value = rows[i].PeriodStart.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 4).Value = rows[i].PeriodEnd.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 5).Value = rows[i].TotalDays;
            worksheet.Cell(row, 6).Value = rows[i].PresentDays;
            worksheet.Cell(row, 7).Value = rows[i].AbsentDays;
            worksheet.Cell(row, 8).Value = rows[i].TotalMinutesLate;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region PDF Exports

    private byte[]? TryLoadLogo()
    {
        try
        {
            var candidates = new List<string>();
            var baseDir = AppContext.BaseDirectory;
            candidates.Add(Path.Combine(baseDir, "wwwroot", "images", "logo.png"));
            var currentDir = Environment.CurrentDirectory;
            candidates.Add(Path.Combine(currentDir, "wwwroot", "images", "logo.png"));
            if (OperatingSystem.IsWindows())
                candidates.Add(LogoAbsolutePath);
            foreach (var path in candidates.Distinct())
            {
                if (File.Exists(path))
                    return File.ReadAllBytes(path);
            }
        }
        catch { }
        return null;
    }

    public byte[] ExportStudentsToPDF(List<Student> students)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell =>
                        {
                            if (_logoBytes != null)
                                cell.Image(_logoBytes);
                        });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Student List Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        // spacing under header
                        column.Item().PaddingVertical(5);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Headers
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("ID").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("First Name").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Last Name").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Email").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Contact").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Status").Bold();
                            });

                            // Data
                            foreach (var student in students)
                            {
                                table.Cell().Element(CellStyle).AlignCenter().Text(student.StudentId.ToString());
                                table.Cell().Element(CellStyle).Text(student.FirstName);
                                table.Cell().Element(CellStyle).Text(student.LastName);
                                table.Cell().Element(CellStyle).Text(student.Email);
                                table.Cell().Element(CellStyle).Text(student.ContactNumber ?? "");
                                table.Cell().Element(CellStyle).Text(student.EnrollmentStatus ?? "");
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportGradesToPDF(List<Grade> grades)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .AlignCenter()
                    .Text("Grade Report")
                    .SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Red.Medium);

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10);
                        column.Item().PaddingVertical(5);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Red.Lighten2).Text("Student").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Red.Lighten2).Text("Subject").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Red.Lighten2).Text("Teacher").Bold();
                                header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Red.Lighten2).Text("Grade").Bold();
                            });

                            foreach (var grade in grades)
                            {
                                table.Cell().Element(CellStyle).Text($"{grade.Student?.FirstName} {grade.Student?.LastName}");
                                table.Cell().Element(CellStyle).Text(grade.Subject?.SubjectName ?? "");
                                table.Cell().Element(CellStyle).Text($"{grade.Teacher?.FirstName} {grade.Teacher?.LastName}");
                                table.Cell().Element(CellStyle).Text(grade.GradeValue.HasValue ? grade.GradeValue.Value.ToString() : "");
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportTeachersToPDF(List<Teacher> teachers)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Teacher List Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content()
                    .PaddingVertical(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("ID").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Email").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Department").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Position").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Registered").Bold();
                        });

                        foreach (var t in teachers)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().Text(t.TeacherId.ToString());
                            table.Cell().Element(CellStyle).Text($"{t.FirstName} {t.LastName}");
                            table.Cell().Element(CellStyle).Text(t.Email ?? "");
                            table.Cell().Element(CellStyle).Text(t.Department ?? "");
                            table.Cell().Element(CellStyle).Text(t.Position ?? "");
                            table.Cell().Element(CellStyle).AlignCenter().Text(t.RegistrationDate != default ? t.RegistrationDate.ToString("yyyy-MM-dd") : "");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportSubjectsToPDF(List<Subject> subjects)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Subject List Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content()
                    .PaddingVertical(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // Code
                            columns.RelativeColumn(2); // Name
                            columns.RelativeColumn(); // Grade
                            columns.RelativeColumn(); // Section
                            columns.RelativeColumn(2); // Teacher
                            columns.RelativeColumn(); // Dept
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Code").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Grade Level").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Section").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Teacher").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Department").Bold();
                        });

                        foreach (var s in subjects)
                        {
                            table.Cell().Element(CellStyle).Text(s.SubjectCode ?? "");
                            table.Cell().Element(CellStyle).Text(s.SubjectName ?? "");
                            table.Cell().Element(CellStyle).Text(s.GradeLevel ?? "");
                            table.Cell().Element(CellStyle).Text(s.Section ?? "");
                            table.Cell().Element(CellStyle).Text(s.Teacher != null ? $"{s.Teacher.FirstName} {s.Teacher.LastName}" : "Unassigned");
                            table.Cell().Element(CellStyle).Text(s.Teacher?.Department ?? "");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportPaymentsToPDF(List<Payment> payments)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Payment List Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content()
                    .PaddingVertical(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("ID").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Student").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Amount").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Date").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Method").Bold();
                            header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Status").Bold();
                        });

                        foreach (var p in payments)
                        {
                            var studentName = p.Student != null ? $"{p.Student.FirstName} {p.Student.LastName}" : "Unknown";
                            table.Cell().Element(CellStyle).AlignCenter().Text(p.PaymentId.ToString());
                            table.Cell().Element(CellStyle).Text(studentName);
                            table.Cell().Element(CellStyle).AlignRight().Text(p.Amount.ToString("N2"));
                            table.Cell().Element(CellStyle).AlignCenter().Text(p.PaymentDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(p.PaymentMethod ?? "");
                            table.Cell().Element(CellStyle).Text(p.PaymentStatus ?? "");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportTeacherIncomeToPDF(List<TeacherIncome> incomes, List<Allowance> allowances, List<Tax> taxes)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Teacher Income Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Teacher ID").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Teacher").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Start").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("End").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Gross").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Allow.").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Tax").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Net").Bold();
                    });

                    foreach (var income in incomes)
                    {
                        var calc = CalculateNetIncome(income, allowances, taxes);
                        table.Cell().Element(CellStyle).Text(income.TeacherId.ToString());
                        table.Cell().Element(CellStyle).Text(income.TeacherName);
                        table.Cell().Element(CellStyle).Text(income.PeriodStartDate.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(income.PeriodEndDate.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).AlignRight().Text(calc.GrossIncome.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(calc.TotalAllowances.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(calc.TotalTax.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(calc.NetIncome.ToString("N2"));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportTaxesToPDF(List<Tax> taxes)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Tax Configuration Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Name").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Description").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Rate").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Type").Bold();
                    });

                    foreach (var tax in taxes)
                    {
                        table.Cell().Element(CellStyle).Text(tax.Name);
                        table.Cell().Element(CellStyle).Text(tax.Description ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(tax.Rate.ToString("N2"));
                        table.Cell().Element(CellStyle).Text(tax.IsPercentage ? "Percentage" : "Fixed");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportAllowancesToPDF(List<Allowance> allowances)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Allowance Configuration Report").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Name").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Description").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Amount").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Type").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Taxable").Bold();
                    });

                    foreach (var allowance in allowances)
                    {
                        table.Cell().Element(CellStyle).Text(allowance.Name);
                        table.Cell().Element(CellStyle).Text(allowance.Description ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(allowance.Amount.ToString("N2"));
                        table.Cell().Element(CellStyle).Text(allowance.IsPercentage ? "Percentage" : "Fixed");
                        table.Cell().Element(CellStyle).Text(allowance.IsTaxable ? "Yes" : "No");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportPayrollAttendanceToPDF(List<HrService.PayrollAttendanceExportDto> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(70).Element(cell => { if (_logoBytes != null) cell.Image(_logoBytes); });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("SmartEduERP").SemiBold().FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                            c.Item().Text("Payroll Attendance Summary").Bold().FontSize(18).FontColor(QuestPDF.Helpers.Colors.Black);
                        });
                        row.ConstantItem(180).AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                    });
                    col.Item().Height(1).Background(QuestPDF.Helpers.Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Employee").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("Start").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text("End").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Total").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Present").Bold();
                        header.Cell().Element(CellStyle).Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight().Text("Late Min").Bold();
                    });

                    foreach (var row in rows)
                    {
                        table.Cell().Element(CellStyle).Text(row.EmployeeName);
                        table.Cell().Element(CellStyle).Text(row.PeriodStart.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(row.PeriodEnd.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).AlignRight().Text(row.TotalDays.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text(row.PresentDays.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text(row.TotalMinutesLate.ToString());
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    #endregion

    private static (decimal GrossIncome, decimal TotalAllowances, decimal TaxableBase, decimal TotalTax, decimal NetIncome) CalculateNetIncome(
        TeacherIncome income,
        IEnumerable<Allowance> allowances,
        IEnumerable<Tax> taxes)
    {
        var baseIncome = income.BasicSalary + income.OvertimePay + income.OtherIncome;

        decimal totalAllowances = 0m;
        decimal taxableAllowances = 0m;

        foreach (var allowance in allowances.Where(a => a.IsActive && !a.IsDeleted))
        {
            decimal value;
            if (allowance.IsPercentage)
            {
                value = Math.Round(baseIncome * (allowance.Amount / 100m), 2);
            }
            else
            {
                value = allowance.Amount;
            }

            totalAllowances += value;
            if (allowance.IsTaxable)
            {
                taxableAllowances += value;
            }
        }

        var taxableBase = baseIncome + taxableAllowances;

        decimal totalTax = 0m;
        foreach (var tax in taxes.Where(t => t.IsActive && !t.IsDeleted))
        {
            decimal value;
            if (tax.IsPercentage)
            {
                value = Math.Round(taxableBase * (tax.Rate / 100m), 2);
            }
            else
            {
                value = tax.Rate;
            }

            totalTax += value;
        }

        var netIncome = baseIncome + totalAllowances - totalTax;
        return (baseIncome, totalAllowances, taxableBase, totalTax, netIncome);
    }

    // Helper method for table styling
    private static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
            .PaddingVertical(5)
            .PaddingHorizontal(3);
    }
}

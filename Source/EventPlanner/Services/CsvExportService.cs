using System.Globalization;
using System.IO;
using System.Text;
using EventPlanner.Domain;

namespace EventPlanner.Services;

public static class CsvExportService
{
    public static void ExportRegistrations(string filePath, IEnumerable<Registration> registrations)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RegistrationId,EventId,PersonId,TicketType,Price,PaidAmount,Status,CheckedInAt");
        foreach (var r in registrations)
        {
            builder.AppendLine(string.Join(',',
                Escape(r.RegistrationId.ToString(CultureInfo.InvariantCulture)),
                Escape(r.EventId.ToString(CultureInfo.InvariantCulture)),
                Escape(r.PersonId.ToString(CultureInfo.InvariantCulture)),
                Escape(r.TicketType),
                Escape(r.Price.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(r.PaidAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(r.Status.ToString()),
                Escape(r.CheckedInAt?.ToString("s", CultureInfo.InvariantCulture) ?? "")));
        }
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    public static void ExportUnpaid(string filePath, IEnumerable<Registration> registrations)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RegistrationId,EventId,PersonId,TicketType,Price,PaidAmount,Outstanding,Status");
        var unpaid = registrations.Where(r => r.Status == PaymentStatus.Unpaid || r.Status == PaymentStatus.PartPaid);
        foreach (var r in unpaid)
        {
            var outstanding = r.Price - r.PaidAmount;
            builder.AppendLine(string.Join(',',
                Escape(r.RegistrationId.ToString(CultureInfo.InvariantCulture)),
                Escape(r.EventId.ToString(CultureInfo.InvariantCulture)),
                Escape(r.PersonId.ToString(CultureInfo.InvariantCulture)),
                Escape(r.TicketType),
                Escape(r.Price.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(r.PaidAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(outstanding.ToString("0.00", CultureInfo.InvariantCulture)),
                Escape(r.Status.ToString())));
        }
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    public static void ExportFinanceSummary(string filePath, Event ev,
                                            IEnumerable<Registration> registrations,
                                            IEnumerable<Expense> expenses)
    {
        var regList = registrations.ToList();
        var expList = expenses.ToList();
        var expected = FinanceService.ExpectedIncome(regList);
        var paid = FinanceService.PaidIncome(regList);
        var outstanding = FinanceService.Outstanding(regList);
        var totalExpenses = FinanceService.TotalExpenses(expList);
        var profit = FinanceService.Profit(regList, expList);

        var builder = new StringBuilder();
        builder.AppendLine("Metric,Value");
        builder.AppendLine($"Event,{Escape(ev.Title)}");
        builder.AppendLine($"ExpectedIncome,{Escape(expected.ToString("0.00", CultureInfo.InvariantCulture))}");
        builder.AppendLine($"PaidIncome,{Escape(paid.ToString("0.00", CultureInfo.InvariantCulture))}");
        builder.AppendLine($"Outstanding,{Escape(outstanding.ToString("0.00", CultureInfo.InvariantCulture))}");
        builder.AppendLine($"TotalExpenses,{Escape(totalExpenses.ToString("0.00", CultureInfo.InvariantCulture))}");
        builder.AppendLine($"Profit,{Escape(profit.ToString("0.00", CultureInfo.InvariantCulture))}");
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    private static string Escape(string value)
    {
        value = NeutralizeFormula(value);
        // RFC 4180: wrap in quotes if value contains a comma, quote or newline; double existing quotes.
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static string NeutralizeFormula(string value)
    {
        // CSV formula injection: a spreadsheet treats a cell starting with = + - @ (or a
        // leading tab/CR) as a formula, so an attacker-supplied value such as "=cmd|..."
        // could execute when the file is opened. Prefix such values with an apostrophe,
        // which forces the spreadsheet to treat the whole cell as literal text.
        if (value.Length > 0 && (value[0] is '=' or '+' or '-' or '@' or '\t' or '\r'))
            return "'" + value;
        return value;
    }
}

using EventPlanner.Domain;

namespace EventPlanner.Services;

public static class FinanceService
{
    public static decimal ExpectedIncome(IEnumerable<Registration> regs) =>
        regs.Where(r => r.Status != PaymentStatus.Cancelled).Sum(r => r.Price);

    public static decimal PaidIncome(IEnumerable<Registration> regs) =>
        regs.Where(r => r.Status != PaymentStatus.Cancelled).Sum(r => r.PaidAmount);

    public static decimal Outstanding(IEnumerable<Registration> regs) =>
        ExpectedIncome(regs) - PaidIncome(regs);

    public static decimal TotalExpenses(IEnumerable<Expense> expenses) =>
        expenses.Sum(e => e.Amount);

    public static decimal Profit(IEnumerable<Registration> regs, IEnumerable<Expense> expenses) =>
        PaidIncome(regs) - TotalExpenses(expenses);
}

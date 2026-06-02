# EventPlanner

EventPlanner is a desktop application for managing events, participants, registrations, payments, check-ins and event finances.

## Technology

- C#
- WPF
- SQLite
- Object-oriented programming
- Repository pattern
- MVVM with commands

## Main features

- Event management with full CRUD
- People management (attendees and speakers)
- Participant registration with capacity check
- Support for free events and zero payments
- Payment statuses: Free, Unpaid, PartPaid, Paid, Cancelled
- Check-in page used during the event
- Finance page with expected income, paid income, outstanding, expenses and profit
- Expense management for each event
- Reports page with three CSV exports

## How to run

1. Open `EventPlanner.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. Build the project.
4. Run the application.

The SQLite database file is created automatically on first start. Demo data is seeded if the database is empty.

## Project structure

```
EventPlanner/
  Domain/         entities, enums, validation interface
  Data/           SQLite connection and schema
  Repositories/   data access, one repository per aggregate
  Services/       finance, registration and CSV export
  ViewModels/     MVVM view models
  Views/          XAML pages and dialog windows
  Commands/       RelayCommand for ICommand bindings
```

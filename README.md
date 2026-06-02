<p align="center">
  <img src="Source/EventPlanner/logo.png" width="110" alt="EventPlanner logo">
</p>

<h1 align="center">EventPlanner</h1>

<p align="center">A single-user desktop application for managing events, participants, registrations, payments, check-in and finances.</p>

---

EventPlanner is a Windows desktop application built as a course work for the
**Object-Oriented Programming** course at the Transport and Telecommunication
Institute (TSI). It covers the full lifecycle of an event: planning, managing
attendees and speakers, registering participants with payment tracking,
checking attendees in during the event, recording expenses and reporting on
the finances.

## Features

- Event management with full create / edit / archive / delete and validation
- People management for **attendees** and **speakers** (with deactivate / reactivate)
- Participant registration with **capacity** and **duplicate-registration** checks
- Free events and zero-amount payments; five payment statuses (Free, Unpaid, PartPaid, Paid, Cancelled)
- One-click **check-in** during the event
- Live **finance** summary: expected income, paid income, outstanding, expenses and profit
- Per-event **expense** management
- Three **CSV exports**: attendee list, unpaid list and finance summary (RFC 4180, with formula-injection protection)

## Technology

| Component | Choice |
|-----------|--------|
| Language | C# 12 |
| Framework | .NET 8 (`net8.0-windows`) |
| UI | WPF with XAML |
| Architecture | MVVM with the Repository pattern |
| Database | SQLite (file-based, single-user) |
| Data access | Microsoft.Data.Sqlite 8.0.5 |
| Testing | NUnit 3 |

## Architecture

The application is organised in layers, each depending only on the layers
below it: **Domain** (entities, enums, `IValidatable`) → **Data** (SQLite
connection and schema) → **Repositories** (`IRepository<T>`) → **Services**
(Finance, Registration, CSV export) → **ViewModels** (MVVM) → **Views** (WPF).

## Getting started

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) on Windows.

```bash
cd Source/EventPlanner
dotnet run
```

To produce a portable, self-contained executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The SQLite database (`eventplanner.db`) is created automatically next to the
executable on first run, and demo data is seeded if it is empty.

## Project structure

```
Source/EventPlanner/
  Domain/         entities, enums, IValidatable
  Data/           SQLite connection and schema
  Repositories/   data access (one repository per aggregate)
  Services/       Finance, Registration and CSV export
  ViewModels/     MainViewModel, row view models, RelayCommand
  Views/          XAML pages and dialog windows
```

## License

Provided as academic course work.

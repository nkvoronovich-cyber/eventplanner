using System.Collections.ObjectModel;
using EventPlanner.Commands;
using EventPlanner.Data;
using EventPlanner.Domain;
using EventPlanner.Repositories;
using EventPlanner.Services;

namespace EventPlanner.ViewModels;

public enum AppPage { Events, Registrations, People, CheckIn, Finance, Reports, EventDetails }

public class MainViewModel : ViewModelBase
{
    private readonly EventRepository _events = new();
    private readonly PersonRepository _people = new();
    private readonly RegistrationRepository _registrations = new();
    private readonly ExpenseRepository _expenses = new();

    public RegistrationService RegistrationService { get; }

    public ObservableCollection<Event> Events { get; } = new();
    public ObservableCollection<EventRowViewModel> EventRows { get; } = new();
    public ObservableCollection<Person> People { get; } = new();
    public ObservableCollection<Registration> Registrations { get; } = new();
    public ObservableCollection<RegistrationRowViewModel> RegistrationRows { get; } = new();
    public ObservableCollection<Expense> Expenses { get; } = new();

    public EventRepository EventRepo => _events;
    public PersonRepository PersonRepo => _people;
    public RegistrationRepository RegistrationRepo => _registrations;
    public ExpenseRepository ExpenseRepo => _expenses;

    public decimal ExpectedIncome => FinanceService.ExpectedIncome(Registrations);
    public decimal PaidIncome => FinanceService.PaidIncome(Registrations);
    public decimal Outstanding => FinanceService.Outstanding(Registrations);
    public decimal TotalExpenses => FinanceService.TotalExpenses(Expenses);
    public decimal Profit => FinanceService.Profit(Registrations, Expenses);

    private AppPage _currentPage = AppPage.Events;
    public AppPage CurrentPage
    {
        get => _currentPage;
        set => SetField(ref _currentPage, value);
    }

    private Event? _selectedEvent;
    public Event? SelectedEvent
    {
        get => _selectedEvent;
        set => SetField(ref _selectedEvent, value);
    }

    public void OpenEventDetails(Event ev)
    {
        SelectedEvent = ev;
        CurrentPage = AppPage.EventDetails;
    }

    public RelayCommand NavigateCommand { get; }

    public MainViewModel()
    {
        RegistrationService = new RegistrationService(_registrations);

        Database.Initialize();
        SeedDataIfEmpty();
        LoadData();

        NavigateCommand = new RelayCommand(target =>
        {
            if (target is AppPage page) CurrentPage = page;
            else if (target is string name && Enum.TryParse<AppPage>(name, out var parsed)) CurrentPage = parsed;
        });
    }

    public void LoadData()
    {
        Events.Clear();
        EventRows.Clear();
        People.Clear();
        Registrations.Clear();
        RegistrationRows.Clear();
        Expenses.Clear();

        var allRegistrations = _registrations.GetAll();
        var allPeople = _people.GetAll();

        foreach (var e in _events.GetAll())
        {
            Events.Add(e);
            EventRows.Add(new EventRowViewModel(e, allRegistrations));
        }
        foreach (var p in allPeople) People.Add(p);
        foreach (var r in allRegistrations)
        {
            Registrations.Add(r);
            var eventTitle = Events.FirstOrDefault(e => e.EventId == r.EventId)?.Title ?? "(unknown)";
            var personName = allPeople.FirstOrDefault(p => p.PersonId == r.PersonId)?.FullName ?? "(unknown)";
            RegistrationRows.Add(new RegistrationRowViewModel(r, eventTitle, personName));
        }
        foreach (var x in _expenses.GetAll()) Expenses.Add(x);

        RefreshFinanceCards();
    }

    public void RefreshFinanceCards()
    {
        OnPropertyChanged(nameof(ExpectedIncome));
        OnPropertyChanged(nameof(PaidIncome));
        OnPropertyChanged(nameof(Outstanding));
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(Profit));
    }

    private void SeedDataIfEmpty()
    {
        if (_events.GetAll().Any()) return;

        var eventId = _events.Add(new Event
        {
            Title = "Beauty Business Day",
            StartAt = DateTime.Now.AddDays(14),
            Location = "Riga",
            Capacity = 60,
            Status = EventStatus.Open,
            Description = "Training and conference event."
        });

        var attendeeId = _people.Add(new Attendee
        {
            FullName = "Anna Petrova",
            Phone = "+371 23456789",
            Email = "anna@example.com",
            Notes = "Repeat client"
        });

        var speakerId = _people.Add(new Speaker
        {
            FullName = "Linda Ozola",
            Phone = "+371 26789123",
            Email = "linda@example.com",
            Topic = "Brand storytelling",
            Bio = "Marketing consultant with 10 years experience."
        });

        var paid = new Registration { EventId = eventId, PersonId = attendeeId, TicketType = "Standard" };
        paid.UpdatePayment(90m, 90m);
        _registrations.Add(paid);

        var free = new Registration { EventId = eventId, PersonId = speakerId, TicketType = "Speaker" };
        free.UpdatePayment(0m, 0m);
        _registrations.Add(free);

        _expenses.Add(new Expense { EventId = eventId, Category = "Room rent", Amount = 350m, Notes = "Main hall" });
        _expenses.Add(new Expense { EventId = eventId, Category = "Catering", Amount = 180m });
    }
}

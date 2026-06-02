using System.Windows;
using System.Windows.Controls;
using EventPlanner.Domain;

namespace EventPlanner.Views;

public partial class PersonFormWindow : Window
{
    public Person? Result { get; private set; }
    private readonly int _existingId;
    private readonly bool _isEdit;

    public PersonFormWindow()
    {
        InitializeComponent();
        RoleBox.Items.Add(PersonRole.Attendee);
        RoleBox.Items.Add(PersonRole.Speaker);
        RoleBox.SelectedItem = PersonRole.Attendee;
        _existingId = 0;
        _isEdit = false;
    }

    public PersonFormWindow(Person existing) : this()
    {
        _existingId = existing.PersonId;
        _isEdit = true;
        NameBox.Text = existing.FullName;
        PhoneBox.Text = existing.Phone;
        EmailBox.Text = existing.Email;
        ActiveCheck.IsChecked = existing.IsActive;
        RoleBox.SelectedItem = existing.Role;
        if (existing is Attendee a) NotesBox.Text = a.Notes;
        if (existing is Speaker s) { TopicBox.Text = s.Topic; BioBox.Text = s.Bio; }
    }

    private void RoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoleBox.SelectedItem is not PersonRole role) return;
        var isSpeaker = role == PersonRole.Speaker;
        SpeakerFields.Visibility = isSpeaker ? Visibility.Visible : Visibility.Collapsed;
        AttendeeFields.Visibility = isSpeaker ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var role = RoleBox.SelectedItem is PersonRole r ? r : PersonRole.Attendee;
        Person person = role == PersonRole.Speaker
            ? new Speaker { Topic = TopicBox.Text.Trim(), Bio = BioBox.Text.Trim() }
            : new Attendee { Notes = NotesBox.Text.Trim() };

        person.PersonId = _isEdit ? _existingId : 0;
        person.FullName = NameBox.Text.Trim();
        person.Phone = PhoneBox.Text.Trim();
        person.Email = EmailBox.Text.Trim();
        person.IsActive = ActiveCheck.IsChecked == true;

        var errors = person.Validate();
        if (errors.Any())
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Please fix the following", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = person;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

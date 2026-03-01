using KapyTask.Database;
using KapyTask.Database.Tables;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KapyTask.Views;

public partial class DisciplineEditModal : ContentPage, INotifyPropertyChanged
{
    private KapyTaskDatabase db;
    public ObservableCollection<KDiscipline> Disciplines { get; set; } = new();
    
    private string _newDisciplineName;
    public string NewDiscipline_Name
    {
        get => _newDisciplineName;
        set
        {
            _newDisciplineName = value;
            OnPropertyChanged();
        }
    }
    
    public DisciplineEditModal()
    {
        InitializeComponent();
        db = new();
        BindingContext = this;
        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDisciplines();
    }

    private async Task LoadDisciplines()
    {
        var disciplines = await db.Disciplines.GetDisciplines();
        Disciplines.Clear();
        foreach (var discipline in disciplines)
        {
            Disciplines.Add(discipline);
        }
        DisciplinenCountLabel.Text = $"Всего: {Disciplines.Count.ToString()}";
    }

    private async void ButtonNewDiscipline_OnClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewDiscipline_Name))
            return;
        
        // Check if discipline already exists
        if (Disciplines.Any(d => d.Name.Equals(NewDiscipline_Name, StringComparison.OrdinalIgnoreCase)))
            return;
        
        await db.InsertDiscipline(new KDiscipline { Name = NewDiscipline_Name });
        
        // Clear entry and hide it
        EntryNameNewDiscipline.IsVisible = false;
        NewDisciplineAddedStatusLabel.IsVisible = true;
        
        await LoadDisciplines();
        
        // Reset entry for next use
        NewDiscipline_Name = string.Empty;
        EntryNameNewDiscipline.IsVisible = true;
        NewDisciplineAddedStatusLabel.IsVisible = false;
    }
    async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    
    public new event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void ButtonDeleteItem_OnClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var discipline = button?.BindingContext as KDiscipline;
        
        if (discipline != null)
        {
            bool confirm = await DisplayAlert("Подтверждение", 
                $"Удалить дисциплину '{discipline.Name}'?", "Да", "Нет");
                
            if (confirm)
            {
                await db.Disciplines.DeleteDiscipline(discipline);
                await LoadDisciplines();
            }
        }
    }
}
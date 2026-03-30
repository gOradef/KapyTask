using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using KapyTask.Database;
using KapyTask.Database.Tables;

namespace KapyTask.Views.Modals;

/// <summary>
/// Modal for editing discipline 
/// </summary>
public partial class DisciplineEditModal : ContentPage, INotifyPropertyChanged
{
    private readonly KapyTaskDatabase _db;
    private KDiscipline _discipline;
    private DisciplineColor _selectedColor;
    
    public KDiscipline Discipline
    {
        get => _discipline;
        set
        {
            _discipline = value;
            OnPropertyChanged();
        }
    }

    public List<DisciplineColor> DisciplineColors { get; private set; }
    
    public DisciplineColor SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor != value)
            {
                _selectedColor = value;
                OnPropertyChanged();
                
                // Обновляем цвет дисциплины при выборе
                if (Discipline != null && _selectedColor != null)
                {
                    Discipline.Color = _selectedColor.Color;
                }
            }
        }
    }

    public DisciplineEditModal(KDiscipline givenDiscipline)
    {
        InitializeComponent();
        _db = new KapyTaskDatabase();
        Discipline = givenDiscipline;
        
        SetDisciplineColors();
        
        // Устанавливаем выбранный цвет на основе текущего цвета дисциплины
        if (Discipline.ColorHexValue != null)
        {
            SelectedColor = DisciplineColors.FirstOrDefault(c => c.Color.ToHex() == Discipline.ColorHexValue);
        }
        
        BindingContext = this;
    }

    private void SetDisciplineColors()
    {
        var colorItems = new List<DisciplineColor>();
        
        // Ищем словарь DisciplineColors
        var disciplineDict = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString?.Contains("DisciplineColors") == true);
        
        if (disciplineDict != null)
        {
            foreach (var key in disciplineDict.Keys)
            {
                if (disciplineDict[key] is Color color && key is string keyName)
                {
                    colorItems.Add(new DisciplineColor
                    {
                        Name = keyName,
                        Color = color
                    });
                }
            }
        }
        else
        {
            // Fallback, если словарь не найден
            Debug.WriteLine("DisciplineColors dictionary not found!");
            colorItems.Add(new DisciplineColor { Name = "Серый", Color = Colors.Gray });
        }

        DisciplineColors = colorItems;
    }

    public class DisciplineColor
    {
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; } = Colors.White;
    }

    private async void ButtonClose_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void ButtonConfirm_OnClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Discipline.Name))
        {
            await DisplayAlert("Ошибка", "Введите название дисциплины", "OK");
            return;
        }
        
        var confirm = await DisplayAlert("Подтверждение",
            "Обновить дисциплину?",
            "Да", "Нет");
            
        if (confirm)
        {
            await _db.Disciplines.UpdateDiscipline(Discipline);
            await Navigation.PopModalAsync();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
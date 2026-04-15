using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using KapyTask.Database;
using KapyTask.Database.Tables;
using KapyTask.Views.Modals;

namespace KapyTask.Views.Pages;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private KapyTaskDatabase db;
    private bool _isUpdating = false;
    private CancellationTokenSource _updateCts;
    
    public bool IsUpdating
    {
        get => _isUpdating;
        set
        {
            _isUpdating = value;
            OnPropertyChanged();
        }
    }

    private bool _isTasksLoaded = false;
    public bool IsTasksLoaded
    {
        get => !_isTasksLoaded;
        set
        {
            _isTasksLoaded = value;
            OnPropertyChanged();
        }
    }

    // Use ObservableCollection for better UI performance
    private ObservableCollection<KTask> _displayedKTasks = new();
    public ObservableCollection<KTask> DisplayedKTasks
    {
        get => _displayedKTasks;
        set
        {
            _displayedKTasks = value;
            OnPropertyChanged();
        }
    }

    // Archived tasks


    private bool _isLoadingArchivedTasks = false;
    public bool IsLoadingArchivedTasks { 
        get => _isLoadingArchivedTasks;
        set
        {
            _isLoadingArchivedTasks = value;
            OnPropertyChanged();
        }
    }
    // Use ObservableCollection for better UI performance
    private List<ArchivedKTask> _archivedKTasks = new();
    public List<ArchivedKTask> ArchivedKTasks
    {
        get => _archivedKTasks;
        set
        {
            _archivedKTasks = value;
            OnPropertyChanged();
        }
    }

    public MainPage(KapyTaskDatabase db)
    {
        InitializeComponent();
        this.db = db;
        BindingContext = this;
        IsTasksLoaded = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = UpdateTasksAsync();
    }

    protected override void OnDisappearing()
    {
        // Cancel any ongoing updates when page disappears
        _updateCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task UpdateTasksAsync()
    {
        // Cancel previous operation if any
        _updateCts?.Cancel();
        _updateCts = new CancellationTokenSource();
        
        if (_isUpdating)
            return;

        try
        {
            IsUpdating = true;
            
            // Optimize: Get tasks with discipline property efficiently
            var tasks = await db.Tasks.GetTasksWithDisciplineProperty();
            
            // Check if cancelled
            if (_updateCts.Token.IsCancellationRequested)
                return;
            
            // Optimize sorting - use more efficient approach
            var sortedTasks = tasks
                .OrderBy(a => a.Deadline.HasValue ? 0 : 1) // Tasks with deadline first
                .ThenBy(a => a.Deadline)
                .ToList();
            
            // Update UI efficiently - batch updates
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Clear and add in batches to reduce UI refreshes
                DisplayedKTasks.Clear();
                
                foreach (var task in sortedTasks)
                {
                    if (_updateCts.Token.IsCancellationRequested)
                        return;
                    DisplayedKTasks.Add(task);
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Expected, ignore
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating tasks: {e.Message}");
            await DisplayAlertAsync("Ошибка", "Не удалось обновить задачи", "Ok");
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsTasksLoaded = true;
                IsUpdating = false;
            });
        }
    }

    private void ButtonCreateTask_Clicked(object? sender, EventArgs e)
    {
        Navigation.PushModalAsync(new TaskEditModal());
    }

    private async void ListViewTasks_OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        var selectedTask = e.CurrentSelection.FirstOrDefault() as KTask;
        if (selectedTask is not null)
            await Navigation.PushModalAsync(new TaskEditModal(selectedTask));
    }

    private async void ButtonArchiveItem_OnClicked(object? sender, EventArgs e)
    {
        var checkBox = sender as CheckBox;
        if (checkBox == null) return;
        
        var task = checkBox.BindingContext as KTask;
        if (task == null) return;
        
        if (await DisplayAlertAsync("Потверждение", $"Точно архивировать '{task.Name}'?", "Да", "Нет"))
        {
            try
            {
                await db.ArchivedTasks.ArchiveTask(task);
                // Remove from UI immediately instead of full refresh
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DisplayedKTasks.Remove(task);
                });
                await Toast.Make($"Задача '{task.Name}' перенесена в архив").Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting task: {ex.Message}");
            }
        }
        else
        {
            checkBox.IsChecked = false;
        }
    }

    // Dont believe to gray highlighting! Using as 'Command' in .xaml
    private Command _buttonToggleArchiveList;
    public Command ButtonToggleArchiveList => _buttonToggleArchiveList ??= new Command(async () =>
    {
        if (!ArchiveTasksVsl.IsVisible)
        {
            // Show the container first (but invisible)
            ArchiveTasksVsl.IsVisible = true;
            ArchiveTasksVsl.Opacity = 0;
            ArchiveTasksVsl.Scale = 0.95;

            IsLoadingArchivedTasks = true;
            ArchivedKTasks = await db.ArchivedTasks.GetArchivedTasksWithDisciplineProperty();
            IsLoadingArchivedTasks = false;

            // Fancy entrance animation
            await ArchiveTasksVsl.FadeToAsync(1, 250, Easing.CubicOut);
            await ArchiveTasksVsl.ScaleToAsync(1, 250, Easing.SpringOut);
        }
        else
        {
            // Exit animation
            var fadeTask = ArchiveTasksVsl.FadeToAsync(0, 200, Easing.CubicIn);
            var scaleTask = ArchiveTasksVsl.ScaleToAsync(0.95, 200, Easing.CubicIn);
            await Task.WhenAll(fadeTask, scaleTask);

            ArchiveTasksVsl.IsVisible = false;
            ArchiveTasksVsl.Opacity = 1; // Reset for next time
            ArchiveTasksVsl.Scale = 1;
        }
    });

    private async void ButtonClearArchiveTasksList_OnClicked(object? sender, EventArgs e)
    {
        if (await DisplayAlertAsync("Подтверждение", "Точно очистить ВЕСЬ архив?", "Да", "Нет"))
        {
            var deleted = await db.ArchivedTasks.ClearWholeArchive();
            await Toast.Make($"Удалено из архива: {deleted}", ToastDuration.Short).Show();
        }
    }
}
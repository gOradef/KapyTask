using KapyTask.Database;
using KapyTask.Database.Tables;
using KapyTask.Views.Modals;
using KapyTask.Views.Pages;
using System.Text.Json;

namespace KapyTask.Views;

public partial class SettingsPage : ContentPage
{
    private KapyTaskDatabase db;
    
    public SettingsPage(KapyTaskDatabase db)
    {
        InitializeComponent();
        this.db = db;
    }
    
    private async void ButtonEditDisciplines_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DisciplineListEditPage());
    }

    private async void ButtonEditSchedule_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ScheduleListEditPage());
    }

    private async void ButtonOpenDevPage_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutPage());
    }

    private async void ButtonImportDialog_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            // Подтверждение от пользователя
            var confirm = await DisplayAlert("Импорт данных", 
                "⚠️ ВНИМАНИЕ: При импорте существующие данные будут удалены!\n\n" +
                "Рекомендуется сделать экспорт перед импортом.\n\n" +
                "Продолжить?", 
                "Да, импортировать", "Отмена");

            if (!confirm)
                return;

            // Показываем индикатор загрузки
            await ShowLoadingIndicator(true);

            // Выбираем файл для импорта
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите файл с резервной копией",
                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/json", "*.json" } },
                        { DevicePlatform.iOS, new[] { "public.json" } },
                        { DevicePlatform.WinUI, new[] { ".json" } },
                        { DevicePlatform.macOS, new[] { "json" } }
                    })
            });

            if (result != null)
            {
                // Читаем JSON из файла
                var jsonContent = await File.ReadAllTextAsync(result.FullPath);
                
                // Импортируем данные
                await db.Config.ImportDisciplinesAndScheduleSimpleAsync(jsonContent);
                
                await DisplayAlert("Успех", "✅ Данные успешно импортированы!", "OK");
                
                // Отправляем уведомление об обновлении данных
                MessagingCenter.Send(this, "DataImported");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"❌ Не удалось импортировать данные:\n{ex.Message}", "OK");
        }
        finally
        {
            await ShowLoadingIndicator(false);
        }
    }

    private async void ButtonExportToBuffer_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            // Показываем индикатор загрузки
            await ShowLoadingIndicator(true);
            
            // Экспортируем данные
            var jsonData = await db.Config.ExportDisciplinesAndScheduleAsync();
            
            // Создаем имя файла
            var fileName = $"backup_disciplines_schedule_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            
            // Сохраняем во временную папку
            var tempFile = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(tempFile, jsonData);
            
            // Открываем диалог сохранения через Share
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Сохранить резервную копию",
                File = new ShareFile(tempFile, "application/json")
            });
            
            await DisplayAlert("Успех", "✅ Данные готовы к экспорту", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"❌ Не удалось экспортировать данные:\n{ex.Message}", "OK");
        }
        finally
        {
            await ShowLoadingIndicator(false);
        }
    }

    // Вспомогательный метод для показа индикатора загрузки
    private async Task ShowLoadingIndicator(bool show)
    {
        // Показываем/скрываем ActivityIndicator
        LoadingIndicator.IsRunning = show;
        LoadingIndicator.IsVisible = show;
        
        // Отключаем/включаем кнопки во время операции
        ButtonExportToBuffer.IsEnabled = !show;
        ButtonImportDialog.IsEnabled = !show;
        
        await Task.CompletedTask;
    }
}
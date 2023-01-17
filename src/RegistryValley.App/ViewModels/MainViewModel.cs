using RegistryValley.App.Services;
using RegistryValley.App.Models;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
        }

        #region Fields and Properties
        #endregion

        public void DeleteSelectedKey(KeyItem key)
        {
            var result = ShellServices.RunPowershellCommand(runAs: true, @$"-command Remove-Item -Path '{key.PathForPwsh}' -Recurse");

            key.Parent.Children.Remove(key);

            if (key.Parent.Children.Count == 0)
                key.Parent.HasChildren = false;
        }

        public void RenameSelectedKey(KeyItem key, string renamingKey)
        {
            key.IsRenaming = false;
            key.Name = renamingKey;

            var pathItems = key.PathForPwsh.Split('\\').ToList();
            pathItems.RemoveAt(pathItems.Count - 1);
            var parentPath = string.Join('\\', pathItems);

            var command = @$"-command if (!(Test-Path '{key.PathForPwsh}')) {{ New-Item -Path '{parentPath}' -Name '{key.Name}' -Force }} else {{ Rename-Item '{key.PathForPwsh}' -NewName '{renamingKey}' }}";
            var result = ShellServices.RunPowershellCommand(runAs: true, command);
        }

        public async Task ExportSelectedKeyTree(KeyItem key)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.FileTypeChoices.Add("Registration Entries", new List<string>() { ".reg" });

            InitializeWithWindow.Initialize(picker, App.WindowHandle);

            var file = await picker.PickSaveFileAsync();

            if (file != null)
                ShellServices.RunCmdPromptCommand(runAs: true, $"/c REG EXPORT '{key.PathForCmd}' '{file.Path}'");
        }
    }
}

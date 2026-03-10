using Notepad__.Helpers;
using Notepad__.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Notepad__.ViewModels
{
    public class FileSystemViewModel : BaseViewModel
    {
        public ObservableCollection<FileSystemItem> Drives { get; }

        private FileSystemItem _selectedItem;
        public FileSystemItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private string _copiedFolderPath = null;

        public event Action<string> OpenFileRequested;

        public ICommand NewFileInFolderCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand CopyFolderCommand { get; }
        public ICommand PasteFolderCommand { get; }

        public FileSystemViewModel()
        {
            Drives = new ObservableCollection<FileSystemItem>();
            foreach (var drive in DriveInfo.GetDrives())
                Drives.Add(new FileSystemItem(drive.Name));

            NewFileInFolderCommand = new RelayCommand(
                _ => NewFileInFolder(),
                _ => SelectedItem?.IsDirectory == true);

            CopyPathCommand = new RelayCommand(
                _ => CopyPath(),
                _ => SelectedItem?.IsDirectory == true);

            CopyFolderCommand = new RelayCommand(
                _ => CopyFolder(),
                _ => SelectedItem?.IsDirectory == true);
            
            PasteFolderCommand = new RelayCommand(
                _ => PasteFolder(),
                _ => SelectedItem?.IsDirectory == true && _copiedFolderPath != null);
        }

        public void OnItemDoubleClicked(FileSystemItem item)
        {
            if (item == null || item.IsDirectory) return;
            OpenFileRequested?.Invoke(item.FullPath);
        }

        private void NewFileInFolder()
        {
            var dialog = new Views.InputDialog("Enter file name (with extension):", "new_file.txt");
            if (dialog.ShowDialog() != true) return;

            string newPath = Path.Combine(SelectedItem.FullPath, dialog.ResponseText);
            try
            {
                File.WriteAllText(newPath, string.Empty);
                SelectedItem.Refresh();
                SelectedItem.IsExpanded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not create file: {ex.Message}", "Error");
            }
        }

        private void CopyPath()
        {
            Clipboard.SetText(SelectedItem.FullPath);
        }

        private void CopyFolder()
        {
            _copiedFolderPath = SelectedItem.FullPath;
            CommandManager.InvalidateRequerySuggested();
        }

        private void PasteFolder()
        {
            try
            {
                string folderName = Path.GetFileName(_copiedFolderPath);
                string destination = Path.Combine(SelectedItem.FullPath, folderName);

                CopyDirectoryRecursive(_copiedFolderPath, destination);

                SelectedItem.Refresh();
                SelectedItem.IsExpanded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not paste folder: {ex.Message}", "Error");
            }
        }

        private void CopyDirectoryRecursive(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);

            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectoryRecursive(dir, Path.Combine(destination, Path.GetFileName(dir)));
        }
    }
}
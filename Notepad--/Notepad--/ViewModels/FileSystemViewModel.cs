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

        // Retinem calea folderului copiat (null daca nu s-a copiat nimic)
        // Paste va fi inactiv cat timp aceasta e null
        private string _copiedFolderPath = null;

        public event Action<string> OpenFileRequested;

        // Comenzile pentru meniul contextual
        public ICommand NewFileInFolderCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand CopyFolderCommand { get; }
        public ICommand PasteFolderCommand { get; }

        public FileSystemViewModel()
        {
            Drives = new ObservableCollection<FileSystemItem>();
            foreach (var drive in DriveInfo.GetDrives())
                Drives.Add(new FileSystemItem(drive.Name));

            // Al doilea parametru al RelayCommand este "CanExecute"
            // adica conditia in care comanda e activa
            NewFileInFolderCommand = new RelayCommand(
                _ => NewFileInFolder(),
                _ => SelectedItem?.IsDirectory == true);

            CopyPathCommand = new RelayCommand(
                _ => CopyPath(),
                _ => SelectedItem?.IsDirectory == true);

            CopyFolderCommand = new RelayCommand(
                _ => CopyFolder(),
                _ => SelectedItem?.IsDirectory == true);

            // Paste e activ doar daca avem un folder copiat SI un director selectat
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
            // Deschidem un dialog simplu ca utilizatorul sa scrie numele fisierului
            var dialog = new Views.InputDialog("Enter file name (with extension):", "new_file.txt");
            if (dialog.ShowDialog() != true) return;

            string newPath = Path.Combine(SelectedItem.FullPath, dialog.ResponseText);
            try
            {
                File.WriteAllText(newPath, string.Empty);
                // Reimprospatam directorul ca sa apara noul fisier
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
            // Anuntam WPF sa reevalueze CanExecute pentru toate comenzile
            // (ca sa activeze butonul Paste)
            CommandManager.InvalidateRequerySuggested();
        }

        private void PasteFolder()
        {
            try
            {
                // Numele folderului copiat (ex: "Documents")
                string folderName = Path.GetFileName(_copiedFolderPath);
                // Destinatia (ex: "D:\Backup\Documents")
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

        // Copiaza recursiv un director cu tot continutul sau
        private void CopyDirectoryRecursive(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            // Copiem intai fisierele din directorul curent
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);

            // Apoi recursiv fiecare subdirector
            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectoryRecursive(dir, Path.Combine(destination, Path.GetFileName(dir)));
        }
    }
}
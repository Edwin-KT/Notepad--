using Notepad__.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;

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

        // Event prin care anuntam MainViewModel sa deschida un fisier
        public event Action<string> OpenFileRequested;

        public FileSystemViewModel()
        {
            Drives = new ObservableCollection<FileSystemItem>();
            foreach (var drive in DriveInfo.GetDrives())
                Drives.Add(new FileSystemItem(drive.Name));
        }

        public void OnItemDoubleClicked(FileSystemItem item)
        {
            if (item == null || item.IsDirectory) return;
            OpenFileRequested?.Invoke(item.FullPath);
        }
    }
}
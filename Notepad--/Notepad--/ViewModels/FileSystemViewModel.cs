using Notepad__.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Notepad__.ViewModels
{
    public class FileSystemViewModel : INotifyPropertyChanged
    {
        // Radacinile arborelui = partitiile de pe calculator (C:\, D:\, etc.)
        public ObservableCollection<FileSystemItem> Drives { get; }

        private FileSystemItem _selectedItem;
        public FileSystemItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        // Event prin care anuntam MainViewModel sa deschida un fisier
        // Il vom folosi la subpunctul 5 (dublu click)
        public event Action<string> OpenFileRequested;

        public FileSystemViewModel()
        {
            Drives = new ObservableCollection<FileSystemItem>();
            foreach (var drive in DriveInfo.GetDrives())
                Drives.Add(new FileSystemItem(drive.Name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
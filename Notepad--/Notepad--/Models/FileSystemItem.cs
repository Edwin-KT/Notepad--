using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Notepad__.Models
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        public string Name { get; }
        public string FullPath { get; }
        public bool IsDirectory { get; }

        public ObservableCollection<FileSystemItem> Children { get; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
                if (value) LoadChildren();
            }
        }

        public FileSystemItem(string path)
        {
            FullPath = path;
            Name = string.IsNullOrEmpty(Path.GetFileName(path)) ? path : Path.GetFileName(path);
            IsDirectory = Directory.Exists(path);
            Children = new ObservableCollection<FileSystemItem>();

            if (IsDirectory)
                Children.Add(null);
        }

        public void LoadChildren()
        {
            if (Children.Count == 1 && Children[0] == null)
                Children.Clear();
            else
                return;

            try
            {
                foreach (var dir in Directory.GetDirectories(FullPath))
                    Children.Add(new FileSystemItem(dir));
                foreach (var file in Directory.GetFiles(FullPath))
                    Children.Add(new FileSystemItem(file));
            }
            catch { }
        }

        public void Refresh()
        {
            Children.Clear();
            if (IsDirectory)
                Children.Add(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
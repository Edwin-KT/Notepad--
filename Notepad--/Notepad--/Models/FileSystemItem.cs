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

        // Copiii nodului (subdirectoare + fisiere)
        public ObservableCollection<FileSystemItem> Children { get; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
                // Incarcare lazy: abia acum citim de pe disc
                if (value) LoadChildren();
            }
        }

        public FileSystemItem(string path)
        {
            FullPath = path;
            // Daca GetFileName returneaza "" (ex: "C:\"), folosim path-ul intreg
            Name = string.IsNullOrEmpty(Path.GetFileName(path)) ? path : Path.GetFileName(path);
            IsDirectory = Directory.Exists(path);
            Children = new ObservableCollection<FileSystemItem>();

            // Placeholder null = "am copii, dar nu i-am incarcat inca"
            // Fara el, TreeView nu ar afisa sageata de expand
            if (IsDirectory)
                Children.Add(null);
        }

        public void LoadChildren()
        {
            // Daca singurul copil e null => inca n-am incarcat, deci incarcam
            if (Children.Count == 1 && Children[0] == null)
                Children.Clear();
            else
                return; // deja incarcat, nu repetam

            try
            {
                foreach (var dir in Directory.GetDirectories(FullPath))
                    Children.Add(new FileSystemItem(dir));
                foreach (var file in Directory.GetFiles(FullPath))
                    Children.Add(new FileSystemItem(file));
            }
            catch { /* director inaccesibil, ignoram */ }
        }

        // Folosit dupa operatii (new file, paste etc.) ca sa reincarcam
        public void Refresh()
        {
            Children.Clear();
            if (IsDirectory)
                Children.Add(null); // resetam la placeholder
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
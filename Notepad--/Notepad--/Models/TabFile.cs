using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notepad__.Models
{
    public class TabFile : INotifyPropertyChanged
    {
        private static int _counter = 1;

        private string _filePath;
        private string _content = string.Empty;
        private bool _isModified;

        public int NewFileIndex { get; } 

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Header)); 
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set
            {
                _isModified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Header));
            }
        }

        public bool IsNew => string.IsNullOrEmpty(_filePath);

        public string Header
        {
            get
            {
                string name = IsNew
                    ? $"File {NewFileIndex}"
                    : System.IO.Path.GetFileName(_filePath);
                return IsModified ? $"{name} ●" : name;
            }
        }

        public TabFile()
        {
            NewFileIndex = _counter++;
        }

        public TabFile(string filePath, string content)
        {
            _filePath = filePath;
            _content = content;
            _isModified = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
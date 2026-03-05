using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notepad__.Models
{
    public class TabFile : INotifyPropertyChanged
    {
        // Contor static — File 1, File 2, File 3...
        private static int _counter = 1;

        private string _filePath;
        private string _content = string.Empty;
        private bool _isModified;

        public int NewFileIndex { get; } // retinut la creare

        // Calea pe disc (null daca e fisier nou)
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Header)); // header-ul se schimba
            }
        }

        // Continutul din editor
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

        // A fost modificat si nesalvat?
        public bool IsModified
        {
            get => _isModified;
            set
            {
                _isModified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Header)); // ● apare/dispare
            }
        }

        public bool IsNew => string.IsNullOrEmpty(_filePath);

        // Ce apare pe tab: "File 1 ●" sau "document.txt ●"
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

        // Constructor pentru fisier NOU (fara cale)
        public TabFile()
        {
            NewFileIndex = _counter++;
        }

        // Constructor pentru fisier DESCHIS de pe disc
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
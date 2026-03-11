using Microsoft.Win32;
using Notepad__.Helpers;
using Notepad__.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Notepad__.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Properties & Fields

        public ObservableCollection<TabFile> Tabs { get; } = new();

        private TabFile _selectedTab;
        public TabFile SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }

        public FileSystemViewModel FileSystemVM { get; private set; }

        private bool _folderExplorerVisible = true;
        public bool FolderExplorerVisible
        {
            get => _folderExplorerVisible;
            set { _folderExplorerVisible = value; OnPropertyChanged(); }
        }

        private bool _searchAllTabs = false;
        public bool SearchAllTabs
        {
            get => _searchAllTabs;
            set
            {
                _searchAllTabs = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SearchModeHeader));
            }
        }

        public string SearchModeHeader => SearchAllTabs ? "Mode: All Tabs" : "Mode: Selected Tab";

        private int _findResultIndex = -1;
        public int FindResultIndex
        {
            get => _findResultIndex;
            set { _findResultIndex = value; OnPropertyChanged(); }
        }

        private int _findResultLength = 0;
        public int FindResultLength
        {
            get => _findResultLength;
            set { _findResultLength = value; OnPropertyChanged(); }
        }

        private bool _exitConfirmed = false;

        #endregion

        #region Commands

        public ICommand NewFileCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand SaveFileCommand { get; private set; }
        public ICommand SaveFileAsCommand { get; private set; }
        public ICommand CloseTabCommand { get; private set; }
        public ICommand CloseAllTabsCommand { get; private set; }
        public ICommand ShowStandardViewCommand { get; private set; }
        public ICommand ShowFolderExplorerCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand FindCommand { get; private set; }
        public ICommand ReplaceCommand { get; private set; }
        public ICommand ReplaceAllCommand { get; private set; }
        public ICommand ToggleSearchModeCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        private void InitializeCommands()
        {
            NewFileCommand = new RelayCommand(_ => NewFile());
            OpenFileCommand = new RelayCommand(_ => OpenFile());

            SaveFileCommand = new RelayCommand(
                _ => SaveFile(),
                _ => SelectedTab != null);

            SaveFileAsCommand = new RelayCommand(
                _ => SaveFileAs(),
                _ => SelectedTab != null);

            CloseTabCommand = new RelayCommand(
                tab => CloseTab(tab as TabFile),
                _ => SelectedTab != null);

            CloseAllTabsCommand = new RelayCommand(
                _ => CloseAllTabs(),
                _ => Tabs.Count > 0);

            ExitCommand = new RelayCommand(_ => TryExit());

            FindCommand = new RelayCommand(_ => OpenFindReplace(false, false));
            ReplaceCommand = new RelayCommand(_ => OpenFindReplace(true, false));
            ReplaceAllCommand = new RelayCommand(_ => OpenFindReplace(true, true));

            ToggleSearchModeCommand = new RelayCommand(_ => SearchAllTabs = !SearchAllTabs);

            AboutCommand = new RelayCommand(_ => new Views.AboutWindow().ShowDialog());

            ShowStandardViewCommand = new RelayCommand(_ => FolderExplorerVisible = false);
            ShowFolderExplorerCommand = new RelayCommand(_ => FolderExplorerVisible = true);
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            InitializeCommands();

            FileSystemVM = new FileSystemViewModel();
            FileSystemVM.OpenFileRequested += OpenFileFromPath;

            NewFile();
        }

        #endregion

        #region File Operations

        public void NewFile()
        {
            var tab = new TabFile();
            Tabs.Add(tab);
            SelectedTab = tab;
        }

        public void OpenFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (dlg.ShowDialog() == true)
                OpenFileFromPath(dlg.FileName);
        }

        public void OpenFileFromPath(string path)
        {
            foreach (var t in Tabs)
            {
                if (t.FilePath == path)
                {
                    SelectedTab = t;
                    return;
                }
            }

            try
            {
                byte[] buffer = File.ReadAllBytes(path);
                for (int i = 0; i < Math.Min(buffer.Length, 8000); i++)
                {
                    if (buffer[i] == 0)
                    {
                        MessageBox.Show(
                            $"'{Path.GetFileName(path)}' appears to be a binary file and cannot be opened as text.",
                            "Cannot open file");
                        return;
                    }
                }

                string content = System.Text.Encoding.UTF8.GetString(buffer);
                var tab = new TabFile(path, content);
                Tabs.Add(tab);
                SelectedTab = tab;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open file: {ex.Message}", "Error");
            }
        }

        public bool SaveFile(TabFile tab = null)
        {
            tab ??= SelectedTab;
            if (tab == null) return false;

            if (tab.IsNew) return SaveFileAs(tab);

            try
            {
                File.WriteAllText(tab.FilePath, tab.Content);
                tab.IsModified = false;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot save: {ex.Message}", "Error");
                return false;
            }
        }

        public bool SaveFileAs(TabFile tab = null)
        {
            tab ??= SelectedTab;
            if (tab == null) return false;

            var dlg = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = tab.IsNew
                    ? $"File {tab.NewFileIndex}"
                    : Path.GetFileName(tab.FilePath)
            };

            if (dlg.ShowDialog() == true)
            {
                tab.FilePath = dlg.FileName;
                return SaveFile(tab);
            }

            return false;
        }

        public void CloseTab(TabFile tab = null)
        {
            tab ??= SelectedTab;
            if (tab == null) return;

            if (tab.IsModified)
            {
                var result = MessageBox.Show(
                    $"'{tab.Header.Replace(" ●", "")}' has unsaved changes. Save?",
                    "Unsaved changes",
                    MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes)
                {
                    bool saved = SaveFile(tab);
                    if (!saved) return;
                }
            }

            int index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);

            if (Tabs.Count == 0)
                NewFile();
            else
                SelectedTab = Tabs[Math.Max(0, index - 1)];
        }

        public void CloseAllTabs()
        {
            foreach (var tab in Tabs.ToList())
            {
                if (tab.IsModified)
                {
                    SelectedTab = tab;

                    var result = MessageBox.Show(
                        $"'{tab.Header.Replace(" ●", "")}' has unsaved changes. Save?",
                        "Unsaved changes",
                        MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.Cancel) return;
                    if (result == MessageBoxResult.Yes)
                    {
                        bool saved = SaveFile(tab);
                        if (!saved) return;
                    }
                }
                Tabs.Remove(tab);
            }
            NewFile();
        }

        public bool TryExit(bool shutdown = true)
        {
            if (_exitConfirmed) return true;

            foreach (var tab in Tabs.ToList())
            {
                if (tab.IsModified)
                {
                    SelectedTab = tab;

                    var result = MessageBox.Show(
                        $"'{tab.Header.Replace(" ●", "")}' has unsaved changes. Save?",
                        "Unsaved changes",
                        MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.Cancel) return false;
                    if (result == MessageBoxResult.Yes)
                    {
                        bool saved = SaveFile(tab);
                        if (!saved) return false;
                    }
                }
            }

            _exitConfirmed = true;

            if (shutdown)
                Application.Current.Shutdown();

            return true;
        }

        #endregion

        #region Search & Replace

        private Views.FindReplaceWindow _findReplaceWindow;

        private void OpenFindReplace(bool replaceMode, bool replaceAll)
        {
            if (_findReplaceWindow != null && _findReplaceWindow.IsVisible)
            {
                _findReplaceWindow.SetMode(replaceMode, replaceAll);
                _findReplaceWindow.Focus();
                return;
            }
            _findReplaceWindow = new Views.FindReplaceWindow(this, replaceMode, replaceAll);
            _findReplaceWindow.Show();
        }

        public void SetFindResult(int index, int length)
        {
            FindResultIndex = index;
            FindResultLength = length;
        }

        public int FindInTab(string searchText, TabFile tab, bool wholeWord = false)
        {
            if (string.IsNullOrEmpty(searchText) || tab == null) return -1;
            return FindIndex(tab.Content, searchText, 0, wholeWord);
        }

        public bool ReplaceInTab(string find, string replace, TabFile tab, bool wholeWord = false)
        {
            if (tab == null || string.IsNullOrEmpty(find)) return false;
            int index = FindIndex(tab.Content, find, 0, wholeWord);
            if (index < 0) return false;
            tab.Content = tab.Content.Remove(index, find.Length).Insert(index, replace);
            return true;
        }

        public int ReplaceAllInTab(string find, string replace, TabFile tab, bool wholeWord = false)
        {
            if (tab == null || string.IsNullOrEmpty(find)) return 0;
            string pattern = wholeWord ? $@"\b{Regex.Escape(find)}\b" : Regex.Escape(find);
            int count = Regex.Matches(tab.Content ?? "", pattern, RegexOptions.IgnoreCase).Count;
            if (count > 0)
                tab.Content = Regex.Replace(tab.Content, pattern, replace, RegexOptions.IgnoreCase);
            return count;
        }

        public int ReplaceAllInAllTabs(string find, string replace, bool wholeWord = false)
        {
            int total = 0;
            foreach (var tab in Tabs)
                total += ReplaceAllInTab(find, replace, tab, wholeWord);
            return total;
        }

        private int FindIndex(string content, string search, int startIndex, bool wholeWord)
        {
            if (string.IsNullOrEmpty(content)) return -1;
            if (wholeWord)
            {
                string pattern = $@"\b{Regex.Escape(search)}\b";
                var match = Regex.Match(content.Substring(startIndex), pattern, RegexOptions.IgnoreCase);
                return match.Success ? match.Index + startIndex : -1;
            }
            return content.IndexOf(search, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
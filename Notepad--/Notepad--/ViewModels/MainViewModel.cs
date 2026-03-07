using Notepad__.Helpers;
using Notepad__.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Notepad__.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<TabFile> Tabs { get; } = new();

        private TabFile _selectedTab;
        public TabFile SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }


        public FileSystemViewModel FileSystemVM { get; }

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

        public ICommand NewFileCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveFileAsCommand { get; }
        public ICommand CloseTabCommand { get; }      
        public ICommand CloseAllTabsCommand { get; }  
        public ICommand ShowStandardViewCommand { get; }
        public ICommand ShowFolderExplorerCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand ReplaceAllCommand { get; }
        public ICommand ToggleSearchModeCommand { get; }



        public MainViewModel()
        {
            NewFileCommand = new RelayCommand(_ => NewFile());

            OpenFileCommand = new RelayCommand(_ => OpenFile());

            // Save e activ doar daca exista un tab selectat
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

            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            FindCommand = new RelayCommand(_ => OpenFindReplace(false, false));
            ReplaceCommand = new RelayCommand(_ => OpenFindReplace(true, false));
            ReplaceAllCommand = new RelayCommand(_ => OpenFindReplace(true, true));
            ToggleSearchModeCommand = new RelayCommand(_ => SearchAllTabs = !SearchAllTabs);

            NewFile();

            FileSystemVM = new FileSystemViewModel();
            FileSystemVM.OpenFileRequested += OpenFileFromPath;

            ShowStandardViewCommand = new RelayCommand(_ => FolderExplorerVisible = false);
            ShowFolderExplorerCommand = new RelayCommand(_ => FolderExplorerVisible = true);
        }

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
            // Daca fisierul e deja deschis, doar il selectam
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
                // Verificare simpla: citim primii 8000 bytes si cautam caractere nule
                // Fisierele binare (imagini, exe etc.) contin de obicei caractere nule
                // Fisierele text nu ar trebui sa contina
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

        // Returneaza true/false ca sa stim daca salvarea a reusit
        // (util mai tarziu la Close)
        public bool SaveFile(TabFile tab = null)
        {
            tab ??= SelectedTab;
            if (tab == null) return false;

            // Daca e fisier nou, trebuie sa alegem calea
            if (tab.IsNew) return SaveFileAs(tab);

            try
            {
                File.WriteAllText(tab.FilePath, tab.Content);
                tab.IsModified = false; // dispare ●
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
                // Propune numele tabului ca nume de fisier
                FileName = tab.IsNew
                    ? $"File {tab.NewFileIndex}"
                    : Path.GetFileName(tab.FilePath)
            };

            if (dlg.ShowDialog() == true)
            {
                tab.FilePath = dlg.FileName; // seteaza calea
                return SaveFile(tab);
            }

            return false; // utilizatorul a apasat Cancel
        }

        public void CloseTab(TabFile tab)
        {
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
                    // Daca salvarea esueaza (ex: utilizatorul a apasat Cancel la SaveAs)
                    // nu inchidem tabul
                    bool saved = SaveFile(tab);
                    if (!saved) return;
                }
            }

            int index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);

            // Selectam tabul din stanga celui inchis, sau primul disponibil
            if (Tabs.Count == 0)
                NewFile();
            else
                SelectedTab = Tabs[Math.Max(0, index - 1)];
        }

        public void CloseAllTabs()
        {
            // Iteram pe o copie a listei ca sa putem modifica originalul
            foreach (var tab in Tabs.ToList())
            {
                if (tab.IsModified)
                {
                    // Selectam tabul ca utilizatorul sa vada despre ce fisier e vorba
                    SelectedTab = tab;

                    var result = MessageBox.Show(
                        $"'{tab.Header.Replace(" ●", "")}' has unsaved changes. Save?",
                        "Unsaved changes",
                        MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.Cancel) return; // oprim tot
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


        private Views.FindReplaceWindow _findReplaceWindow;

        private void OpenFindReplace(bool replaceMode, bool replaceAll)
        {
            // Daca fereastra e deja deschisa, o aducem in prim plan
            if (_findReplaceWindow != null && _findReplaceWindow.IsVisible)
            {
                _findReplaceWindow.SetMode(replaceMode, replaceAll);
                _findReplaceWindow.Focus();
                return;
            }
            _findReplaceWindow = new Views.FindReplaceWindow(this, replaceMode, replaceAll);
            _findReplaceWindow.Show();
        }

        // Returneaza indexul primei aparitii in tab-ul dat
        // sau -1 daca nu gaseste
        public int FindInTab(string searchText, TabFile tab)
        {
            if (string.IsNullOrEmpty(searchText) || tab == null) return -1;
            return tab.Content?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1;
        }

        // Inlocuieste prima aparitie in tab-ul dat
        // Returneaza true daca a inlocuit ceva
        public bool ReplaceInTab(string find, string replace, TabFile tab)
        {
            if (tab == null || string.IsNullOrEmpty(find)) return false;

            int index = tab.Content?.IndexOf(find, StringComparison.OrdinalIgnoreCase) ?? -1;
            if (index < 0) return false;

            tab.Content = tab.Content.Remove(index, find.Length).Insert(index, replace);
            return true;
        }

        // Inlocuieste toate aparitiile intr-un tab
        // Returneaza numarul de inlocuiri
        public int ReplaceAllInTab(string find, string replace, TabFile tab)
        {
            if (tab == null || string.IsNullOrEmpty(find)) return 0;

            int count = 0;
            string content = tab.Content ?? "";
            string lower = content.ToLower();
            string findLower = find.ToLower();
            int index;

            while ((index = lower.IndexOf(findLower)) >= 0)
            {
                content = content.Remove(index, find.Length).Insert(index, replace);
                lower = content.ToLower();
                count++;
            }

            if (count > 0) tab.Content = content;
            return count;
        }

        // Inlocuieste toate aparitiile in toate taburile
        public int ReplaceAllInAllTabs(string find, string replace)
        {
            int total = 0;
            foreach (var tab in Tabs)
                total += ReplaceAllInTab(find, replace, tab);
            return total;
        }
    }
}
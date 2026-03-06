using Notepad__.Helpers;
using Notepad__.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System;
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

        public ICommand NewFileCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveFileAsCommand { get; }
        public ICommand ShowStandardViewCommand { get; }
        public ICommand ShowFolderExplorerCommand { get; }



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

            NewFile();

            FileSystemVM = new FileSystemViewModel();

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

            // Altfel il deschidem intr-un tab nou
            try
            {
                string content = File.ReadAllText(path);
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
    }
}
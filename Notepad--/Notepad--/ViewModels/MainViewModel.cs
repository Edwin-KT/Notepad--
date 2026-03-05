using Notepad__.Helpers;
using Notepad__.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Notepad__.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Lista de taburi vizibila in UI
        public ObservableCollection<TabFile> Tabs { get; } = new();

        private TabFile _selectedTab;
        public TabFile SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }

        // Comenzi
        public ICommand NewFileCommand { get; }

        public MainViewModel()
        {
            NewFileCommand = new RelayCommand(_ => NewFile());

            // Subpunctul 1: la pornire se deschide un tab gol
            NewFile();
        }

        public void NewFile()
        {
            var tab = new TabFile();
            Tabs.Add(tab);
            SelectedTab = tab;
        }
    }
}
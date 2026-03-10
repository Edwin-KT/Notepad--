using Notepad__.ViewModels;
using System.Windows;

namespace Notepad__.Views
{
    public partial class FindReplaceWindow : Window
    {
        private readonly MainViewModel _vm;
        private bool _replaceMode;
        private bool _replaceAll;

        public FindReplaceWindow(MainViewModel vm, bool replaceMode, bool replaceAll)
        {
            InitializeComponent();
            _vm = vm;
            SetMode(replaceMode, replaceAll);
        }

        public void SetMode(bool replaceMode, bool replaceAll)
        {
            _replaceMode = replaceMode;
            _replaceAll = replaceAll;

            ReplacePanel.Visibility = replaceMode ? Visibility.Visible : Visibility.Collapsed;

            Height = replaceMode ? 220 : 175;
            
            ReplaceBtn.Visibility = replaceMode && !replaceAll ? Visibility.Visible : Visibility.Collapsed;
            ReplaceAllBtn.Visibility = replaceMode ? Visibility.Visible : Visibility.Collapsed;

            Title = replaceMode ? (_replaceAll ? "Replace All" : "Replace") : "Find";
            ResultText.Text = string.Empty;
        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            string searchText = FindBox.Text;
            if (string.IsNullOrEmpty(searchText))
            {
                ResultText.Text = "Please enter a search term.";
                return;
            }

            if (_vm.SearchAllTabs)
            {
                int found = 0;
                foreach (var tab in _vm.Tabs)
                    if (_vm.FindInTab(searchText, tab) >= 0)
                        found++;

                ResultText.Text = found > 0
                    ? $"Found in {found} tab(s)."
                    : "Not found in any tab.";
            }
            else
            {
                int index = _vm.FindInTab(searchText, _vm.SelectedTab);
                ResultText.Text = index >= 0
                    ? $"Found at position {index}."
                    : "Not found in current tab.";
            }
        }

        private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            string find = FindBox.Text;
            string replace = ReplaceBox.Text;

            if (string.IsNullOrEmpty(find))
            {
                ResultText.Text = "Please enter a search term.";
                return;
            }

            if (_vm.SearchAllTabs)
            {
                int count = 0;
                foreach (var tab in _vm.Tabs)
                    if (_vm.ReplaceInTab(find, replace, tab)) count++;
                ResultText.Text = count > 0
                    ? $"Replaced first occurrence in {count} tab(s)."
                    : "Not found in any tab.";
            }
            else
            {
                bool replaced = _vm.ReplaceInTab(find, replace, _vm.SelectedTab);
                ResultText.Text = replaced ? "Replaced first occurrence." : "Not found.";
            }
        }

        private void ReplaceAllBtn_Click(object sender, RoutedEventArgs e)
        {
            string find = FindBox.Text;
            string replace = ReplaceBox.Text;

            if (string.IsNullOrEmpty(find))
            {
                ResultText.Text = "Please enter a search term.";
                return;
            }

            int total;
            if (_vm.SearchAllTabs)
            {
                total = _vm.ReplaceAllInAllTabs(find, replace);
                ResultText.Text = total > 0
                    ? $"Replaced {total} occurrence(s) across all tabs."
                    : "Not found in any tab.";
            }
            else
            {
                total = _vm.ReplaceAllInTab(find, replace, _vm.SelectedTab);
                ResultText.Text = total > 0
                    ? $"Replaced {total} occurrence(s)."
                    : "Not found.";
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
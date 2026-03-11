using Notepad__.Models;
using Notepad__.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Notepad__
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) return;

                bool canClose = vm.TryExit(shutdown: false);
                if (!canClose)
                    e.Cancel = true;
            };

            Loaded += (s, e) =>
            {
                var vm = DataContext as MainViewModel;
                if (vm == null) return;

                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.FindResultIndex)
                        && vm.FindResultIndex >= 0)
                    {
                        var textBox = GetActiveTextBox();
                        if (textBox == null) return;

                        textBox.Focus();
                        textBox.Select(vm.FindResultIndex, vm.FindResultLength);
                        textBox.ScrollToLine(textBox.GetLineIndexFromCharacterIndex(vm.FindResultIndex));
                    }
                };
            };
        }

        private TextBox GetActiveTextBox()
        {
            TabControlMain.UpdateLayout();
            var cp = TabControlMain.Template.FindName("PART_SelectedContentHost", TabControlMain)
                     as ContentPresenter;
            if (cp == null) return null;
            cp.UpdateLayout();
            return FindVisualChild<TextBox>(cp);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found) return found;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            var selectedItem = (sender as TreeView)?.SelectedItem as FileSystemItem;
            vm?.FileSystemVM.OnItemDoubleClicked(selectedItem);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
                vm.FileSystemVM.SelectedItem = e.NewValue as FileSystemItem;
        }
    }
}
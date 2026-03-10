using Notepad__.Models;
using Notepad__.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Notepad__
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
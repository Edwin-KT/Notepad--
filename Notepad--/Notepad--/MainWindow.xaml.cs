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
            // Luam ViewModel-ul din DataContext
            var vm = DataContext as MainViewModel;

            // SelectedItem din TreeView este de tip FileSystemItem
            var selectedItem = (sender as TreeView)?.SelectedItem as FileSystemItem;

            // Trimitem la ViewModel — toata logica e acolo
            vm?.FileSystemVM.OnItemDoubleClicked(selectedItem);
        }
    }
}
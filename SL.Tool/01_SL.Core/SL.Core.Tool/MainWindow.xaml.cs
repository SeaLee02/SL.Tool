using Data.Abstractions.Entities;
using HandyControl.Tools;
using SL.Core.Tool.Data;
using SL.Core.Tool.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SL.Core.Tool
{

    public partial class MainWindow : HandyControl.Controls.Window, IComponentConnector
    {
        MainWindowViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            GlobalData.Init();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            GlobalShortcut.Init(new List<KeyBinding>
            {
                new KeyBinding(_viewModel.SaveConfig, Key.S, ModifierKeys.Control | ModifierKeys.Shift), 
                new KeyBinding(_viewModel.LoadConfig, Key.D, ModifierKeys.Control | ModifierKeys.Shift),
                new KeyBinding(_viewModel.CreateCode, Key.C, ModifierKeys.Control | ModifierKeys.Shift)
            });
        }

        private void CheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (DbTable item in e.AddedItems)
            {
                item.Selected = true;
            }
            // For on each add Items
            // do what you want
            foreach (DbTable item in e.RemovedItems)
            {
                item.Selected = false;
            }
            // For on each removed Items
            // do what you want
        }

    }
}
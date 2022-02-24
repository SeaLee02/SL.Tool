using SL.Core.Tool.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SL.Core.Tool.UserControl
{
    /// <summary>
    /// ProLoadIng.xaml 的交互逻辑
    /// </summary>
    public partial class ProLoadIngDialog
    {
        ProLoadIngDialogViewModel _proLoadIngDialogViewModel;

        public ProLoadIngDialog(string msg)
        {
            InitializeComponent();
            _proLoadIngDialogViewModel=new ProLoadIngDialogViewModel();
            _proLoadIngDialogViewModel.Text = msg;
            DataContext = _proLoadIngDialogViewModel;
        }
    }
}

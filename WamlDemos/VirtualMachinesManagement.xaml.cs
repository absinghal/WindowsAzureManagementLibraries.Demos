﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WamlDemos.ViewModels;
using WamlDemos.Views;

namespace WamlDemos
{
    [Export(typeof(IView<VirtualMachinesManagementViewModel>))]
    public partial class VirtualMachinesManagement : IView<VirtualMachinesManagementViewModel>
    {
        public VirtualMachinesManagement()
        {
            InitializeComponent();
        }

        public VirtualMachinesManagementViewModel ViewModel
        {
            get { return DataContext as VirtualMachinesManagementViewModel; }
            set { DataContext = value; }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows;
using WamlDemos.ViewModels;
using WamlDemos.Views;

namespace WamlDemos.Navigation
{
    public static class ModelBasedNavigationHelper
    {
        private static readonly CompositionContainer Container;
        private static readonly List<IHostedControlViewModel> ViewModelsSource;
        private static readonly MethodInfo OpenGenericGetExportedValue;
        private static readonly ConcurrentDictionary<IHostedControlViewModel, FrameworkElement> HostedControlLookup = new ConcurrentDictionary<IHostedControlViewModel, FrameworkElement>(); 

        static ModelBasedNavigationHelper()
        {
            var catalog = new AssemblyCatalog(typeof(ModelBasedNavigationHelper).Assembly);
            Container = new CompositionContainer(catalog);
            ViewModelsSource = Container.GetExportedValues<IHostedControlViewModel>().ToList();

            OpenGenericGetExportedValue = ((Func<object>) Container.GetExportedValue<object>).Method.GetGenericMethodDefinition();
        }

        public static List<IHostedControlViewModel> ViewModels
        {
            get { return ViewModelsSource; }
        }

        public static FrameworkElement GetControl(IHostedControlViewModel viewModel)
        {
            return HostedControlLookup.GetOrAdd(viewModel, GetControlInternal);
        }

        private static FrameworkElement GetControlInternal(IHostedControlViewModel viewModel)
        {
            var exportType = typeof (IView<>).MakeGenericType(viewModel.GetType());
            var element = (FrameworkElement)OpenGenericGetExportedValue.MakeGenericMethod(exportType).Invoke(Container, null);
            element.DataContext = viewModel;
            return element;
        }
    }
}

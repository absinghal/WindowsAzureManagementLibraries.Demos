using Microsoft.WindowsAzure;
using System;
using System.Windows;

namespace WamlDemos
{
    public partial class TraceWindow : Window
    {
        public TraceWindow()
        {
            InitializeComponent();

            this.Loaded += (sender, e) =>
                {
                    StartTrace();
                };
        }

        private void StartTrace()
        {
            var traceAction = new Action<string>(trace =>
            {
                Dispatcher.Invoke(() => _trace.Text =
                    trace + Environment.NewLine + _trace.Text);
            });

            CloudContext.Configuration.Tracing.AddTracingInterceptor(
                new WpfAppTracingInterceptor(traceAction)
                );
        }
    }
}

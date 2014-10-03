namespace Simple.Wpf.Template.ViewModels
{
    using System;
    using System.Globalization;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Extensions;
    using NLog;
    using Services;

    public sealed class DiagnosticsViewModel : BaseViewModel, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly CompositeDisposable _disposable;

        private string _cpu;
        private string _managedMemory;
        private string _totalMemory;

        public DiagnosticsViewModel(IDiagnosticsService diagnosticsService, ISchedulerService schedulerService)
        {
            Cpu = Constants.DefaultCpuString;
            ManagedMemory = Constants.DefaultManagedMemoryString;
            TotalMemory = Constants.DefaultTotalMemoryString;

            _disposable = new CompositeDisposable
            {
                diagnosticsService.CpuUtilisation
                    .Select(x => x < 10
                        ? string.Format("CPU: 0{0}%", x.ToString(CultureInfo.InvariantCulture))
                        : string.Format("CPU: {0}%", x.ToString(CultureInfo.InvariantCulture)))
                    .ObserveOn(schedulerService.Dispatcher)
                    .Subscribe(x =>
                    {
                        Cpu = x;
                    }, e =>
                    {
                        Cpu = Constants.DefaultCpuString;
                    }),

                diagnosticsService.Memory
                    .Select(x =>
                    {
                        var managedMemory = string.Format("Managed Memory: {0}", x.ManagedAsString());
                        var totalMemory = string.Format("Total Memory: {0}", x.WorkingSetPrivateAsString());

                        return new Tuple<string, string>(managedMemory, totalMemory);
                    })
                    .ObserveOn(schedulerService.Dispatcher)
                    .Subscribe(x =>
                    {
                        ManagedMemory = x.Item1;
                        TotalMemory = x.Item2;
                    }, e =>
                    {
                        ManagedMemory = Constants.DefaultManagedMemoryString;
                        TotalMemory = Constants.DefaultTotalMemoryString;
                    })
            };
        }

        public void Dispose()
        {
            using (Duration.Measure(Logger, "Dispose"))
            {
                _disposable.Dispose();
            }
        }

        public string Cpu
        {
            get
            {
                return _cpu;
            }

            private set
            {
                SetPropertyAndNotify(ref _cpu, value, () => Cpu);
            }
        }

        public string ManagedMemory
        {
            get
            {
                return _managedMemory;
            }

            private set
            {
                SetPropertyAndNotify(ref _managedMemory, value, () => ManagedMemory);
            }
        }

        public string TotalMemory
        {
            get
            {
                return _totalMemory;
            }

            private set
            {
                SetPropertyAndNotify(ref _totalMemory, value, () => TotalMemory);
            }
        }
    }
}
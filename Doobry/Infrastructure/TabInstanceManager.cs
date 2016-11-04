using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Doobry.Settings;
using Dragablz;
using ICSharpCode.AvalonEdit.Highlighting;
using MaterialDesignThemes.Wpf;
using StructureMap;

namespace Doobry.Infrastructure
{
    public class TabInstanceManager : ITabInstanceManager
    {
        private readonly IConnectionCache _connectionCache;
        private readonly IHighlightingDefinition _sqlHighlightingDefinition;
        private readonly IQueryFileService _queryFileService;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly Func<MainWindowViewModel> _windowViewModelFactory;

        private readonly IDictionary<TabViewModel, IDisposable> _modelCleanUpIndex =
            new Dictionary<TabViewModel, IDisposable>();

        private readonly IDictionary<Window, IDisposable> _windowCleanUpIndex = new Dictionary<Window, IDisposable>();

        public TabInstanceManager(IConnectionCache connectionCache,
            IHighlightingDefinition sqlHighlightingDefinition, IQueryFileService queryFileService, ISnackbarMessageQueue snackbarMessageQueue)
        {
            if (connectionCache == null) throw new ArgumentNullException(nameof(connectionCache));
            if (sqlHighlightingDefinition == null) throw new ArgumentNullException(nameof(sqlHighlightingDefinition));
            if (queryFileService == null) throw new ArgumentNullException(nameof(queryFileService));
            if (snackbarMessageQueue == null) throw new ArgumentNullException(nameof(snackbarMessageQueue));

            _connectionCache = connectionCache;
            _sqlHighlightingDefinition = sqlHighlightingDefinition;
            _queryFileService = queryFileService;
            _snackbarMessageQueue = snackbarMessageQueue;

            ClosingTabItemCallback = OnItemClosingHandler;
        }

        public ItemActionCallback ClosingTabItemCallback { get; }

        public TabViewModel CreateManagedTabViewModel()
        {            
            var result = new TabViewModel(Guid.NewGuid(), _connectionCache, _sqlHighlightingDefinition, _snackbarMessageQueue);
            Watch(result);
            return result;
        }

        public TabViewModel CreateManagedTabViewModel(Guid id, Connection connection)
        {
            var result = new TabViewModel(id, connection, _connectionCache, _sqlHighlightingDefinition, _snackbarMessageQueue);
            PopulateDocument(result);
            Watch(result);
            return result;
        }

        public void Manage(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            Watch(window);
        }

        private void OnItemClosingHandler(ItemActionCallbackArgs<TabablzControl> args)
        {
            var tabViewModel = args.DragablzItem.DataContext as TabViewModel;
            if (tabViewModel == null) return;
            Release(tabViewModel);
            RemoveDocument(tabViewModel);
        }

        private void Release(TabViewModel tabViewModel)
        {
            if (tabViewModel == null) throw new ArgumentNullException(nameof(tabViewModel));

            _modelCleanUpIndex[tabViewModel].Dispose();
            _modelCleanUpIndex.Remove(tabViewModel);
        }

        private void Watch(Window window)
        {
            window.Closed += WindowOnClosed;
            var disposable = Disposable.Create(() => window.Closed -= WindowOnClosed);
            _windowCleanUpIndex.Add(window, disposable);
        }

        private void WindowOnClosed(object sender, EventArgs eventArgs)
        {
            var window = (Window) sender;
            _windowCleanUpIndex[window].Dispose();
            _windowCleanUpIndex.Remove(window);

            var redundantTabs = TabablzControl.GetLoadedInstances()
                .SelectMany(
                    tabControl =>
                        tabControl.Items.OfType<TabViewModel>()
                            .Select(tabViewModel => new {tabControl, tabViewModel}))
                .Where(a => window.Equals(Window.GetWindow(a.tabControl)))
                .ToList();

            foreach (var redundantTab in redundantTabs)
            {
                Release(redundantTab.tabViewModel);
                if (Application.Current.Windows.OfType<MainWindow>().Any())
                    RemoveDocument(redundantTab.tabViewModel);
            }
        }

        private void RemoveDocument(TabViewModel tabViewModel)
        {
            var fileName = _queryFileService.GetFileName(tabViewModel.Id);
            if (!File.Exists(fileName)) return;

            try
            {
                File.Delete(fileName);
            }
            catch { /* TODO uhm something */ }
        }

        private void PopulateDocument(TabViewModel tabViewModel)
        {
            Task.Factory.StartNew(() =>
            {
                var fileName = _queryFileService.GetFileName(tabViewModel.Id);
                return File.Exists(fileName) ? File.ReadAllText(fileName) : string.Empty;
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    //TODO uhm something
                }
                else
                {
                    if (string.IsNullOrEmpty(tabViewModel.QueryRunnerViewModel.Document.Text))
                    {
                        tabViewModel.QueryRunnerViewModel.Document.Text = t.Result;
                    }
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Watch(TabViewModel tabViewModel)
        {
            var disposable = tabViewModel.DocumentChangedObservable.Throttle(TimeSpan.FromSeconds(3))
                .ObserveOn(new EventLoopScheduler())
                .Subscribe(SaveDocument);

            _modelCleanUpIndex.Add(tabViewModel, disposable);
        }

        private void SaveDocument(DocumentChangedUnit documentChangedUnit)
        {
            try
            {
                var directoryName = Path.GetDirectoryName(_queryFileService.GetFileName(documentChangedUnit.TabId));
                Directory.CreateDirectory(directoryName);
                File.WriteAllText(_queryFileService.GetFileName(documentChangedUnit.TabId), documentChangedUnit.Text);
            }
            catch (Exception)
            {
                //TODO...erm summit                
            }            
        }
    }
}
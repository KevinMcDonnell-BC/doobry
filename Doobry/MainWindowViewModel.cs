﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Doobry.Features;
using Doobry.Features.QueryDeveloper;
using Doobry.Infrastructure;
using Doobry.Settings;
using Dragablz;
using Dragablz.Dockablz;
using DynamicData.Kernel;
using MaterialDesignThemes.Wpf;

namespace Doobry
{
    public class MainWindowViewModel
    {
        private readonly FeatureRegistry _featureRegistry;
        private readonly ITabInstanceManager _tabInstanceManager;
        private readonly IConnectionCache _connectionCache;
        private readonly IGeneralSettings _generalSettings;
        private readonly IInitialLayoutStructureProvider _initialLayoutStructureProvider;

        private static bool _isStartupInitiated;

        public MainWindowViewModel(
            FeatureRegistry featureRegistry,
            ITabInstanceManager tabInstanceManager,
            IConnectionCache connectionCache,
            IGeneralSettings generalSettings,
            IInitialLayoutStructureProvider initialLayoutStructureProvider,
            ISnackbarMessageQueue snackbarSnackbarMessageQueue)
        {
            if (featureRegistry == null) throw new ArgumentNullException(nameof(featureRegistry));
            if (tabInstanceManager == null)
                throw new ArgumentNullException(nameof(tabInstanceManager));
            if (connectionCache == null) throw new ArgumentNullException(nameof(connectionCache));
            if (generalSettings == null) throw new ArgumentNullException(nameof(generalSettings));
            if (initialLayoutStructureProvider == null)
                throw new ArgumentNullException(nameof(initialLayoutStructureProvider));

            _featureRegistry = featureRegistry;
            _tabInstanceManager = tabInstanceManager;
            _connectionCache = connectionCache;
            _generalSettings = generalSettings;
            _initialLayoutStructureProvider = initialLayoutStructureProvider;
            SnackbarMessageQueue = snackbarSnackbarMessageQueue;

            StartupCommand = new Command(RunStartup);
            ShutDownCommand = new Command(o => RunShutdown());
            Tabs = new ObservableCollection<TabViewModel>();
        }

        public ObservableCollection<TabViewModel> Tabs { get; }

        public ICommand StartupCommand { get; }

        public ICommand ShutDownCommand { get; }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        private void RunStartup(object sender)
        {
            if (_isStartupInitiated) return;
            _isStartupInitiated = true;

            //TODO assert this stuff, provide a fall back
            var senderDependencyObject = sender as DependencyObject;
            var mainWindow = Window.GetWindow(senderDependencyObject) as MainWindow;
            var rootTabControl = mainWindow.InitialTabablzControl;

            LayoutStructure layoutStructure;
            if (_initialLayoutStructureProvider.TryTake(out layoutStructure))
            {
                RestoreLayout(rootTabControl, layoutStructure);
            }

            if (TabablzControl.GetLoadedInstances().SelectMany(tc => tc.Items.OfType<object>()).Any()) return;

            var tabContentLifetimeHost = _featureRegistry.Default.CreateTabContent();
            var tabContentContainer = new TabItemContainer(Guid.NewGuid(), tabContentLifetimeHost, _featureRegistry.Default);
            rootTabControl.AddToSource(tabContentContainer);
            TabablzControl.SelectItem(tabContentContainer);

            //TODO sort out this nasty cast
            ((TabViewModel)tabContentContainer.ViewModel).EditConnectionCommand.Execute(rootTabControl);
        }

        private void RestoreLayout(TabablzControl rootTabControl, LayoutStructure layoutStructure)
        {
            try
            {
                //we only currently support a single window, can build on in future
                var layoutStructureWindow = layoutStructure.Windows.Single();

                var layoutStructureTabSets = layoutStructureWindow.TabSets.ToDictionary(tabSet => tabSet.Id);

                if (layoutStructureWindow.Branches.Any())
                {
                    var branchIndex = layoutStructureWindow.Branches.ToDictionary(b => b.Id);
                    var rootBranch = GetRoot(branchIndex);

                    //do the nasty recursion to build the layout, populate the tabs after, keep it simple...
                    foreach (var tuple in BuildLayout(rootTabControl, rootBranch, branchIndex))
                    {
                        PopulateTabControl(tuple.Item2, layoutStructureTabSets[tuple.Item1]);
                    }
                }
                else
                {
                    var layoutStructureTabSet = layoutStructureTabSets.Values.FirstOrDefault();
                    if (layoutStructureTabSet != null)
                        PopulateTabControl(rootTabControl, layoutStructureTabSet);
                }
            }
            catch
            {
                SnackbarMessageQueue.Enqueue("Unable to restore your previous layout.");                
            }

        }

        private void PopulateTabControl(TabablzControl tabablzControl, LayoutStructureTabSet layoutStructureTabSet)
        {
            foreach (var tabItem in layoutStructureTabSet.TabItems)
            {
                //TODO select correct feature
                var featureFactory = _featureRegistry.Default;
                var tabContentLifetimeHost = featureFactory.RestoreTabContent(tabItem);
                var tabContentContainer = new TabItemContainer(tabItem.Id, tabContentLifetimeHost, featureFactory);
                tabablzControl.AddToSource(tabContentContainer);

                if (tabContentContainer.TabId == layoutStructureTabSet.SelectedTabItemId)
                {
                    tabablzControl.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        tabablzControl.SetCurrentValue(Selector.SelectedItemProperty, tabContentContainer);
                    }), DispatcherPriority.Loaded);                    
                }
            }
        }

        public static readonly DependencyProperty TabSetIdProperty = DependencyProperty.RegisterAttached(
            "TabSetId", typeof(Guid?), typeof(MainWindowViewModel), new PropertyMetadata(default(Guid?)));

        public static void SetTabSetId(DependencyObject element, Guid? value)
        {
            element.SetValue(TabSetIdProperty, value);
        }

        public static Guid? GetTabSetId(DependencyObject element)
        {
            return (Guid?) element.GetValue(TabSetIdProperty);
        }

        private static IEnumerable<Tuple<Guid, TabablzControl>> BuildLayout(
            TabablzControl intoTabablzControl, 
            LayoutStructureBranch layoutStructureBranch,
            IDictionary<Guid, LayoutStructureBranch> layoutStructureBranchIndex)
        {            
            var newSiblingTabablzControl = CreateTabablzControl();
            var branchResult = Layout.Branch(intoTabablzControl, newSiblingTabablzControl, layoutStructureBranch.Orientation, false, layoutStructureBranch.Ratio);            

            if (layoutStructureBranch.ChildFirstBranchId.HasValue)
            {
                var firstChildBranch = layoutStructureBranchIndex[layoutStructureBranch.ChildFirstBranchId.Value];
                foreach (var tuple in BuildLayout(intoTabablzControl, firstChildBranch, layoutStructureBranchIndex))
                    yield return tuple;
            }
            else if (layoutStructureBranch.ChildFirstTabSetId.HasValue)
            {
                SetTabSetId(intoTabablzControl, layoutStructureBranch.ChildFirstTabSetId.Value);
                yield return new Tuple<Guid, TabablzControl>(layoutStructureBranch.ChildFirstTabSetId.Value, intoTabablzControl);
            }            

            if (layoutStructureBranch.ChildSecondBranchId.HasValue)
            {
                var secondChildBranch = layoutStructureBranchIndex[layoutStructureBranch.ChildSecondBranchId.Value];
                foreach (var tuple in BuildLayout(branchResult.TabablzControl, secondChildBranch, layoutStructureBranchIndex))
                    yield return tuple;
            }
            else if (layoutStructureBranch.ChildSecondTabSetId.HasValue)
            {
                SetTabSetId(newSiblingTabablzControl, layoutStructureBranch.ChildSecondTabSetId.Value);
                yield return new Tuple<Guid, TabablzControl>(layoutStructureBranch.ChildSecondTabSetId.Value, newSiblingTabablzControl);
            }         
        }

        private static TabablzControl CreateTabablzControl()
        {
            return new TabablzControl();
        }        

        private static LayoutStructureBranch GetRoot(Dictionary<Guid, LayoutStructureBranch> branches)
        {
            var lookup = branches.Values.SelectMany(ChildBranchIds).Distinct().ToLookup(guid => guid);
            return branches.Values.Single(branch => !lookup.Contains(branch.Id));
        }

        private static IEnumerable<Guid> ChildBranchIds(LayoutStructureBranch branch)
        {
            if (branch.ChildFirstBranchId.HasValue)
                yield return branch.ChildFirstBranchId.Value;
            if (branch.ChildSecondBranchId.HasValue)
                yield return branch.ChildSecondBranchId.Value;
        }

        private void RunShutdown()
        {                        
            var windowCollection = Application.Current.Windows.OfType<MainWindow>();
            if (windowCollection.Count() == 1)
                RunApplicationShutdown();
        }

        private void RunApplicationShutdown()
        {
            //TODO, inject manual saver instead of general settings
            new ManualSaver().Save(_connectionCache, _generalSettings);            

            Application.Current.Shutdown();
        }
    }

    public class TabItemContainer
    {
        public TabItemContainer(Guid tabId, ITabContentLifetimeHost tabContentLifetimeHost, IBackingStoreWriter backingStoreWriter)
        {
            TabId = tabId;
            TabContentLifetimeHost = tabContentLifetimeHost;
            BackingStoreWriter = backingStoreWriter;
            ViewModel = tabContentLifetimeHost.ViewModel;
            Name = "New World";
        }

        public string Name { get; }

        public Guid TabId { get; }

        public ITabContentLifetimeHost TabContentLifetimeHost { get; }

        public IBackingStoreWriter BackingStoreWriter { get; }

        public object ViewModel { get; }
    }
}

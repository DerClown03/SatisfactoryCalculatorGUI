using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SatisfactoryCalculatorGUI.core;
using System.Windows.Media.Effects;
using System.Windows.Controls;
using SatisfactoryCalculatorGUI.MVVM.View.InstructionsView;
using SatisfactoryCalculatorGUI.MVVM.Model;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace SatisfactoryCalculatorGUI.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private ObservableCollection<LoadedRecipesModel> loadedRecipes { get; set; }
        public ObservableCollection<LoadedRecipesModel> LoadedRecipes
        {
            get => loadedRecipes;
            set
            {
                loadedRecipes = value;
                OnPropertyChanged();
            }
        }

        public StringFormatting CustomCalculations = new StringFormatting();
        public SatisfactoryCalculator SatisfactoryCalculator = new SatisfactoryCalculator();
        // Important (ViewCommands working)
        public RelayCommand QuantityChosenCommand { get; set; }
        public RelayCommand ItemQuantityCommand { get; set; }
        public RelayCommand RecipeOnceCommand { get; set; }
        public RelayCommand RecipeAlwaysCommand { get; set; }
        public RelayCommand SaveFactoryPlanCommand { get; set; }

        // Not Important (ViewCommands Working)
        public RelayCommand AmmosViewCommand { get; set; }
        public RelayCommand CommuicationsViewCommand { get; set; }
        public RelayCommand ConsumedViewCommand { get; set; }
        public RelayCommand ContainersViewCommand { get; set; }
        public RelayCommand ElectronicsViewCommand { get; set; }
        public RelayCommand FuelsViewCommand { get; set; }
        public RelayCommand GasViewCommand { get; set; }
        public RelayCommand IndustrialPartsViewCommand { get; set; }
        public RelayCommand IngotsViewCommand { get; set; }
        public RelayCommand LiquidsViewCommand { get; set; }
        public RelayCommand MineralsViewCommand { get; set; }
        public RelayCommand NuclearViewCommand { get; set; }
        public RelayCommand QuantumTechnologyViewCommand { get; set; }
        public RelayCommand SpecialViewCommand { get; set; }
        public RelayCommand StandardPartsViewCommand { get; set; }

        // Important (Working)
        public ItemQuantityViewModel ItemQuantityVM { get; set; }
        public RecipesViewModel RecipesVM { get; set; }
        public ResultsViewModel ResultsVM { get; set; }

        // Not Important (Working)
        public AmmosViewModel AmmosVM { set; get; }
        public CommunicationsViewModel CommunicationsVM { set; get; }
        public ConsumedViewModel ConsumedVM { set; get; }
        public ContainersViewModel ContainersVM { set; get; }
        public ElectronicsViewModel ElectronicsVM { set; get; }
        public FuelsViewModel FuelsVM { set; get; }
        public GasViewModel GasVM { set; get; }
        public IndustrialPartsViewModel IndustrialPartsVM { set; get; }
        public IngotsViewModel IngotsVM { set; get; }
        public LiquidsViewModel LiquidsVM { set; get; }
        public MineralsViewModel MineralsVM { set; get; }
        public NuclearViewModel NuclearVM { set; get; }
        public QuantumTechnologyViewModel QuantumTechnologyVM { set; get; }
        public SpecialViewModel SpecialVM { set; get; }
        public StandardPartsViewModel StandardPartsVM { set; get; }

        public string AllResultsInString { get; set; }
        public string ChosenItem { set; get; }
        public double QuantityOfItemDouble { set; get; }
        public int ChosenRecipeNumber { get; set; }

        // Not Important (working)
        private object _ItemListView;
        public object ItemListView
        {
            get => _ItemListView;
            set
            {
                _ItemListView = value;
                OnPropertyChanged();
            }
        }

        // Important (not working)
        private object _mainView;
        public object MainView
        {
            get => _mainView;
            set
            {
                _mainView = value;
                OnPropertyChanged();
            }
        }
        private string _blurEffectSetter;
        public string BlurEffectSetter
        {
            get => _blurEffectSetter;
            set
            {
                _blurEffectSetter = value;
                OnPropertyChanged();
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        private string defaultResources;
        public string DefaultResources
        {
            get => defaultResources;
            set
            {
                defaultResources = value;
                OnPropertyChanged();
            }
        }

        private string leftoverResources;
        public string LeftoverResources
        {
            get => leftoverResources;
            set
            {
                leftoverResources = value;
                OnPropertyChanged();
            }
        }

        private string buildingResources;
        public string BuildingResources
        {
            get => buildingResources;
            set 
            { 
                buildingResources = value;
                OnPropertyChanged();
            }
        }


        private string machinesTree;
        public string MachinesTree
        {
            get => machinesTree;
            set
            {
                machinesTree = value;
                OnPropertyChanged();
            }
        }

        private string machinesList;
        public string MachinesList
        {
            get => machinesList;
            set
            {
                machinesList = value;
                OnPropertyChanged();
            }
        }

        private string countedMachines;
        public string CountedMachines
        {
            get => countedMachines;
            set
            {
                countedMachines = value;
                OnPropertyChanged();
            }
        }


        public static event EventHandler<OnRecipeChosenEventArgs> OnRecipeChosen;
        public static event EventHandler<OnStartCalculatingEventArgs> OnStartCalculating;
        public static event EventHandler<EventArgs> OnCancelCalculation;
        public static event EventHandler<EventArgs> OnResetCancellationToken;

        public class OnStartCalculatingEventArgs : EventArgs
        {
            public string Item;
            public double ItemQuantity;
            public ObservableCollection<LoadedRecipesModel> LoadedRecipesCollection;
        }

        public class OnRecipeChosenEventArgs : EventArgs
        {
            public int ChosenRecipeIndex;
            public bool UseAlwaysThisRecipe;
        }
        public MainViewModel()
        {
            StringFormatting.OnShowResults += Testing_OnShowResults;

            LoadedRecipes = new ObservableCollection<LoadedRecipesModel>();
            BlurEffectSetter = "0";
            ImagePath = "/images/items/iron_ingot.png";

            // Important (not working)
            ItemQuantityVM = new ItemQuantityViewModel();
            RecipesVM = new RecipesViewModel();
            ResultsVM = new ResultsViewModel();

            // Not Important (working)
            AmmosVM = new AmmosViewModel();
            CommunicationsVM = new CommunicationsViewModel();
            ConsumedVM = new ConsumedViewModel();
            ContainersVM = new ContainersViewModel();
            ElectronicsVM = new ElectronicsViewModel();
            FuelsVM = new FuelsViewModel();
            GasVM = new GasViewModel();
            IndustrialPartsVM = new IndustrialPartsViewModel();
            IngotsVM = new IngotsViewModel();
            LiquidsVM = new LiquidsViewModel();
            MineralsVM = new MineralsViewModel();
            NuclearVM = new NuclearViewModel();
            QuantumTechnologyVM = new QuantumTechnologyViewModel();
            SpecialVM = new SpecialViewModel();
            StandardPartsVM = new StandardPartsViewModel();

            // Not Important (working)
            ItemListView = IngotsVM;

            AmmosViewCommand = new RelayCommand(o =>
            {
                ItemListView = AmmosVM;
            });

            CommuicationsViewCommand = new RelayCommand(o =>
            {
                ItemListView = CommunicationsVM;
            });

            ConsumedViewCommand = new RelayCommand(o =>
            {
                ItemListView = ConsumedVM;
            });

            ContainersViewCommand = new RelayCommand(o =>
            {
                ItemListView = ContainersVM;
            });

            ElectronicsViewCommand = new RelayCommand(o =>
            {
                ItemListView = ElectronicsVM;
            });

            FuelsViewCommand = new RelayCommand(o =>
            {
                ItemListView = FuelsVM;
            });

            GasViewCommand = new RelayCommand(o =>
            {
                ItemListView = GasVM;
            });

            IndustrialPartsViewCommand = new RelayCommand(o =>
            {
                ItemListView = IndustrialPartsVM;
            });

            IngotsViewCommand = new RelayCommand(o =>
            {
                ItemListView = IngotsVM;
            });

            LiquidsViewCommand = new RelayCommand(o =>
            {
                ItemListView = LiquidsVM;
            });

            MineralsViewCommand = new RelayCommand(o =>
            {
                ItemListView = MineralsVM;
            });

            NuclearViewCommand = new RelayCommand(o =>
            {
                ItemListView = NuclearVM;
            });

            QuantumTechnologyViewCommand = new RelayCommand(o =>
            {
                ItemListView = QuantumTechnologyVM;
            });

            SpecialViewCommand = new RelayCommand(o =>
            {
                ItemListView = SpecialVM;
            });

            StandardPartsViewCommand = new RelayCommand(o =>
            {
                ItemListView = StandardPartsVM;
            });

            ItemQuantityCommand = new RelayCommand(o =>
            {
                MainView = ItemQuantityVM;
            });

            // Important (not working)
            MainView = null;
            ItemQuantityCommand = new RelayCommand(o =>
            {
                MainView = ItemQuantityVM;
                BlurEffectSetter = "5";
                ImagePath = $"/images/items/{o}.png";
                ChosenItem = Convert.ToString(o);
            });

            QuantityChosenCommand = new RelayCommand(o =>
            {
                string QuantityOfItem = Convert.ToString(o);

                if (QuantityOfItem != "escape")
                {
                    MainView = RecipesVM;
                    QuantityOfItemDouble = SatisfactoryCalculator.StringToDouble(QuantityOfItem);
                    OnResetCancellationToken?.Invoke(this, EventArgs.Empty);
                    OnStartCalculatingEventArgs StartCalculatingEA = new OnStartCalculatingEventArgs
                    {
                        LoadedRecipesCollection = LoadedRecipes,
                        Item = ChosenItem,
                        ItemQuantity = QuantityOfItemDouble
                    };
                    OnStartCalculating?.Invoke(this, StartCalculatingEA);
                }
                else
                {
                    MainView = null;
                    BlurEffectSetter = "0";
                    LoadedRecipes.Clear();
                    OnCancelCalculation?.Invoke(this, EventArgs.Empty);
                }
            });

            RecipeOnceCommand = new RelayCommand(o =>
            {
                ChosenRecipeNumber = Convert.ToInt32(o);
                OnRecipeChosen?.Invoke(this, new OnRecipeChosenEventArgs { ChosenRecipeIndex = ChosenRecipeNumber, UseAlwaysThisRecipe = false });
            });

            RecipeAlwaysCommand = new RelayCommand(o =>
            {
                ChosenRecipeNumber = Convert.ToInt32(o);
                OnRecipeChosen?.Invoke(this, new OnRecipeChosenEventArgs { ChosenRecipeIndex = ChosenRecipeNumber, UseAlwaysThisRecipe = true });
            });

            SaveFactoryPlanCommand = new RelayCommand(o =>
            {
                CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
                openFileDialog.IsFolderPicker = true;
                CommonFileDialogResult result = openFileDialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    string folder = openFileDialog.FileName + "\\";
                    string name = "";
                    string fullpath;
                    string item = StringFormatting.ToReadableItem(ChosenItem);
                    string quantity = Convert.ToString(QuantityOfItemDouble);

                    name = $"{item} {quantity} ∕ min Factory.txt"; // Slash here is not a normal '/' as you cant use it in a normal filename, instead its a division slash
                    fullpath = folder + name;
                    Debug.WriteLine(fullpath);
                    File.WriteAllText(@fullpath, AllResultsInString);
                }
            });
        }

        public void Testing_OnShowResults(object sender, StringFormatting.OnShowResultsEventArgs e)
        {
            MainView = ResultsVM;
            DefaultResources = e.DefaultResourcesArgs;
            BuildingResources = e.BuildingResources;
            MachinesList = e.MachinesListArgs;
            MachinesTree = e.MachinesTreeArgs;
            CountedMachines = e.CountedMachinesArgs;
            LeftoverResources = e.LeftoverResourcesArgs;
            AllResultsInString = e.AllInformationArgs;
        }
    }
}

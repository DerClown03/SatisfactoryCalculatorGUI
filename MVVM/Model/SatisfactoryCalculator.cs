using SatisfactoryCalculatorGUI.core;
using SatisfactoryCalculatorGUI.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SatisfactoryCalculatorGUI.MVVM.Model
{
    public class SatisfactoryCalculator : ObservableObject
    {
        public static string RECIPEPATH = FindRecipesFolder();
        public static string[] DEFAULTRESOURCES = new string[] { "limestone", "iron_ore", "copper_ore", "caterium_ore", "coal", "raw_quartz", "sulfur", "bauxite", "uranium", "water", "crude_oil", "nitrogen_gas", "uranium_waste", "excited_photonic_matter", "sam" };
        public static string[] MACHINES = new string[] { "smelter", "foundry", "constructor", "assambler", "manufacturer", "refinery", "blender", "particle_accelerator", "quantum_encoder", "converter", "packager" };

        public static Dictionary<string, int> NeededMachines = new Dictionary<string, int>();
        public static Dictionary<string, int> NeededRecipes = new Dictionary<string, int>();
        public static Dictionary<string, int> NeededResources = new Dictionary<string, int>();
        public static Dictionary<string, int> NeededBuildingResources = new Dictionary<string, int>();
        public static Dictionary<string, int> FactoryTree = new Dictionary<string, int>();
        public static Dictionary<string, int> PermanentRecipes = new Dictionary<string, int>();
        
        public static Dictionary<string, MachineModel> MachineResources = CreateMachineModelHashMap(MACHINES);

        public static List<RecipeModel>[] AllRecipes = new List<RecipeModel>[MACHINES.Length];

        public static bool RecipeIndexRecived = false;
        public static bool AlwaysUseThisRecipe = false;
        public static int RecipeIndex = 0;
        public static bool IsRunning = false;
        static CancellationTokenSource cts = new CancellationTokenSource();

        public static event EventHandler<EventArgs> OnRecipesListUpdated;
        public static event EventHandler<OnCalculationFinishedEventArgs> OnCalculationFinished;

        public class OnCalculationFinishedEventArgs : EventArgs
        {
            public string FactoryTreeString;
            public string NeededRecipesString;
            public string NeededResourcesString;
            public string NeededMachinesString;
            public string NeededBuildingResourcesString;
        }

        public SatisfactoryCalculator()
        {
            MainViewModel.OnRecipeChosen += Testing_OnRecipeChosen;
            MainViewModel.OnStartCalculating += Testing_OnStartCalculating;
            MainViewModel.OnCancelCalculation += Testing_OnCancelCalculation;
            MainViewModel.OnResetCancellationToken += Testing_OnResetCancellationToken;
        }

        public static void ResetVariables()
        {
            NeededMachines.Clear();
            NeededRecipes.Clear();
            NeededResources.Clear();
            NeededBuildingResources.Clear();
            FactoryTree.Clear();
            PermanentRecipes.Clear();
        }

        private void Testing_OnResetCancellationToken(object sender, EventArgs e)
        {
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        private void Testing_OnCancelCalculation(object sender, EventArgs e)
        {
            cts.Cancel();
            IsRunning = false;
        }

        public void Testing_OnRecipeChosen(object sender, MainViewModel.OnRecipeChosenEventArgs e)
        {
            RecipeIndexRecived = true;
            RecipeIndex = e.ChosenRecipeIndex;
            AlwaysUseThisRecipe = e.UseAlwaysThisRecipe;
        }

        public async void Testing_OnStartCalculating(object sender, MainViewModel.OnStartCalculatingEventArgs e)
        {
            if (!IsRunning)
            {
                AllRecipes = ReadAllRecipes(RECIPEPATH, AllRecipes);
                //MachineResources = CreateMachineModelHashMap(MACHINES);
                IsRunning = true;
                try
                {
                    ResetVariables();
                    await calculateFactory(e.Item, e.ItemQuantity, e.LoadedRecipesCollection);

                    var test = HashMapToSortedString(NeededResources, DEFAULTRESOURCES, true, false, true);
                    var test2 = test;

                    OnCalculationFinishedEventArgs CalculationFinishedArgs = new OnCalculationFinishedEventArgs
                    {
                        FactoryTreeString = HashMapToSortedString(FactoryTree, null, true, false, true),
                        NeededRecipesString = HashMapToSortedString(NeededRecipes, MACHINES, false, true),
                        NeededResourcesString = HashMapToSortedString(NeededResources, DEFAULTRESOURCES, true, false, true),
                        NeededMachinesString = HashMapToSortedString(NeededMachines, MACHINES),
                        NeededBuildingResourcesString = HashMapToSortedString(NeededBuildingResources, null)
                    };

                    OnCalculationFinished?.Invoke(this, CalculationFinishedArgs);
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Calculation Cancelled");
                }
            }
        }

        static void NotMain(string[] args)
        {
            AllRecipes = ReadAllRecipes(RECIPEPATH, AllRecipes);

            string searchedItem = AskForItem();
            double wantedQuantity = AskForQuantity();

            // calculateFactory(searchedItem, wantedQuantity);
            Console.WriteLine();
            PrintHashMap(NeededMachines, MACHINES);
            Console.WriteLine();
            PrintHashMap(NeededRecipes, MACHINES, false, true);
            Console.WriteLine();
            PrintHashMap(NeededResources, DEFAULTRESOURCES, true);
            Console.WriteLine();
            PrintHashMap(FactoryTree, null, true);
            Console.WriteLine();
            PrintHashMap(NeededBuildingResources, null);
        }

        static async Task calculateFactory(string searchedItem, double wantedQuantity, ObservableCollection<LoadedRecipesModel> loadedRecipes ,string treeDepth = "", bool treeLast = true, bool treeFirst = true)
        {
            // Recipe Tree
            string treeOutput = treeDepth;
            if (treeFirst == false)
            {
                if (treeLast)
                {
                    treeOutput += "└──";
                    treeDepth += "   ";
                }
                else
                {
                    treeOutput += "├──";
                    treeDepth += "│  ";
                }
            }

            List<RecipeModel> searchedRecipes = GetRecipesWithOutput(searchedItem, loadedRecipes);
            int index = await GetRecipeIndex(searchedItem, searchedRecipes);
            RecipeIndexRecived = false;     // Dont place these variables in the function GetRecipe Index. They wont reset!
            AlwaysUseThisRecipe = false;    // Keep them here it works!
            RecipeIndex = 0;
            RecipeModel chosenRecipe = searchedRecipes[index];
            double itemsProducedInOneMachine = chosenRecipe.Outputs[chosenRecipe.DesiredOutputIndex].ProducedPerMinute;
            int neededMachines = Convert.ToInt32(Math.Ceiling(wantedQuantity / itemsProducedInOneMachine));
            AddToBuildingResources(neededMachines, chosenRecipe.NameOfMachine);
            NeededMachines = SaveOrAddToHashMap(NeededMachines, chosenRecipe.NameOfMachineReadable, neededMachines);
            NeededRecipes = SaveOrAddToHashMap(NeededRecipes, chosenRecipe.NameOfMachineReadable + " for " + chosenRecipe.DesiredOutputItemName, neededMachines);
            FactoryTree = SaveOrAddToHashMap(FactoryTree, treeOutput + RecipeModel.MakeStringReadable(searchedItem) + " in " + neededMachines + " " + chosenRecipe.NameOfMachineReadable + "s:", Convert.ToInt32(wantedQuantity));
            int i = 0;
            foreach (Input input in chosenRecipe.Inputs)
            {
                double itemQuantity = input.NeededPerMinute * neededMachines;
                if (!IsDefaultRessource(input.ItemName))
                {
                    bool lastIsDefaultResource = false;
                    if (i == chosenRecipe.Inputs.Length - 2) // Magic Number: Checks if we´re currently at the second to last Input Item
                                                             // to check then if the last Item is a default resource to choose the corrct FactoryTree output
                    {
                        lastIsDefaultResource = IsDefaultRessource(chosenRecipe.Inputs[i + 1].ItemName);
                    }
                    bool lastItem = i == chosenRecipe.Inputs.Length - 1 || lastIsDefaultResource;
                    await calculateFactory(input.ItemName, itemQuantity, loadedRecipes ,treeDepth, lastItem, false);
                }
                else
                {
                    NeededResources = SaveOrAddToHashMap(NeededResources, input.ItemNameReadable, Convert.ToInt32(itemQuantity));
                }
                i++;
            }
        }

        static void AddToBuildingResources(int quantity, string nameOfMachine)
        {
            MachineModel machine = MachineResources[nameOfMachine];
            for (int i = 0; i < machine.Items.Count; i++)
            {
                string itemNameReadable = RecipeModel.MakeStringReadable(machine.Items[i].ItemName);
                int itemAmount = machine.Items[i].AmountRequired;
                NeededBuildingResources = SaveOrAddToHashMap(NeededBuildingResources, itemNameReadable, itemAmount * quantity);
            }
        }

        static Dictionary<string, MachineModel> CreateMachineModelHashMap(string[] machines)
        {
            Dictionary<string, MachineModel> machineModels = new Dictionary<string, MachineModel>();
            for (int i = 0; i < machines.Length; i++)
            {
                machineModels.Add(machines[i], new MachineModel(machines[i]));
            }
            return machineModels;
        }

        static async Task<int> GetRecipeIndex(string searchedItem, List<RecipeModel> recipeList)
        {
            if (PermanentRecipes.ContainsKey(searchedItem))
            {
                return PermanentRecipes[searchedItem];
            }
            // PrintRecipeList(recipeList); // Debug Print
            await Task.Run(() => WaitForButtonPress(cts.Token));

            if (AlwaysUseThisRecipe)
            {
                PermanentRecipes.Add(recipeList[RecipeIndex].DesiredOutputItemRaw, RecipeIndex);
            }

            return RecipeIndex;
        }

        static string HashMapToSortedString(Dictionary<string, int> hashmap, string[]? sortingReference = null, bool keyfirst = false, bool reverse = false, bool addPerMinute = false)
        {
            if (sortingReference == null)
            {
                string noSortString = "";
                foreach (KeyValuePair<string, int> item in hashmap)
                {
                    if (keyfirst)
                    {
                        if (addPerMinute)
                        {
                            noSortString += item.Key + " " + item.Value + "/min\n";
                        }
                        else
                        {
                            noSortString += item.Key + " " + item.Value + "\n";
                        }
                    }
                    else
                    {
                        if (addPerMinute)
                        {
                            noSortString += item.Value + " " + item.Key + "/min\n";
                        }
                        else
                        {
                            noSortString += item.Value + " " + item.Key + "\n";
                        }
                    }
                }
                return noSortString.Remove(noSortString.Length - 1);
            }

            string[] copySortingReference = new string[sortingReference.Length]; // Copy is needed to avoid sorting mistakes due to the fact that strings are immutable
            sortingReference.CopyTo(copySortingReference, 0);                    // If not copied the Array will stay reversed and when calculating the next factory f.e. recipes wont be sorted right

            string[] sorted = new string[sortingReference.Length];
            bool sortedByDefaultResource = sortingReference == DEFAULTRESOURCES;

            if (reverse)
            {
                Array.Reverse(copySortingReference, 0, copySortingReference.Length);
            }

            foreach (KeyValuePair<string, int> item in hashmap)
            {
                for (int i = 0; i < sortingReference.Length; i++)
                {
                    // If not sorted by DEFAULTRESOURCES we need to look if the Key contains our sortingReference,
                    // because f.e. a part from NeededRecipes only contains the sortingReference but it will never be an exact match
                    if (!item.Key.Contains(RecipeModel.MakeStringReadable(sortingReference[i])) && !sortedByDefaultResource)
                    {
                        continue;
                    }
                    // If it is sorted by DEFAULRESOURCES we need to look for an exact match as Uranium Waste also contains Uranium (edgecase)
                    if (!(item.Key == RecipeModel.MakeStringReadable(sortingReference[i])) && sortedByDefaultResource)
                    {
                        continue;
                    }
                    if (keyfirst)
                    {
                        if(addPerMinute)
                        {
                            sorted[i] += item.Key + " " + item.Value + "/min\n";
                        }
                        else
                        {
                            sorted[i] += item.Key + " " + item.Value + "\n";
                        }
                    }
                    else
                    {
                        if (addPerMinute)
                        {
                            sorted[i] += item.Value + " " + item.Key + "/min\n";
                        }
                        else
                        {
                            sorted[i] += item.Value + " " + item.Key + "\n";
                        }
                    }
                }
            }

            string sortedString = "";
            for (int i = 0; i < sorted.Length; i++)
            {
                sortedString += sorted[i];
            }
            return sortedString.Remove(sortedString.Length - 1);
        }

        static void PrintHashMap(Dictionary<string, int> hashmap, string[] sortingReference = null, bool keyfirst = false, bool reverse = false)
        {
            Console.WriteLine(HashMapToSortedString(hashmap, sortingReference, keyfirst, reverse));
        }

        static Dictionary<string, int> SaveOrAddToHashMap(Dictionary<string, int> hashmap, string key, int value)
        {
            if (hashmap.ContainsKey(key))
            {
                hashmap[key] += value;
                return hashmap;
            }
            hashmap.Add(key, value);
            return hashmap;
        }

        static bool IsDefaultRessource(string item)
        {
            for (int i = 0; i < DEFAULTRESOURCES.Length; i++)
            {
                if (DEFAULTRESOURCES[i] == item)
                {
                    return true;
                }
            }
            return false;
        }

        static string AskForItem()
        {
            Console.WriteLine("Which item should be produced?");
            return Console.ReadLine();
        }

        static double AskForQuantity()
        {
            Console.WriteLine("How many items should be produced per minute?");
            return StringToDouble(Console.ReadLine());
        }

        static int ChooseRecipeFromList(List<RecipeModel> recipes)
        {
            Console.WriteLine("Which recipe do you want to use? Choose from 1 to " + recipes.Count + ": ");
            char[] splittedInput = Console.ReadLine().ToCharArray();
            if (splittedInput.Length == 0)
            {
                return 0;
            }
            if (splittedInput.Length == 1)
            {
                return (splittedInput[0] - '0') - 1; // Converts the char with a number to an actual int
            }
            if (splittedInput.Length == 2 && splittedInput[1] == 'p')
            {
                int index = (splittedInput[0] - '0') - 1;
                PermanentRecipes.Add(recipes[index].DesiredOutputItemRaw, index);
                return index;
            }
            return 0;
        }

        public static void WaitForButtonPress(CancellationToken cancellation)
        {
            while (!RecipeIndexRecived)
            {
                Thread.Sleep(1); // Apparantly needed to keep CPU usage down, 0 does not work
                cancellation.ThrowIfCancellationRequested();
            }
            return;
        }

        static void PrintRecipeList(List<RecipeModel> recipes)
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                PrintRecipe(recipes[i]);
            }
        }

        static List<RecipeModel> SortRecipesToNormal(List<RecipeModel> recipes)
        {
            List<RecipeModel> sortedRecipes = new List<RecipeModel>();
            int normalRecipeIndex = 0;
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i].IsAlternate == false)
                {
                    sortedRecipes.Add(recipes[i]);
                    normalRecipeIndex = i;
                    break;
                }
            }

            for (int i = 0; i < recipes.Count; i++)
            {
                if (i == normalRecipeIndex)
                {
                    continue;
                }
                sortedRecipes.Add(recipes[i]);
            }

            return sortedRecipes;
        }

        public static double StringToDouble(string numberstr)
        {
            double output = 0;
            string[] split = numberstr.Split(',', '.');
            output = Convert.ToInt32(split[0]);
            if (split.GetLength(0) >= 2) // Check if there is a decimal part
            {
                double dec = Math.Pow(10, split[1].Length);
                output += Convert.ToInt32(split[1]) / dec;
            }

            return output;
        }

        static List<RecipeModel> GetRecipesWithOutput(string itemName, ObservableCollection<LoadedRecipesModel> loadedRecipes)
        {
            List<RecipeModel> searchedRecipes = new List<RecipeModel>();
            for (int i = 0; i < AllRecipes.Length; i++)
            {
                for (int j = 0; j < AllRecipes[i].Count; j++)
                {
                    if (AllRecipes[i][j].HasOutput(itemName))
                    {
                        searchedRecipes.Add(AllRecipes[i][j]);
                    }
                }
            }
            searchedRecipes = SortRecipesToNormal(searchedRecipes);
            loadedRecipes.Clear();
            for (int i = 0; i < searchedRecipes.Count; i++)
            {
                loadedRecipes.Add(RecipeToObject(searchedRecipes[i]));
            }
            OnRecipesListUpdated?.Invoke(null, EventArgs.Empty);

            return searchedRecipes;
        }

        static LoadedRecipesModel RecipeToObject(RecipeModel recipe)
        {
            LoadedRecipesModel Recipe = new LoadedRecipesModel();
            int InputsCounter = recipe.NumberOfInputs;
            int OutputsCounter = recipe.NumberOfOutputs;
            int machineNumber = recipe.MachineIndex;

            Recipe.Machine = $"/images/machines/{MACHINES[machineNumber]}.png";

            switch (InputsCounter)
            {
                case 1:
                    Recipe.Input1 = $"/images/items/{recipe.Inputs[0].ItemName}.png";
                    Recipe.InputQuantity1 = $"{recipe.Inputs[0].NeededPerMinute}/min";
                    break;
                case 2:
                    Recipe.Input1 = $"/images/items/{recipe.Inputs[0].ItemName}.png";
                    Recipe.InputQuantity1 = $"{recipe.Inputs[0].NeededPerMinute}/min";

                    Recipe.Input2 = $"/images/items/{recipe.Inputs[1].ItemName}.png";
                    Recipe.InputQuantity2 = $"{recipe.Inputs[1].NeededPerMinute}/min";
                    break;
                case 3:
                    Recipe.Input1 = $"/images/items/{recipe.Inputs[0].ItemName}.png";
                    Recipe.InputQuantity1 = $"{recipe.Inputs[0].NeededPerMinute}/min";

                    Recipe.Input2 = $"/images/items/{recipe.Inputs[1].ItemName}.png";
                    Recipe.InputQuantity2 = $"{recipe.Inputs[1].NeededPerMinute}/min";

                    Recipe.Input3 = $"/images/items/{recipe.Inputs[2].ItemName}.png";
                    Recipe.InputQuantity3 = $"{recipe.Inputs[2].NeededPerMinute}/min";
                    break;
                case 4:
                    Recipe.Input1 = $"/images/items/{recipe.Inputs[0].ItemName}.png";
                    Recipe.InputQuantity1 = $"{recipe.Inputs[0].NeededPerMinute}/min";

                    Recipe.Input2 = $"/images/items/{recipe.Inputs[1].ItemName}.png";
                    Recipe.InputQuantity2 = $"{recipe.Inputs[1].NeededPerMinute}/min";

                    Recipe.Input3 = $"/images/items/{recipe.Inputs[2].ItemName}.png";
                    Recipe.InputQuantity3 = $"{recipe.Inputs[2].NeededPerMinute}/min";

                    Recipe.Input4 = $"/images/items/{recipe.Inputs[3].ItemName}.png";
                    Recipe.InputQuantity4 = $"{recipe.Inputs[3].NeededPerMinute}/min";
                    break;
                default:
                    break;
            }
            
            switch (OutputsCounter)
            {
                case 1:
                    Recipe.Output1 = $"/images/items/{recipe.Outputs[0].ItemName}.png";
                    Recipe.OutputQuantity1 = $"{recipe.Outputs[0].ProducedPerMinute}/min";
                    break;
                case 2:
                    Recipe.Output1 = $"/images/items/{recipe.Outputs[0].ItemName}.png";
                    Recipe.OutputQuantity1 = $"{recipe.Outputs[0].ProducedPerMinute}/min";

                    Recipe.Output2 = $"/images/items/{recipe.Outputs[1].ItemName}.png";
                    Recipe.OutputQuantity2 = $"{recipe.Outputs[1].ProducedPerMinute}/min";
                    break;
            }

            if (recipe.IsAlternateChar == 'n')
            {
                Recipe.AlternateRecipeIndicator = "Normal";
            }
            else if (recipe.IsAlternateChar == 'a')
            {
                Recipe.AlternateRecipeIndicator = "Alternate";
            }
            else
            {
                Recipe.AlternateRecipeIndicator = "Error";
            }

            return Recipe;
        }

        static List<RecipeModel>[] ReadAllRecipes(string recipePath, List<RecipeModel>[] allRecipes)
        {
            for (int i = 0; i < MACHINES.Length; i++)
            {
                allRecipes[i] = ReadMachinesRecipes(MACHINES[i], i);
            }
            return allRecipes;
        }

        static List<RecipeModel> ReadMachinesRecipes(string machine, int machineIndex)
        {
            List<RecipeModel> recipes = new List<RecipeModel>();
            string path = RECIPEPATH + "\\" + machine + ".txt";
            string[] recipeLines = File.ReadAllLines(@path);
            foreach (string recipeLine in recipeLines)
            {
                RecipeModel recipe = new RecipeModel(recipeLine, machine, machineIndex);
                recipes.Add(recipe);
            }
            return recipes;
        }

        // make a method that prints a single recipe in one line 
        static void PrintRecipe(RecipeModel recipe)
        {
            Console.Write(recipe.NumberOfInputs + ", " + recipe.NumberOfOutputs + ", i, ");
            foreach (Input input in recipe.Inputs)
            {
                Console.Write(input.ItemName + ", " + input.NeededPerMinute + ", ");
            }
            Console.Write("o, ");
            foreach (Output output in recipe.Outputs)
            {
                Console.Write(output.ItemName + ", " + output.ProducedPerMinute + ", ");
            }
            if (recipe.IsAlternate)
            {
                Console.Write("a");
            }
            else
            {
                Console.Write("n");
            }
            Console.WriteLine();
        }

        // Returns the path to the recipes folder
        static string FindRecipesFolder()
        {
            int counter = 0;
            string currentPath = Directory.GetCurrentDirectory();
            string recipesPath = Path.Combine(currentPath, "recipes");
            while (!Directory.Exists(recipesPath) && counter < 5)
            {
                currentPath = Directory.GetParent(currentPath).FullName;
                recipesPath = Path.Combine(currentPath, "recipes");
                // Console.WriteLine("Current Path: " + currentPath + " \nRecipes Path: " + recipesPath + "\n Found: " + Directory.Exists(recipesPath));
                counter++;
            }
            if (counter == 5)
            {
                throw new Exception("Could not find recipes folder");
            }
            return recipesPath;
        }
    }
}

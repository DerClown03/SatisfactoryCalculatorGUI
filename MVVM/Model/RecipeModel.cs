using System;
using System.Collections.Generic;
using System.Text;

namespace SatisfactoryCalculatorGUI.MVVM.Model
{
    public class Input
    {
        public string ItemName { get; private set; }
        public string ItemNameReadable { get; private set; }
        public double NeededPerMinute { get; private set; }

        public Input(string _itemName, double _neededPerMinute)
        {
            ItemName = _itemName;
            ItemNameReadable = RecipeModel.MakeStringReadable(_itemName);
            NeededPerMinute = _neededPerMinute;
        }
    }

    public class Output
    {
        public string ItemName { get; private set; }
        public string ItemNameReadable { get; private set; }
        public double ProducedPerMinute { get; private set; }

        public Output(string _itemName, double _producedPerMinute)
        {
            ItemName = _itemName;
            ItemNameReadable = RecipeModel.MakeStringReadable(_itemName);
            ProducedPerMinute = _producedPerMinute;
        }
    }

    public class RecipeModel
    {
        public int NumberOfOutputs { get; private set; }
        public int NumberOfInputs { get; private set; }

        public Input[] Inputs { get; private set; }
        public Output[] Outputs { get; private set; }

        public bool IsAlternate { get; private set; }
        public char IsAlternateChar { get; private set; }

        public int MachineIndex { get; private set; }
        public string NameOfMachine { get; private set; }
        public string NameOfMachineReadable { get; private set; }

        public int DesiredOutputIndex { get; private set; } = 0;
        public string DesiredOutputItemName { get; private set; }
        public string DesiredOutputItemRaw { get; private set; }

        public RecipeModel(string recipeInString, string _machine, int _machineIndex)
        {
            this.MachineIndex = _machineIndex;
            this.NameOfMachine = _machine;
            this.NameOfMachineReadable = MakeStringReadable(_machine);

            // Split the string into an array of strings
            string[] recipeInArray = recipeInString.Split(", ");
            // The first two numbers are the number of inputs and outputs
            this.NumberOfInputs = int.Parse(recipeInArray[0]);
            this.NumberOfOutputs = int.Parse(recipeInArray[1]);
            // Create the arrays of inputs and outputs
            this.Inputs = new Input[NumberOfInputs];
            this.Outputs = new Output[NumberOfOutputs];

            int saverCount = 0;
            for (int i = GetIndexOfWord(recipeInArray, "i") + 1; i < this.NumberOfInputs * 2 + GetIndexOfWord(recipeInArray, "i"); i++) // +1 is needed to jump directly to the first ItemName instead of looking at "i"
            {
                Input input = new Input(recipeInArray[i], StringToDouble(recipeInArray[i + 1]));
                this.Inputs[saverCount] = input;
                i++;
                saverCount++;
            }

            saverCount = 0;
            for (int i = GetIndexOfWord(recipeInArray, "o") + 1; i < this.NumberOfOutputs * 2 + GetIndexOfWord(recipeInArray, "o"); i++) // +1 is needed to jump directly to the first ItemName instead of looking at "o"
            {
                Output output = new Output(recipeInArray[i], StringToDouble(recipeInArray[i + 1]));
                this.Outputs[saverCount] = output;
                i++;
                saverCount++;
            }

            SaveAlternateStatus(recipeInArray);
        }

        public bool HasOutput(string searchedItem)
        {
            int counter = 0;
            foreach (Output item in this.Outputs)
            {
                if (item.ItemName == searchedItem)
                {
                    this.DesiredOutputIndex = counter;
                    this.DesiredOutputItemName = MakeStringReadable(item.ItemName);
                    this.DesiredOutputItemRaw = item.ItemName;
                    return true;
                }
                counter++;
            }
            return false;
        }

        public static string MakeStringReadable(string item)
        {
            if (item.Length == 0)
            {
                throw new Exception("String conversion error: String is empty");
            }

            string[] words = item.Split('_');
            string ReadableString = "";
            foreach (string word in words)
            {
                ReadableString = ReadableString + char.ToUpper(word[0]) + word.Substring(1) + " ";
            }

            if (ReadableString[ReadableString.Length - 1] == ' ')
            {
                ReadableString = ReadableString.Remove(ReadableString.Length - 1);
            }

            return ReadableString;
        }

        private void SaveAlternateStatus(string[] recipe)
        {
            if (recipe[recipe.Length - 1] == "n")
            {
                this.IsAlternate = false;
                this.IsAlternateChar = 'n';
            }
            else if (recipe[recipe.Length - 1] == "a")
            {
                this.IsAlternate = true;
                this.IsAlternateChar = 'a';
            }
            else
            {
                throw new Exception("The last character in the recipe string must be either n or a");
            }
        }

        private int GetIndexOfWord(string[] array, string word)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == word)
                {
                    return i;
                }
            }
            return -1;
        }

        public static double StringToDouble(string numberstr)
        {
            double output = 0;
            string[] split = numberstr.Split(',', '.');
            output = Convert.ToInt32(split[0]);
            if (split.GetLength(0) >= 2) // Check if there is a decimal part
            {
                double dec = 10;
                for (int i = 0; i < split[1].Length - 1; i++)
                {
                    dec *= 10;
                }
                output += Convert.ToInt32(split[1]) / dec;
            }

            return output;
        }
    }
}

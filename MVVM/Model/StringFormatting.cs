using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SatisfactoryCalculatorGUI.MVVM.Model
{
    public class StringFormatting
    {
        public class OnShowResultsEventArgs : EventArgs
        {
            public string CountedMachinesArgs;
            public string MachinesListArgs;
            public string MachinesTreeArgs;
            public string DefaultResourcesArgs;
            public string BuildingResources;
            public string LeftoverResourcesArgs;
            public string AllInformationArgs;
        }

        public static event EventHandler<OnShowResultsEventArgs> OnShowResults;
        public StringFormatting()
        {
            SatisfactoryCalculator.OnCalculationFinished += Testing_OnCalculationFinished;
        }

        private void Testing_OnCalculationFinished(object sender, SatisfactoryCalculator.OnCalculationFinishedEventArgs e)
        {
            string AllInformation = $"Diagram\n{e.FactoryTreeString}\n\nAll Recipes\n{e.NeededRecipesString}\n\nAll Machines\n{e.NeededMachinesString}\n\nResources\n{e.NeededResourcesString}\n\nLeftover Resources\n{e.LeftoverResourcesString}\n\nBuilding Resources\n{e.NeededBuildingResourcesString}\n ";

            OnShowResultsEventArgs ShowResultsEA = new OnShowResultsEventArgs
            {
                CountedMachinesArgs = e.NeededMachinesString,
                MachinesListArgs = e.NeededRecipesString,
                MachinesTreeArgs = e.FactoryTreeString,
                DefaultResourcesArgs = e.NeededResourcesString,
                BuildingResources = e.NeededBuildingResourcesString,
                LeftoverResourcesArgs = e.LeftoverResourcesString,
                AllInformationArgs = AllInformation
            };

            OnShowResults?.Invoke(this, ShowResultsEA);
        }

        public static string ToReadableItem(string item)
        {
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
    }
}

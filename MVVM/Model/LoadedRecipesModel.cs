using System;
using System.Collections.Generic;
using System.Text;

namespace SatisfactoryCalculatorGUI.MVVM.Model
{
    public class LoadedRecipesModel
    {
        // Number of Inputs/Outputs
        public string CountInputs { get; set; }
        public string CountOutputs { get; set; }

        // Input indicator
        public string InputIndicator { get; set; }

        // Inputs
        public string Input1 { get; set; }
        public string InputQuantity1 { get; set; }

        public string Input2 { get; set; }
        public string InputQuantity2 { get; set; }

        public string Input3 { get; set; }
        public string InputQuantity3 { get; set; }

        public string Input4 { get; set; }
        public string InputQuantity4 { get; set; }

        // Output indicator
        public string OutputIndicator { get; set; }

        // Outputs
        public string Output1 { get; set; }
        public string OutputQuantity1 { get; set; }

        public string Output2 { get; set; }
        public string OutputQuantity2 { get; set; }

        // AlternateRecipeIndicator
        public string AlternateRecipeIndicator { get; set; }

        // MaschineNumber
        public string Machine { get; set; }

        public string SkipRecipe { get; set; }
        public string SkipRecipeNote { get; set; }
    }
}

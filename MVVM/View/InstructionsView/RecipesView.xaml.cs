using SatisfactoryCalculatorGUI.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatisfactoryCalculatorGUI.MVVM.View.InstructionsView
{
    /// <summary>
    /// Interaktionslogik für RecipesView.xaml
    /// </summary>
    public partial class RecipesView : UserControl
    {
        public RecipesView()
        {
            InitializeComponent();
            SatisfactoryCalculator.OnRecipesListUpdated += Testing_OnRecipesListUpdated;
        }

        private void Testing_OnRecipesListUpdated(object sender, EventArgs e)
        {
            RecipesList.SelectedIndex = 0;
        }
    }
}

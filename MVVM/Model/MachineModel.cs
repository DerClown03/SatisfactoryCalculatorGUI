using System;
using System.Collections.Generic;
using System.Text;

namespace SatisfactoryCalculatorGUI.MVVM.Model
{
    public class Item
    {
        public string ItemName { get; set; }
        public int AmountRequired { get; set; }

        public Item(string _itemName, int _amountRequired)
        {
            this.ItemName = _itemName;
            this.AmountRequired = _amountRequired;
        }
    }

    public class MachineModel
    {
        //private Dictionary<string, string[,]> MACHINESDICT = new Dictionary<string, string[,]>();

        public List<Item> Items = new List<Item>();

        public MachineModel(string machine)
        {
            Dictionary<string, string[,]> MACHINESDICT = new Dictionary<string, string[,]>();
            MACHINESDICT.Add("smelter", new string[,] { { "iron_rod", "5" }, { "wire", "8" }, { "concrete", "6" } }); // , { "concrete", "6" }
            MACHINESDICT.Add("foundry", new string[,] { { "modular_frame", "10" }, { "rotor", "10" }, { "concrete", "20" } }); // , { "concrete", "8" }
            MACHINESDICT.Add("constructor", new string[,] { { "reinforced_iron_plate", "2" }, { "cable", "8" } }); // , { "concrete", "9" }
            MACHINESDICT.Add("assambler", new string[,] { { "reinforced_iron_plate", "8" }, { "rotor", "4" }, { "cable", "10" } }); // , { "concrete", "17" }
            MACHINESDICT.Add("manufacturer", new string[,] { { "motor", "5" }, { "heavy_modular_frame", "10" }, { "cable", "50" }, { "plastic", "50" } }); // , { "concrete", "49" }
            MACHINESDICT.Add("refinery", new string[,] { { "motor", "10" }, { "encased_industrial_beam", "10" }, { "steel_pipe", "30" }, { "copper_sheet", "20" } }); // , { "concrete", "22" }
            MACHINESDICT.Add("blender", new string[,] { { "motor", "20" }, { "heavy_modular_frame", "10" }, { "aluminum_casing", "50" }, { "radio_control_unit", "5" } }); // , { "concrete", "34" }
            MACHINESDICT.Add("particle_accelerator", new string[,] { { "radio_control_unit", "25" }, { "electromagnetic_control_rod", "100" }, { "supercomputer", "10" }, { "cooling_system", "50" }, { "fused_modular_frame", "20" }, { "turbo_motor", "10" } }); // , { "concrete", "100" }
            MACHINESDICT.Add("packager", new string[,] { { "steel_beam", "20" }, { "rubber", "10" }, { "plastic", "10" } }); // , { "concrete", "7" }

            for (int i = 0; i < MACHINESDICT[machine].GetLength(0); i++)
            {
                Items.Add(new Item(MACHINESDICT[machine][i, 0], int.Parse(MACHINESDICT[machine][i, 1])));
            }
        }
    }
}

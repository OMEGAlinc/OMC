using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OMC
{
    public class ModEntry : Mod
    {
        private ModConfig Config = new();
        public static ModEntry Instance { get; private set; } = null!;

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            // Load configuration
            Config = helper.ReadConfig<ModConfig>();

            // Register commands
            helper.ConsoleCommands.Add("setmenukey", "Changes the keybind to open the menu. This can also be done in the config.json. Usage: setmenukey <key>", (cmd, args) => ChangeMenuKeybind(args));
            helper.ConsoleCommands.Add("finishbuildings", "Finish all buildings under construction.", (_, _) => FinishBuildings());
            helper.ConsoleCommands.Add("sleepanywhere", "Save and sleep from your current location.", (_, _) => SleepAnywhere());
            helper.ConsoleCommands.Add("growall", "Instantly grow all crops to their final stage.", (_, _) => GrowAllCrops());
            helper.ConsoleCommands.Add("npclocations", "Lists the current locations of all NPCs.", (_, _) => ListNpcLocations());
            helper.ConsoleCommands.Add("listanimals", "Lists all animals on the farm.", (_, _) => ListAnimals());
            helper.ConsoleCommands.Add("petall", "Pet all animals on the farm.", (_, _) => PetAllAnimals());
            helper.ConsoleCommands.Add("waterallcrops", "Water all crops on the farm.", (_, _) => WaterAllCrops());
            helper.ConsoleCommands.Add("skillincrease", "Increase skill points. Usage: skillincrease <skill> <amount>", SkillIncrease);

            // Keybind for menu
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu == null && Context.IsWorldReady && Config.MenuKey.JustPressed())
            {
                Game1.activeClickableMenu = new ModMenu();
            }
        }

        private void ChangeMenuKeybind(string[] args)
        {
            if (args.Length != 1)
            {
                Monitor.Log("Changes the keybind to open the menu. This can also be done in the config.json. Usage: setmenukey <key>", LogLevel.Info);
                return;
            }

            try
            {
                Config.MenuKey = KeybindList.Parse(args[0]);
                Helper.WriteConfig(Config);
                Monitor.Log($"Menu keybind updated to: {Config.MenuKey}", LogLevel.Info);
            }
            catch
            {
                Monitor.Log($"Invalid key '{args[0]}'. Please provide a valid key.", LogLevel.Warn);
            }
        }

        public static void FinishBuildings()
        {
            Farm farm = Game1.getFarm();
            foreach (Building building in farm.buildings)
            {
                if (building.daysOfConstructionLeft.Value > 0 || building.daysUntilUpgrade.Value > 0)
                {
                    building.daysOfConstructionLeft.Value = 0;
                    building.daysUntilUpgrade.Value = 0;
                }
            }
            Game1.addHUDMessage(new HUDMessage("All buildings are now complete!"));
        }

        public static void SleepAnywhere()
        {
            Game1.player.isInBed.Value = true;
            Game1.playSound("coin");
            Game1.NewDay(600f);
        }

        public static void GrowAllCrops()
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> feature in location.terrainFeatures.Pairs)
                {
                    if (feature.Value is HoeDirt hoeDirt && hoeDirt.crop != null)
                    {
                        hoeDirt.crop.growCompletely();
                    }
                }
            }
            Game1.addHUDMessage(new HUDMessage("All crops are fully grown!"));
        }

        public static void ListNpcLocations()
        {
            foreach (NPC npc in Utility.getAllCharacters())
            {
                if (npc.IsVillager)
                {
                    Instance.Monitor.Log($"{npc.Name} is in {npc.currentLocation?.Name ?? "Unknown location"}.", LogLevel.Info);
                }
            }
        }

        public static void ListAnimals()
        {
            Farm farm = Game1.getFarm();
            foreach (FarmAnimal animal in farm.getAllFarmAnimals())
            {
                Instance.Monitor.Log($"Animal: {animal.displayName}, Type: {animal.type.Value}, Age: {animal.age.Value}, Friendship: {animal.friendshipTowardFarmer}", LogLevel.Info);
            }
        }

        public static void PetAllAnimals()
        {
            Farm farm = Game1.getFarm();
            foreach (FarmAnimal animal in farm.getAllFarmAnimals())
            {
                if (!animal.wasPet.Value)
                {
                    animal.pet(Game1.player);
                }
            }
            Game1.addHUDMessage(new HUDMessage("All animals have been pet."));
        }

        public static void WaterAllCrops()
        {
            Farm farm = Game1.getFarm();
            foreach (KeyValuePair<Vector2, TerrainFeature> feature in farm.terrainFeatures.Pairs)
            {
                if (feature.Value is HoeDirt hoeDirt && hoeDirt.crop != null)
                {
                    hoeDirt.state.Value = 1; // Set to watered state
                }
            }
            Game1.addHUDMessage(new HUDMessage("All crops are watered!"));
        }

        public static void SkillIncrease(string command, string[] args)
        {
            if (args.Length < 2)
            {
                Instance.Monitor.Log("Usage: skillincrease <skill> <amount>", LogLevel.Info);
                return;
            }

            string skillName = args[0].ToLower();
            if (!int.TryParse(args[1], out int amount))
            {
                Instance.Monitor.Log("Invalid amount provided. Please enter a valid number.", LogLevel.Info);
                return;
            }

            switch (skillName)
            {
                case "farming":
                    Game1.player.gainExperience(Farmer.farmingSkill, amount);
                    break;
                case "fishing":
                    Game1.player.gainExperience(Farmer.fishingSkill, amount);
                    break;
                case "foraging":
                    Game1.player.gainExperience(Farmer.foragingSkill, amount);
                    break;
                case "mining":
                    Game1.player.gainExperience(Farmer.miningSkill, amount);
                    break;
                case "combat":
                    Game1.player.gainExperience(Farmer.combatSkill, amount);
                    break;
                default:
                    Instance.Monitor.Log($"Unknown skill: {skillName}. Valid options: farming, fishing, foraging, mining, combat.", LogLevel.Info);
                    return;
            }

            Instance.Monitor.Log($"Added {amount} experience points to {skillName}.", LogLevel.Info);
        }
    }
}

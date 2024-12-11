using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using GenericModConfigMenu;
using StardewModdingAPI.Utilities;
using StardewValley;
using static StardewValley.Debris;
using SObject = StardewValley.Object;
using StardewModdingAPI.Mods.ConsoleCommands.Framework;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValley.Objects.Trinkets;
namespace Item_Every_Second
{
    internal sealed class ModEntry : Mod
    {
        [AllowNull]
        public ModConfig Config;

        public List<Item> Items = new List<Item>();

        [AllowNull]
        public static ModEntry Instance;

        public Random ItemRandom = new Random();

        // Not the best way but idc tbh
        public static HashSet<Type> ObjGenerators = new HashSet<Type> {
            typeof(MeleeWeapon),  
            typeof(Axe),  
            typeof(Hoe),  
            typeof(CombinedRing),  
            typeof(Pickaxe),  
            typeof(Ring),  
            typeof(Raft),  
            typeof(BedFurniture),  
            typeof(Boots),  
            typeof(BreakableContainer),  
            typeof(Cask),  
            typeof(Chest),  
            typeof(Clothing),  
            typeof(ColoredObject),  
            typeof(CrabPot),  
            typeof(Fence),  
            typeof(FishingRod),  
            typeof(FishTankFurniture),  
            typeof(Hat),  
            typeof(IndoorPot),  
            typeof(ItemPedestal),  
            typeof(Lantern),  
            typeof(Mannequin),  
            typeof(MilkPail),  
            typeof(MiniJukebox),  
            typeof(Pan),  
            typeof(PetLicense),  
            typeof(Phone),  
            typeof(RandomizedPlantFurniture),  
            typeof(Shears),  
            typeof(Sign),  
            typeof(Slingshot),  
            typeof(SpecialItem),  
            typeof(StorageFurniture),  
            typeof(Torch),  
            typeof(Trinket),  
            typeof(TV),  
            typeof(Wallpaper),  
            typeof(Wand),  
            typeof(WateringCan),  
            typeof(WoodChipper),  
            typeof(Workbench),  
            typeof(Furniture),  
            typeof(Tool),  
            typeof(StardewValley.Object),
        };

        public override void Entry(IModHelper helper)
        {
            Instance = this;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdate;
            Config = helper.ReadConfig<ModConfig>();
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Items.Clear(); // clear list before use
            List<Item> ItemsToAdd = new ItemRepository().GetAll()
                .Select(item => item.Item)
                .Where(item => !Items.Contains(item))
                .ToList();

            Items.AddRange(ItemsToAdd);
        }

        private void OnUpdate(object? sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && Game1.activeClickableMenu == null && Game1.game1.IsActive)
            {
                if (Config.Time != 0f && e.IsMultipleOf((uint)(Config.Time * 60f))) {
                    GenerateItem();
                }
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Main",
                tooltip: () => "The Main Configs for the mod"
            );

            // add some config options
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Time",
                getValue: () => Config.Time,
                setValue: value => Config.Time = value,
                tooltip: () => "Amount of time between randomly generated Items. Set to 0 to have none generate."
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Stacks",
                getValue: () => Config.Stacks,
                setValue: value => Config.Stacks = value,
                tooltip: () => $"How many items should spawn.",
                min: 1
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Extra",
                tooltip: () => "Additional mod configs for the mod"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable Unshippable Items",
                getValue: () => Config.NoUnShipItems,
                setValue: value => Config.NoUnShipItems = value,
                tooltip: () => "Makes it so you don't get Unshippable items"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable 0 Sell Price Items",
                getValue: () => Config.No0SellPriceItems,
                setValue: value => Config.No0SellPriceItems = value,
                tooltip: () => "Makes it so you don't get 0 Sell Price items"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable Error Items",
                getValue: () => Config.NoErrorItems,
                setValue: value => Config.NoErrorItems = value,
                tooltip: () => "Makes it so you don't get Error items"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable Special Items",
                getValue: () => Config.NoSpecialItems,
                setValue: value => Config.NoSpecialItems = value,
                tooltip: () => "Makes it so you don't get Special items"
            );
            foreach (Type newObjectType in ObjGenerators) {
                string newObject = newObjectType.Name;
                if (!Config.Chances.ContainsKey(newObject)) Config.Chances.Add(newObject, 0);
                if (!Config.AllowChances.ContainsKey(newObject)) Config.AllowChances.Add(newObject, false);
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => $"Custom {newObject} Chance",
                    getValue: () => Config.AllowChances[newObject],
                    setValue: value => Config.AllowChances[newObject] = value,
                    tooltip: () => $"If Enabled {newObject}s will have a custom chance to spawn if False {newObject}s will have the default parent chance."
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => $"{newObject} Rarity",
                    getValue: () => Config.Chances[newObject],
                    setValue: value => Config.Chances[newObject] = value,
                    tooltip: () => $"How Rare Should {newObject}s Be The Higher the Rarer.",
                    min: 0,
                    max: 100,
                    interval: 1
                );
            }
        }

        public void GenerateItem()
        {     
            Debris debris;
            try
            {
                Vector2 pos = Game1.player != null ? Game1.player.getStandingPosition() - new Vector2(32,64): new Vector2(0,0);
                Item item = Items[ItemRandom.Next(0, Items.Count - 1)];
                double NextDouble = ItemRandom.NextDouble();
                Type NextType = item.GetType();
                bool ChancesMatch = true;
                if (ObjGenerators.Contains(NextType) && Config.AllowChances[NextType.Name]) {
                    ChancesMatch = GetChance(NextType, NextDouble);
                }
                if (
                    !item.canBeShipped() && Config.NoUnShipItems ||
                    item.salePrice() <= 0 && Config.No0SellPriceItems ||
                    item.Name.Contains("Error Item") is true && Config.NoErrorItems ||
                    item is SpecialItem && Config.NoSpecialItems ||
                    !ChancesMatch
                )
                {
                    GenerateItem();
                    return;
                }
                item.stack.Value = Config.Stacks;
                debris = new Debris(item, pos);
                Game1.currentLocation.debris.Add(debris);
            }
            catch (Exception)
            {
                GenerateItem();
                throw;
            }
        }
        public bool GetChance(Type Type1, double Multiplier) {
            return Multiplier < Convert.ToDouble(Config.Chances[Type1.Name]) / 100.0;
        }
    }
}
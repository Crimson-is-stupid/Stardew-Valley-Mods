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
namespace Item_Every_Second
{
    internal sealed class ModEntry : Mod
    {
        [AllowNull]
        public ModConfig Config;

        public List<Item> Items = new List<Item>();

        [AllowNull]
        public static ModEntry Instance;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdate;
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
            if (Context.IsWorldReady && Game1.activeClickableMenu == null)
            {
                if (this.Config.Time != 0f && e.IsMultipleOf((uint)(this.Config.Time * 60f))) {
                    generateItem();
                }
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            // add some config options
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Time",
                getValue: () => this.Config.Time,
                setValue: value => this.Config.Time = value,
                tooltip: () => "Amount of time between randomly generated Items. Set to 0 to have none generate."
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Stacks",
                getValue: () => this.Config.Stacks,
                setValue: value => this.Config.Stacks = value,
                tooltip: () => $"How many items should spawn.",
                min: 1
            );
        }

        public void generateItem()
        {     
            Debris debris;
            try
            {
                Vector2? pos = Game1.player != null ? Game1.player.getStandingPosition() : new Vector2(0,0);
                Item item = Items[new Random().Next(0, Items.Count - 1)];
                item.stack.Value = this.Config.Stacks;
                debris = new Debris(item, (Vector2)pos);
                if (debris.item?.Name.Contains("Error Item") is false or null)
                    Game1.currentLocation.debris.Add(debris);
                else this.generateItem();
            }
            catch (Exception)
            {
                this.generateItem();
                throw;
            }
            
        }
    }
}
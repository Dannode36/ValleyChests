using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using SMUI;
using SMUI.Elements;
using Microsoft.Xna.Framework.Graphics;

namespace ValleyChests
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        readonly RootElement root = new();
        Button stealAllButton = new();
        Button dumpAllButton = new();

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += OnUpdateTick;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.WindowResized += OnWindowResized;

            stealAllButton = new(helper.ModContent.Load<Texture2D>("assets/stealButton.png"), new(0, 0, 16, 16))
            {
                BoxDraw = false,
                Tooltip = "Take all"
            };
            root.AddChild(stealAllButton);

            dumpAllButton = new(helper.ModContent.Load<Texture2D>("assets/dumpButton.png"), new(0, 0, 16, 16))
            {
                BoxDraw = false,
                Tooltip = "Add all from inventory"
            };
            root.AddChild(dumpAllButton);
        }

        private void OnWindowResized(object? sender, WindowResizedEventArgs e)
        {
            stealAllButton.LocalPosition = new(100, 100);
        }

        private void OnUpdateTick(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            root.Update();
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (e.NewMenu != null 
                && e.NewMenu is ItemGrabMenu itemGrabmenu 
                && itemGrabmenu.fillStacksButton != null)
            {
                stealAllButton.Callback = (e) =>
                {
                    var inv = itemGrabmenu.ItemsToGrabMenu.actualInventory;
                    for (int i = 0; i < inv.Count; i++)
                    {
                        inv[i] = Game1.player.addItemToInventory(inv[i]);
                    }
                };
                stealAllButton.LocalPosition = itemGrabmenu.fillStacksButton.getVector2() + new Vector2(64 + 16, 0);

                dumpAllButton.Callback = (e) =>
                {
                    var inv = itemGrabmenu.ItemsToGrabMenu.actualInventory;
                    for (int i = 0; i < inv.Count; i++)
                    {
                        inv[i] = Game1.player.addItemToInventory(inv[i]);
                    }
                };
                dumpAllButton.LocalPosition = itemGrabmenu.fillStacksButton.getVector2() + new Vector2(64 + 16, 64 + 16);
            }
        }

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if(Game1.activeClickableMenu is ItemGrabMenu itemGrabmenu 
                && itemGrabmenu.fillStacksButton != null)
            {
                root.Draw(e.SpriteBatch);
                itemGrabmenu.drawMouse(e.SpriteBatch);
            }
        }
    }
}

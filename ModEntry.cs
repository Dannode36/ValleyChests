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

            dumpAllButton = new(helper.ModContent.Load<Texture2D>("assets/dumpButton.png"), new(0, 0, 16, 16))
            {
                BoxDraw = false,
                Tooltip = "Add All From Inventory"
            };
            root.AddChild(dumpAllButton);

            stealAllButton = new(helper.ModContent.Load<Texture2D>("assets/stealButton.png"), new(0, 0, 16, 16))
            {
                BoxDraw = false,
                Tooltip = "Take All From Chest"
            };
            root.AddChild(stealAllButton);
        }

        private void OnWindowResized(object? sender, WindowResizedEventArgs e)
        {
            //stealAllButton.LocalPosition = new(100, 100);
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
                    var grabableInv = itemGrabmenu.ItemsToGrabMenu.actualInventory;
                    for (int i = 0; i < grabableInv.Count; i++)
                    {
                        grabableInv[i] = Game1.player.addItemToInventory(grabableInv[i]);
                    }
                };
                stealAllButton.LocalPosition = itemGrabmenu.fillStacksButton.getVector2() + new Vector2(64 + 16, 64 + 16);

                dumpAllButton.Callback = (e) =>
                {
                    var playerInv = itemGrabmenu.inventory.actualInventory;

                    for (int i = 0; i < playerInv.Count; i++)
                    {
                        playerInv[i] = itemGrabmenu.ItemsToGrabMenu.tryToAddItem(playerInv[i]);

                        //Weird af fix (assumes worst case that item will not stack cleanly)
                        //Probably due to Stardew skipping slots if actualInventory is smaller than
                        //the Grab Menu's actual capacity
                        if (playerInv[i] != null && itemGrabmenu.ItemsToGrabMenu.actualInventory.Count < itemGrabmenu.inventory.capacity)
                        {
                            itemGrabmenu.ItemsToGrabMenu.actualInventory.Add(null);
                            playerInv[i] = itemGrabmenu.ItemsToGrabMenu.tryToAddItem(playerInv[i]);
                        }
                    }
                };
                dumpAllButton.LocalPosition = itemGrabmenu.fillStacksButton.getVector2() + new Vector2(64 + 16, 0);
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

                //Redraw mouse and tooltips over SMUI elements
                itemGrabmenu.drawMouse(e.SpriteBatch);
                if (itemGrabmenu.hoverText != null 
                    && (itemGrabmenu.hoveredItem == null || itemGrabmenu.hoveredItem == null || itemGrabmenu.ItemsToGrabMenu == null))
                {
                    if (itemGrabmenu.hoverAmount > 0)
                    {
                        IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabmenu.hoverText, "", null, heldItem: true, -1, 0, null, -1, null, itemGrabmenu.hoverAmount);
                    }
                    else
                    {
                        IClickableMenu.drawHoverText(e.SpriteBatch, itemGrabmenu.hoverText, Game1.smallFont);
                    }
                }
            }
        }
    }
}

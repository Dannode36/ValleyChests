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

        ItemGrabMenu? activeItemGrabMenu = null;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += OnUpdateTick;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.WindowResized += OnWindowResized;

            dumpAllButton = new(helper.ModContent.Load<Texture2D>("assets/dumpButton.png"), new(0, 0, 16, 16))
            {
                BoxDraw = false,
                Tooltip = "Dump All",
                HoveredSound = string.Empty,
                ClickedSound = "Ship"
            };
            root.AddChild(dumpAllButton);

            stealAllButton = new(helper.ModContent.Load<Texture2D>("assets/stealButton.png"), new(0, 0, 16, 16))
            {
                BoxDraw = false,
                Tooltip = "Take All",
                HoveredSound = string.Empty,
                ClickedSound = "Ship"
            };
            root.AddChild(stealAllButton);
        }

        private void OnWindowResized(object? sender, WindowResizedEventArgs e)
        {
            RepositionButtons();
        }

        private void OnUpdateTick(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            root.Update();
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            activeItemGrabMenu = null;

            if (!Context.IsWorldReady)
                return;

            if (e.NewMenu != null 
                && e.NewMenu is ItemGrabMenu itemGrabmenu 
                && itemGrabmenu.fillStacksButton != null)
            {
                activeItemGrabMenu = itemGrabmenu;
                stealAllButton.OnClick = (e) =>
                {
                    int log_grabMenuItemCount = itemGrabmenu.ItemsToGrabMenu.actualInventory.Count;
                    var grabableInv = itemGrabmenu.ItemsToGrabMenu.actualInventory;
                    for (int i = 0; i < grabableInv.Count; i++)
                    {
                        grabableInv[i] = Game1.player.addItemToInventory(grabableInv[i]);
                    }
                    Monitor.Log($"{Game1.player.Name} used TakeAll on {itemGrabmenu.context.GetType().Name}. {log_grabMenuItemCount} -> {Game1.player.Items.CountItemStacks()}");
                };

                dumpAllButton.OnClick = (e) =>
                {
                    int log_playerItemCount = Game1.player.Items.CountItemStacks();
                    var playerInv = itemGrabmenu.inventory.actualInventory;

                    for (int i = 0; i < playerInv.Count; i++)
                    {
                        playerInv[i] = itemGrabmenu.ItemsToGrabMenu.tryToAddItem(playerInv[i], string.Empty);

                        //Weird af fix
                        //Probably due to Stardew skipping slots if actualInventory is smaller than
                        //the Grab Menu's actual capacity when using tryToAddItem
                        if (playerInv[i] != null && playerInv[i].Stack != 0 && itemGrabmenu.ItemsToGrabMenu.actualInventory.Count < itemGrabmenu.ItemsToGrabMenu.capacity)
                        {
                            itemGrabmenu.ItemsToGrabMenu.actualInventory.Add(null);
                            playerInv[i] = itemGrabmenu.ItemsToGrabMenu.tryToAddItem(playerInv[i], string.Empty);
                        }
                    }

                    Monitor.Log($"{Game1.player.Name} used DumpAll on {itemGrabmenu.context.GetType().Name}. {Game1.player.Items.CountItemStacks()} -> {itemGrabmenu.ItemsToGrabMenu.actualInventory.Count}");
                };

                RepositionButtons();
            }
        }

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if(activeItemGrabMenu != null && activeItemGrabMenu.fillStacksButton != null)
            {
                root.Draw(e.SpriteBatch);

                //Redraw mouse and tooltips over SMUI elements
                activeItemGrabMenu.drawMouse(e.SpriteBatch);
                if (activeItemGrabMenu.hoverText != null 
                    && (activeItemGrabMenu.hoveredItem == null || activeItemGrabMenu.hoveredItem == null || activeItemGrabMenu.ItemsToGrabMenu == null))
                {
                    if (activeItemGrabMenu.hoverAmount > 0)
                    {
                        IClickableMenu.drawToolTip(e.SpriteBatch, activeItemGrabMenu.hoverText, "", null, heldItem: true, -1, 0, null, -1, null, activeItemGrabMenu.hoverAmount);
                    }
                    else
                    {
                        IClickableMenu.drawHoverText(e.SpriteBatch, activeItemGrabMenu.hoverText, Game1.smallFont);
                    }
                }
            }
        }

        private void RepositionButtons()
        {
            if(activeItemGrabMenu != null)
            {
                stealAllButton.LocalPosition = activeItemGrabMenu.fillStacksButton.getVector2() + new Vector2(64 + 16, 64 + 16);
                dumpAllButton.LocalPosition = activeItemGrabMenu.fillStacksButton.getVector2() + new Vector2(64 + 16, 0);
            }
        }
    }
}

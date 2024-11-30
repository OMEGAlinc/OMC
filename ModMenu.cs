using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace OMC
{
    public class ModMenu : IClickableMenu
    {
        private readonly List<ClickableComponent> tabs;
        private readonly List<ClickableComponent> buttons;
        private readonly TextBox skillIncreaseTextBox;
        private readonly TextBox menuKeyTextBox;
        private readonly ClickableTextureComponent okSkillIncreaseButton;
        private readonly ClickableTextureComponent okMenuKeyButton;
        private readonly int padding = 20;
        private readonly int buttonHeight = 60;
        private readonly int buttonSpacing = 10;
        private int currentTab = 0; // 0: Buttons, 1: Values
        private Rectangle background;
        private readonly string hoverText = "";

        public ModMenu() : base(0, 0, Game1.viewport.Width, Game1.viewport.Height)
        {
            // Initialize tabs
            tabs = new()
            {
                new ClickableComponent(Rectangle.Empty, "Buttons"),
                new ClickableComponent(Rectangle.Empty, "Values")
            };

            // Initialize buttons for Buttons tab
            buttons = new()
            {
                new ClickableComponent(Rectangle.Empty, "NPC Locations"),
                new ClickableComponent(Rectangle.Empty, "List Animals"),
                new ClickableComponent(Rectangle.Empty, "Pet All Animals"),
                new ClickableComponent(Rectangle.Empty, "Water All Crops"),
                new ClickableComponent(Rectangle.Empty, "Grow All Crops"),
                new ClickableComponent(Rectangle.Empty, "Finish Buildings"),
                new ClickableComponent(Rectangle.Empty, "Sleep Anywhere")
            };

            // Text box and buttons for Values tab
            skillIncreaseTextBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor)
            {
                Text = "",
                Width = 150
            };

            menuKeyTextBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor)
            {
                Text = "",
                Width = 150
            };

            okSkillIncreaseButton = new ClickableTextureComponent(
                "OK",
                Rectangle.Empty,
                null,
                "Confirm Skill Increase",
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64),
                1f
            );

            okMenuKeyButton = new ClickableTextureComponent(
                "OK",
                Rectangle.Empty,
                null,
                "Confirm Menu Key",
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64),
                1f
            );

            CalculatePositions();
        }

        private void CalculatePositions()
        {
            // Center the menu
            int menuWidth = 300;
            int menuHeight = 400;
            int startX = Game1.viewport.Width / 2 - menuWidth / 2;
            int startY = Game1.viewport.Height / 2 - menuHeight / 2;

            background = new Rectangle(startX, startY, menuWidth, menuHeight);

            // Position tabs
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].bounds = new Rectangle(startX + i * 60, startY - 50, 48, 48);
            }

            // Position buttons
            int buttonStartY = background.Y + padding;
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].bounds = new Rectangle(
                    background.X + padding,
                    buttonStartY + i * (buttonHeight + buttonSpacing),
                    background.Width - padding * 2,
                    buttonHeight
                );
            }

            // Position text boxes and OK buttons
            skillIncreaseTextBox.X = background.X + padding;
            skillIncreaseTextBox.Y = background.Y + padding;
            menuKeyTextBox.X = background.X + padding;
            menuKeyTextBox.Y = skillIncreaseTextBox.Y + skillIncreaseTextBox.Height + buttonSpacing;

            okSkillIncreaseButton.bounds = new Rectangle(
                skillIncreaseTextBox.X + skillIncreaseTextBox.Width + 10,
                skillIncreaseTextBox.Y,
                64,
                64
            );

            okMenuKeyButton.bounds = new Rectangle(
                menuKeyTextBox.X + menuKeyTextBox.Width + 10,
                menuKeyTextBox.Y,
                64,
                64
            );
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            // Draw background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                background.X,
                background.Y,
                background.Width,
                background.Height,
                Color.White * 0.9f,
                1f,
                false
            );

            // Draw tabs
            foreach (var tab in tabs)
            {
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60),
                    tab.bounds.X,
                    tab.bounds.Y,
                    tab.bounds.Width,
                    tab.bounds.Height,
                    tab.name == "Buttons" && currentTab == 0 || tab.name == "Values" && currentTab == 1 ? Color.Goldenrod : Color.White,
                    1f,
                    false
                );
            }

            // Draw buttons or text boxes based on the current tab
            if (currentTab == 0) // Buttons tab
            {
                foreach (var button in buttons)
                {
                    IClickableMenu.drawTextureBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        button.bounds.X,
                        button.bounds.Y,
                        button.bounds.Width,
                        button.bounds.Height,
                        Color.White,
                        1f,
                        false
                    );
                    Utility.drawTextWithShadow(
                        b,
                        button.name,
                        Game1.dialogueFont,
                        new Vector2(button.bounds.X + padding, button.bounds.Y + padding / 2),
                        Game1.textColor
                    );
                }
            }
            else if (currentTab == 1) // Values tab
            {
                skillIncreaseTextBox.Draw(b);
                menuKeyTextBox.Draw(b);
                okSkillIncreaseButton.draw(b);
                okMenuKeyButton.draw(b);
            }

            // Draw hover text
            if (!string.IsNullOrEmpty(hoverText))
                drawHoverText(b, hoverText, Game1.smallFont);

            // Draw mouse cursor
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Check tabs
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].containsPoint(x, y))
                {
                    currentTab = i;
                    Game1.playSound("coin");
                    return;
                }
            }

            // Handle clicks on buttons
            if (currentTab == 0)
            {
                foreach (var button in buttons)
                {
                    if (button.containsPoint(x, y))
                    {
                        HandleButtonClick(button.name);
                        Game1.playSound("coin");
                        return;
                    }
                }
            }

            // Handle text box and OK button clicks
            if (currentTab == 1)
            {
                if (okSkillIncreaseButton.containsPoint(x, y))
                {
                    Game1.addHUDMessage(new HUDMessage($"Skill increased: {skillIncreaseTextBox.Text}"));
                    return;
                }

                if (okMenuKeyButton.containsPoint(x, y))
                {
                    Game1.addHUDMessage(new HUDMessage($"Menu key set to: {menuKeyTextBox.Text}"));
                    return;
                }
            }
        }

        private static void HandleButtonClick(string buttonName)
        {
            switch (buttonName)
            {
                case "NPC Locations":
                    Game1.addHUDMessage(new HUDMessage("NPC locations listed."));
                    ModEntry.ListNpcLocations();
                    break;

                case "List Animals":
                    Game1.addHUDMessage(new HUDMessage("Animals listed!"));
                    ModEntry.ListAnimals();
                    break;

                case "Pet All Animals":
                    Game1.addHUDMessage(new HUDMessage("All animals petted!"));
                    ModEntry.PetAllAnimals();
                    break;

                case "Water All Crops":
                    Game1.addHUDMessage(new HUDMessage("All crops watered!"));
                    ModEntry.WaterAllCrops();
                    break;

                case "Grow All Crops":
                    Game1.addHUDMessage(new HUDMessage("All crops are fully grown!"));
                    ModEntry.GrowAllCrops();
                    break;

                case "Finish Buildings":
                    Game1.addHUDMessage(new HUDMessage("All buildings completed!"));
                    ModEntry.FinishBuildings();
                    break;

                case "Sleep Anywhere":
                    Game1.addHUDMessage(new HUDMessage("Going to sleep!"));
                    ModEntry.SleepAnywhere();
                    break;
            }
        }
    }
}

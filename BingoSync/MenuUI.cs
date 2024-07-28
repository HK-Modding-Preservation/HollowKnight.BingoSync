﻿using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BingoSync
{
    public static class MenuUI
    {
        private static readonly int colorButtonWidth = 100;
        private static readonly int lockoutButtonWidth = 50;
        private static readonly int handModeButtonWidth = 43;
        private static readonly int textFieldWidth = colorButtonWidth * 5 + 40;
        private static readonly int joinRoomButtonWidth = textFieldWidth - handModeButtonWidth - 10;
        private static readonly int gameModeButtonWidth = (textFieldWidth - 20) / 3;
        private static readonly int generateButtonWidth = (int)(1.5 * gameModeButtonWidth) + 10;
        private static readonly int seedFieldWidth = textFieldWidth - generateButtonWidth - lockoutButtonWidth - 20;
        private static readonly int fontSize = 22;

        public static readonly LayoutRoot layoutRoot = new(true, "Persistent layout")
        {
            // this is needed to hide the menu while the game is loading
            VisibilityCondition = () => false,
        };

        private static readonly StackLayout connectionMenu = new(layoutRoot)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 10,
            Orientation = Orientation.Vertical,
            Padding = new Padding(0, 50, 20, 0),
        };

        private static readonly TextInput roomCodeInput = new (layoutRoot, "RoomCode")
        {
            FontSize = fontSize,
            MinWidth = textFieldWidth,
            Placeholder = "Room Link",
        };
        public static readonly TextInput nicknameInput = new (layoutRoot, "NickName")
        {
            FontSize = fontSize,
            MinWidth = textFieldWidth,
            Placeholder = "Nickname",
        };
        public static readonly TextInput passwordInput = new(layoutRoot, "Password")
        {
            FontSize = fontSize,
            MinWidth = textFieldWidth,
            Placeholder = "Password",
        };
        private static readonly List<Button> colorButtons = 
        [
            CreateColorButton("Orange", Colors.Orange), 
            CreateColorButton("Red", Colors.Red), 
            CreateColorButton("Blue", Colors.Blue), 
            CreateColorButton("Green", Colors.Green), 
            CreateColorButton("Purple", Colors.Purple), 
            CreateColorButton("Navy", Colors.Navy), 
            CreateColorButton("Teal", Colors.Teal), 
            CreateColorButton("Brown", Colors.Brown), 
            CreateColorButton("Pink", Colors.Pink), 
            CreateColorButton("Yellow", Colors.Yellow)
        ];
        private static readonly Button joinRoomButton = new(layoutRoot, "roomButton")
        {
            Content = "Join Room",
            FontSize = fontSize,
            Margin = 20,
            MinWidth = joinRoomButtonWidth,
        };
        private static ToggleButton handModeToggleButton;

        public static StackLayout generationMenu = new(layoutRoot)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Spacing = 15,
            Orientation = Orientation.Vertical,
            Padding = new Padding(0, 50, 20, 15),
        };
        private static readonly TextInput generationSeedInput = new(layoutRoot, "Seed")
        {
            FontSize = fontSize,
            MinWidth = seedFieldWidth,
            Placeholder = "Seed",
        };
        private static readonly Button generateBoardButton = new(layoutRoot, "generateBoardButton")
        {
            Content = "Generate Board",
            FontSize = fontSize,
            Margin = 20,
            MinWidth = generateButtonWidth,
            MinHeight = 50,
        };
        private static readonly TextureLoader Loader = new(Assembly.GetExecutingAssembly(), "BingoSync.Resources.Images");
        private static readonly List<Button> gameModeButtons = [];

        private static ToggleButton lockoutToggleButton;


        public static void Setup()
        {
            Loader.Preload();

            Sprite handModeSprite = Loader.GetTexture("BingoSync Hand Icon.png").ToSprite();
            Sprite nonHandModeSprite = Loader.GetTexture("BingoSync Eye Icon.png").ToSprite();

            handModeToggleButton = new(layoutRoot, handModeSprite, nonHandModeSprite, Controller.HandModeButtonClicked, "Hand Mode Toggle");
            Button handModeButton = new(layoutRoot, "handModeToggleButton")
            {
                MinWidth = handModeButtonWidth,
                MinHeight = handModeButtonWidth,
            };
            handModeToggleButton.SetButton(handModeButton);


            connectionMenu.Children.Add(roomCodeInput);
            connectionMenu.Children.Add(nicknameInput);
            connectionMenu.Children.Add(passwordInput);

            SetupColorButtons(connectionMenu);

            joinRoomButton.Click += JoinRoomButtonClicked;

            StackLayout bottomRow = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            bottomRow.Children.Add(joinRoomButton);
            bottomRow.Children.Add(handModeToggleButton);

            connectionMenu.Children.Add(bottomRow);


            Sprite lockoutSprite = Loader.GetTexture("BingoSync Lockout Icon.png").ToSprite();
            Sprite nonLockoutSprite = Loader.GetTexture("BingoSync Non-Lockout Icon.png").ToSprite();

            lockoutToggleButton = new(layoutRoot, lockoutSprite, nonLockoutSprite, Controller.LockoutButtonClicked, "Lockout Toggle");
            Button lockoutButton = new(layoutRoot, "lockoutToggleButton")
            {
                MinWidth = lockoutButtonWidth,
                MinHeight = lockoutButtonWidth,
            };
            lockoutToggleButton.SetButton(lockoutButton);
            lockoutToggleButton.Toggle(null);


            
            LoadDefaults();

            SetupGenerationMenu();

            layoutRoot.VisibilityCondition = () => {
                BingoSyncClient.Update();
                Update();
                return Controller.MenuIsVisible;
            };

            layoutRoot.ListenForPlayerAction(BingoSync.modSettings.Keybinds.HideMenu, () => {
                Controller.MenuIsVisible = !Controller.MenuIsVisible;
            });
        }

        private static Button CreateColorButton(string text, Color color)
        {
            Button button = new(layoutRoot, text.ToLower())
            {
                Content = text,
                FontSize = 15,
                Margin = 20,
                BorderColor = color,
                ContentColor = color,
                MinWidth = colorButtonWidth,
            };
            button.Click += SelectColor;
            return button;
        }

        private static void SetupColorButtons(StackLayout connectionMenu)
        {
            StackLayout colorButtonsLayout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Vertical,
            };

            StackLayout row1 = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            StackLayout row2 = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            for(int i = 0; i < 5; ++i)
            {
                row1.Children.Add(colorButtons.ElementAt(i));
                row2.Children.Add(colorButtons.ElementAt(5 + i));
            }

            colorButtonsLayout.Children.Add(row1);
            colorButtonsLayout.Children.Add(row2);

            connectionMenu.Children.Add(colorButtonsLayout);
        }

        public static Button CreateGameModeButton(string name)
        {
            Button button = new(layoutRoot, name)
            {
                Content = name,
                FontSize = 15,
                Margin = 20,
                MinWidth = gameModeButtonWidth,
            };
            button.Click += SelectGameMode;
            return button;
        }

        public static void SetupGameModeButtons()
        {
            StackLayout buttonLayout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Vertical,
            };

            StackLayout row = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            foreach (string gameMode in GameModesManager.GameModeNames())
            {
                if (row.Children.Count >= 3)
                {
                    buttonLayout.Children.Insert(0, row);
                    row = new(layoutRoot)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Spacing = 10,
                        Orientation = Orientation.Horizontal,
                    };
                }
                Button gameModeButton = CreateGameModeButton(gameMode);
                gameModeButtons.Add(gameModeButton);
                row.Children.Add(gameModeButton);
            }
            buttonLayout.Children.Insert(0, row);
            generationMenu.Children.Insert(0, buttonLayout);
            SelectGameMode(gameModeButtons[0]);
        }

        private static void SetupGenerationMenu()
        {
            generateBoardButton.Click += Controller.GenerateButtonClicked;

            StackLayout bottomRow = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            bottomRow.Children.Add(generateBoardButton);
            bottomRow.Children.Add(generationSeedInput);
            bottomRow.Children.Add(lockoutToggleButton);

            generationMenu.Children.Add(bottomRow);
        }

        public static void SetEnabled(bool enabled)
        {
            roomCodeInput.Enabled = enabled;
            nicknameInput.Enabled = enabled;
            passwordInput.Enabled = enabled;
            colorButtons.ForEach(button =>
            {
                button.Enabled = enabled;
            });
        }

        private static void Update()
        {
            if (BingoSyncClient.GetState() == BingoSyncClient.State.Connected)
            {
                joinRoomButton.Content = "Exit Room";
                joinRoomButton.Enabled = true;
                SetEnabled(false);
            } else if (BingoSyncClient.GetState() == BingoSyncClient.State.Loading)
            {
                joinRoomButton.Content = "Loading...";
                joinRoomButton.Enabled = false;
                SetEnabled(false);
            } else
            {
                joinRoomButton.Content = "Join Room";
                joinRoomButton.Enabled = true;
                SetEnabled(true);
            }
        }

        public static int GetSeed()
        {
            string inputStr = generationSeedInput.Text;
            int seed = unchecked(DateTime.Now.Ticks.GetHashCode());
            if (inputStr != string.Empty)
            {
                bool isNumeric = int.TryParse(inputStr, out seed);
                if(!isNumeric)
                {
                    seed = inputStr.GetHashCode();
                }
            }
            return seed;
        }

        private static string SanitizeRoomCode(string input)
        {
            return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c)).ToArray())
            .Split('/').Last();
        }

        private static void JoinRoomButtonClicked(Button sender)
        {
            if (BingoSyncClient.GetState() != BingoSyncClient.State.Connected)
            {
                Controller.RoomCode = SanitizeRoomCode(roomCodeInput.Text);
                Controller.RoomNickname = nicknameInput.Text;
                Controller.RoomPassword = passwordInput.Text;
                BingoSyncClient.JoinRoom((ex) =>
                {
                    Update();
                });
                Update();
            }
            else
            {
                BingoSyncClient.ExitRoom(() =>
                {
                    Update();
                });
                Update();
            }
        }

        public static bool IsLockoutToggleButtonOn()
        {
            return lockoutToggleButton.IsOn;
        }

        private static void SelectColor(Button sender)
        {
            Button previousSelectedColor = layoutRoot.GetElement<Button>(Controller.RoomColor);
            previousSelectedColor.BorderColor = previousSelectedColor.ContentColor;
            Controller.RoomColor = sender.Name;
            sender.BorderColor = Color.white;
        }

        public static void SelectGameMode(Button sender)
        {
            Controller.ActiveGameMode = sender.Name;
            foreach (Button gameMode in gameModeButtons)
            {
                gameMode.BorderColor = Color.white;
            }
            sender.BorderColor = Color.red;
        }

        public static void LoadDefaults()
        {
            if (nicknameInput != null)
                nicknameInput.Text = BingoSync.modSettings.DefaultNickname;
            if (passwordInput != null)
                passwordInput.Text = BingoSync.modSettings.DefaultPassword;
            Controller.RoomColor = BingoSync.modSettings.DefaultColor;
            if (layoutRoot == null)
                return;
            Button selectedColorButton = layoutRoot.GetElement<Button>(Controller.RoomColor);
            if (selectedColorButton != null)
            {
                selectedColorButton.BorderColor = Color.white;
            }
        }
    }
}
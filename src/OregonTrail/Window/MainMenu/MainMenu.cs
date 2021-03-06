﻿// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System.IO;
using System.Reflection;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Allows the configuration of party names, player profession, and purchasing initial items for trip.
    /// </summary>
    public sealed class MainMenu : Window<MainMenuCommands, NewGameInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Window{TCommands,TData}" /> class.
        /// </summary>
        /// <param name="game">Core simulation which is controlling the form factory.</param>
        public MainMenu(GameSimulationApp game) : base(game)
        {
        }

        /// <summary>
        ///     Called after the Windows has been added to list of modes and made active.
        /// </summary>
        public override void OnWindowPostCreate()
        {
            var headerText = new StringBuilder();

            // Graphical title for main menu.
            ImagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "title.gif");
            headerText.Append("You may:");
            MenuHeader = headerText.ToString();

            AddCommand(TravelTheTrail, MainMenuCommands.TravelTheTrail);
            AddCommand(LearnAboutTrail, MainMenuCommands.LearnAboutTheTrail);
            AddCommand(SeeTopTen, MainMenuCommands.SeeTheOregonTopTen);
            AddCommand(ChooseManagementOptions, MainMenuCommands.ChooseManagementOptions);
            //AddCommand(CloseSimulation, MainMenuCommands.CloseSimulation);
        }

        ///// <summary>
        /////     Does exactly what it says on the tin, closes the simulation and releases all memory.
        ///// </summary>
        //private void CloseSimulation()
        //{
        //    UserData.Game.Destroy();
        //}

        /// <summary>
        ///     Glorified options menu, used to clear top ten, Tombstone messages, and saved games.
        /// </summary>
        private void ChooseManagementOptions()
        {
            SetForm(typeof (ManagementOptions));
        }

        /// <summary>
        ///     High score list, defaults to hard-coded values if no custom ones present.
        /// </summary>
        private void SeeTopTen()
        {
            SetForm(typeof (CurrentTopTen));
        }

        /// <summary>
        ///     Instruction manual that explains how the game works and what is expected of the player.
        /// </summary>
        private void LearnAboutTrail()
        {
            SetForm(typeof (RulesHelp));
        }

        /// <summary>
        ///     Start with choosing profession in the new game Windows, the others are chained together after this one.
        /// </summary>
        private void TravelTheTrail()
        {
            SetForm(typeof (ProfessionSelector));
        }
    }
}
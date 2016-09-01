// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;

namespace OregonTrail
{
    /// <summary>
    ///     Prompts the player with a yes or no question regarding if they would like to use the services offered by the Indian
    ///     guide. However, he requires sets of clothing and not money like the ferry operator. If they player does not have
    ///     enough clothing in their inventory then the message will say so here since there is no opportunity to trade once
    ///     you are actually at the river crossing. The amount of clothing he asks for will also change based on the amount of
    ///     animals killed while hunting.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class IndianGuidePrompt : InputForm<TravelInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IndianGuidePrompt" /> class.
        ///     This constructor will be used by the other one
        /// </summary>
        /// <param name="window">The window.</param>
        public IndianGuidePrompt(IWindow window) : base(window)
        {
        }

        /// <summary>
        ///     Changes up the behavior of the input dialog based on if the player has enough clothes to trade the Indian guide.
        /// </summary>
        protected override DialogType DialogType
        {
            get { return HasEnoughClothingToTrade ? DialogType.YesNo : DialogType.Prompt; }
        }

        /// <summary>
        ///     Determines if the player has enough clothing to trade the Indian guide for his services in crossing the river.
        /// </summary>
        private bool HasEnoughClothingToTrade
        {
            get
            {
                return UserData.Game.Vehicle.Inventory[Entities.Clothes].Quantity >=
                       UserData.River.IndianCost;
            }
        }

        /// <summary>
        ///     Only allows input from the player if they have enough clothing to trade with the Indian guide, otherwise we will
        ///     treat this as a prompt only and no input.
        /// </summary>
        public override bool InputFillsBuffer
        {
            get { return HasEnoughClothingToTrade; }
        }

        public override object MenuCommands
        {
            get
            {
                return HasEnoughClothingToTrade
                    ? new[] {"Yes", "No"}
                    : new[] {"Ok"};
            }
        }

        /// <summary>
        ///     Fired when dialog prompt is attached to active game Windows and would like to have a string returned.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        protected override string OnDialogPrompt()
        {
            // Builds up the first part about the Indian guide for river crossing.
            var indianGuidePrompt = new StringBuilder();
            indianGuidePrompt.AppendLine(
                $"A Shoshoni guide says that he will take your wagon across the river in exchange for {UserData.River.IndianCost.ToString("N0")} sets of clothing.{Environment.NewLine}");

            // Change up the message based on if the player has enough clothing, they won't be able to get more if they don't here.
            indianGuidePrompt.AppendLine(HasEnoughClothingToTrade
                ? "Will you accept this offer?"
                : $"You don't have {UserData.River.IndianCost.ToString("N0")} sets of clothing.{Environment.NewLine}");

            // Renders out the Indian guide river crossing confirmation and or denial.
            return indianGuidePrompt.ToString();
        }

        /// <summary>
        ///     Fired when the dialog receives favorable input and determines a response based on this. From this method it is
        ///     common to attach another state, or remove the current state based on the response.
        /// </summary>
        /// <param name="reponse">The response the dialog parsed from simulation input buffer.</param>
        protected override void OnDialogResponse(DialogResponse reponse)
        {
            // Depending on if the player has enough clothing their response to Indian guide changes.
            if (HasEnoughClothingToTrade)
            {
                // Only care about the user response if they actually have
                switch (reponse)
                {
                    case DialogResponse.Yes:
                        UserData.River.CrossingType = RiverCrossChoice.Indian;
                        SetForm(typeof (UseIndianConfirm));
                        break;
                    case DialogResponse.No:
                    case DialogResponse.Custom:
                        CancelIndianCrossing();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(reponse), reponse, null);
                }
            }
            else
            {
                // Returns back to the river cross choice menu.
                CancelIndianCrossing();
            }
        }

        /// <summary>
        ///     Player does not have enough clothing to satisfy the Indian cost.
        /// </summary>
        private void CancelIndianCrossing()
        {
            UserData.River.CrossingType = RiverCrossChoice.None;
            SetForm(typeof (RiverCross));
        }
    }
}
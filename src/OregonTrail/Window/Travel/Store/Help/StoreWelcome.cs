// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 01/03/2016@1:50 AM

using System;
using System.Text;
using OregonTrail.Form;

namespace OregonTrail.Travel.Store.Help
{
    /// <summary>
    ///     Offers up some free information about what items are important to the player and what they mean for the during the
    ///     course of the simulation.
    /// </summary>
    [ParentWindow(typeof (Travel))]
    public sealed class StoreWelcome : Form<TravelInfo>
    {
        /// <summary>
        ///     Keeps track if the player has read all the advice and this dialog needs to be closed.
        /// </summary>
        private bool _hasReadAdvice;

        /// <summary>
        ///     Keeps track of the message we want to show to the player but only build it actually once.
        /// </summary>
        private StringBuilder _storeHelp;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StoreWelcome" /> class.
        ///     Offers up some free information about what items are important to the player and what they mean for the during the
        ///     course of the simulation.
        /// </summary>
        /// <param name="window">The window.</param>
        public StoreWelcome(IWindow window) : base(window)
        {
        }

        /// <summary>
        ///     Determines if user input is currently allowed to be typed and filled into the input buffer.
        /// </summary>
        /// <remarks>Default is FALSE. Setting to TRUE allows characters and input buffer to be read when submitted.</remarks>
        public override bool InputFillsBuffer
        {
            get { return false; }
        }

        public override object MenuCommands
        {
            get { return new[] {"Ok"}; }
        }

        /// <summary>
        ///     Fired after the state has been completely attached to the simulation letting the state know it can browse the user
        ///     data and other properties below it.
        /// </summary>
        public override void OnFormPostCreate()
        {
            base.OnFormPostCreate();

            _hasReadAdvice = false;
            _storeHelp = new StringBuilder();
            UpdateAdvice();
        }

        /// <summary>
        ///     Since the advice can change we have to do this in chunks.
        /// </summary>
        private void UpdateAdvice()
        {
            // Clear any previous string builder message.
            _storeHelp.Clear();

            // Create the current state of our advice to player.
            _storeHelp.AppendLine(
                $"Hello, I'm Matt. So you're going to Oregon! I can fix you up with what you need:{Environment.NewLine}");

            _storeHelp.AppendLine(" - a team of oxen to pull your vehicle");
            _storeHelp.AppendLine(" - clothing for both summer and winter");
            _storeHelp.AppendLine(" - plenty of food for the trip");
            _storeHelp.AppendLine(" - ammunition for your rifles");
            _storeHelp.AppendLine(" - spare parts for your wagon");
        }

        /// <summary>
        ///     Returns a text only representation of the current game Windows state. Could be a statement, information, question
        ///     waiting input, etc.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string OnRenderForm()
        {
            return _storeHelp.ToString();
        }

        /// <summary>Fired when the game Windows current state is not null and input buffer does not match any known command.</summary>
        /// <param name="input">Contents of the input buffer which didn't match any known command in parent game Windows.</param>
        public override void OnInputBufferReturned(string input)
        {
            // On the last advice panel we flip a normal boolean to know we are definitely done here.
            if (_hasReadAdvice)
            {
                ClearForm();
                return;
            }

            _hasReadAdvice = true;
            SetForm(typeof (Store));
        }
    }
}
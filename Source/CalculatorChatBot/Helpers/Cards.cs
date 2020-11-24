﻿// <copyright file="Cards.cs" company="Tata Consultancy Services Ltd">
// Copyright (c) Tata Consultancy Services Ltd. All rights reserved.
// </copyright>

namespace CalculatorChatBot.Helpers
{
    using System.Collections.Generic;
    using CalculatorChatBot.Properties;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Class that allows for the returning of attachments.
    /// </summary>
    public static class Cards
    {
        /// <summary>
        /// Method that returns the welcome card attachment for a team.
        /// </summary>
        /// <param name="botDisplayName">The bot display name.</param>
        /// <returns>Welcome adaptive card that is to be attached to the welcome message.</returns>
        public static Attachment WelcomeTeamCardAttachment(string botDisplayName)
        {
            var welcomeTeamCardAttachment = WelcomeTeamAdaptiveCard.GetCard(botDisplayName);
            return welcomeTeamCardAttachment;
        }

        /// <summary>
        /// Returns the arithmetic carousel card.
        /// </summary>
        /// <returns>Returns the attachment that is to be a part of the tour carousel card.</returns>
        public static Attachment GetArithmeticCarouselAttachment()
        {
            var arithmeticHeroCard = new HeroCard()
            {
                Title = Resources.ArithmeticCarouselCardTitleText,
                Text = Resources.ArithmeticCarouselCardContent,
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Sample Arithmetic Command",
                        DisplayText = "Sample Arithmetic Command",
                        Type = ActionTypes.MessageBack,
                        Text = "add 1,3,4",
                    },
                },
            };

            return arithmeticHeroCard.ToAttachment();
        }

        /// <summary>
        /// Returns the geometric carousel card.
        /// </summary>
        /// <returns>Returns the attachment that is to be a part of the tour carousel card.</returns>
        public static Attachment GetGeometricCarouselAttachment()
        {
            var geometricHeroCard = new HeroCard()
            {
                Title = Resources.GeometricCarouselCardTitleText,
                Text = Resources.GeometricCarouselCardContent,
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Sample Geometric Command",
                        DisplayText = "Sample Geometric Command",
                        Type = ActionTypes.MessageBack,
                        Text = "hypotenuse 3,4",
                    },
                },
            };

            return geometricHeroCard.ToAttachment();
        }

        /// <summary>
        /// Returns the statistical carousel card.
        /// </summary>
        /// <returns>Returns the attachment that is to be part of the tour carousel card.</returns>
        public static Attachment GetStatisticalCarouselAttachment()
        {
            var statisticalHeroCard = new HeroCard()
            {
                Title = Resources.StatisticalCarouselCardTitleText,
                Text = Resources.StatisticalCarouselCardContent,
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Sample Statistical Command",
                        DisplayText = "Sample Statistical Command",
                        Type = ActionTypes.MessageBack,
                        Text = "mean 3,4,5,6,7",
                    },
                },
            };

            return statisticalHeroCard.ToAttachment();
        }
    }
}
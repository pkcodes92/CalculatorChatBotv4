﻿// <copyright file="CalcChatBot.cs" company="Tata Consultancy Services Ltd">
// Copyright (c) Tata Consultancy Services Ltd. All rights reserved.
// </copyright>

namespace CalculatorChatBot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CalculatorChatBot.Helpers;
    using CalculatorChatBot.Properties;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// This class will be allowing for the separation of logic.
    /// </summary>
    public class CalcChatBot : ICalcChatBot
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcChatBot"/> class.
        /// </summary>
        /// <param name="telemetryClient">ApplicationInsights DI.</param>
        public CalcChatBot(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Method which fires at the time the bot sends a proactive welcome message after being installed to a team.
        /// </summary>
        /// <param name="teamId">The teamId.</param>
        /// <param name="botDisplayName">The bot display name.</param>
        /// <param name="connectorClient">The connector client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        public async Task SendTeamWelcomeMessageAsync(
            string teamId,
            string botDisplayName,
            ConnectorClient connectorClient,
            CancellationToken cancellationToken)
        {
            if (connectorClient is null)
            {
                throw new ArgumentNullException(nameof(connectorClient));
            }

            var welcomeTeamCardAttachment = Cards.WelcomeTeamCardAttachment(botDisplayName);
            await NotifyTeam(connectorClient, welcomeTeamCardAttachment, teamId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a welcome message to the user.
        /// </summary>
        /// <param name="memberAddedId">The newly added team member.</param>
        /// <param name="teamId">The teamId.</param>
        /// <param name="botDisplayName">The bot display name.</param>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="botId">The botId.</param>
        /// <param name="connectorClient">The turn connector client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        public async Task SendUserWelcomeMessageAsync(
            string memberAddedId,
            string teamId,
            string botDisplayName,
            string tenantId,
            string botId,
            ConnectorClient connectorClient,
            CancellationToken cancellationToken)
        {
            if (connectorClient is null)
            {
                throw new ArgumentNullException(nameof(connectorClient));
            }

            var allMembers = await connectorClient.Conversations.GetConversationMembersAsync(teamId, cancellationToken).ConfigureAwait(false);

            ChannelAccount userThatJustJoined = null;
            foreach (var m in allMembers)
            {
                // both values are 29: values
                if (m.Id == memberAddedId)
                {
                    userThatJustJoined = m;
                    break;
                }
            }

            if (userThatJustJoined != null)
            {
                var welcomeUserCardAttachment = WelcomeAdaptiveCard.GetCard(botDisplayName);
                await this.NotifyUser(connectorClient, userThatJustJoined, welcomeUserCardAttachment, botId, tenantId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method that returns the necessary tour carousel card.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        public async Task SendTourCarouselCard(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            if (turnContext is null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var tourCarouselReply = turnContext.Activity.CreateReply();
            tourCarouselReply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            tourCarouselReply.Attachments = new List<Attachment>()
            {
                Cards.GetArithmeticCarouselAttachment(),
                Cards.GetGeometricCarouselAttachment(),
                Cards.GetStatisticalCarouselAttachment(),
            };

            await turnContext.SendActivityAsync(tourCarouselReply, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method that will send out the team notification.
        /// </summary>
        /// <param name="connectorClient">The connector client.</param>
        /// <param name="attachmentToAppend">The attachment/adaptive card to attach to the message.</param>
        /// <param name="teamId">The team Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution.</returns>
        private static async Task NotifyTeam(
            ConnectorClient connectorClient,
            Attachment attachmentToAppend,
            string teamId,
            CancellationToken cancellationToken)
        {
            if (connectorClient is null)
            {
                throw new ArgumentNullException(nameof(connectorClient));
            }

            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount()
                {
                    Id = teamId,
                },
                Attachments = new List<Attachment>()
                {
                    attachmentToAppend,
                },
            };

            await connectorClient.Conversations.SendToConversationAsync(teamId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Notifies the user.
        /// </summary>
        /// <param name="connectorClient">The connector client.</param>
        /// <param name="user">The user that joined the team.</param>
        /// <param name="attachmentToAppend">The attachment to send.</param>
        /// <param name="botId">The bot Id.</param>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit of execution that contains a boolean value.</returns>
        private async Task<bool> NotifyUser(
            ConnectorClient connectorClient,
            ChannelAccount user,
            Attachment attachmentToAppend,
            string botId,
            string tenantId,
            CancellationToken cancellationToken)
        {
            if (connectorClient is null)
            {
                throw new ArgumentNullException(nameof(connectorClient));
            }

            try
            {
                // ensure conversation exists
                var bot = new ChannelAccount { Id = botId };
                var conversationParameters = new ConversationParameters()
                {
                    Bot = bot,
                    Members = new List<ChannelAccount>()
                    {
                        user,
                    },
                    TenantId = tenantId,
                };

                var response = await connectorClient.Conversations.CreateConversationAsync(conversationParameters, cancellationToken).ConfigureAwait(false);

                var conversationId = response.Id;

                var activity = new Activity()
                {
                    Type = ActivityTypes.Message,
                    Attachments = new List<Attachment>()
                    {
                        attachmentToAppend,
                    },
                };

                await connectorClient.Conversations.SendToConversationAsync(conversationId, activity).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }
    }
}
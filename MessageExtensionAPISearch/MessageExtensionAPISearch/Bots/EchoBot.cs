// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessageExtensionAPISearch.Bots
{
    public class EchoBot : TeamsActivityHandler
    {
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var text = query?.Parameters?[0]?.Value as string ?? string.Empty;

            var patients = await FindPatient(text);

            var attachments = patients.Select(patient =>
            {
                var previewCard = new ThumbnailCard { Title = patient.Item2, Subtitle = "Gender : " + patient.Item4, Text = "Contact Number : " + patient.Item5, Tap = new CardAction { Type = "invoke", Value = patient } };
                if (!string.IsNullOrEmpty(patient.Item6))
                {
                    previewCard.Images = new List<CardImage>() { new CardImage(patient.Item6, "Profile Pic") };
                }

                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard { Title = patient.Item1 },
                    Preview = previewCard.ToAttachment()
                };

                return attachment;
            }).ToList();

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            var (Id, PatientName, PatientAge, PatientGender, PatientContactNo, Photograph) = query.ToObject<(string, string, string, string, string, string)>();

            var card = new AdaptiveCard("1.2")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveColumnSet()
                    {
                        Columns =new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width=AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveImage(Photograph)
                                    {
                                        Style=AdaptiveImageStyle.Default,
                                        Size=AdaptiveImageSize.Medium
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Width=AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Text=PatientName,
                                        Wrap=true,
                                        Size=AdaptiveTextSize.Large
                                    },
                                }
                            },
                        }
                    },
                    new AdaptiveTextBlock()
                    {
                        Text="Patient Name : "+ PatientName,
                        Size=AdaptiveTextSize.ExtraLarge,
                        Color=AdaptiveTextColor.Accent
                    },
                    new AdaptiveColumnSet()
                     {
                        Columns =new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                    Width=AdaptiveColumnWidth.Stretch,
                                    Items = new List<AdaptiveElement>()
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "PatientAge",
                                            Weight =AdaptiveTextWeight.Bolder
                                        },
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "PatientGender",
                                            Weight =AdaptiveTextWeight.Bolder
                                        },
                                      
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "PatientContactNo.",
                                            Weight =AdaptiveTextWeight.Bolder
                                        },
                                        new AdaptiveActionSet()
                                        {
                                            Actions = new List<AdaptiveAction>(){
                                                new AdaptiveOpenUrlAction()
                                                {
                                                    UrlString=$"https://teams.microsoft.com/l/entity/2ca68a73-5072-43de-b4f7-1a9319fc041b/_djb2_msteams_prefix_3169093601?context=%7B%22subEntityId%22%3A{Id}%2C%22channelId%22%3A%2219%3A1ce385e57c0646deb67d00af44b2fa32%40thread.tacv2%22%7D",
                                                    Title="More details"
                                                }
                                            }
                                        }
                                    }
                            },
                            new AdaptiveColumn()
                            {
                                    Width=AdaptiveColumnWidth.Stretch,
                                    Items = new List<AdaptiveElement>()
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = PatientAge
                                        },
                                        new AdaptiveTextBlock()
                                        {
                                            Text = PatientGender
                                        },
                                        new AdaptiveTextBlock()
                                        {
                                            Text = PatientContactNo
                                        },
                                      
                                    }
                            }
                        }
                    },
                }
            };
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
                Preview = new Attachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = new ThumbnailCard
                    {
                        Title = PatientName,
                    },
                }
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }
        private async Task<IEnumerable<(string, string, string, string, string, string)>> FindPatient(string text)
        {
            var responseString = await (new HttpClient()).GetStringAsync($"http://cusotmapi01.azurewebsites.net/api/Patient?name={text}");
            var obj = JArray.Parse(responseString);
            return obj.Select(item => (item["Id"].ToString(), item["PatientName"].ToString(), item["PatientAge"].ToString(), item["PatientGender"]?.ToString(), item["PatientContactNo"]?.ToString(), item["Photograph"]?.ToString()));
        }
    }
}

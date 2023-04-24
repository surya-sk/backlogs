using Backlogs.Models;
using Backlogs.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Backlogs.Utils.UWP
{
    public class LiveTileManager : ILiveTileService
    {
        private string m_tileStyle;
        public void EnableLiveTileQueue()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        public void ShowLiveTiles(string tileSetting, string tileStyle, List<Backlog> backlogs)
        {
            m_tileStyle = tileStyle;  
            switch (tileSetting)
            {
                case "Recently Created":
                    {
                        foreach (var b in backlogs.Take(5))
                        {
                            GenerateRecentlyAddedLiveTile(b);
                        }
                    }
                    break;
                case "Recently Completed":
                    {
                        foreach(var b in backlogs.Take(5))
                        {
                            GenerateRecentlyCompletedLiveTile(b);
                        }
                    }
                    break;
                case "In Progress":
                    {
                        foreach(var b in backlogs.Take(5))
                        {
                            GenerateInProgressLiveTile(b);
                        }
                    }
                    break;
                case "Upcoming":
                    {
                        foreach(var b in backlogs.Take(5))
                        {
                            GenerateUpcomingLiveTile(b);
                        }
                    }
                    break;
            }
        }
        private void GenerateRecentlyAddedLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (m_tileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name,
                                    HintWrap = true,
                                    HintStyle= AdaptiveTextStyle.Caption,
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.TargetDate,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = false
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = false
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void GenerateRecentlyCompletedLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (m_tileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.UserRating} / 5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Rating: {b.UserRating}/5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"Rating: {b.UserRating} / 5",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = false
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.UserRating} / 5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Rating: {b.UserRating}/5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Rating: {b.UserRating} / 5",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = false
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void GenerateInProgressLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (m_tileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} of {b.Length} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Progress} of {b.Length} {b.Units}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} of {b.Length} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Progress} of {b.Length} {b.Units}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void GenerateUpcomingLiveTile(Backlog b)
        {
            TileContent tileContent = null;
            if (m_tileStyle == "Peeking")
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Target Date: {b.TargetDate}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = b.ImageURL
                                },
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"Target Date: {b.TargetDate}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                            }
                        }
                    }
                };
            }
            else
            {
                tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            DisplayName = "Backlogs",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Type,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.TargetDate,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Target Date: {b.TargetDate}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.NameAndLogo,
                            DisplayName = "Backlogs (Beta)",
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = b.ImageURL,
                                    HintOverlay = 50
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = b.Name
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{b.Type} - {b.Director}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = b.Description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"Target Date: {b.TargetDate}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true
                                    }
                                }
                            }
                        }
                    }
                };
            }


            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }
    }
}

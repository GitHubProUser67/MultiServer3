using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
    /// <summary>
	/// Uplay achievements service
	/// </summary>
    [RMCService(RMCProtocolId.UplayWinService)]
    public class UplayWinService : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult GetActions(int start_row_index, int maximum_rows, string sort_expression, string culture_name, string platform_code, string game_code)
        {
            var result = new List<UplayAction>();
            return Result(result);
        }

        [RMCMethod(2)]
        public RMCResult GetActionsCompleted(int start_row_index, int maximum_rows, string sort_expression, string culture_name, string platform_code, string game_code)
        {
            var result = new List<UplayAction>();
            return Result(result);
        }

        [RMCMethod(3)]
        public RMCResult GetActionsCount(string platform_code, string game_code)
        {
            int actions_count = 0;
            return Result(new { actions_count });
        }

        [RMCMethod(4)]
        public RMCResult GetActionsCompletedCount(string platform_code, string game_code)
        {
            int actions_count = 0;
            return Result(new { actions_count });
        }

        [RMCMethod(5)]
        public RMCResult GetRewards(int start_row_index, int maximum_rows, string sort_expression, string culture_name, string platform_code, string game_code)
        {
            var rewards = new List<UPlayReward>()
            {
                new UPlayReward() // useless but we still adding it
                {
                    m_code = "DRV5REWARD01PC",
                    m_name = "Exclusive Wallpaper",
                    m_description = "Download the DRIVER San Francisco Wallpaper.",
                    m_rewardTypeName = "Downloadable",
                    m_gameCode = "DRV5",
                    m_value = 0,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platform_code,
                            m_purchased = true
                        }
                    }
                },
                new UPlayReward()
                {
                    m_code = "DRV5REWARD02",
                    m_name = "Tanner's Day Off Challenge",
                    m_description = "Tear through Russian Hill in Tannerâ\u0080\u0099s iconic Dodge Challenger.",
                    m_rewardTypeName = "Unlockable",
                    m_gameCode = "DRV5",
                    m_value = 20,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platform_code,
                            m_purchased = true
                        }
                    }
                },
                new UPlayReward()
                {
                    m_code = "DRV5REWARD03",
                    m_name = "Dodge Charger SRT8 Police Car",
                    m_description = "Unlocks the Dodge Charger SRT8 Police Car for use in Online games.",
                    m_rewardTypeName = "Unlockable",
                    m_gameCode = "DRV5",
                    m_value = 30,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platform_code,
                            m_purchased = true
                        }
                    }
                },
                new UPlayReward()
                {
                    m_code = "DRV5REWARD04",
                    m_name = "San Francisco Challenges",
                    m_description = "Four Challenges that showcase different areas of San Francisco.",
                    m_rewardTypeName = "Unlockable",
                    m_gameCode = "DRV5",
                    m_value = 40,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platform_code,
                            m_purchased = true
                        }
                    }
                },
            };

            // return 
            return Result(rewards);
        }

        [RMCMethod(6)]
        public RMCResult GetRewardsPurchased(int startRowIndex, int maximumRows, string sortExpression, string cultureName, string platformCode)
        {
            var rewards = new List<UPlayReward>()
            {
                new UPlayReward() // useless but we still adding it
                {
                    m_code = "DRV5REWARD01PC",
                    m_name = "Exclusive Wallpaper",
                    m_description = "Download the DRIVER San Francisco Wallpaper.",
                    m_rewardTypeName = "Downloadable",
                    m_gameCode = "DRV5",
                    m_value = 0,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platformCode,
                            m_purchased = true
                        }
                    }
                },
                new UPlayReward()
                {
                    m_code = "DRV5REWARD02",
                    m_name = "Tanner's Day Off Challenge",
                    m_description = "Tear through Russian Hill in Tannerâ\u0080\u0099s iconic Dodge Challenger.",
                    m_rewardTypeName = "Unlockable",
                    m_gameCode = "DRV5",
                    m_value = 20,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platformCode,
                            m_purchased = true
                        }
                    }
                },
                new UPlayReward()
                {
                    m_code = "DRV5REWARD03",
                    m_name = "Dodge Charger SRT8 Police Car",
                    m_description = "Unlocks the Dodge Charger SRT8 Police Car for use in Online games.",
                    m_rewardTypeName = "Unlockable",
                    m_gameCode = "DRV5",
                    m_value = 30,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platformCode,
                            m_purchased = true
                        }
                    }
                },
                new UPlayReward()
                {
                    m_code = "DRV5REWARD04",
                    m_name = "San Francisco Challenges",
                    m_description = "Four Challenges that showcase different areas of San Francisco.",
                    m_rewardTypeName = "Unlockable",
                    m_gameCode = "DRV5",
                    m_value = 40,
                    m_platforms = new List<UPlayRewardPlatform>()
                    {
                        new UPlayRewardPlatform()
                        {
                            m_platformCode = platformCode,
                            m_purchased = true
                        }
                    }
                },
            };

            // return 
            return Result(rewards);
        }

        [RMCMethod(7)]
        public RMCResult UplayWelcome(string culture, string platformCode)
        {
            var result = new List<UplayAction>();
            return Result(result);
        }

        [RMCMethod(8)]
        public RMCResult SetActionCompleted(string actionCode, string cultureName, string platformCode)
        {
            UNIMPLEMENTED();
            var unlockedAction = new UplayAction()
            {
                m_code = actionCode,
                m_description = actionCode + "_description",
                m_gameCode = "DRV5",
                m_name = actionCode + "_action",
                m_value = 1,
            };
            unlockedAction.m_platforms.Add(new UplayActionPlatform()
            {
                m_completed = true,
                m_platformCode = platformCode,
                m_specificKey = string.Empty
            });

            return Result(unlockedAction);
        }

        [RMCMethod(9)]
        public RMCResult SetActionsCompleted(IEnumerable<string> actionCodeList, string cultureName, string platformCode)
        {
            var actionList = new List<UplayAction>();
            return Result(actionList);
        }

        [RMCMethod(10)]
        public RMCResult GetUserToken()
        {
            // This is a basic JWT token, ideally it's generated from player.
            return Result(new { token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.S9fU6h-NFl8A9G3nJAzW7hNL1sU2Rkf1DkZfnU8PXGw" });
        }

        [RMCMethod(11)]
        public RMCResult GetVirtualCurrencyUserBalance(string platform_code)
        {
            int virtual_currency_user_balance = 10;
            return Result(new { virtual_currency_user_balance });
        }

        [RMCMethod(12)]
        public RMCResult GetSectionsByKey(string culture_name, string section_key, string platform_code, string game_code)
        {
            var actionList = new List<UplaySection>();
            return Result(actionList);
        }

        [RMCMethod(13)]
        public RMCResult BuyReward(string reward_code, string platform_code)
        {
            int virtual_currency_user_balance = 10;
            return Result(new { virtual_currency_user_balance });
        }
    }
}

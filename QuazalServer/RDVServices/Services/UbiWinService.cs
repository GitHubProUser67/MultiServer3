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
	class UbiWinService : RMCServiceBase
	{
		[RMCMethod(1)]
		public void GetActions()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(2)]
		public void GetActionsCompleted()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(3)]
		public void GetActionsCount()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(4)]
		public void GetActionsCompletedCount()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(5)]
		public void GetRewards()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(6)]
		public RMCResult GetRewardsPurchased(int startRowIndex, int maximumRows, string sortExpression, string cultureName, string platformCode)
		{
			var rewards = new List<UPlayReward>()
			{
				new UPlayReward() // uselsess but we still adding it
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
		public void GetUserToken()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(11)]
		public void GetVirtualCurrencyUserBalance()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(12)]
		public void GetSectionsByKey()
		{
			UNIMPLEMENTED();
		}

	}
}

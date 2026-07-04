using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HextechRunes;

internal static class HextechReplayCompat
{
	private sealed class CardPlayDestination
	{
		internal CardPlayDestination(PileType pileType)
		{
			PileType = pileType;
		}

		internal PileType PileType { get; }
	}

	private static readonly ConditionalWeakTable<CardModel, CardPlayDestination> CardPlayDestinations = new();
	internal static bool IsFirstUserPlayInSeries(CardPlay cardPlay)
	{
		return cardPlay.IsFirstInSeries && !cardPlay.IsAutoPlay;
	}

	internal static bool IsLastPlayInSeries(CardPlay cardPlay)
	{
		return cardPlay.IsLastInSeries;
	}

	internal static bool ShouldSkipUnsafeNetworkPowerReplay(CardModel? card, bool isNetworkMultiplayer)
	{
		return isNetworkMultiplayer && card?.Type == CardType.Power;
	}

	internal static (PileType pileType, CardPilePosition position) RecordCardPlayResultPile(
		CardModel card,
		PileType pileType,
		CardPilePosition position)
	{
		CardPlayDestinations.Remove(card);
		CardPlayDestinations.Add(card, new CardPlayDestination(pileType));
		return (pileType, position);
	}

	internal static bool WasCardPlayResultForcedToExhaust(CardModel card)
	{
		return CardPlayDestinations.TryGetValue(card, out CardPlayDestination? destination)
			&& destination.PileType == PileType.Exhaust;
	}
}
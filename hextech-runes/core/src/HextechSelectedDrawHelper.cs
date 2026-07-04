using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HextechRunes;

internal static class HextechSelectedDrawHelper
{
	private static readonly LocString SelectDrawPrompt = new("card_selection", "HEXTECH_TO_DRAW");

	internal static async Task<IEnumerable<CardModel>> DrawSelectedFromDrawPile(
		PlayerChoiceContext choiceContext,
		Player player,
		int requestedDraws,
		bool fromHandDraw)
	{
		if (requestedDraws <= 0 || CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}

		if (player.Creature.CombatState == null)
		{
			return Array.Empty<CardModel>();
		}

		if (!CombatCompat.ShouldDraw(player, fromHandDraw, out AbstractModel? modifier))
		{
			if (modifier != null)
			{
				await CombatCompat.AfterPreventingDraw(player, modifier);
			}

			return Array.Empty<CardModel>();
		}

		CardPile hand = PileType.Hand.GetPile(player);
		CardPile drawPile = PileType.Draw.GetPile(player);
		int handSpace = Math.Max(0, CombatCompat.MaxCardsInHand - hand.Cards.Count);
		if (handSpace == 0)
		{
			ShowDrawFailureThoughtBubble(player);
			return Array.Empty<CardModel>();
		}

		int cardsToSelect = Math.Min(requestedDraws, handSpace);
		if (!CanDrawAnyCards(player))
		{
			ShowDrawFailureThoughtBubble(player);
			return Array.Empty<CardModel>();
		}

		await ShuffleIntoDrawPileIfShort(choiceContext, player, cardsToSelect);
		if (!CanDrawAnyCards(player))
		{
			ShowDrawFailureThoughtBubble(player);
			return Array.Empty<CardModel>();
		}

		cardsToSelect = Math.Min(cardsToSelect, drawPile.Cards.Count);
		if (cardsToSelect <= 0)
		{
			return Array.Empty<CardModel>();
		}

		CardSelectorPrefs prefs = new(SelectDrawPrompt, cardsToSelect);
		List<CardModel> selected = await SelectCardsFromDrawPile(choiceContext, drawPile, player, prefs);
		selected = selected.Take(cardsToSelect).ToList();

		List<CardModel> drawn = new(selected.Count);
		foreach (CardModel card in selected)
		{
			if (CombatManager.Instance.IsOverOrEnding || hand.Cards.Count >= CombatCompat.MaxCardsInHand)
			{
				break;
			}

			if (card.Pile != drawPile)
			{
				continue;
			}

			drawn.Add(card);
			await CardPileCmd.Add(card, hand);
			CombatCompat.RecordCardDrawn(player, card, fromHandDraw);
			await CombatCompat.AfterCardDrawn(player, choiceContext, card, fromHandDraw);
			card.InvokeDrawn();
			NDebugAudioManager.Instance?.Play("card_deal.mp3", 0.25f, PitchVariance.Small);
		}

		return drawn;
	}

	private static async Task<List<CardModel>> SelectCardsFromDrawPile(
		PlayerChoiceContext choiceContext,
		CardPile drawPile,
		Player player,
		CardSelectorPrefs prefs)
	{
#if STS2_99_1 || STS2_100_0
		// v0.99/v0.100 lack CardSelectCmd.FromCombatPile; show draw pile via simple grid instead.
		return (await CardSelectCmd.FromSimpleGrid(
			choiceContext,
			drawPile.Cards.ToList(),
			player,
			prefs)).ToList();
#else
		return (await CardSelectCmd.FromCombatPile(choiceContext, drawPile, player, prefs)).ToList();
#endif
	}

	private static async Task ShuffleIntoDrawPileIfShort(PlayerChoiceContext choiceContext, Player player, int requestedDraws)
	{
		CardPile drawPile = PileType.Draw.GetPile(player);
		CardPile discardPile = PileType.Discard.GetPile(player);
		if (drawPile.Cards.Count < requestedDraws && discardPile.Cards.Any())
		{
			await CardPileCmd.Shuffle(choiceContext, player);
		}
	}

	private static bool CanDrawAnyCards(Player player)
	{
		return PileType.Draw.GetPile(player).Cards.Count + PileType.Discard.GetPile(player).Cards.Count > 0
			&& PileType.Hand.GetPile(player).Cards.Count < CombatCompat.MaxCardsInHand;
	}

	private static void ShowDrawFailureThoughtBubble(Player player)
	{
		string key = PileType.Hand.GetPile(player).Cards.Count >= CombatCompat.MaxCardsInHand
			? "HAND_FULL"
			: "NO_DRAW";
		ThinkCmd.Play(new LocString("combat_messages", key), player.Creature, 2.0);
	}
}

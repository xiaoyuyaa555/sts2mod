using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HextechRunes;

internal static class CombatCompat
{
	internal static int MaxCardsInHand
	{
		get
		{
#if STS2_99_1 || STS2_100_0
			// v0.99/v0.100 do not expose CardPile.MaxCardsInHand.
			// TODO verify whether old builds have an equivalent hand-limit API.
			return 10;
#else
			return CardPile.MaxCardsInHand;
#endif
		}
	}

	internal static bool ShouldDraw(Player player, bool fromHandDraw, out AbstractModel? modifier)
	{
#if STS2_99_1 || STS2_100_0
		CombatState? combatState = player.Creature.CombatState;
		if (combatState == null)
		{
			modifier = null;
			return false;
		}

		return Hook.ShouldDraw(combatState, player, fromHandDraw, out modifier);
#else
		ICombatState? combatState = player.Creature.CombatState;
		if (combatState == null)
		{
			modifier = null;
			return false;
		}

		return Hook.ShouldDraw(combatState, player, fromHandDraw, out modifier);
#endif
	}

	internal static Task AfterPreventingDraw(Player player, AbstractModel modifier)
	{
		return Hook.AfterPreventingDraw(player.Creature.CombatState!, modifier);
	}

	internal static Task AfterCardDrawn(Player player, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		return Hook.AfterCardDrawn(player.Creature.CombatState!, choiceContext, card, fromHandDraw);
	}

	internal static void RecordCardDrawn(Player player, CardModel card, bool fromHandDraw)
	{
		CombatManager.Instance.History.CardDrawn(player.Creature.CombatState!, card, fromHandDraw);
	}
}
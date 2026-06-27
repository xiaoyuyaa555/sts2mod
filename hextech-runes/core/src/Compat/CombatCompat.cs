using MegaCrit.Sts2.Core.Entities.Cards;

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
}
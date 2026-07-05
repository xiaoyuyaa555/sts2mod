using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HextechRunes;

public sealed class OkBoomerangRune : HextechRelicBase
{
	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CardsVar(1)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromCard<OkBoomerangCard>()
	];

	public override Task AfterObtained()
	{
		return AddCardCopiesToDeckOrHand<OkBoomerangCard>(DynamicVars.Cards.IntValue);
	}

	public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
		CardModel card,
		bool isAutoPlay,
		ResourceInfo resources,
		PileType pileType,
		CardPilePosition position)
	{
		return card is OkBoomerangCard
			? HextechReplayCompat.RecordCardPlayResultPile(card, PileType.Hand, CardPilePosition.Bottom)
			: HextechReplayCompat.RecordCardPlayResultPile(card, pileType, position);
	}
}

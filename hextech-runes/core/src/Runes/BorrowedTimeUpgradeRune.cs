using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HextechRunes;

public sealed class BorrowedTimeUpgradeRune : CardUpgradeRuneBase<BorrowedTime>
{
	private decimal _borrowedTimeBeforePlay;
	private bool _shouldCleanBorrowedTime;

	protected override bool IsAvailableForCharacter(Player player)
	{
		return IsNecrobinderPlayer(player);
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		_shouldCleanBorrowedTime = cardPlay.Card.Owner == Owner && cardPlay.Card is BorrowedTime;
		_borrowedTimeBeforePlay = _shouldCleanBorrowedTime && Owner != null
			? GetBorrowedTimeDebtAmount(Owner.Creature)
			: 0m;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (!_shouldCleanBorrowedTime || Owner == null || cardPlay.Card.Owner != Owner || cardPlay.Card is not BorrowedTime)
		{
			return;
		}

		_shouldCleanBorrowedTime = false;
		decimal excess = GetBorrowedTimeDebtAmount(Owner.Creature) - _borrowedTimeBeforePlay;
		if (excess <= 0m)
		{
			return;
		}

		Flash();
		await RemoveBorrowedTimeDebt(Owner.Creature, excess, cardPlay.Card);
	}
	private static decimal GetBorrowedTimeDebtAmount(Creature creature)
	{
#if STS2_99_1 || STS2_100_0
		return creature.GetPowerAmount<DoomPower>();
#else
		return creature.GetPowerAmount<BorrowedTimePower>();
#endif
	}

	private static Task RemoveBorrowedTimeDebt(Creature creature, decimal amount, CardModel source)
	{
#if STS2_99_1 || STS2_100_0
		return PowerCmd.Apply<DoomPower>(creature, -amount, creature, source, silent: true);
#else
		return PowerCmd.Apply<BorrowedTimePower>(creature, -amount, creature, source, silent: true);
#endif
	}
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace HextechRunes;

public sealed class OkBoomerangCard : CardModel
{
	public override CardPoolModel Pool => IsMutable && Owner != null
		? Owner.Character.CardPool
		: ModelDb.CardPool<TokenCardPool>();

	public override CardPoolModel VisualCardPool => Pool;

	public override string PortraitPath => HextechAssets.OkBoomerangCardPortraitPath;

	public override IEnumerable<string> AllPortraitPaths => [PortraitPath];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(6m, ValueProp.Move),
		new DynamicVar("Hits", 2m)
	];

	public OkBoomerangCard()
		: base(1, CardType.Attack, CardRarity.Token, TargetType.AllEnemies, shouldShowInCardLibrary: true)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner?.Creature.CombatState == null)
		{
			return;
		}

		List<Creature> enemies = Owner.Creature.CombatState.HittableEnemies.ToList();
		if (enemies.Count == 0)
		{
			return;
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", 0f);
		for (int hit = 0; hit < DynamicVars["Hits"].IntValue; hit++)
		{
			foreach (Creature enemy in enemies.ToList())
			{
				if (enemy.IsDead || Owner?.Creature.CombatState == null)
				{
					continue;
				}

				await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
					.FromCard(this)
					.Targeting(enemy)
					.Execute(choiceContext);
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}
}

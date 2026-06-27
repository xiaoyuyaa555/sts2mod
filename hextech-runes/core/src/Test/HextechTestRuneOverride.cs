using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace HextechRunes;

internal static class HextechTestRuneOverride
{
	private const int RequiredOfferCount = 3;

	public static List<RelicModel>? TryBuildPlayerRuneOffers(
		Player player,
		HextechRarityTier rarity,
		RunState runState,
		IReadOnlyList<RelicModel> filteredPool,
		IReadOnlyDictionary<string, int> tagCounts,
		bool useEndlessTagWindow)
	{
		HextechTestConfig config = HextechTestMode.Config;
		if (!HextechTestMode.Enabled || config.ForcePlayerRuneOffers.Count == 0)
		{
			return null;
		}

		if (!HextechTestMode.IsSingleplayerAllowed("player rune override"))
		{
			return null;
		}

		if (!HextechTestMode.IsActAllowed(runState.CurrentActIndex))
		{
			HextechTestMode.Info($"player rune override skipped: act mismatch current={runState.CurrentActIndex + 1} force_act={config.ForceAct}.");
			return null;
		}

		Dictionary<string, RelicModel> poolById = filteredPool
			.GroupBy(static relic => (relic.CanonicalInstance?.Id ?? relic.Id).Entry, StringComparer.Ordinal)
			.ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
		List<RelicModel> selected = [];
		HashSet<string> selectedIds = new(StringComparer.Ordinal);

		foreach (string id in config.ForcePlayerRuneOffers)
		{
			if (!poolById.TryGetValue(id, out RelicModel? relic))
			{
				HextechTestMode.Warn($"player rune id skipped: id={id} reason=not-in-current-filtered-pool rarity={rarity}.");
				continue;
			}

			ModelId modelId = relic.CanonicalInstance?.Id ?? relic.Id;
			if (!selectedIds.Add(modelId.Entry))
			{
				HextechTestMode.Warn($"player rune id skipped: id={id} reason=duplicate.");
				continue;
			}

			selected.Add(HextechRunePoolBuilder.CreateSelectableRuneOption(player, relic));
			HextechTestMode.Info($"player rune applied: id={modelId.Entry} rarity={rarity}.");
		}

		if (selected.Count == 0)
		{
			HextechTestMode.Warn("player rune override produced no valid offers; falling back to normal random logic.");
			return null;
		}

		if (selected.Count < RequiredOfferCount)
		{
			if (config.DisableRandomRunePool)
			{
				HextechTestMode.Warn($"player rune override has only {selected.Count} valid offers with disable_random_rune_pool=true; filling from normal pool to keep UI safe.");
			}

			FillFromNormalPool(player, filteredPool, selected, selectedIds, runState, tagCounts, useEndlessTagWindow);
		}

		HextechTestMode.Info($"player rune offers final=[{string.Join(",", selected.Select(static relic => (relic.CanonicalInstance?.Id ?? relic.Id).Entry))}].");
		return selected;
	}

	public static MonsterHexKind? TryOverrideEnemyHex(
		HextechMayhemModifier modifier,
		HextechRarityTier rarity,
		RunState runState,
		int actIndex,
		IReadOnlyList<MonsterHexKind> filteredPool,
		MonsterHexKind? fallback)
	{
		HextechTestConfig config = HextechTestMode.Config;
		if (!HextechTestMode.Enabled || string.IsNullOrWhiteSpace(config.ForceEnemyRune))
		{
			return fallback;
		}

		if (!HextechTestMode.IsSingleplayerAllowed("enemy hex override"))
		{
			return fallback;
		}

		if (!HextechTestMode.IsActAllowed(actIndex))
		{
			HextechTestMode.Info($"enemy hex override skipped: act mismatch current={actIndex + 1} force_act={config.ForceAct}.");
			return fallback;
		}

		if (!Enum.TryParse(config.ForceEnemyRune, ignoreCase: true, out MonsterHexKind forcedKind))
		{
			HextechTestMode.Warn($"enemy hex skipped: id={config.ForceEnemyRune} reason=not-a-MonsterHexKind.");
			return fallback;
		}

		if (!HextechContentRegistry.MonsterHexMetadata.IsRegistered(forcedKind))
		{
			HextechTestMode.Warn($"enemy hex skipped: id={config.ForceEnemyRune} reason=not-registered.");
			return fallback;
		}

		if (!HextechContentRegistry.MonsterHexMetadata.IsEnabled(forcedKind))
		{
			HextechTestMode.Warn($"enemy hex skipped: id={config.ForceEnemyRune} reason=disabled.");
			return fallback;
		}

		if (!filteredPool.Contains(forcedKind))
		{
			HextechTestMode.Warn($"enemy hex skipped: id={config.ForceEnemyRune} reason=not-in-current-pool rarity={rarity} known={modifier.GetKnownMonsterHexes().Count}.");
			return fallback;
		}

		HextechTestMode.Info($"enemy hex applied: id={forcedKind} rarity={rarity}.");
		return forcedKind;
	}

	private static void FillFromNormalPool(
		Player player,
		IReadOnlyList<RelicModel> filteredPool,
		List<RelicModel> selected,
		HashSet<string> selectedIds,
		RunState runState,
		IReadOnlyDictionary<string, int> tagCounts,
		bool useEndlessTagWindow)
	{
		List<RelicModel> pool = filteredPool
			.Where(relic => selectedIds.Add((relic.CanonicalInstance?.Id ?? relic.Id).Entry))
			.ToList();

		while (selected.Count < RequiredOfferCount && pool.Count > 0)
		{
			List<int> weights = HextechRunePoolBuilder.BuildRuneTagWeights(pool, tagCounts, useEndlessTagWindow, out int totalWeight);
			int index = HextechRunePoolBuilder.SelectWeightedIndex(weights, runState.Rng.Niche.NextInt(totalWeight));
			RelicModel relic = pool[index];
			selected.Add(HextechRunePoolBuilder.CreateSelectableRuneOption(player, relic));
			pool.RemoveAt(index);
		}
	}
}
#if STS2_99_1 || STS2_100_0
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using static HextechRunes.HextechHookReflection;

namespace HextechRunes;

internal static class HextechAncientRelicCompatHooks
{
	private static readonly MethodInfo? GetTranscendenceStarterCardMethod =
		AccessTools.Method(typeof(ArchaicTooth), "GetTranscendenceStarterCard", [typeof(Player)]);

	private static readonly MethodInfo? GetTranscendenceTransformedCardMethod =
		AccessTools.Method(typeof(ArchaicTooth), "GetTranscendenceTransformedCard", [typeof(CardModel)]);

	public static void Install(Harmony harmony)
	{
		harmony.Patch(
			RequireMethod(typeof(ArchaicTooth), nameof(ArchaicTooth.AfterObtained), BindingFlags.Instance | BindingFlags.Public),
			prefix: new HarmonyMethod(typeof(HextechAncientRelicCompatHooks), nameof(ArchaicToothAfterObtainedPrefix)));
		Log.Info($"[{ModInfo.Id}][Mayhem] Installed ArchaicTooth obtain compatibility hook for {ModInfo.TargetGameVersion}.");
	}

	private static bool ArchaicToothAfterObtainedPrefix(ArchaicTooth __instance, ref Task __result)
	{
		__result = SafeArchaicToothAfterObtained(__instance);
		return false;
	}

	private static async Task SafeArchaicToothAfterObtained(ArchaicTooth tooth)
	{
		Player? player = tooth.Owner;
		if (player == null || GetTranscendenceStarterCardMethod == null || GetTranscendenceTransformedCardMethod == null)
		{
			return;
		}

		CardModel starter = (CardModel)GetTranscendenceStarterCardMethod.Invoke(tooth, [player])!;
		CardModel transformed;
		try
		{
			transformed = (CardModel)GetTranscendenceTransformedCardMethod.Invoke(tooth, [starter])!;
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("Cannot enchant", StringComparison.Ordinal))
		{
			Log.Warn(
				$"[{ModInfo.Id}][Mayhem] ArchaicTooth SPIRAL enchant blocked on {starter.Id.Entry}; applying upgrade-only transcendence fallback.");
			transformed = (CardModel)starter.MutableClone();
			if (!transformed.IsUpgraded)
			{
				CardCmd.Upgrade(transformed, CardPreviewStyle.None);
			}
		}

		try
		{
			await CardCmd.Transform(starter, transformed, CardPreviewStyle.None);
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("Non-removable cards cannot be transformed", StringComparison.Ordinal))
		{
			Log.Warn(
				$"[{ModInfo.Id}][Mayhem] ArchaicTooth transform skipped for non-removable starter card {starter.Id.Entry} on {ModInfo.TargetGameVersion}.");
		}
	}
}
#endif


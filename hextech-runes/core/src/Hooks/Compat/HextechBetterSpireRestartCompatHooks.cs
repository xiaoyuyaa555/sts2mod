using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace HextechRunes;

internal static class HextechBetterSpireRestartCompatHooks
{
	public static void Install(Harmony harmony)
	{
		Type? restartTracker = AccessTools.TypeByName("BetterSpire2.RestartTracker");
		MethodInfo? restartRun = restartTracker != null
			? AccessTools.Method(restartTracker, "RestartRun")
			: null;
		if (restartRun == null)
		{
			return;
		}

		harmony.Patch(
			restartRun,
			postfix: new HarmonyMethod(typeof(HextechBetterSpireRestartCompatHooks), nameof(RestartRunPostfix)));
		Log.Info($"[{ModInfo.Id}][BetterSpire] Installed Hold-R restart compatibility hook.");
	}

	private static void RestartRunPostfix()
	{
		Type? restartTracker = AccessTools.TypeByName("BetterSpire2.RestartTracker");
		FieldInfo? modifiersField = restartTracker != null
			? AccessTools.Field(restartTracker, "_modifiers")
			: null;
		if (modifiersField?.GetValue(null) is not List<ModifierModel> modifiers)
		{
			return;
		}

		int removed = modifiers.RemoveAll(static modifier => modifier is HextechMayhemModifier);
		if (removed > 0)
		{
			Log.Info($"[{ModInfo.Id}][BetterSpire] Removed {removed} stale HextechMayhemModifier instance(s) from Hold-R restart snapshot.");
		}
	}
}

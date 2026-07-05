using System.Collections;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves.Runs;
using static HextechRunes.HextechHookReflection;

namespace HextechRunes;

/// <summary>
/// Normalizes SavedProperties net-id assignment so multiplayer peers do not
/// depend on local mod load/injection order.
/// </summary>
internal static class HextechSavedPropertyNetIdHooks
{
	private const BindingFlags StaticNonPublic = BindingFlags.NonPublic | BindingFlags.Static;
	private const BindingFlags InstanceProperties = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static bool _installed;
	private static string? _lastCanonicalSignature;

	public static void Install(Harmony harmony)
	{
		if (_installed)
		{
			return;
		}

		_installed = true;

		MethodInfo? startupTarget = FindStartupTarget();
		if (startupTarget == null)
		{
			Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] Could not find a startup hook target for SavedProperty net-id canonicalization; falling back to direct canonicalization only.");
		}
		else
		{
			TryPatch(
				harmony,
				startupTarget,
				$"{startupTarget.DeclaringType?.FullName}.{startupTarget.Name} SavedProperty net-id canonicalization",
				postfix: new HarmonyMethod(typeof(HextechSavedPropertyNetIdHooks), nameof(EnsureCanonicalized)));
		}

		EnsureCanonicalized();
	}

	internal static void EnsureCanonicalized()
	{
		try
		{
			IReadOnlySet<string>? vanillaNames = BuildVanillaPropertyNameSet();
			if (vanillaNames == null || vanillaNames.Count == 0)
			{
				Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] Could not determine vanilla SavedProperty names; skipping net-id canonicalization.");
				return;
			}

			FieldInfo? netIdToNameField = TryGetField(typeof(SavedPropertiesTypeCache), "_netIdToPropertyNameMap", StaticNonPublic);
			FieldInfo? nameToNetIdField = TryGetField(typeof(SavedPropertiesTypeCache), "_propertyNameToNetIdMap", StaticNonPublic);
			if (netIdToNameField?.GetValue(null) is not List<string> netIdToName
				|| nameToNetIdField?.GetValue(null) is not Dictionary<string, int> nameToNetId)
			{
				Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] SavedPropertiesTypeCache maps unavailable; skipping net-id canonicalization.");
				return;
			}

			List<string>? canonical = HextechSavedPropertyNetIdCanonicalizer.Canonicalize(netIdToName, vanillaNames);
			if (canonical == null || canonical.Count != netIdToName.Count)
			{
				Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] Net-id canonicalization produced an invalid result (mapCount={netIdToName.Count}); leaving the map unchanged.");
				return;
			}

			string signature = string.Join('\n', canonical);
			if (string.Equals(signature, _lastCanonicalSignature, StringComparison.Ordinal))
			{
				return;
			}

			netIdToName.Clear();
			netIdToName.AddRange(canonical);
			nameToNetId.Clear();
			for (int i = 0; i < canonical.Count; i++)
			{
				nameToNetId[canonical[i]] = i;
			}

			SetNetIdBitSize(HextechSavedPropertyNetIdCanonicalizer.ComputeNetIdBitSize(canonical.Count));
			_lastCanonicalSignature = signature;
			HextechLog.Info($"[{ModInfo.Id}][MultiplayerCompat] Canonicalized SavedProperty net-id map: vanilla={vanillaNames.Count} total={canonical.Count} bitSize={SavedPropertiesTypeCache.NetIdBitSize}.");
		}
		catch (Exception ex)
		{
			Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] SavedProperty net-id canonicalization failed: {ex.GetType().Name}: {ex.Message}");
		}
	}

	private static MethodInfo? FindStartupTarget()
	{
		Type? oneTimeInit = AccessTools.TypeByName("MegaCrit.Sts2.Core.Helpers.OneTimeInitialization");
		MethodInfo? executeEssential = oneTimeInit == null ? null : AccessTools.Method(oneTimeInit, "ExecuteEssential");
		if (executeEssential != null)
		{
			return executeEssential;
		}

		Type? nodeOneTimeInit = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.NOneTimeInitialization");
		return nodeOneTimeInit == null ? null : AccessTools.Method(nodeOneTimeInit, "_Ready");
	}

	private static IReadOnlySet<string>? BuildVanillaPropertyNameSet()
	{
		IEnumerable<Type>? modelTypes = GetAbstractModelSubtypes();
		if (modelTypes == null)
		{
			return null;
		}

		HashSet<string> names = new(StringComparer.Ordinal);
		foreach (Type type in modelTypes)
		{
			foreach (PropertyInfo property in type.GetProperties(InstanceProperties))
			{
				if (property.GetCustomAttribute<SavedPropertyAttribute>() != null)
				{
					names.Add(property.Name);
				}
			}
		}

		return names;
	}

	private static IEnumerable<Type>? GetAbstractModelSubtypes()
	{
		Type? abstractModelSubtypes = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.AbstractModelSubtypes");
		if (abstractModelSubtypes == null)
		{
			return null;
		}

		object? raw = abstractModelSubtypes.GetProperty("All", StaticNonPublic | BindingFlags.Public)?.GetValue(null)
			?? abstractModelSubtypes.GetField("All", StaticNonPublic | BindingFlags.Public)?.GetValue(null);
		if (raw is IEnumerable<Type> typed)
		{
			return typed;
		}

		if (raw is not IEnumerable enumerable)
		{
			return null;
		}

		List<Type> result = [];
		foreach (object? item in enumerable)
		{
			if (item is Type type)
			{
				result.Add(type);
			}
		}

		return result;
	}

	private static void SetNetIdBitSize(int bitSize)
	{
		FieldInfo? backing = TryGetField(typeof(SavedPropertiesTypeCache), "<NetIdBitSize>k__BackingField", StaticNonPublic);
		if (backing == null)
		{
			Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] SavedPropertiesTypeCache NetIdBitSize backing field not found; net-id bit size left unchanged.");
			return;
		}

		backing.SetValue(null, bitSize);
	}

	private static bool TryPatch(Harmony harmony, MethodBase target, string label, HarmonyMethod? prefix = null, HarmonyMethod? postfix = null)
	{
		try
		{
			harmony.Patch(target, prefix, postfix);
			return true;
		}
		catch (Exception ex)
		{
			Log.Warn($"[{ModInfo.Id}][MultiplayerCompat] Skipped {label}: {ex.GetType().Name}: {ex.Message}");
			return false;
		}
	}
}

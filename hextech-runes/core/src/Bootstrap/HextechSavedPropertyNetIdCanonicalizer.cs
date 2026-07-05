namespace HextechRunes;

/// <summary>
/// Builds a deterministic SavedProperties net-id layout.
/// Vanilla property names keep their game-defined order; modded property names
/// are sorted ordinally so multiplayer peers do not depend on local mod load order.
/// </summary>
internal static class HextechSavedPropertyNetIdCanonicalizer
{
	internal static List<string>? Canonicalize(IReadOnlyList<string>? netIdToPropertyName, IReadOnlySet<string>? vanillaPropertyNames)
	{
		if (netIdToPropertyName == null || vanillaPropertyNames == null)
		{
			return null;
		}

		List<string> vanilla = [];
		List<string> modded = [];
		foreach (string name in netIdToPropertyName)
		{
			if (vanillaPropertyNames.Contains(name))
			{
				vanilla.Add(name);
			}
			else
			{
				modded.Add(name);
			}
		}

		modded.Sort(StringComparer.Ordinal);

		List<string> result = new(vanilla.Count + modded.Count);
		result.AddRange(vanilla);
		result.AddRange(modded);
		return result;
	}

	internal static int ComputeNetIdBitSize(int propertyNameCount)
	{
		return propertyNameCount <= 0 ? 0 : (int)Math.Ceiling(Math.Log2(propertyNameCount));
	}
}

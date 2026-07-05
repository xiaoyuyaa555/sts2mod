namespace HextechRunes;

/// <summary>
/// Compatibility shim for upstream 0.8.5 enemy-hex identity splits.
/// v0.99.1-lite does not enable the new enemy hexes yet; when the target
/// enum value is not present, migration leaves the old value/name unchanged.
/// </summary>
internal static class MonsterHexKindMigration
{
	private static readonly IReadOnlyDictionary<int, string> RetiredValueToNewName =
		new Dictionary<int, string>
		{
			[18] = "Queen",
			[47] = "SkulkingColony",
			[64] = "LagavulinMatriarch",
			[71] = "PhantasmalGardener",
			[72] = "Exoskeleton",
			[82] = "TestSubject"
		};

	private static readonly IReadOnlyDictionary<string, string> RetiredNameToNewName =
		new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["ImmortalBone"] = "SkulkingColony",
			["ScaredStiff"] = "PhantasmalGardener",
			["Misery"] = "LagavulinMatriarch",
			["GhostForm"] = "Exoskeleton",
			["SymphonyOfWar"] = "TestSubject"
		};

	internal static int RemapRawValue(int rawHex)
	{
		return RetiredValueToNewName.TryGetValue(rawHex, out string? newName)
			&& Enum.TryParse(newName, out MonsterHexKind newKind)
			? (int)newKind
			: rawHex;
	}

	internal static string RemapName(string name)
	{
		return RetiredNameToNewName.TryGetValue(name, out string? newName)
			&& Enum.TryParse(newName, out MonsterHexKind _)
			? newName
			: name;
	}
}

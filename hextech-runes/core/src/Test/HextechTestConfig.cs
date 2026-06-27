using System.Text.Json.Serialization;

namespace HextechRunes;

internal sealed class HextechTestConfig
{
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	[JsonPropertyName("force_player_rune_offers")]
	public List<string> ForcePlayerRuneOffers { get; set; } = [];

	[JsonPropertyName("force_enemy_rune")]
	public string? ForceEnemyRune { get; set; }

	[JsonPropertyName("force_act")]
	public int? ForceAct { get; set; }

	[JsonPropertyName("disable_random_rune_pool")]
	public bool DisableRandomRunePool { get; set; }

	[JsonPropertyName("log_test_decisions")]
	public bool LogTestDecisions { get; set; } = true;
}
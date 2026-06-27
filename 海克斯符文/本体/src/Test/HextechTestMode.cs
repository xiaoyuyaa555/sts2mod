using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace HextechRunes;

internal static class HextechTestMode
{
	private const string LogPrefix = "[HextechTestMode]";
	private const string ConfigFileName = "hextech_test_config.json";
	private static readonly JsonSerializerOptions JsonOptions = new() { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

	private static bool _initialized;
	private static HextechTestConfig _config = new();
	private static string? _configPath;
	private static bool _configPresent;

	public static HextechTestConfig Config => _config;

	public static bool Enabled => IsBuildAllowed && _configPresent && _config.Enabled;

	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		_config = LoadConfig(out _configPath, out _configPresent);
		if (!IsBuildAllowed)
		{
			Log.Info($"{LogPrefix} disabled: build does not allow test mode.");
			return;
		}

		if (!_configPresent)
		{
			Log.Info($"{LogPrefix} disabled: config not found. checked={_configPath ?? "<none>"}");
			return;
		}

		if (!_config.Enabled)
		{
			Log.Info($"{LogPrefix} disabled: config enabled=false path={_configPath}");
			return;
		}

		Log.Info($"{LogPrefix} enabled path={_configPath} force_player_rune_offers=[{string.Join(",", _config.ForcePlayerRuneOffers)}] force_enemy_rune={_config.ForceEnemyRune ?? "<none>"} force_act={_config.ForceAct?.ToString() ?? "<any>"} disable_random_rune_pool={_config.DisableRandomRunePool}");
	}

	public static bool IsActAllowed(int actIndex)
	{
		int? forceAct = _config.ForceAct;
		return forceAct is null || forceAct.Value == actIndex + 1;
	}

	public static bool IsSingleplayerAllowed(string context)
	{
		try
		{
			NetGameType gameType = RunManager.Instance.NetService.Type;
			if (gameType is NetGameType.Host or NetGameType.Client)
			{
				Warn($"{context}: disabled in multiplayer gameType={gameType}.");
				return false;
			}
		}
		catch (Exception ex)
		{
			Warn($"{context}: could not inspect multiplayer state; falling back to normal logic: {ex.GetType().Name}: {ex.Message}");
			return false;
		}

		return true;
	}

	public static void Info(string message)
	{
		if (Config.LogTestDecisions)
		{
			Log.Info($"{LogPrefix} {message}");
		}
	}

	public static void Warn(string message)
	{
		Log.Warn($"{LogPrefix} {message}");
	}

	private static bool IsBuildAllowed
	{
		get
		{
#if HEXTECH_TEST_MODE || STS2_99_1 || STS2_100_0
			return true;
#else
			return false;
#endif
		}
	}

	private static HextechTestConfig LoadConfig(out string? configPath, out bool configPresent)
	{
		foreach (string candidate in GetConfigCandidates())
		{
			configPath = candidate;
			if (!File.Exists(candidate))
			{
				continue;
			}

			configPresent = true;
			try
			{
				HextechTestConfig? loaded = JsonSerializer.Deserialize<HextechTestConfig>(File.ReadAllText(candidate), JsonOptions);
				if (loaded == null)
				{
					Warn($"config parse returned null; disabling test mode. path={candidate}");
					return new HextechTestConfig();
				}

				loaded.ForcePlayerRuneOffers = loaded.ForcePlayerRuneOffers
					.Where(static id => !string.IsNullOrWhiteSpace(id))
					.Select(static id => id.Trim())
					.ToList();
				loaded.ForceEnemyRune = string.IsNullOrWhiteSpace(loaded.ForceEnemyRune) ? null : loaded.ForceEnemyRune.Trim();
				return loaded;
			}
			catch (Exception ex)
			{
				Warn($"config read failed; disabling test mode. path={candidate} error={ex.GetType().Name}: {ex.Message}");
				return new HextechTestConfig();
			}
		}

		configPath = GetConfigCandidates().FirstOrDefault();
		configPresent = false;
		return new HextechTestConfig();
	}

	private static IEnumerable<string> GetConfigCandidates()
	{

		string? userDir = null;
		try
		{
			string godotUserDir = OS.GetUserDataDir();
			if (!string.IsNullOrWhiteSpace(godotUserDir))
			{
				userDir = Path.Combine(godotUserDir, ModInfo.Id);
			}
		}
		catch
		{
			// Godot user path is not always available during early initialization.
		}

		if (!string.IsNullOrWhiteSpace(userDir))
		{
			yield return Path.Combine(userDir, "test_config.json");
		}
	}
}
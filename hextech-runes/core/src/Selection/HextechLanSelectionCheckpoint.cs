using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace HextechRunes;

internal static class HextechLanSelectionCheckpoint
{
	private const string CheckpointSaveFileName = "current_lan_run_mp.hextech_pre_selection.save";
	private const string CheckpointPlayerNamesFileName = "current_lan_run_mp.hextech_pre_selection_player_names.json";
	private const string CheckpointMetadataFileName = "current_lan_run_mp.hextech_pre_selection_meta.json";

	public static async Task TryCreateBeforeSelectionAsync(RunState runState, int actIndex)
	{
		if (!IsLanHost())
		{
			return;
		}

		try
		{
			await SaveManager.Instance.SaveRun(null!, saveProgress: false);

			object saveStore = GetSaveStore();
			object lanSaveService = GetLanSaveService();
			string currentSavePath = GetStringProperty(lanSaveService, "CurrentMultiplayerRunSavePath");
			string currentPlayerNamesPath = GetStringProperty(lanSaveService, "CurrentMultiplayerRunPlayerNamesPath");

			if (!FileExists(saveStore, currentSavePath))
			{
				Log.Warn($"[{ModInfo.Id}][LanCheckpoint] Cannot create Hextech selection checkpoint: LAN save does not exist at {currentSavePath}");
				return;
			}

			string checkpointSavePath = ReplaceFileName(currentSavePath, CheckpointSaveFileName);
			string checkpointPlayerNamesPath = ReplaceFileName(currentPlayerNamesPath, CheckpointPlayerNamesFileName);
			string checkpointMetadataPath = ReplaceFileName(currentSavePath, CheckpointMetadataFileName);

			CopyTextFile(saveStore, currentSavePath, checkpointSavePath);
			if (FileExists(saveStore, currentPlayerNamesPath))
			{
				CopyTextFile(saveStore, currentPlayerNamesPath, checkpointPlayerNamesPath);
			}

			string metadata = $$"""
				{
				  "kind": "hextech_pre_selection",
				  "act_index": {{actIndex}},
				  "run_current_act_index": {{runState.CurrentActIndex}},
				  "created_utc": "{{DateTimeOffset.UtcNow:O}}"
				}
				""";
			WriteFile(saveStore, checkpointMetadataPath, metadata);

			HextechLog.Info($"[{ModInfo.Id}][LanCheckpoint] Created Hextech pre-selection checkpoint: act={actIndex} save={checkpointSavePath}");
		}
		catch (Exception ex)
		{
			Log.Warn($"[{ModInfo.Id}][LanCheckpoint] Failed to create Hextech pre-selection checkpoint: {ex}");
		}
	}

	private static bool IsLanHost()
	{
		RunManager? runManager = RunManager.Instance;
		if (runManager == null || !runManager.IsInProgress)
		{
			return false;
		}

		INetGameService? netService = runManager.NetService;
		if (netService == null || netService.Type != NetGameType.Host || netService.Platform != PlatformType.None)
		{
			return false;
		}

		Type type = netService.GetType();
		string fullName = type.FullName ?? string.Empty;
		string assemblyName = type.Assembly.GetName().Name ?? string.Empty;
		return fullName.StartsWith("SlayTheSpire2.LAN.Multiplayer.", StringComparison.Ordinal)
			|| assemblyName.Contains("SlayTheSpire2.LAN.Multiplayer", StringComparison.Ordinal);
	}

	private static object GetSaveStore()
	{
		return Traverse.Create(SaveManager.Instance).Field("_saveStore").GetValue()
			?? throw new InvalidOperationException("SaveManager._saveStore is null.");
	}

	private static object GetLanSaveService()
	{
		Type serviceType = AccessTools.TypeByName("SlayTheSpire2.LAN.Multiplayer.Services.LanRunSaveManagerService")
			?? throw new InvalidOperationException("LAN LanRunSaveManagerService type was not found.");
		return AccessTools.Property(serviceType, "Instance")?.GetValue(null)
			?? throw new InvalidOperationException("LAN LanRunSaveManagerService.Instance is null.");
	}

	private static string GetStringProperty(object instance, string propertyName)
	{
		return AccessTools.Property(instance.GetType(), propertyName)?.GetValue(instance) as string
			?? throw new InvalidOperationException($"{instance.GetType().FullName}.{propertyName} is null.");
	}

	private static bool FileExists(object saveStore, string path)
	{
		return (bool)(AccessTools.Method(saveStore.GetType(), "FileExists")?.Invoke(saveStore, new object[] { path })
			?? throw new InvalidOperationException("ISaveStore.FileExists was not found."));
	}

	private static void CopyTextFile(object saveStore, string sourcePath, string destinationPath)
	{
		string? content = AccessTools.Method(saveStore.GetType(), "ReadFile")?.Invoke(saveStore, new object[] { sourcePath }) as string;
		if (content == null)
		{
			throw new InvalidOperationException($"Could not read {sourcePath}.");
		}

		WriteFile(saveStore, destinationPath, content);
	}

	private static void WriteFile(object saveStore, string path, string content)
	{
		AccessTools.Method(saveStore.GetType(), "WriteFile", new[] { typeof(string), typeof(string) })?.Invoke(saveStore, new object[] { path, content });
	}

	private static string ReplaceFileName(string path, string fileName)
	{
		string? directory = Path.GetDirectoryName(path);
		return string.IsNullOrEmpty(directory) ? fileName : Path.Combine(directory, fileName);
	}
}

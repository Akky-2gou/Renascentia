using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// キャラクターのスロットの表示・非表示を管理するクラス
/// </summary>
public class CharacterSlotManager : MonoBehaviour
{
	[SerializeField] private CharacterSlot characterSlotPrefab;
	[SerializeField] private GridLayoutGroup gridLayoutGroup;
	[SerializeField] private ShowCharacterDescription characterDescription;
	[SerializeField] private CharacterDetailDatabase characterDetailDatabase;
	[SerializeField] private int slotsPerRow = 4;
	[SerializeField] private bool refreshOnEnable = true;

	private readonly List<CharacterSlot> spawnedSlots = new List<CharacterSlot>();
	private GameProgress gameProgress;

	private void Awake()
	{
		if (gridLayoutGroup == null)
		{
			gridLayoutGroup = GetComponent<GridLayoutGroup>();
		}

		if (gridLayoutGroup == null)
		{
			gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>(true);
		}
	}

	private void OnEnable()
	{
		if (refreshOnEnable)
		{
			BuildUnlockedCharacterSlots();
		}
	}

	public void RefreshSlots()
	{
		BuildUnlockedCharacterSlots();
	}

	/// <summary>
	/// GameProgress.UnlockedCharacterIds を参照し、解放済みキャラの CharacterSlot prefab を
	/// 指定 GridLayoutGroup に、指定列数で改行しながら生成して並べる。
	/// </summary>
	public void BuildUnlockedCharacterSlots()
	{
		if (gameProgress == null)
		{
			gameProgress = GameProgress.Instance;
		}

		if (gameProgress == null)
		{
			Debug.LogWarning("GameProgress が見つからないため、キャラスロットを生成できません。");
			return;
		}

		if (gridLayoutGroup == null)
		{
			Debug.LogWarning("GridLayoutGroup が設定されていないため、キャラスロットを生成できません。");
			return;
		}

		if (characterSlotPrefab == null)
		{
			Debug.LogWarning("CharacterSlot prefab が設定されていないため、キャラスロットを生成できません。");
			return;
		}

		ClearSpawnedSlots();

		int columns = Mathf.Max(1, slotsPerRow);
		gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		gridLayoutGroup.constraintCount = columns;

		List<int> unlockedCharacterIds = new List<int>(gameProgress.UnlockedCharacterIds);
		unlockedCharacterIds.RemoveAll(id => id <= 0);
		unlockedCharacterIds.Sort();

		TeamConpositionManager teamConpositionManager = Object.FindFirstObjectByType<TeamConpositionManager>();

		foreach (int characterId in unlockedCharacterIds)
		{
			CharacterSlot slot = Instantiate(characterSlotPrefab, gridLayoutGroup.transform, false);
			slot.CharacterID = characterId;
			slot.TeamConpositionManager = teamConpositionManager;

			UnityEngine.UI.Button uiButton = slot.GetComponent<UnityEngine.UI.Button>();
			if (uiButton == null)
			{
				uiButton = slot.gameObject.AddComponent<UnityEngine.UI.Button>();
			}
			uiButton.onClick.RemoveAllListeners();
			uiButton.onClick.AddListener(() => slot.OnClick());

			ReadCharacterDetail readCharacterDetail = slot.GetComponent<ReadCharacterDetail>();
			if (readCharacterDetail == null)
			{
				readCharacterDetail = slot.gameObject.AddComponent<ReadCharacterDetail>();
			}

			readCharacterDetail.Initialize(characterId, characterDetailDatabase, characterDescription, gameProgress);
			spawnedSlots.Add(slot);
		}

		TeamMemberManager teamMemberManager = Object.FindFirstObjectByType<TeamMemberManager>();
		if (teamMemberManager != null)
		{
			teamMemberManager.SetCharacterSlots(spawnedSlots);
		}
	}

	private void ClearSpawnedSlots()
	{
		for (int i = spawnedSlots.Count - 1; i >= 0; i--)
		{
			CharacterSlot slot = spawnedSlots[i];
			if (slot != null)
			{
				Destroy(slot.gameObject);
			}
		}

		spawnedSlots.Clear();
	}
}

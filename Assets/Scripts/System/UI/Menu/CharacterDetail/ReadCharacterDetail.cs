using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

/*
 * キャラクターの詳細情報を呼び込むスクリプト
 */
public class ReadCharacterDetail : MonoBehaviour, IPointerClickHandler
{
	private const int InitialDisplayCharacterId = 1;

	[SerializeField] private int slotId = -1;
	[SerializeField] private bool _canShowDetail;
	[SerializeField] private CharacterDetailDatabase characterDetailDatabase;
	[SerializeField] private ShowCharacterDescription characterDescription;
	[SerializeField] private GameProgress gameProgress;

	public bool CanShowDetail => _canShowDetail;

	private void OnEnable()
	{
		RefreshCanShowDetail();

		// 初期状態ではID1のスロットだけが詳細を表示する
		if (slotId == InitialDisplayCharacterId)
		{
			DisplayCharacterDetail();
		}
	}

	private void Start()
	{
		RefreshCanShowDetail();
	}

	public void RefreshCanShowDetail()
	{
		if (gameProgress == null)
		{
			gameProgress = GameProgress.Instance;
		}

		if (gameProgress == null)
		{
			_canShowDetail = false;
			Debug.LogError("gameProgressがnull.");
			return;
		}

		if (slotId <= 0)
		{
			_canShowDetail = false;
			Debug.LogWarning($"slotId が無効です: {slotId}");
			return;
		}

		_canShowDetail = gameProgress.IsCharacterUnlocked(slotId);
		if (!_canShowDetail)
		{
			string unlocked = gameProgress.UnlockedCharacterIds.Count > 0
				? string.Join(", ", gameProgress.UnlockedCharacterIds)
				: "なし";
			Debug.LogWarning($"キャラクターID {slotId} はアンロックされていない. 現在の解放ID: [{unlocked}]");
			return;
		}

	}

	public void Initialize(int characterId, CharacterDetailDatabase database, ShowCharacterDescription description, GameProgress progress = null)
	{
		slotId = characterId;
		characterDetailDatabase = database;
		characterDescription = description;
		if (progress != null)
		{
			gameProgress = progress;
		}
		RefreshCanShowDetail();
	}

	public void DisplayCharacterDetail()
	{
		RefreshCanShowDetail();
		if (!_canShowDetail)
		{
			return;
		}

		if (characterDetailDatabase == null)
		{
			Debug.LogError("characterDetailDatabase が未設定です。");
			characterDescription?.Clear();
			return;
		}

		if (characterDescription == null)
		{
			Debug.LogError("characterDescription が未設定です。");
			return;
		}

		CharacterDetailRecord record = characterDetailDatabase.GetByCharacterId(slotId);
		if (record != null)
		{
			characterDescription.SetCharacterDetail(record);
		}
		else
		{
			Debug.LogWarning($"解放済みだが詳細データが見つかりません。slotId={slotId}");
			characterDescription.Clear();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		DisplayCharacterDetail();
	}
}

using UnityEngine;
using TMPro;

/*
 * キャラクターの詳細情報を呼び込むスクリプト
 */
public class ReadCharacterDetail : MonoBehaviour
{
	[SerializeField] private int slotId = -1;
	[SerializeField] private bool _canShowDetail;
	[SerializeField] private CharacterDetailDatabase characterDetailDatabase;
	[SerializeField] private ShowCharacterDescription characterDescription;
	[SerializeField] private GameProgress gameProgress;

	public bool CanShowDetail => _canShowDetail;

	private void OnEnable()
	{
		RefreshCanShowDetail();
		
		CharacterDetailRecord record = characterDetailDatabase.GetByCharacterId(1);
		if (record != null)
		{
			characterDescription.SetCharacterDetail(record);
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
			_canShowDetail = false;
			Debug.LogError("gameProgressがnull.");
			return;
		}

		_canShowDetail = gameProgress.UnlockedCharacterIds.Contains(slotId);
		if (!_canShowDetail)
		{
			Debug.LogWarning($"キャラクターID {slotId} はアンロックされていない.");
			return;
		}

		CharacterDetailRecord record = characterDetailDatabase.GetByCharacterId(slotId);
		if (record != null)
		{
			characterDescription.SetCharacterDetail(record);
		}
		else
		{
			characterDescription.Clear();
		}
	}

	public void DisplayCharacterDetail()
	{
		CharacterDetailRecord record = characterDetailDatabase.GetByCharacterId(slotId);
		if (record != null)
		{
			characterDescription.SetCharacterDetail(record);
		}
		else
		{
			characterDescription.Clear();
		}
	}
}

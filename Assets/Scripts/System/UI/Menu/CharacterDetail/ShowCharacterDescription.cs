using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
 * キャラクター詳細の表示内容を受け取ってUIへ反映するスクリプト
 */
public class ShowCharacterDescription : MonoBehaviour
{
	[SerializeField] private Image _backCharaImage;
	[SerializeField] private TextMeshProUGUI _characterNameText;
	[SerializeField] private TextMeshProUGUI _speciesText;
	[SerializeField] private TextMeshProUGUI _hpText;
	[SerializeField] private TextMeshProUGUI _attackText;
	[SerializeField] private TextMeshProUGUI _defenceText;
	[SerializeField] private TextMeshProUGUI _normalSkillText;
	[SerializeField] private TextMeshProUGUI _specialSkillText;
	[SerializeField] private TextMeshProUGUI _specialSkill2Text;
	[SerializeField] private TextMeshProUGUI _specialSkill3Text;
	[SerializeField] private TextMeshProUGUI _passiveText;
	[SerializeField] private TextMeshProUGUI _bonusPassiveText;

	public void SetCharacterDetail(CharacterDetailRecord record)
	{
		if (record == null)
		{
			Clear();
			return;
		}

		if (_backCharaImage != null)
		{
			_backCharaImage.sprite = record.detailBackgroundIcon;
			if (_backCharaImage.rectTransform != null)
			{
				_backCharaImage.rectTransform.anchoredPosition = record.detailBackgroundIconOffset;
				_backCharaImage.rectTransform.localScale = Vector3.one * record.detailBackgroundIconScale;
			}
		}

		SetText(_characterNameText, $"キャラ名: {record.characterName}");
		SetText(_speciesText, $"属性: {record.species}");
		SetText(_hpText, $"HP: {record.hp}");
		SetText(_attackText, $"攻撃: {record.attack}");
		SetText(_defenceText, $"防御: {record.defence}");
		SetText(_normalSkillText, $"通常: {record.normalElement} / {record.normalRange} / {record.normalDamage}");
		SetText(_specialSkillText, $"S1: {record.skillElement} / {record.skillRange} / {record.skillDamage}\n{record.skillDescription}");
		SetText(_specialSkill2Text, $"S2: {record.skill2Damage}\n{record.skill2Description}");
		SetText(_specialSkill3Text, $"S3: {record.skill3Damage}\n{record.skill3Description}");
		SetText(_passiveText, $"パッシブ: {record.passive}");
		SetText(_bonusPassiveText, $"ボーナスパッシブ: {record.bonusPassive}");
	}

	public void Clear()
	{
		if (_backCharaImage != null)
		{
			_backCharaImage.sprite = null;
			if (_backCharaImage.rectTransform != null)
			{
				_backCharaImage.rectTransform.anchoredPosition = Vector2.zero;
				_backCharaImage.rectTransform.localScale = Vector3.one;
			}
		}

		SetText(_characterNameText, string.Empty);
		SetText(_speciesText, string.Empty);
		SetText(_hpText, string.Empty);
		SetText(_attackText, string.Empty);
		SetText(_defenceText, string.Empty);
		SetText(_normalSkillText, string.Empty);
		SetText(_specialSkillText, string.Empty);
		SetText(_specialSkill2Text, string.Empty);
		SetText(_specialSkill3Text, string.Empty);
		SetText(_passiveText, string.Empty);
		SetText(_bonusPassiveText, string.Empty);
	}

	private void SetText(TextMeshProUGUI target, string value)
	{
		if (target != null)
		{
			target.text = value;
		}
	}
}

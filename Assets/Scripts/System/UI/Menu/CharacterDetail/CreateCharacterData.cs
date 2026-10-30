using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * CharacterDetail.csv から CharacterDetailDatabase を生成するスクリプト
 */

[CreateAssetMenu(fileName = "CharacterDetailDatabase", menuName = "Renascentia/Character Detail Database")]
public class CharacterDetailDatabase : ScriptableObject
{
    [SerializeField] private List<CharacterDetailRecord> _records = new List<CharacterDetailRecord>();

    public IReadOnlyList<CharacterDetailRecord> Records => _records;

    public CharacterDetailRecord GetByCharacterId(int characterId)
    {
        return _records.Find(record => record.characterId == characterId);
    }
}

[System.Serializable]
public class CharacterDetailRecord
{
    public int characterId;
    public string characterName;
    public string species;
    public string hp;
    public string attack;
    public string defence;
    public string normalElement;
    public string normalRange;
    public string normalDamage;
    public string skillElement;
    public string skillRange;
    public string skillDamage;
    public string skillDescription;
    public string skill2Damage;
    public string skill2Description;
    public string skill3Damage;
    public string skill3Description;
    public string passive;
    public string bonusPassive;

    // 後から Inspector で差し込む背景アイコンと位置補正
    public Sprite detailBackgroundIcon;
    public Vector2 detailBackgroundIconOffset;
    public float detailBackgroundIconScale = 1f;

    // 後から Inspector で差し込む、該当キャラクターの Prefab
    public GameObject characterPrefab;
}

#if UNITY_EDITOR
public class CreateCharacterData : AssetPostprocessor
{
    private const string CSV_FILE_NAME = "CharacterDetail.csv";

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string filePath in importedAssets)
        {
            if (!filePath.EndsWith(CSV_FILE_NAME))
            {
                continue;
            }

            TextAsset csvTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (csvTextAsset == null)
            {
                Debug.LogWarning($"CharacterDetail.csv が読み込めませんでした: {filePath}");
                continue;
            }

            string assetFilePath = filePath.Replace(".csv", ".asset");
            CharacterDetailDatabase database = AssetDatabase.LoadAssetAtPath<CharacterDetailDatabase>(assetFilePath);

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CharacterDetailDatabase>();
                AssetDatabase.CreateAsset(database, assetFilePath);
            }

            SerializedObject serializedObject = new SerializedObject(database);
            SerializedProperty recordsProperty = serializedObject.FindProperty("_records");
            recordsProperty.ClearArray();

            List<CharacterDetailRecord> records = ParseCharacterDetailCsv(csvTextAsset.text);
            for (int i = 0; i < records.Count; i++)
            {
                recordsProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = recordsProperty.GetArrayElementAtIndex(i);
                SetRecordValue(element.FindPropertyRelative("characterId"), records[i].characterId);
                SetRecordValue(element.FindPropertyRelative("characterName"), records[i].characterName);
                SetRecordValue(element.FindPropertyRelative("species"), records[i].species);
                SetRecordValue(element.FindPropertyRelative("hp"), records[i].hp);
                SetRecordValue(element.FindPropertyRelative("attack"), records[i].attack);
                SetRecordValue(element.FindPropertyRelative("defence"), records[i].defence);
                SetRecordValue(element.FindPropertyRelative("normalElement"), records[i].normalElement);
                SetRecordValue(element.FindPropertyRelative("normalRange"), records[i].normalRange);
                SetRecordValue(element.FindPropertyRelative("normalDamage"), records[i].normalDamage);
                SetRecordValue(element.FindPropertyRelative("skillElement"), records[i].skillElement);
                SetRecordValue(element.FindPropertyRelative("skillRange"), records[i].skillRange);
                SetRecordValue(element.FindPropertyRelative("skillDamage"), records[i].skillDamage);
                SetRecordValue(element.FindPropertyRelative("skillDescription"), records[i].skillDescription);
                SetRecordValue(element.FindPropertyRelative("skill2Damage"), records[i].skill2Damage);
                SetRecordValue(element.FindPropertyRelative("skill2Description"), records[i].skill2Description);
                SetRecordValue(element.FindPropertyRelative("skill3Damage"), records[i].skill3Damage);
                SetRecordValue(element.FindPropertyRelative("skill3Description"), records[i].skill3Description);
                SetRecordValue(element.FindPropertyRelative("passive"), records[i].passive);
                SetRecordValue(element.FindPropertyRelative("bonusPassive"), records[i].bonusPassive);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }

    private static List<CharacterDetailRecord> ParseCharacterDetailCsv(string csvText)
    {
        List<CharacterDetailRecord> records = new List<CharacterDetailRecord>();
        string[] lines = csvText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] columns = line.Split(',');
            if (columns.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(GetColumn(columns, 0), out int characterId))
            {
                Debug.LogWarning($"CharacterDetail.csv の {i + 1} 行目でキャラクターIDを読み取れませんでした。");
                continue;
            }

            records.Add(new CharacterDetailRecord
            {
                characterId = characterId,
                characterName = GetColumn(columns, 1),
                species = GetColumn(columns, 2),
                hp = GetColumn(columns, 3),
                attack = GetColumn(columns, 4),
                defence = GetColumn(columns, 5),
                normalElement = GetColumn(columns, 6),
                normalRange = GetColumn(columns, 7),
                normalDamage = GetColumn(columns, 8),
                skillElement = GetColumn(columns, 9),
                skillRange = GetColumn(columns, 10),
                skillDamage = GetColumn(columns, 11),
                skillDescription = GetColumn(columns, 12),
                skill2Damage = GetColumn(columns, 13),
                skill2Description = GetColumn(columns, 14),
                skill3Damage = GetColumn(columns, 15),
                skill3Description = GetColumn(columns, 16),
                passive = GetColumn(columns, 17),
                bonusPassive = GetColumn(columns, 18)
            });
        }

        return records;
    }

    private static string GetColumn(string[] columns, int index)
    {
        if (index < 0 || index >= columns.Length)
        {
            return string.Empty;
        }

        return columns[index].Trim().Trim('"');
    }

    private static void SetRecordValue(SerializedProperty property, int value)
    {
        property.intValue = value;
    }

    private static void SetRecordValue(SerializedProperty property, string value)
    {
        property.stringValue = value;
    }
}
#endif

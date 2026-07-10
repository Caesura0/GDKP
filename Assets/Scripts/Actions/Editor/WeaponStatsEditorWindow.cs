#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class WeaponStatsEditorWindow : EditorWindow
{
    private BaseAttackAction selectedAction;
    private GUIStyle headerStyle;
    private GUIStyle rowStyle;

    [MenuItem("Window/Weapon Stats Preview")]
    public static void ShowWindow()
    {
        GetWindow<WeaponStatsEditorWindow>("Weapon Stats");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeGameObject != null)
        {
            selectedAction = Selection.activeGameObject.GetComponent<BaseAttackAction>();
        }
        else
        {
            selectedAction = null;
        }
        Repaint();
    }

    private void OnInspectorUpdate()
    {
        // This ensures the window repaints when the user types in the inspector
        Repaint();
    }

    private void OnGUI()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            rowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
        }

        GUILayout.Space(10);
        GUILayout.Label("Weapon Stats Preview", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(10);
        
        if (selectedAction == null)
        {
            EditorGUILayout.HelpBox("Select a GameObject with a BaseAttackAction (e.g. ShootAction, ShotgunAction, SwordAction) in the Hierarchy to view its stats.", MessageType.Info);
            return;
        }

        SerializedObject so = new SerializedObject(selectedAction);
        so.Update();

        // Read all relevant properties from BaseAttackAction using SerializedObject
        // This avoids needing an active scene or running game to preview stats
        int maxAttackDistance = so.FindProperty("maxAttackDistance").intValue;
        int minShootDistance = so.FindProperty("minShootDistance").intValue;
        int damage = so.FindProperty("damage").intValue;
        bool scalesDamageWithDistance = so.FindProperty("scalesDamageWithDistance").boolValue;
        int maxCloseRangeBonus = so.FindProperty("maxCloseRangeBonus").intValue;
        float weaponAccuracy = so.FindProperty("weaponAccuracy").floatValue;
        float distancePenaltyWeight = so.FindProperty("distancePenaltyWeight").floatValue;

        GUILayout.Label($"Selected: {selectedAction.gameObject.name} ({selectedAction.GetType().Name})", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Dist", headerStyle, GUILayout.Width(40));
        GUILayout.Label("Hit% (No Cover)", headerStyle, GUILayout.Width(110));
        GUILayout.Label("Hit% (Half Cover)", headerStyle, GUILayout.Width(110));
        GUILayout.Label("Hit% (Full Cover)", headerStyle, GUILayout.Width(110));
        GUILayout.Label("Damage", headerStyle, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        for (int distance = minShootDistance; distance <= maxAttackDistance; distance++)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(distance.ToString(), rowStyle, GUILayout.Width(40));

            // Math matches BaseAttackAction.CalculateAccuracy
            float accuracyReduction = (float)distance / maxAttackDistance;
            float baseHitChance = weaponAccuracy - accuracyReduction * distancePenaltyWeight;

            // Apply cover multipliers matching LevelGrid.GetCoverAccuracyReduction
            float hitNone = Mathf.Clamp(baseHitChance * 1.00f, 0.01f, 1f); // 0% reduction
            float hitHalf = Mathf.Clamp(baseHitChance * 0.90f, 0.01f, 1f); // 10% reduction
            float hitFull = Mathf.Clamp(baseHitChance * 0.75f, 0.01f, 1f); // 25% reduction

            GUI.color = GetAccuracyColor(hitNone);
            GUILayout.Label(Mathf.RoundToInt(hitNone * 100) + "%", rowStyle, GUILayout.Width(110));
            GUI.color = GetAccuracyColor(hitHalf);
            GUILayout.Label(Mathf.RoundToInt(hitHalf * 100) + "%", rowStyle, GUILayout.Width(110));
            GUI.color = GetAccuracyColor(hitFull);
            GUILayout.Label(Mathf.RoundToInt(hitFull * 100) + "%", rowStyle, GUILayout.Width(110));
            GUI.color = Color.white;

            // Math matches BaseAttackAction.CalculateDamage
            int finalDamage = damage;
            if (scalesDamageWithDistance)
            {
                float distanceRange = maxAttackDistance - 1f;
                if (distanceRange <= 0)
                {
                    finalDamage = damage + maxCloseRangeBonus;
                }
                else
                {
                    float distanceProgress = Mathf.Clamp01((distance - 1f) / distanceRange);
                    int bonusDamage = Mathf.RoundToInt(Mathf.Lerp(maxCloseRangeBonus, 0, distanceProgress));
                    finalDamage = damage + bonusDamage;
                }
            }
            GUILayout.Label(finalDamage.ToString(), rowStyle, GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(15);
        EditorGUILayout.HelpBox("These stats are previewed based on the math in BaseAttackAction. They update automatically in real time as you adjust values in the Inspector.", MessageType.None);
    }

    private Color GetAccuracyColor(float chance)
    {
        if (chance >= 0.75f) return new Color(0.4f, 1f, 0.4f); // Green
        if (chance >= 0.45f) return new Color(1f, 0.9f, 0.4f); // Yellow
        if (chance > 0.01f) return new Color(1f, 0.6f, 0.4f); // Orange
        return new Color(1f, 0.4f, 0.4f); // Red
    }
}
#endif

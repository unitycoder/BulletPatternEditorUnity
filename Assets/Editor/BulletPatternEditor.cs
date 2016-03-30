// This vicious beast handles the UI for BulletPattern. This is what happens when you design a large and intricate GUI with code
//  - do not try this at home.
// 
// This file consists of 4 parts, the FireTag stuff, FireAction, BulletTag, and BulletTag
// There is much code repeated throughout the four parts. Unfortunately I dont think creating functions to 
// wrap said code would noticeably increase navigation through this jungle. It is however contained in 4 main functions - 
// FireTagGUI();
// FireTagActionsGUI();
// BulletTagsGUI();
// BulletTagActionsGUI();
//
// Enter at your own risk, and if you need to add or edit something to the GUI may mercy be with you
//


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BulletPattern))]
[CanEditMultipleObjects]
public class BulletPatternEditor : Editor
{
    [SerializeField]
    BulletPattern bp;
    [SerializeField]
    BulletManager bm;

    void OnEnable()
    {
        bp = target as BulletPattern;
        bm = GameObject.Find("BulletManager").GetComponent<BulletManager>();

    }

    public override void OnInspectorGUI()
    {
        FireTagsGUI();
        BulletTagsGUI();

        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 160;
        EditorGUIUtility.fieldWidth = 130;

        bp.waitBeforeRepeating = EditorGUILayout.FloatField("WaitBeforeRepeat", bp.waitBeforeRepeating);
        bm.rank = EditorGUILayout.Slider("Rank", bm.rank, 0, 1);

        // temporary fix for losing prefab inspector fields when entering play
        if (GUI.changed)
        {
            EditorUtility.SetDirty(bp);
            serializedObject.ApplyModifiedProperties();
        }
    }

    void FireTagsGUI()
    {
        if (bp.fireTags == null)
            bp.fireTags = new FireTag[0];

        List<FireTag> fireTags = new List<FireTag>(bp.fireTags);

        if (fireTags.Count != bp.ftFoldouts.Count)
            bp.ftFoldouts = new List<bool>(new bool[fireTags.Count]);

        if (fireTags.Count != bp.ftaFoldouts.Count)
        {
            bp.ftaFoldouts = new List<ActionFoldouts>(new ActionFoldouts[fireTags.Count]);
            if (bp.ftaFoldouts.Count > 0)
            {
                for (var zz = 0; zz < bp.ftaFoldouts.Count; zz++)
                {
                    bp.ftaFoldouts[zz] = new ActionFoldouts();
                }
            }
        }

        GUILayout.BeginHorizontal();
        bp.ftFoldout = EditorGUILayout.Foldout(bp.ftFoldout, "FireTags");
        if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
        {
            bp.ftFoldout = !bp.ftFoldout;

            for (int zz = 0; zz < bp.ftFoldouts.Count; zz++)
                bp.ftFoldouts[zz] = bp.ftFoldout;
        }
        GUILayout.EndHorizontal();

        if (bp.ftFoldout)
        {
            EditorGUI.indentLevel++;
            var removeIndex = -1;
            var moveIndex = -1;

            for (var l = 0; l < fireTags.Count; l++)
            {
                EditorGUIUtility.labelWidth = 160;
                EditorGUIUtility.fieldWidth = 120;

                GUILayout.BeginHorizontal();
                var str = "FireTag " + (l + 1);
                bp.ftFoldouts[l] = EditorGUILayout.Foldout(bp.ftFoldouts[l], str);

                if (GUILayout.Button("Down", GUILayout.Width(50)))
                    moveIndex = l;

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    removeIndex = l;
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                if (bp.ftFoldouts[l])
                {
                    GUI.changed = false;

                    EditorGUI.indentLevel++;

                    if (GUI.changed)
                        SceneView.RepaintAll();

                    FireTagActionsGUI(l);

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }

            // if the "down" button was pressed then we move that array index down one time, MAGIC
            if (moveIndex >= 0 && moveIndex != fireTags.Count - 1)
            {
                var temp = fireTags[moveIndex];
                fireTags[moveIndex] = fireTags[moveIndex + 1];
                fireTags[moveIndex + 1] = temp;

                var temp2 = bp.ftFoldouts[moveIndex];
                bp.ftFoldouts[moveIndex] = bp.ftFoldouts[moveIndex + 1];
                bp.ftFoldouts[moveIndex + 1] = temp2;

                var temp3 = bp.ftaFoldouts[moveIndex];
                bp.ftaFoldouts[moveIndex] = bp.ftaFoldouts[moveIndex + 1];
                bp.ftaFoldouts[moveIndex + 1] = temp3;
            }
            // hmm what could remove do
            if (removeIndex >= 0)
            {
                fireTags.RemoveAt(removeIndex);
                bp.ftFoldouts.RemoveAt(removeIndex);
                bp.ftaFoldouts.RemoveAt(removeIndex);
            }

            //add a space to the GUI, adding a number in those paranthesis(brackets to you brits) will increase the space size
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            if (GUILayout.Button("Add Fire Tag", GUILayout.Width(100)))
            {
                var ft = new FireTag();
                ft.actions = new FireAction[1];

                fireTags.Add(ft);

                bp.ftFoldouts.Add(true);
                bp.ftaFoldouts.Add(new ActionFoldouts());

            }
            GUILayout.EndHorizontal();

            bp.fireTags = fireTags.ToArray();

            EditorGUI.indentLevel--;

        }

        EditorGUILayout.Space();
    }

    //start the FireActions stuff. Its actually even longer and uglier then the previous function
    void FireTagActionsGUI(int i)
    {

        if (bp.fireTags[i].actions.Length == 0)
        {
            bp.fireTags[i].actions = new FireAction[1];
        }
        
        var actions = new List<FireAction>(bp.fireTags[i].actions);

        if (actions.Count != bp.ftaFoldouts[i].sub.Count)
            bp.ftaFoldouts[i].sub = new List<bool>(new bool[actions.Count]);

        GUILayout.BeginHorizontal();
        bp.ftaFoldouts[i].main = EditorGUILayout.Foldout(bp.ftaFoldouts[i].main, "Actions");
        if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
        {
            bp.ftaFoldouts[i].main = !bp.ftaFoldouts[i].main;

            for (var zz = 0; zz < bp.ftaFoldouts[i].sub.Count; zz++)
                bp.ftaFoldouts[i].sub[zz] = bp.ftaFoldouts[i].main;
        }
        GUILayout.EndHorizontal();

        if (bp.ftaFoldouts[i].main)
        {
            EditorGUI.indentLevel++;
            var removeIndex = -1;
            var moveIndex = -1;

            for (int l = 0; l < actions.Count; l++)
            {
                EditorGUIUtility.labelWidth = 160;
                EditorGUIUtility.fieldWidth = 125;

                GUILayout.BeginHorizontal();
                var str = "Action " + (l + 1);
                bp.ftaFoldouts[i].sub[l] = EditorGUILayout.Foldout(bp.ftaFoldouts[i].sub[l], str);

                if (GUILayout.Button("Down", GUILayout.Width(50)))
                    moveIndex = l;
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    removeIndex = l;
                GUILayout.EndHorizontal();

                if (bp.ftaFoldouts[i].sub[l])
                {
                    GUI.changed = false;

                    EditorGUI.indentLevel++;

                    var ac = actions[l];

                    ac.type = (FireActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);

                    //an extremely ugly block of GUI code
                    switch (ac.type)
                    {
                        case (FireActionType.Wait):
                            GUILayout.BeginHorizontal();
                            if (!ac.randomWait)
                                ac.waitTime.x = EditorGUILayout.FloatField("Wait Time", ac.waitTime.x);
                            else
                                ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
                            ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            ac.rankWait = EditorGUILayout.Toggle("Add Rank", ac.rankWait);
                            if (ac.rankWait)
                                ac.waitTime.z = EditorGUILayout.FloatField("RankWaitTime", ac.waitTime.z);
                            GUILayout.EndHorizontal();
                            break;

                        case (FireActionType.Fire):
                            ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
                            if (!ac.useParam)
                            {
                                GUILayout.BeginHorizontal();
                                if (!ac.randomAngle)
                                    ac.angle.x = EditorGUILayout.IntField("Angle", (int)ac.angle.x);
                                else
                                    ac.angle = EditorGUILayout.Vector2Field("Angle Range", ac.angle);
                                ac.randomAngle = EditorGUILayout.Toggle("Randomize", ac.randomAngle);
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                ac.rankAngle = EditorGUILayout.Toggle("Add Rank", ac.rankAngle);
                                if (ac.rankAngle)
                                    ac.angle.z = EditorGUILayout.FloatField("RankAngle", ac.angle.z);
                                GUILayout.EndHorizontal();
                            }
                            ac.useParam = EditorGUILayout.Toggle("Use Param", ac.useParam);
                            EditorGUILayout.Space();

                            ac.overwriteBulletSpeed = EditorGUILayout.Toggle("OverwriteSpd", ac.overwriteBulletSpeed);
                            if (ac.overwriteBulletSpeed)
                            {
                                GUILayout.BeginHorizontal();
                                if (!ac.randomSpeed)
                                    ac.speed.x = EditorGUILayout.FloatField("New Speed", ac.speed.x);
                                else
                                    ac.speed = EditorGUILayout.Vector2Field("Speed Range", ac.speed);
                                ac.randomSpeed = EditorGUILayout.Toggle("Randomize", ac.randomSpeed);
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                ac.rankSpeed = EditorGUILayout.Toggle("Add Rank", ac.rankSpeed);
                                if (ac.rankSpeed)
                                    ac.speed.z = EditorGUILayout.FloatField("RankSpeed", ac.speed.z);
                                GUILayout.EndHorizontal();
                                ac.useSequenceSpeed = EditorGUILayout.Toggle("UseSequence", ac.useSequenceSpeed);
                            }

                            EditorGUILayout.Space();
                            GUILayout.BeginHorizontal();
                            ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
                            if (!ac.passParam)
                                ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
                            GUILayout.EndHorizontal();
                            if (ac.passParam)
                                ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
                            ac.bulletTagIndex = EditorGUILayout.IntSlider("BulletTag Index", ac.bulletTagIndex, 1, bp.bulletTags.Length);
                            break;

                        case (FireActionType.CallFireTag):
                            ac.fireTagIndex = EditorGUILayout.IntSlider("Fire Tag Idx", ac.fireTagIndex, 1, bp.fireTags.Length);
                            GUILayout.BeginHorizontal();
                            ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
                            if (!ac.passParam)
                                ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
                            GUILayout.EndHorizontal();
                            if (ac.passParam)
                                ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
                            break;

                        case (FireActionType.StartRepeat):
                            ac.repeatCount.x = EditorGUILayout.IntField("RepeatCount", (int)ac.repeatCount.x);
                            GUILayout.BeginHorizontal();
                            ac.rankRepeat = EditorGUILayout.Toggle("AddRank", ac.rankRepeat);
                            if (ac.rankRepeat)
                                ac.repeatCount.y = EditorGUILayout.FloatField("RankRepeat", ac.repeatCount.y);
                            GUILayout.EndHorizontal();
                            break;
                    }
                    EditorGUI.indentLevel--;
                    if (GUI.changed)
                        SceneView.RepaintAll();
                }
            }

            if (moveIndex >= 0 && moveIndex != actions.Count - 1)
            {
                var temp = actions[moveIndex];
                actions[moveIndex] = actions[moveIndex + 1];
                actions[moveIndex + 1] = temp;

                var temp2 = bp.ftaFoldouts[i].sub[moveIndex];
                bp.ftaFoldouts[i].sub[moveIndex] = bp.ftaFoldouts[i].sub[moveIndex + 1];
                bp.ftaFoldouts[i].sub[moveIndex + 1] = temp2;
            }
            // Ive seen this before somewhere
            if (removeIndex >= 0)
            {
                actions.RemoveAt(removeIndex);
                bp.ftaFoldouts[i].sub.RemoveAt(removeIndex);
            }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            if (GUILayout.Button("Add Action", GUILayout.Width(100)))
            {
                var ac = new FireAction();
                actions.Add(ac);
                bp.ftaFoldouts[i].sub.Add(true);
            }
            GUILayout.EndHorizontal();

            bp.fireTags[i].actions = actions.ToArray();
            EditorGUI.indentLevel--;
        }
    }

    //BulletTag stuff, somewhat shorter than the last one
    void BulletTagsGUI()
    {
        if (bp.bulletTags == null)
            bp.bulletTags = new BulletTag[0];

        var bulletTags = new List<BulletTag>(bp.bulletTags);

        if (bulletTags.Count != bp.btFoldouts.Count)
            bp.btFoldouts = new List<bool>(new bool[bulletTags.Count]);

        if (bulletTags.Count != bp.btaFoldouts.Count)
        {
            bp.btaFoldouts = new List<ActionFoldouts>(new ActionFoldouts[bulletTags.Count]);
            if (bp.btaFoldouts.Count > 0)
            {
                for (var zz = 0; zz < bp.btaFoldouts.Count; zz++)
                {
                    bp.btaFoldouts[zz] = new ActionFoldouts();
                }
            }
        }

        GUILayout.BeginHorizontal();
        bp.btFoldout = EditorGUILayout.Foldout(bp.btFoldout, "BulletTags");
        if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
        {
            bp.btFoldout = !bp.btFoldout;

            for (int zz = 0; zz < bp.btFoldouts.Count; zz++)
                bp.btFoldouts[zz] = bp.btFoldout;
        }
        GUILayout.EndHorizontal();

        if (bp.btFoldout)
        {
            EditorGUI.indentLevel++;
            var removeIndex = -1;
            var moveIndex = -1;

            for (var l = 0; l < bulletTags.Count; l++)
            {
                EditorGUIUtility.labelWidth = 140;
                EditorGUIUtility.fieldWidth = 100;

                GUILayout.BeginHorizontal();
                var str = "BulletTag " + (l + 1);
                bp.btFoldouts[l] = EditorGUILayout.Foldout(bp.btFoldouts[l], str);

                if (GUILayout.Button("Down", GUILayout.Width(50)))
                    moveIndex = l;

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    removeIndex = l;
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                if (bp.btFoldouts[l])
                {
                    GUI.changed = false;

                    EditorGUI.indentLevel++;

                    var bt = bulletTags[l];

                    GUILayout.BeginHorizontal();
                    if (!bt.randomSpeed)
                        bt.speed.x = EditorGUILayout.FloatField("Speed", bt.speed.x);
                    else
                        bt.speed = EditorGUILayout.Vector2Field("Speed Range", bt.speed);
                    bt.randomSpeed = EditorGUILayout.Toggle("Randomize", bt.randomSpeed);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    bt.rankSpeed = EditorGUILayout.Toggle("Add Rank", bt.rankSpeed);
                    if (bt.rankSpeed)
                        bt.speed.z = EditorGUILayout.FloatField("RankSpeed", bt.speed.z);
                    GUILayout.EndHorizontal();
                    bt.prefabIndex = EditorGUILayout.IntSlider("PrefabIndex", bt.prefabIndex, 0, bm.bulletPrefab.Length - 1);

                    if (GUI.changed)
                        SceneView.RepaintAll();

                    BulletTagActionsGUI(l);

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }

            if (moveIndex >= 0 && moveIndex != bulletTags.Count - 1)
            {
                var temp = bulletTags[moveIndex];
                bulletTags[moveIndex] = bulletTags[moveIndex + 1];
                bulletTags[moveIndex + 1] = temp;

                var temp2 = bp.btFoldouts[moveIndex];
                bp.btFoldouts[moveIndex] = bp.btFoldouts[moveIndex + 1];
                bp.btFoldouts[moveIndex + 1] = temp2;

                var temp3 = bp.btaFoldouts[moveIndex];
                bp.btaFoldouts[moveIndex] = bp.btaFoldouts[moveIndex + 1];
                bp.btaFoldouts[moveIndex + 1] = temp3;
            }

            if (removeIndex >= 0)
            {
                bulletTags.RemoveAt(removeIndex);
                bp.btFoldouts.RemoveAt(removeIndex);
                bp.btaFoldouts.RemoveAt(removeIndex);
            }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            if (GUILayout.Button("Add Bullet Tag", GUILayout.Width(100)))
            {
                var bt = new BulletTag();
                bulletTags.Add(bt);
                bp.btFoldouts.Add(true);
                bp.btaFoldouts.Add(new ActionFoldouts());

            }
            GUILayout.EndHorizontal();

            bp.bulletTags = bulletTags.ToArray();

        }

        EditorGUILayout.Space();
    }

    //obligatory comment that makes this function stand out so you can find it better
    void BulletTagActionsGUI(int i)
    {
        if (bp.bulletTags[i].actions == null)
            bp.bulletTags[i].actions = new BulletAction[0];

        var actions = new List<BulletAction>(bp.bulletTags[i].actions);

        if (actions.Count != bp.btaFoldouts[i].sub.Count)
            bp.btaFoldouts[i].sub = new List<bool>(new bool[actions.Count]);

        GUILayout.BeginHorizontal();
        bp.btaFoldouts[i].main = EditorGUILayout.Foldout(bp.btaFoldouts[i].main, "Actions");
        if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
        {
            bp.btaFoldouts[i].main = !bp.btaFoldouts[i].main;

            for (var zz = 0; zz < bp.btaFoldouts[i].sub.Count; zz++)
                bp.btaFoldouts[i].sub[zz] = bp.btaFoldouts[i].main;
        }
        GUILayout.EndHorizontal();

        if (bp.btaFoldouts[i].main)
        {
            EditorGUI.indentLevel++;
            var removeIndex = -1;
            var moveIndex = -1;

            for (var l = 0; l < actions.Count; l++)
            {
                EditorGUIUtility.labelWidth = 140;
                EditorGUIUtility.fieldWidth = 100;

                GUILayout.BeginHorizontal();
                var str = "Action " + (l + 1);
                bp.btaFoldouts[i].sub[l] = EditorGUILayout.Foldout(bp.btaFoldouts[i].sub[l], str);

                if (GUILayout.Button("Down", GUILayout.Width(50)))
                    moveIndex = l;
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    removeIndex = l;
                GUILayout.EndHorizontal();

                if (bp.btaFoldouts[i].sub[l])
                {
                    GUI.changed = false;

                    EditorGUI.indentLevel++;

                    var ac = actions[l];

                    ac.type = (BulletActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);

                    switch (ac.type)
                    {
                        case (BulletActionType.Wait):
                            GUILayout.BeginHorizontal();
                            if (!ac.randomWait)
                                ac.waitTime.x = EditorGUILayout.FloatField("Wait Time", ac.waitTime.x);
                            else
                                ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
                            ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            ac.rankWait = EditorGUILayout.Toggle("AddRank", ac.rankWait);
                            if (ac.rankWait)
                                ac.waitTime.z = EditorGUILayout.FloatField("RankTime", ac.waitTime.z);
                            GUILayout.EndHorizontal();
                            break;

                        case (BulletActionType.ChangeDirection):
                            ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
                            GUILayout.BeginHorizontal();
                            if (!ac.randomAngle)
                                ac.angle.x = EditorGUILayout.IntField("Angle", (int)ac.angle.x);
                            else
                                ac.angle = EditorGUILayout.Vector2Field("Angle Range", ac.angle);
                            ac.randomAngle = EditorGUILayout.Toggle("Randomize", ac.randomAngle);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            ac.rankAngle = EditorGUILayout.Toggle("Add Rank", ac.rankAngle);
                            if (ac.rankAngle)
                                ac.angle.z = EditorGUILayout.FloatField("RankAngle", ac.angle.z);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            if (!ac.randomWait)
                                ac.waitTime.x = EditorGUILayout.FloatField("Time", ac.waitTime.x);
                            else
                                ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
                            ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            ac.rankWait = EditorGUILayout.Toggle("AddRank", ac.rankWait);
                            if (ac.rankWait)
                                ac.waitTime.z = EditorGUILayout.FloatField("RankTime", ac.waitTime.z);
                            GUILayout.EndHorizontal();

                            ac.waitForChange = EditorGUILayout.Toggle("WaitToFinish", ac.waitForChange);
                            break;

                        case (BulletActionType.ChangeSpeed):
                        case (BulletActionType.VerticalChangeSpeed):
                            GUILayout.BeginHorizontal();
                            if (!ac.randomSpeed)
                                ac.speed.x = EditorGUILayout.FloatField("New Speed", ac.speed.x);
                            else
                                ac.speed = EditorGUILayout.Vector2Field("Speed Range", ac.speed);
                            ac.randomSpeed = EditorGUILayout.Toggle("Randomize", ac.randomSpeed);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            ac.rankSpeed = EditorGUILayout.Toggle("Add Rank", ac.rankSpeed);
                            if (ac.rankSpeed)
                                ac.speed.z = EditorGUILayout.FloatField("RankSpeed", ac.speed.z);
                            GUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            GUILayout.BeginHorizontal();
                            if (!ac.randomWait)
                                ac.waitTime.x = EditorGUILayout.FloatField("Time", ac.waitTime.x);
                            else
                                ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
                            ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            ac.rankWait = EditorGUILayout.Toggle("Add Rank", ac.rankWait);
                            if (ac.rankWait)
                                ac.waitTime.z = EditorGUILayout.FloatField("RankTime", ac.waitTime.z);
                            GUILayout.EndHorizontal();

                            ac.waitForChange = EditorGUILayout.Toggle("WaitToFinish", ac.waitForChange);
                            break;

                        case (BulletActionType.StartRepeat):
                            ac.repeatCount.x = EditorGUILayout.IntField("Repeat Count", (int)ac.repeatCount.x);
                            GUILayout.BeginHorizontal();
                            ac.rankRepeat = EditorGUILayout.Toggle("AddRank", ac.rankRepeat);
                            if (ac.rankRepeat)
                                ac.repeatCount.y = EditorGUILayout.FloatField("RepeatRank", ac.repeatCount.y);
                            GUILayout.EndHorizontal();
                            break;

                        case (BulletActionType.Fire):
                            ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);

                            if (!ac.useParam)
                            {
                                GUILayout.BeginHorizontal();
                                if (!ac.randomAngle)
                                    ac.angle.x = EditorGUILayout.IntField("Angle", (int)ac.angle.x);
                                else
                                    ac.angle = EditorGUILayout.Vector2Field("Angle Range", ac.angle);
                                ac.randomAngle = EditorGUILayout.Toggle("Randomize", ac.randomAngle);
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                ac.rankAngle = EditorGUILayout.Toggle("Add Rank", ac.rankAngle);
                                if (ac.rankAngle)
                                    ac.angle.z = EditorGUILayout.FloatField("RankAngle", ac.angle.z);
                                GUILayout.EndHorizontal();
                            }
                            ac.useParam = EditorGUILayout.Toggle("UseParamAngle", ac.useParam);
                            EditorGUILayout.Space();
                            ac.overwriteBulletSpeed = EditorGUILayout.Toggle("OverwriteSpeed", ac.overwriteBulletSpeed);
                            if (ac.overwriteBulletSpeed)
                            {
                                GUILayout.BeginHorizontal();
                                if (!ac.randomSpeed)
                                    ac.speed.x = EditorGUILayout.FloatField("New Speed", ac.speed.x);
                                else
                                    ac.speed = EditorGUILayout.Vector2Field("Speed Range", ac.speed);
                                ac.randomSpeed = EditorGUILayout.Toggle("Randomize", ac.randomSpeed);
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                ac.rankSpeed = EditorGUILayout.Toggle("Add Rank", ac.rankSpeed);
                                if (ac.rankSpeed)
                                    ac.speed.z = EditorGUILayout.FloatField("RankSpeed", ac.speed.z);
                                GUILayout.EndHorizontal();
                                ac.useSequenceSpeed = EditorGUILayout.Toggle("UseSequence", ac.useSequenceSpeed);
                            }
                            EditorGUILayout.Space();
                            ac.bulletTagIndex = EditorGUILayout.IntSlider("BulletTagIdx", ac.bulletTagIndex, 1, bp.bulletTags.Length);
                            break;
                    }
                    EditorGUI.indentLevel--;
                    if (GUI.changed)
                        SceneView.RepaintAll();
                }
            }

            if (moveIndex >= 0 && moveIndex != actions.Count - 1)
            {
                var temp = actions[moveIndex];
                actions[moveIndex] = actions[moveIndex + 1];
                actions[moveIndex + 1] = temp;

                var temp2 = bp.btaFoldouts[i].sub[moveIndex];
                bp.btaFoldouts[i].sub[moveIndex] = bp.btaFoldouts[i].sub[moveIndex + 1];
                bp.btaFoldouts[i].sub[moveIndex + 1] = temp2;
            }

            if (removeIndex >= 0)
            {
                actions.RemoveAt(removeIndex);
                bp.btaFoldouts[i].sub.RemoveAt(removeIndex);
            }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            if (GUILayout.Button("Add Action", GUILayout.Width(100)))
            {
                var ac = new BulletAction();
                actions.Add(ac);
                bp.btaFoldouts[i].sub.Add(true);
            }
            GUILayout.EndHorizontal();

            bp.bulletTags[i].actions = actions.ToArray();
        }
    }


}

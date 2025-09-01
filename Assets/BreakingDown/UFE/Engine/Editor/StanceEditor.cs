using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UFE3D;
using System.Drawing;
using System;

namespace BD
{
    [CustomEditor(typeof(StanceInfo))]
    public class StanceEditor : Editor
    {

        private string titleStyle;
        private string addButtonStyle;
        private string rootGroupStyle;
        private string subGroupStyle;
        private string arrayElementStyle;
        private string subArrayElementStyle;
        private string toggleStyle;
        private string foldStyle;
        private string enumStyle;
        private StanceInfo stanceInfo;
        private MoveSetData moveSet;

        private void OnEnable()
        {
            stanceInfo = (StanceInfo)target;
            moveSet = stanceInfo.ConvertData(); // Get editable MoveSetData from the stance
        }
        public void Init()
        {
            titleStyle = "MeTransOffRight";
            addButtonStyle = "CN CountBadge";
            rootGroupStyle = "GroupBox";
            subGroupStyle = "ObjectFieldThumb";
            arrayElementStyle = "FrameBox";
            subArrayElementStyle = "HelpBox";
            foldStyle = "Foldout";
            enumStyle = "MiniPopup";
            toggleStyle = "BoldToggle";

            StanceBlock(moveSet,true);
        }

        public override void OnInspectorGUI()
        {
            /*GUILayout.Label("Stance File");
            if (GUILayout.Button("Open Character Editor"))
                CharacterEditorWindow.Init();*/
            Init();

        }
        
        public void StanceBlock(MoveSetData moveSet, bool resource = false)
        {
            EditorGUILayout.BeginVertical(resource ? subArrayElementStyle : arrayElementStyle);
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    moveSet.combatStance = (CombatStances)EditorGUILayout.EnumPopup("Stance Number:", moveSet.combatStance, enumStyle);
                   
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                moveSet.cinematicIntro = (MoveInfo)EditorGUILayout.ObjectField("Intro Move:", moveSet.cinematicIntro, typeof(MoveInfo), false);
                EditorGUILayout.LabelField("Outro Moves");
                moveSet.roundOutro = (MoveInfo)EditorGUILayout.ObjectField("- Round Won:", moveSet.roundOutro, typeof(MoveInfo), false);
                moveSet.timeOutOutro = (MoveInfo)EditorGUILayout.ObjectField("- Time Out (Lost):", moveSet.timeOutOutro, typeof(MoveInfo), false);
                moveSet.cinematicOutro = (MoveInfo)EditorGUILayout.ObjectField("- Game Won:", moveSet.cinematicOutro, typeof(MoveInfo), false);

                EditorGUILayout.Space();

                moveSet.basicMovesToggle = EditorGUILayout.Foldout(moveSet.basicMovesToggle, "Basic Moves", foldStyle);
                if (moveSet.basicMovesToggle)
                {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUILayout.Space();

                        moveSet.enabledBasicMovesToggle = EditorGUILayout.Foldout(moveSet.enabledBasicMovesToggle, "Enabled Moves", foldStyle);
                        if (moveSet.enabledBasicMovesToggle)
                        {
                            EditorGUILayout.BeginVertical(subArrayElementStyle);
                            {
                                EditorGUI.indentLevel += 1;
                                EditorGUILayout.Space();
                                moveSet.basicMoves.moveEnabled = EditorGUILayout.Toggle("Move", moveSet.basicMoves.moveEnabled);
                                moveSet.basicMoves.jumpEnabled = EditorGUILayout.Toggle("Jump", moveSet.basicMoves.jumpEnabled);
                                moveSet.basicMoves.crouchEnabled = EditorGUILayout.Toggle("Crouch", moveSet.basicMoves.crouchEnabled);
                                moveSet.basicMoves.blockEnabled = EditorGUILayout.Toggle("Block", moveSet.basicMoves.blockEnabled);
                                moveSet.basicMoves.parryEnabled = EditorGUILayout.Toggle("Parry", moveSet.basicMoves.parryEnabled);
                                EditorGUILayout.Space();
                                EditorGUI.indentLevel -= 1;
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.Space();

                        SubGroupTitle("Standard Animations");
                        BasicMoveBlock("Idle (*)", moveSet.basicMoves.idle, WrapMode.Loop, false, true, false, false, false);
                        EditorGUI.BeginDisabledGroup(!moveSet.basicMoves.moveEnabled);
                        {
                            BasicMoveBlock("Move Forward (*)", moveSet.basicMoves.moveForward, WrapMode.Loop, false, true, false, false, false);
#if !UFE_LITE && !UFE_BASIC
                            //if (characterInfo.gameplayType != GameplayType._3DArena)
                            BasicMoveBlock("Move Back (*)", moveSet.basicMoves.moveBack, WrapMode.Loop, false, true, false, false, false);
                         
                                BasicMoveBlock("Move Sideways (*)", moveSet.basicMoves.moveSideways, WrapMode.Loop, false, true, false, false, false);
#else
                        BasicMoveBlock("Move Back (*)", moveSet.basicMoves.moveBack, WrapMode.Loop, false, true, false, false, false);
#endif
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!moveSet.basicMoves.crouchEnabled);
                        {
                            BasicMoveBlock("Crouching (*)", moveSet.basicMoves.crouching, WrapMode.Loop, false, true, false, false, false);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.Space();

                        if (moveSet.basicMoves.jumpEnabled)
                        {
                            SubGroupTitle("Jump Animations");
                            BasicMoveBlock("Take Off", moveSet.basicMoves.takeOff, WrapMode.ClampForever, true, true, false, true, false);
                            BasicMoveBlock("Jump Straight (*)", moveSet.basicMoves.jumpStraight, WrapMode.ClampForever, true, true, false, false, false);
                            BasicMoveBlock("Jump Back", moveSet.basicMoves.jumpBack, WrapMode.ClampForever, true, true, false, false, false);
                            BasicMoveBlock("Jump Forward", moveSet.basicMoves.jumpForward, WrapMode.ClampForever, true, true, false, false, false);
                            EditorGUILayout.Space();

                            SubGroupTitle("Fall Animations");
                            BasicMoveBlock("Fall Straight (*)", moveSet.basicMoves.fallStraight, WrapMode.ClampForever, true, true, false, false, false);
                            BasicMoveBlock("Fall Back", moveSet.basicMoves.fallBack, WrapMode.ClampForever, true, true, false, false, false);
                            BasicMoveBlock("Fall Forward", moveSet.basicMoves.fallForward, WrapMode.ClampForever, true, true, false, false, false);
                            BasicMoveBlock("Landing", moveSet.basicMoves.landing, WrapMode.Once, true, true, false, false, false);
                            EditorGUILayout.Space();
                        };

                        if (moveSet.basicMoves.blockEnabled)
                        {
                            SubGroupTitle("Block Animations");
                            BasicMoveBlock("Standing Pose", moveSet.basicMoves.blockingHighPose, WrapMode.ClampForever, false, true, false, false, false);
                            BasicMoveBlock("Standing High Hit", moveSet.basicMoves.blockingHighHit, WrapMode.Once, true, true, true, false, false);
                            BasicMoveBlock("Standing Low Hit", moveSet.basicMoves.blockingLowHit, WrapMode.Once, true, true, true, false, false);
                            BasicMoveBlock("Crouching Pose", moveSet.basicMoves.blockingCrouchingPose, WrapMode.ClampForever, false, true, false, false, false);
                            BasicMoveBlock("Crouching Hit", moveSet.basicMoves.blockingCrouchingHit, WrapMode.Once, true, true, true, false, false);
                            BasicMoveBlock("Air Pose", moveSet.basicMoves.blockingAirPose, WrapMode.ClampForever, false, true, false, false, false);
                            BasicMoveBlock("Air Hit", moveSet.basicMoves.blockingAirHit, WrapMode.Once, true, true, true, false, false);
                            EditorGUILayout.Space();
                        };

                        if (moveSet.basicMoves.parryEnabled)
                        {
                            SubGroupTitle("Parry Animations");
                            BasicMoveBlock("Standing High Hit", moveSet.basicMoves.parryHigh, WrapMode.Once, true, true, false, false, false);
                            BasicMoveBlock("Standing Low Hit", moveSet.basicMoves.parryLow, WrapMode.Once, true, true, false, false, false);
                            BasicMoveBlock("Crouching Hit", moveSet.basicMoves.parryCrouching, WrapMode.Once, true, true, false, false, false);
                            BasicMoveBlock("Air Hit", moveSet.basicMoves.parryAir, WrapMode.Once, true, true, false, false, false);
                            EditorGUILayout.Space();
                        };

                        SubGroupTitle("Hit Reactions");
                        BasicMoveBlock("Standing High Hit (*)", moveSet.basicMoves.getHitHigh, WrapMode.Once, true, true, true, true, false);
                        BasicMoveBlock("Standing Low Hit", moveSet.basicMoves.getHitLow, WrapMode.Once, true, true, true, true, false);
                        EditorGUI.BeginDisabledGroup(!moveSet.basicMoves.crouchEnabled);
                        {
                            BasicMoveBlock("Crouching Hit (*)", moveSet.basicMoves.getHitCrouching, WrapMode.Once, true, true, true, true, false);
                        }
                        EditorGUI.EndDisabledGroup();
                        BasicMoveBlock("Air Juggle [Launcher]", moveSet.basicMoves.getHitAir, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("Knock Back [Knockdown]", moveSet.basicMoves.getHitKnockBack, WrapMode.ClampForever, true, true, false, true, true);
                        BasicMoveBlock("Standing High Hit [Knockdown]", moveSet.basicMoves.getHitHighKnockdown, WrapMode.ClampForever, false, true, false, true, true);
                        BasicMoveBlock("Standing Mid Hit [Knockdown]", moveSet.basicMoves.getHitMidKnockdown, WrapMode.ClampForever, false, true, false, true, true);
                        BasicMoveBlock("Sweep [Knockdown]", moveSet.basicMoves.getHitSweep, WrapMode.ClampForever, false, true, false, true, true);
                        BasicMoveBlock("Crumple [Knockdown]", moveSet.basicMoves.getHitCrumple, WrapMode.ClampForever, true, true, false, true, true);
                        EditorGUILayout.Space();

                        SubGroupTitle("Stage Reactions");
                        BasicMoveBlock("Ground Bounce [Launcher]", moveSet.basicMoves.groundBounce, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("Standing Wall Bounce", moveSet.basicMoves.standingWallBounce, WrapMode.ClampForever, false, true, false, true, false);
                        BasicMoveBlock("Standing Wall Bounce [Knockdown]", moveSet.basicMoves.standingWallBounceKnockdown, WrapMode.ClampForever, false, true, false, true, false);
                        BasicMoveBlock("Air Wall Bounce [Knockdown]", moveSet.basicMoves.airWallBounce, WrapMode.ClampForever, true, true, false, true, false);
                        EditorGUILayout.Space();

                        SubGroupTitle("Fall Down Reactions");
                        BasicMoveBlock("Default (*)", moveSet.basicMoves.fallDown, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Air Juggle [Knockdown]", moveSet.basicMoves.fallingFromAirHit, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Ground Bounce [Knockdown]", moveSet.basicMoves.fallingFromGroundBounce, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("Air Recovery", moveSet.basicMoves.airRecovery, WrapMode.ClampForever, true, true, false, true, false);
                        EditorGUILayout.Space();

                        SubGroupTitle("Stand Up Animations");
                        BasicMoveBlock("Default (*)", moveSet.basicMoves.standUp, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Air Juggle", moveSet.basicMoves.standUpFromAirHit, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Knock Back", moveSet.basicMoves.standUpFromKnockBack, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Standing High Hit", moveSet.basicMoves.standUpFromStandingHighHit, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Standing Mid Hit", moveSet.basicMoves.standUpFromStandingMidHit, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Sweep", moveSet.basicMoves.standUpFromSweep, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Crumple", moveSet.basicMoves.standUpFromCrumple, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Standing Wall Bounce", moveSet.basicMoves.standUpFromStandingWallBounce, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Air Wall Bounce", moveSet.basicMoves.standUpFromAirWallBounce, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("From Ground Bounce", moveSet.basicMoves.standUpFromGroundBounce, WrapMode.ClampForever, true, true, false, true, false);

                        EditorGUILayout.Space();

                        GUILayout.Label("* Required", "MiniBoldLabel");

                    }
                    EditorGUILayout.EndVertical();
                }

                moveSet.attackMovesToggle = EditorGUILayout.Foldout(moveSet.attackMovesToggle, "Attack & Special Moves (" + moveSet.attackMoves.Length + ")", foldStyle);
                if (moveSet.attackMovesToggle)
                {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel += 1;

                        for (int y = 0; y < moveSet.attackMoves.Length; y++)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical(subArrayElementStyle);
                            {
                                EditorGUILayout.Space();
                                EditorGUIUtility.labelWidth = 120;
                                EditorGUILayout.BeginHorizontal();
                                {
                                    moveSet.attackMoves[y] = (MoveInfo)EditorGUILayout.ObjectField("Move File:", moveSet.attackMoves[y], typeof(MoveInfo), false);
                                    if (GUILayout.Button("", "PaneOptions"))
                                    {
                                        PaneOptions<MoveInfo>(moveSet.attackMoves, moveSet.attackMoves[y], delegate (MoveInfo[] newElement) { moveSet.attackMoves = newElement; });
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUIUtility.labelWidth = 150;

                                if (GUILayout.Button("Open in the Move Editor"))
                                {
                                    MoveEditorWindow.sentMoveInfo = moveSet.attackMoves[y];
                                    MoveEditorWindow.Init();
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.Space();
                        if (StyledButton("New Move"))
                            moveSet.attackMoves = AddElement<MoveInfo>(moveSet.attackMoves, null);

                        EditorGUILayout.Space();
                        EditorGUI.indentLevel -= 1;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space();
            }

            if (!resource && StyledButton("Export Stance"))
                ScriptableObjectUtility.CreateAsset<StanceInfo>(moveSet.ConvertData());

            EditorGUILayout.EndVertical();
        }
        private void SubGroupTitle(string _name)
        {
            Texture2D originalBackground = GUI.skin.box.normal.background;
            GUI.skin.box.normal.background = Texture2D.grayTexture;

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_name);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            GUI.skin.box.normal.background = originalBackground;
        }
        public bool StyledButton(string label)
        {
            EditorGUILayout.Space();
            GUILayoutUtility.GetRect(1, 20);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool clickResult = GUILayout.Button(label, addButtonStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            return clickResult;
        }
        public void PaneOptions<T>(T[] elements, T element, System.Action<T[]> callback)
        {
            if (elements == null || elements.Length == 0) return;
            GenericMenu toolsMenu = new GenericMenu();

            if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) || elements.Length == 1)
            {
                toolsMenu.AddDisabledItem(new GUIContent("Move Up"));
                toolsMenu.AddDisabledItem(new GUIContent("Move To Top"));
            }
            else
            {
                toolsMenu.AddItem(new GUIContent("Move Up"), false, delegate () { callback(MoveElement<T>(elements, element, -1)); });
                toolsMenu.AddItem(new GUIContent("Move To Top"), false, delegate () { callback(MoveElement<T>(elements, element, -elements.Length)); });
            }
            if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) || elements.Length == 1)
            {
                toolsMenu.AddDisabledItem(new GUIContent("Move Down"));
                toolsMenu.AddDisabledItem(new GUIContent("Move To Bottom"));
            }
            else
            {
                toolsMenu.AddItem(new GUIContent("Move Down"), false, delegate () { callback(MoveElement<T>(elements, element, 1)); });
                toolsMenu.AddItem(new GUIContent("Move To Bottom"), false, delegate () { callback(MoveElement<T>(elements, element, elements.Length)); });
            }

            toolsMenu.AddSeparator("");

            if (element != null && element is System.ICloneable)
            {
                toolsMenu.AddItem(new GUIContent("Copy"), false, delegate () { callback(CopyElement<T>(elements, element)); });
            }
            else
            {
                toolsMenu.AddDisabledItem(new GUIContent("Copy"));
            }

            if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T))
            {
                toolsMenu.AddItem(new GUIContent("Paste"), false, delegate () { callback(PasteElement<T>(elements, element)); });
            }
            else
            {
                toolsMenu.AddDisabledItem(new GUIContent("Paste"));
            }

            toolsMenu.AddSeparator("");

            if (!(element is System.ICloneable))
            {
                toolsMenu.AddDisabledItem(new GUIContent("Duplicate"));
            }
            else
            {
                toolsMenu.AddItem(new GUIContent("Duplicate"), false, delegate () { callback(DuplicateElement<T>(elements, element)); });
            }
            toolsMenu.AddItem(new GUIContent("Remove"), false, delegate () { callback(RemoveElement<T>(elements, element)); });

            toolsMenu.ShowAsContext();
            EditorGUIUtility.ExitGUI();
        }
        public T[] RemoveElement<T>(T[] elements, T element)
        {
            List<T> elementsList = new List<T>(elements);
            elementsList.Remove(element);
            return elementsList.ToArray();
        }

        public T[] AddElement<T>(T[] elements, T element)
        {
            List<T> elementsList = new List<T>(elements);
            elementsList.Add(element);
            return elementsList.ToArray();
        }

        public T[] CopyElement<T>(T[] elements, T element)
        {
            CloneObject.objCopy = (object)(element as ICloneable).Clone();
            return elements;
        }

        public T[] PasteElement<T>(T[] elements, T element)
        {
            if (CloneObject.objCopy == null) return elements;
            List<T> elementsList = new List<T>(elements);
            elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
            //CloneObject.objCopy = null;
            return elementsList.ToArray();
        }

        public T[] DuplicateElement<T>(T[] elements, T element)
        {
            List<T> elementsList = new List<T>(elements);
            elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
            return elementsList.ToArray();
        }

        public T[] MoveElement<T>(T[] elements, T element, int steps)
        {
            List<T> elementsList = new List<T>(elements);
            int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
            elementsList.Remove(element);
            elementsList.Insert(newIndex, element);
            return elementsList.ToArray();
        }
        public void BasicMoveBlock(string label, BasicMoveInfo basicMove, WrapMode wrapMode, bool autoSpeed, bool hasSound, bool hasHitStrength, bool invincible, bool loops)
        {
            basicMove.editorToggle = EditorGUILayout.Foldout(basicMove.editorToggle, label, foldStyle);

            //GUIStyle foldoutStyle;
            //foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.normal.textColor = Color.cyan;
            //basicMove.editorToggle = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), basicMove.editorToggle, label, true, foldoutStyle);

            if (basicMove.editorToggle)
            {
                EditorGUILayout.BeginVertical(subArrayElementStyle);
                {
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel += 1;
                    EditorGUIUtility.labelWidth = 180;

                    if (label != "Idle (*)")
                    {
                        basicMove.useMoveFile = EditorGUILayout.Toggle("Use Move File", basicMove.useMoveFile, toggleStyle);
                    }
                    else
                    {
                        basicMove.useMoveFile = false;
                    }

                    if (basicMove.useMoveFile)
                    {
                        basicMove.moveInfo = (MoveInfo)EditorGUILayout.ObjectField("Move:", basicMove.moveInfo, typeof(MoveInfo), false);
                    }
                    else
                    {
                        if (hasHitStrength)
                        {
                            // UFE 2.0.3 update
                            if (basicMove.animMap.Length <= 8)
                            {
                                Array.Resize(ref basicMove.animMap, 9);
                                basicMove.animMap[6] = new SerializedAnimationMap();
                                basicMove.animMap[7] = new SerializedAnimationMap();
                                basicMove.animMap[8] = new SerializedAnimationMap();
                            }

                            string required = label.IndexOf("*") != -1 ? " (*)" : "";
                            AnimationFieldBlock(basicMove.animMap[0], "Weak Hit", required);
                            AnimationFieldBlock(basicMove.animMap[1], "Medium Hit");
                            AnimationFieldBlock(basicMove.animMap[2], "Heavy Hit");
                            AnimationFieldBlock(basicMove.animMap[3], "Custom 1 Hit");
                            AnimationFieldBlock(basicMove.animMap[4], "Custom 2 Hit");
                            AnimationFieldBlock(basicMove.animMap[5], "Custom 3 Hit");
                            AnimationFieldBlock(basicMove.animMap[6], "Custom 4 Hit");
                            AnimationFieldBlock(basicMove.animMap[7], "Custom 5 Hit");
                            AnimationFieldBlock(basicMove.animMap[8], "Custom 6 Hit");

                        }
                        else if (label == "Idle (*)")
                        {
                            AnimationFieldBlock(basicMove.animMap[0], "Default", " (*)");
                            AnimationFieldBlock(basicMove.animMap[1], "AFK 1");
                            AnimationFieldBlock(basicMove.animMap[2], "AFK 2");
                            AnimationFieldBlock(basicMove.animMap[3], "AFK 3");
                            AnimationFieldBlock(basicMove.animMap[4], "AFK 4");
                            AnimationFieldBlock(basicMove.animMap[5], "AFK 5");
                            basicMove._restingClipInterval = EditorGUILayout.FloatField("Resting Interval:", (float)basicMove._restingClipInterval);

                        }
                        else if (label == "Stand Up (*)")
                        {
                            AnimationFieldBlock(basicMove.animMap[0], "Default", " (*)");
                            AnimationFieldBlock(basicMove.animMap[1], "High Knockdown");
                            AnimationFieldBlock(basicMove.animMap[2], "Low Knockdown");
                            AnimationFieldBlock(basicMove.animMap[3], "Sweep");
                            AnimationFieldBlock(basicMove.animMap[4], "Crumple");
                            AnimationFieldBlock(basicMove.animMap[5], "Wall Bounce");

                        }
                        else if (label == "Crouching (*)")
                        {
                            AnimationFieldBlock(basicMove.animMap[0], "Crouched");
                            AnimationFieldBlock(basicMove.animMap[1], "Crouching Down");
                            AnimationFieldBlock(basicMove.animMap[2], "Standing Up");
                        }
                        else if (label.IndexOf("[Knockdown]") != -1)
                        {
                            AnimationFieldBlock(basicMove.animMap[0], "Fall Clip", " (*)");
                            AnimationFieldBlock(basicMove.animMap[1], "Down Clip");
                            basicMove.loopDownClip = EditorGUILayout.Toggle("Loop Down Clip", basicMove.loopDownClip, toggleStyle);
                        }
                        else if (loops)
                        {
                            AnimationFieldBlock(basicMove.animMap[1], "Transition");
                            AnimationFieldBlock(basicMove.animMap[0], "Animation", " (*)");
                        }
                        else
                        {
                            AnimationFieldBlock(basicMove.animMap[0], "Animation");
                        }

                        if (autoSpeed)
                        {
                            basicMove.autoSpeed = EditorGUILayout.Toggle("Auto Speed", basicMove.autoSpeed, toggleStyle);
                        }
                        else
                        {
                            basicMove.autoSpeed = false;
                        }

                        if (basicMove.autoSpeed)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextField("Animation Speed:", basicMove._animationSpeed.ToString());
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            basicMove._animationSpeed = EditorGUILayout.FloatField("Animation Speed:", (float)basicMove._animationSpeed);
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            basicMove.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", basicMove.wrapMode, enumStyle);
                            if (basicMove.wrapMode == WrapMode.Default) basicMove.wrapMode = wrapMode;
                            if (GUILayout.Button("Default", "minibutton", GUILayout.Width(60))) basicMove.wrapMode = wrapMode;

                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                        EditorGUILayout.Space();

                        basicMove.overrideBlendingIn = EditorGUILayout.Toggle("Override Blending (In)", basicMove.overrideBlendingIn, toggleStyle);
                        if (basicMove.overrideBlendingIn)
                        {
                            basicMove._blendingIn = EditorGUILayout.FloatField("Blend In Duration:", (float)basicMove._blendingIn);
                        }

                        basicMove.overrideBlendingOut = EditorGUILayout.Toggle("Override Blending (Out)", basicMove.overrideBlendingOut, toggleStyle);
                        if (basicMove.overrideBlendingOut)
                        {
                            basicMove._blendingOut = EditorGUILayout.FloatField("Blend Out Duration:", (float)basicMove._blendingOut);
                        }

                        if (invincible) basicMove.invincible = EditorGUILayout.Toggle("Hide hitboxes", basicMove.invincible, toggleStyle);

                        basicMove.disableHeadLook = EditorGUILayout.Toggle("Disable Head Look", basicMove.disableHeadLook, toggleStyle);
                        basicMove.applyRootMotion = EditorGUILayout.Toggle("Apply Root Motion", basicMove.applyRootMotion, toggleStyle);
                        if (basicMove.applyRootMotion)
                        {
                            EditorGUI.indentLevel += 1;
                            basicMove.lockXMotion = EditorGUILayout.Toggle("Lock X Motion", basicMove.lockXMotion, toggleStyle);
                            basicMove.lockYMotion = EditorGUILayout.Toggle("Lock Y Motion", basicMove.lockYMotion, toggleStyle);
                            basicMove.lockZMotion = EditorGUILayout.Toggle("Lock Z Motion", basicMove.lockZMotion, toggleStyle);
                            EditorGUI.indentLevel -= 1;
                        }

                        EditorGUILayout.Space();
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                        EditorGUILayout.Space();

                        basicMove.particleEffect.editorToggle = EditorGUILayout.Foldout(basicMove.particleEffect.editorToggle, "Particle Effect", foldStyle);
                        if (basicMove.particleEffect.editorToggle)
                        {
                            EditorGUILayout.BeginVertical(subGroupStyle);
                            {
                                EditorGUILayout.Space();
                                basicMove.particleEffect.prefab = (GameObject)EditorGUILayout.ObjectField("Particle Prefab:", basicMove.particleEffect.prefab, typeof(UnityEngine.GameObject), true);
                                basicMove.particleEffect.duration = EditorGUILayout.FloatField("Duration (seconds):", basicMove.particleEffect.duration);
                                basicMove.particleEffect.stick = EditorGUILayout.Toggle("Sticky", basicMove.particleEffect.stick, toggleStyle);
                                basicMove.particleEffect.bodyPart = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", basicMove.particleEffect.bodyPart, enumStyle);
                                basicMove.particleEffect.positionOffSet = EditorGUILayout.Vector3Field("Off Set (relative):", basicMove.particleEffect.positionOffSet);
                                basicMove.particleEffect.mirrorOn2PSide = EditorGUILayout.Toggle("Mirror on Right Side", basicMove.particleEffect.mirrorOn2PSide);

                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndVertical();
                        }
                        if (hasSound)
                        {
                            basicMove.soundEffectsToggle = EditorGUILayout.Foldout(basicMove.soundEffectsToggle, "Possible Sound Effects (" + basicMove.soundEffects.Length + ")", EditorStyles.foldout);
                            if (basicMove.soundEffectsToggle)
                            {
                                EditorGUILayout.BeginVertical(subGroupStyle);
                                {
                                    basicMove.continuousSound = EditorGUILayout.Toggle("Continuous Sound", basicMove.continuousSound, toggleStyle);
                                    EditorGUILayout.Space();

                                    EditorGUIUtility.labelWidth = 150;
                                    for (int i = 0; i < basicMove.soundEffects.Length; i++)
                                    {
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginVertical(subArrayElementStyle);
                                        {
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginHorizontal();
                                            {
                                                basicMove.soundEffects[i] = (AudioClip)EditorGUILayout.ObjectField("Audio Clip:", basicMove.soundEffects[i], typeof(UnityEngine.AudioClip), true);
                                                if (GUILayout.Button("", "PaneOptions"))
                                                {
                                                    PaneOptions<AudioClip>(basicMove.soundEffects, basicMove.soundEffects[i], delegate (AudioClip[] newElement) { basicMove.soundEffects = newElement; });
                                                }
                                            }
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space();
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                    if (StyledButton("New Sound Effect"))
                                        basicMove.soundEffects = AddElement<AudioClip>(basicMove.soundEffects, null);

                                }
                                EditorGUILayout.EndVertical();
                            }
                        }
                    }

                    EditorGUI.indentLevel -= 1;
                    EditorGUILayout.Space();

                }
                EditorGUILayout.EndVertical();
            }
        }
        private void AnimationFieldBlock(SerializedAnimationMap animMap, string label, string required = "")
        {
            EditorGUILayout.BeginHorizontal();
            animMap.clip = (AnimationClip)EditorGUILayout.ObjectField(label + " Clip" + required + ":", animMap.clip, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
           
            EditorGUILayout.EndHorizontal();

            if (animMap.hitBoxDefinitionType == HitBoxDefinitionType.Custom)
            {
                animMap.customHitBoxDefinition = (CustomHitBoxesInfo)EditorGUILayout.ObjectField(label + " Map" + required + ":", animMap.customHitBoxDefinition, typeof(CustomHitBoxesInfo), false, GUILayout.ExpandWidth(true));
                if (animMap.customHitBoxDefinition != null && animMap.customHitBoxDefinition.clip != null && animMap.clip == null)
                {
                    animMap.clip = animMap.customHitBoxDefinition.clip;
                    animMap.length = animMap.clip.length;
                }
            }

        }
    }

}
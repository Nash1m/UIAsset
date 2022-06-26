using System;
using System.Linq;
using Nash1m.UI.Animator;
using UnityEditor;
using UnityEngine;

namespace Nash1m.UI.Editor
{
    public class UIAnimatorWindow : EditorWindow
    {
        public GUISkin skin;

        private UIAnimator _animator;
        private UIAnimator _backup;

        private int selectedAnimationIndex;
        private int _selectedTweenNodeIndex = -1;
        private string _lastBindingKey;

        private Timeline _timeline;

        #region Widnow Rects

        private Rect _animationsListRect;
        private Rect _editorRect;
        private Rect _inspectorRect;

        #endregion

        private float _playStartTime;
        private bool _isPlaying;
        private bool _preview;
        private float clickTime;
        private Vector3 createNodePosition;
        private NodeState nodeState;


        #region Tweens menu
        private string[] _availableTween;
        private GenericMenu _tweenCreateMenu;
        #endregion

        private UIAnimator CurrentAnimator => _backup ? _backup : _animator;

        private UIAnimation selectedAnimation =>
            selectedAnimationIndex < 0 || selectedAnimationIndex > CurrentAnimator.animations.Count - 1
                ? null
                : CurrentAnimator.animations[selectedAnimationIndex];

        #region Lock

        [NonSerialized] private GUIStyle lockButtonStyle;
        [NonSerialized] private bool locked;

        #endregion


        [MenuItem("Nash1m/UI Animator")]
        public static void Init()
        {
            var window = GetWindow<UIAnimatorWindow>();
            window.minSize = new Vector2(200 + 250 + 400, 200);
            window.Show();
        }

        private void OnEnable()
        {
            _timeline ??= new Timeline();
            _timeline.onTimelineGUI = DrawTweenNodes;
            _timeline.TimelineTitle = OnDrawTimelineTitle;
            _timeline.TimelineClick = OnCurrentTimeChanged;

            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            #region Get all tween names

            var type = typeof(ITween);
            _availableTween = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .Select(x => $"{x.Namespace}.{x.Name}").ToArray();


            _tweenCreateMenu = new GenericMenu();
            foreach (var tweenName in _availableTween)
            {
                _tweenCreateMenu.AddItem(new GUIContent($"{tweenName}"), false, AddTween, tweenName);
            }

            #endregion
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            UndoObject();
        }

        private void Update()
        {
            if(!locked)
            {
                if (!Selection.activeGameObject)
                {
                    _animator = null;
                    return;
                }

                var selectionAnimator = Selection.activeGameObject.GetComponent<UIAnimator>();
                if (_animator != selectionAnimator)
                    UndoObject();

                _animator = selectionAnimator;
            }
            if (!_animator)
            {
                _preview = false;
                Repaint();
                return;
            }

            _animationsListRect = new Rect(0, 0, 200, position.height);
            _editorRect = new Rect(200, 0, position.width - 450, position.height);
            _inspectorRect = new Rect(position.width - 250, 0, 250, position.height);

            if (_animator && Application.isPlaying && _animator.IsPlaying)
            {
                _selectedTweenNodeIndex = -1;
                selectedAnimationIndex = _animator.animations.IndexOf(_animator.CurrentAnimation);
                var animationTime = UIAnimator.GetAnimationTime(_animator.CurrentTime, selectedAnimation);
                _timeline.currentTime = animationTime;
                Repaint();
            }

            if (_preview && !Application.isPlaying)
                BackupObject();
            else
                UndoObject();

            if (_isPlaying)
            {
                var currentTime = (float) EditorApplication.timeSinceStartup - _playStartTime;
                var animationTime = UIAnimator.GetAnimationTime(currentTime, selectedAnimation);
                _timeline.currentTime = animationTime;
                OnCurrentTimeChanged(animationTime);
            }
        }

        private void OnGUI()
        {
            if (!_animator)
            {
                EditorGUILayout.HelpBox(
                    "UI Animator is not selected. \n Please select an object with UIAnimator component",
                    MessageType.Error);
                return;
            }


            #region Animations

            GUILayout.BeginArea(_animationsListRect);
            DrawAnimations();
            GUILayout.EndArea();

            #endregion

            #region Animation Timeline

            _timeline.DrawTimeline(_editorRect);

            #endregion

            #region Animation Inspector

            GUILayout.BeginArea(_inspectorRect);
            DrawInspector();
            GUILayout.EndArea();

            #endregion

            ProcessEditorEvents();
        }

        #region Draw Methods
        private void DrawAnimations()
        {
            GUILayout.BeginVertical();
            DrawAnimationsMenuBar();
            DrawAnimationsDropdown();

            if (selectedAnimation != null)
                DrawAnimationInfo(selectedAnimation);
            GUILayout.EndVertical();

            void DrawAnimationsMenuBar()
            {
                GUILayout.BeginHorizontal("Toolbar");
                var previewOnTexture = "Assets/UI/UIAnimator/Icons/Preview_On.png";
                var previewOffTexture = "Assets/UI/UIAnimator/Icons/Preview_Off.png";

                if (EditorGIUExtensions.ImagedButton(_preview ? previewOnTexture : previewOffTexture))
                {
                    _preview = !_preview;
                }

                
                _timeline.currentTime = EditorGUILayout.FloatField(_timeline.currentTime);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (EditorGIUExtensions.ImagedButton("Assets/UI/UIAnimator/Icons/To_Start.png"))
                {
                    _timeline.currentTime = 0;
                    OnCurrentTimeChanged(_timeline.currentTime);
                }

                if (EditorGIUExtensions.ImagedButton("Assets/UI/UIAnimator/Icons/Previous_Second.png"))
                {
                    _timeline.currentTime--;
                    _timeline.currentTime = Mathf.Clamp(_timeline.currentTime, 0, float.MaxValue);
                    OnCurrentTimeChanged(_timeline.currentTime);
                }

                if (EditorGIUExtensions.ImagedButton(!_isPlaying
                    ? "Assets/UI/UIAnimator/Icons/Play_Off.png"
                    : "Assets/UI/UIAnimator/Icons/Play_On.png"))
                {
                    _isPlaying = !_isPlaying;
                    _preview = _isPlaying;
                    _playStartTime = (float) EditorApplication.timeSinceStartup;
                }

                if (EditorGIUExtensions.ImagedButton("Assets/UI/UIAnimator/Icons/Next_Second.png"))
                {
                    _timeline.currentTime++;
                    _timeline.currentTime = Mathf.Clamp(_timeline.currentTime, 0, float.MaxValue);
                    OnCurrentTimeChanged(_timeline.currentTime);
                }

                if (EditorGIUExtensions.ImagedButton("Assets/UI/UIAnimator/Icons/To_End.png"))
                {
                    _timeline.currentTime = selectedAnimation.Duration;
                    OnCurrentTimeChanged(_timeline.currentTime);
                }

                GUILayout.EndHorizontal();
            }

            void DrawAnimationsDropdown()
            {
                if (!EditorGUILayout.DropdownButton(selectedAnimation is {} ?new GUIContent(selectedAnimation.key): new GUIContent($"Animations list"), FocusType.Passive)) return;

                var menu = new GenericMenu();
                for (var i = 0; i < CurrentAnimator.animations.Count; i++)
                {
                    var index = i;
                    var animation = CurrentAnimator.animations[index];
                    var isCurrent = selectedAnimation is { } && selectedAnimation.key == animation.key;
                    menu.AddItem(new GUIContent(animation.key), isCurrent,
                        () =>
                        {
                            _selectedTweenNodeIndex = -1;
                            selectedAnimationIndex = index;
                        });
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Add Animation"), false, AddAnimation);

                menu.DropDown(new Rect(4, 42, 0, 0));
            }

            void DrawAnimationInfo(UIAnimation animation)
            {
                #region Animation Key

                GUILayout.BeginHorizontal();
                GUILayout.Label("Key");
                animation.key = GUILayout.TextField(animation.key);
                GUILayout.EndHorizontal();

                #endregion

                var animationTypes = Enum.GetNames(typeof(AnimationType));
                selectedAnimation.animationType =
                    (AnimationType) EditorGUILayout.Popup("Animation Type", (int) selectedAnimation.animationType,
                        animationTypes);
                GUILayout.Label($"Tween index {_selectedTweenNodeIndex}");
                GUILayout.Label($"Duration: {animation.Duration}");
            }
        }
        private void DrawInspector()
        {
            if (selectedAnimation == null) return;

            if (EditorGUILayout.DropdownButton(new GUIContent("Add tween"), FocusType.Passive))
            {
                _tweenCreateMenu.DropDown(new Rect(1, 18, 0, 0));
            }

            if (_selectedTweenNodeIndex == -1) return;
            var selectedTween = selectedAnimation.tweenNodes[_selectedTweenNodeIndex];

            GUILayout.Label("Tween node info");
            selectedTween.nodeColor = EditorGUILayout.ColorField(selectedTween.nodeColor);

            #region Channel Title

            GUILayout.BeginHorizontal();
            GUILayout.Label("Channel");
            selectedTween.channel = EditorGUILayout.IntField(selectedTween.channel);
            GUILayout.EndHorizontal();

            #endregion

            #region Start time Title

            GUILayout.BeginHorizontal();
            GUILayout.Label("Start Time");
            selectedTween.startTime = EditorGUILayout.FloatField(selectedTween.startTime);
            GUILayout.EndHorizontal();

            #endregion

            #region End time Title

            GUILayout.BeginHorizontal();
            GUILayout.Label("End Time");
            selectedTween.endTime = EditorGUILayout.FloatField(selectedTween.endTime);
            GUILayout.EndHorizontal();

            #endregion

            if (_selectedTweenNodeIndex < 0) return;

            GUILayout.Label("Tween info");
            if (EditorGUILayout.DropdownButton(new GUIContent(selectedTween.tween.BindingKey), FocusType.Passive))
            {
                var bindingKeys = CurrentAnimator.animationBindings.Select(x => x.key);
                var menu = new GenericMenu();
                foreach (var bindingKey in bindingKeys)
                {
                    menu.AddItem(new GUIContent(bindingKey), selectedTween.tween.BindingKey == bindingKey,
                        () => selectedTween.tween.BindingKey = bindingKey);
                }

                menu.DropDown(new Rect(1, 156, 0, 0));
            }

            selectedTween.tween.Draw();
        }
        private void DrawTweenNodes(Rect rect)
        {
            if (selectedAnimation == null) return;

            var nodes = selectedAnimation.tweenNodes.ToList();
            foreach (var node in nodes)
            {
                AddCursorRect(node);
                DrawNode(node);
            }

            ProcessNodeEvents();

            void AddCursorRect(TweenNode node)
            {
                EditorGUIUtility.AddCursorRect(
                    new Rect(_timeline.SecondsToGUI(node.startTime) - 5, node.channel * 20, 10, 20),
                    MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(
                    new Rect(_timeline.SecondsToGUI(node.endTime) - 5, node.channel * 20, 10, 20),
                    MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(
                    new Rect(_timeline.SecondsToGUI(node.startTime), node.channel * 20,
                        _timeline.SecondsToGUI(node.endTime - node.startTime), 20), MouseCursor.Pan);
            }
            void DrawNode(TweenNode node)
            {
                var startPoint = _timeline.SecondsToGUI(node.startTime);
                var duration = _timeline.SecondsToGUI(node.endTime - node.startTime);
                var nodeRect = new Rect(startPoint, node.channel * 20, duration, 20);

                GUI.color = node.nodeColor;
                GUI.Box(nodeRect, $"{node.tween.BindingKey} [{node.tween.GetType().Name}]");
                GUI.color = Color.white;
            }
        }
        private void OnDrawTimelineTitle()
        {
            var content = selectedAnimation is { } ? new GUIContent($"[{selectedAnimation.key}]") : new GUIContent("");

            var style = skin.GetStyle("TimelineTitle");
            var size = style.CalcSize(content);

            var xPosition = _editorRect.x + _editorRect.width - size.x - 15;
            var yPosition = _editorRect.y + _editorRect.height - size.y - 15;

            var titleRect = new Rect(xPosition, yPosition, size.x, size.y);
            GUI.Label(titleRect, content, style);
        }
        #endregion

        #region Events
        private void ProcessNodeEvents()
        {
            var e = Event.current;
            switch (e.rawType)
            {
                case EventType.MouseDown:
                    MouseDownEvent();
                    break;
                case EventType.MouseUp:
                    _timeline.canClickTimeline = true;
                    nodeState = NodeState.None;
                    break;
                case EventType.MouseDrag:
                    MouseDragEvent();
                    break;
            }

            void MouseDownEvent()
            {
                if (e.button != 0) return;
                if (selectedAnimation == null) return;

                for (var i = 0; i < selectedAnimation.tweenNodes.Count; i++)
                {
                    var checkNode = selectedAnimation.tweenNodes[i];

                    clickTime = _timeline.GUIToSeconds(e.mousePosition.x);

                    if (NodeRect(checkNode).Contains(e.mousePosition))
                    {
                        _timeline.canClickTimeline = false;

                        _selectedTweenNodeIndex = i;
                        var node = selectedAnimation.tweenNodes[_selectedTweenNodeIndex];
                        if (_timeline.SecondsToGUI(clickTime - node.startTime) < 5)
                            nodeState = NodeState.ResizeStartTime;
                        else if (_timeline.SecondsToGUI(node.endTime - clickTime) < 5)
                            nodeState = NodeState.ResizeEndTime;
                        else
                            nodeState = NodeState.Move;

                        e.Use();
                    }
                }
            }

            void MouseDragEvent()
            {
                if (e.button != 0 || _selectedTweenNodeIndex == -1) return;
                var dragNode = selectedAnimation.tweenNodes[_selectedTweenNodeIndex];
                var currentTime = _timeline.GUIToSeconds(e.mousePosition.x);
                switch (nodeState)
                {
                    case NodeState.ResizeStartTime:
                        currentTime = Mathf.Clamp(currentTime, 0, dragNode.endTime - 1);
                        dragNode.startTime = currentTime;
                        e.Use();
                        break;
                    case NodeState.ResizeEndTime:
                        currentTime = Mathf.Clamp(currentTime, dragNode.startTime + 1, float.MaxValue);
                        dragNode.endTime = currentTime;
                        e.Use();
                        break;
                    case NodeState.Move:
                        var y = e.mousePosition.y;
                        var channel = Mathf.FloorToInt((y - y % 20) / 20);
                        channel = Mathf.Clamp(channel, 0, int.MaxValue);
                        dragNode.channel = channel;

                        var delta = currentTime - clickTime;
                        dragNode.endTime += delta;
                        dragNode.startTime += delta;
                        clickTime = currentTime;
                        e.Use();

                        //TODO: Check intersection
                        break;
                }

                Repaint();
            }
        }
        private void ProcessEditorEvents()
        {
            var e = Event.current;
            switch (e.rawType)
            {
                case EventType.DragUpdated:
                    DragUpdateEvent();
                    break;
                case EventType.DragPerform:
                    DragPerformedEvent();
                    break;
                case EventType.DragExited:
                    e.Use();
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete)
                    {
                        DeleteSelectedNode();
                        e.Use();
                    }
                    break;
            }

            void DragUpdateEvent()
            {
                DragAndDrop.visualMode = _editorRect.Contains(e.mousePosition)
                    ? DragAndDropVisualMode.Link
                    : DragAndDropVisualMode.Rejected;

                e.Use();
            }
            void DragPerformedEvent()
            {
                if (!_editorRect.Contains(e.mousePosition)) return;
                if (DragAndDrop.objectReferences.Length > 1) return;

                DragAndDrop.AcceptDrag();

                var dragObject = DragAndDrop.objectReferences[0];
                if (dragObject is GameObject go)
                {
                    var binding = CurrentAnimator.AddBindingObject(go);
                    _lastBindingKey = binding.key;
                }

                createNodePosition = e.mousePosition - _editorRect.position - new Vector2(0, 20);
                _tweenCreateMenu.ShowAsContext();
                e.Use();
            }
        }
        #endregion

        #region Add Methods
        private void AddAnimation()
        {
            CurrentAnimator.animations.Add(new UIAnimation
                {animator = CurrentAnimator, key = $"New Animation {CurrentAnimator.animations.Count}"});
            selectedAnimationIndex = CurrentAnimator.animations.Count - 1;
            _selectedTweenNodeIndex = -1;
            EditorUtility.SetDirty(CurrentAnimator);
        }
        private void AddTween(object userData)
        {
            //Find type of the selected tween
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.FullName == (string) userData);
            if (type == null) throw new Exception($"Type with name {(string) userData} not found");

            var tween = (ITween) Activator.CreateInstance(type); //Create instance of tween
            tween.BindingKey = _lastBindingKey; //Set Binding Key

            var channel =
                Mathf.FloorToInt((createNodePosition.y - createNodePosition.y % 20) / 20); //Calculate channel 
            var startTime = _timeline.GUIToSeconds(createNodePosition.x); //Calculate Start time
            var node = new TweenNode(tween)
            {
                startTime = startTime,
                endTime = startTime + 10,
                channel = channel,
            };

            selectedAnimation.tweenNodes.Add(node);
            _selectedTweenNodeIndex = selectedAnimation.tweenNodes.Count - 1;

            createNodePosition = Vector3.zero;
            _lastBindingKey = string.Empty;
            EditorUtility.SetDirty(CurrentAnimator);
        }

        private void DeleteSelectedNode()
        {
            if(_selectedTweenNodeIndex < 0) return;
            
            selectedAnimation.tweenNodes.RemoveAt(_selectedTweenNodeIndex);
            _selectedTweenNodeIndex--;
        }
        #endregion

        #region Preview
        private void BackupObject()
        {
            if (_backup) return;

            _backup = Instantiate(_animator, _animator.transform.parent, false);
            _backup.gameObject.name = $"{_animator.gameObject.name}(Preview)";
            _backup.transform.SetSiblingIndex(_animator.transform.GetSiblingIndex());
            _animator.gameObject.SetActive(false);
            _animator.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        private void UndoObject()
        {
            if (!_backup) return;

            DestroyImmediate(_backup.gameObject);
            _animator.gameObject.SetActive(true);
            _animator.gameObject.hideFlags = HideFlags.None;
        }
        #endregion

        private void OnCurrentTimeChanged(float time)
        {
            Repaint();

            if (!_preview) return;
            time = Mathf.Clamp(time, 0, selectedAnimation.Duration);
            selectedAnimation.UpdateAnimation(time);
        }
        private void OnPlayModeChanged(PlayModeStateChange obj)
        {
            UndoObject();
        }


        private Rect NodeRect(TweenNode node)
        {
            var startX = _timeline.SecondsToGUI(node.startTime);
            var width = _timeline.SecondsToGUI(node.endTime - node.startTime);
            return new Rect(startX, node.channel * 20, width, 20);
        }

        public void ShowButton(Rect rect)
        {
            lockButtonStyle ??= "IN LockButton";
            locked = GUI.Toggle(rect, locked, GUIContent.none, lockButtonStyle);
        }
    }

    public enum NodeState
    {
        None,
        ResizeStartTime,
        ResizeEndTime,
        Move
    }
}
using System;
using Nash1m.Extensions;
using UnityEditor;
using UnityEngine;

namespace Nash1m.UI.Editor
{
    public class Timeline
    {
        #region Delegates

        public delegate void TimelineGUICallback(Rect rect);

        public TimelineGUICallback onTimelineGUI;

        public delegate void TimelineClickCallback(float time);

        public TimelineClickCallback TimelineClick;

        public delegate void DrawTitleCallback();

        public DrawTitleCallback TimelineTitle;

        #endregion

        private Vector2 _scroll;
        private Vector2 _expandView = new Vector2(1000, 100);

        private float zoom = 20f;
        private float _zoomFactor = 1f;


        public float currentTime;
        public bool timeReversed;
        public bool canClickTimeline;

        private int timeFactorIndex = 1;
        private readonly float[] timeFactors = new[] {1f, 5f, 10f, 15f, 30f, 60f, 120f, 240f};

        
        public void DrawTimeline(Rect rect)
        {
            var ticksRect = new Rect(rect.x, rect.y, rect.width - 15, 20);
            //Add events rect

            DrawTicks(ticksRect);

            _scroll = GUI.BeginScrollView(
                new Rect(rect.x, ticksRect.height, rect.width, rect.height - ticksRect.height), _scroll,
                new Rect(0, 0, rect.width + _expandView.x, rect.height - 20 + _expandView.y), true, true);

            onTimelineGUI?.Invoke(new Rect(0, 0, rect.width, rect.height));
            DrawLines(rect);
            DrawCurrentTime(rect);
            ProcessEvents(rect);
            GUI.EndScrollView();

            TimelineTitle?.Invoke();
        }


        private void DrawTicks(Rect ticksRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                EditorStyles.toolbar.Draw(ticksRect, GUIContent.none, 0);
            }

            var count = 0;
            for (var i = ticksRect.x - _scroll.x; i < ticksRect.width + ticksRect.x; i += zoom * _zoomFactor)
            {
                if (i < ticksRect.x)
                {
                    count++;
                    continue;
                }

                if (count % 5 == 0)
                {
                    Handles.color = new Color(0.7f, 0.7f, 0.7f);
                    Handles.DrawLine(new Vector3(i, 20, 0), new Vector3(i, 5, 0));

                    if (i < ticksRect.width + ticksRect.x - 30)
                    {
                        var displayMinutes = Mathf.FloorToInt((count / 5.0f) * timeFactors[timeFactorIndex] / 60.0f);
                        var displaySeconds = Mathf.FloorToInt((count / 5.0f) * timeFactors[timeFactorIndex] % 60.0f);
                        var content = new GUIContent($"{displayMinutes:0}:{displaySeconds:00}");
                        var size = ((GUIStyle) "Label").CalcSize(content);
                        size.x = Mathf.Clamp(size.x, 0.0f, ticksRect.width - i);

                        GUI.Label(new Rect(i, -3, 60, 20), content);
                    }
                }
                else
                {
                    Handles.color = new Color(0.5f, 0.5f, 0.5f);
                    Handles.DrawLine(new Vector3(i, 20, 0), new Vector3(i, 15, 0));
                }

                count++;
            }
        }

        private void DrawLines(Rect rect)
        {
            Handles.color = new Color(0.3f, 0.3f, 0.3f);
            for (var y = 20; y < (int) rect.height + _scroll.y; y += 20)
            {
                Handles.DrawLine(new Vector3(0, y, 0), new Vector3(rect.width + _scroll.x - 15, y, 0));
            }

            Handles.color = Color.white;
        }

        private void DrawCurrentTime(Rect rect)
        {
            Handles.color = Color.red;
            var xPos = SecondsToGUI(currentTime);
            Handles.DrawLine(new Vector3(xPos, -20, 0), new Vector3(xPos, rect.height + _expandView.y, 0));
            Handles.color = Color.white;
        }

        private int count = 0;
        private void ProcessEvents(Rect rect)
        {
            if (!canClickTimeline) return;

            var e = Event.current;
            if (e.mousePosition.x < 0 || e.mousePosition.x > rect.width) return;
            switch (e.rawType)
            {
                case EventType.MouseDown:
                    currentTime = GUIToSeconds(e.mousePosition.x);
                    currentTime = Mathf.Clamp(currentTime, 0, float.MaxValue);
                    TimelineClick?.Invoke(currentTime);
                    e.Use();
                    break;
                case EventType.MouseDrag:
                    currentTime = GUIToSeconds(e.mousePosition.x);
                    currentTime = Mathf.Clamp(currentTime, 0, float.MaxValue);
                    TimelineClick?.Invoke(currentTime);
                    e.Use();
                    break;
                case EventType.ScrollWheel:
                    zoom -= e.delta.y;
                    zoom = Mathf.Clamp(zoom, 10, 100);
                    if (Math.Abs(zoom - 10) < 0.1f || Math.Abs(zoom - 100) < 0.1f)
                    {
                        timeFactorIndex += (int) (e.delta.y / 3);
                    }

                    timeFactorIndex = Mathf.Clamp(timeFactorIndex, 0, timeFactors.Length - 1);

                    e.Use();
                    break;
            }
        }
        #region Helpers

        public float SecondsToGUI(float seconds)
        {
            var guiSecond = (seconds / timeFactors[timeFactorIndex]) * zoom * _zoomFactor * 5.0f;
            return guiSecond;
        }

        public float GUIToSeconds(float x)
        {
            var guiSecond = zoom * _zoomFactor * 5.0f / timeFactors[timeFactorIndex];
            var res = (x) / guiSecond;
            return res;
        }

        #endregion
    }
}
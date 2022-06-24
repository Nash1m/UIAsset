using UnityEngine;
using UnityEditor;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class MoveTween : ITween
    {
        public bool fixedStart = true;
        public Vector2 startPosition;
        public Vector2 endPosition;
        public bool anchoredPosition;

        public string bindingKey;
        public string BindingKey
        {
            get => bindingKey;
            set => bindingKey = value;
        }

        public void UpdateTween(float time, UIAnimator animator, TweenNode tweenNode)
        {
            RectTransform rectTransform = null;
            var binding = animator.GetBindingByKey(BindingKey);
            if (binding is { } && binding.bindObject.rectTransform)
                rectTransform = binding.bindObject.rectTransform;
            if(rectTransform is null) return;
            
            Vector2 position;
            if (fixedStart)
            {
                position = Vector3.Lerp(startPosition, endPosition, time);
            }
            else
            {
                var current = anchoredPosition? (Vector3)rectTransform.anchoredPosition : rectTransform.position;
                
                var distance = ((Vector2)current - endPosition).magnitude;
                var remainedTime = (1 - time) * tweenNode.Duration;
                var speed = distance / remainedTime;
                position = Vector2.MoveTowards(current, endPosition, speed * Time.deltaTime);
            }

            if (anchoredPosition)
                rectTransform.anchoredPosition = position;
            else
                rectTransform.position = position;
        }
        
        public void Draw()
        {
            anchoredPosition = EditorGUILayout.Toggle("Anchored position", anchoredPosition);
            fixedStart = EditorGUILayout.Toggle("Fixed Start", fixedStart);
            
            if(fixedStart)
                startPosition = EditorGUILayout.Vector2Field("Start position", startPosition);
            endPosition = EditorGUILayout.Vector2Field("End position", endPosition);
        }
    }
}

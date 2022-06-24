using UnityEditor;
using UnityEngine;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class FadeTween : ITween
    {
        public float startAlpha;
        public float endAlpha;
        
        public string bindingKey;
        public string BindingKey
        {
            get => bindingKey;
            set => bindingKey = value;
        }
        
        public void UpdateTween(float time, UIAnimator animator, TweenNode tweenNode)
        {
            var binding = animator.GetBindingByKey(BindingKey);
            if (binding is {} && binding.bindObject.canvasGroup)
                binding.bindObject.canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time);
        }
        
        public void Draw()
        {
            startAlpha = EditorGUILayout.FloatField("Start Alpha", startAlpha);
            endAlpha = EditorGUILayout.FloatField("End Alpha", endAlpha);
        }
    }
}
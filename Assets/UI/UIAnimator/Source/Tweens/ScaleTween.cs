using UnityEditor;
using UnityEngine;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class ScaleTween : ITween
    {
        public Vector3 startScale = Vector3.one;
        public Vector3 endScale = Vector3.one;
        
        public string bindingKey;
        public string BindingKey
        {
            get => bindingKey;
            set => bindingKey = value;
        }

        public void UpdateTween(float time, UIAnimator animator, TweenNode tweenNode)
        {
            var binding = animator.GetBindingByKey(BindingKey);
            if (binding is {} && binding.bindObject.rectTransform)
                binding.bindObject.rectTransform.localScale = Vector3.Lerp(startScale, endScale, time);
        }

        public void Draw()
        {
            startScale = EditorGUILayout.Vector3Field("Start scale", startScale);
            endScale = EditorGUILayout.Vector3Field("End scale", endScale);
        }
    }
}
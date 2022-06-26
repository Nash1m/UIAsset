using UnityEditor;
using UnityEngine;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class ColorTween : ITween
    {
        public Color startColor;
        public Color endColor;

        public string bindingKey;

        public string BindingKey
        {
            get => bindingKey;
            set => bindingKey = value;
        }


        public void UpdateTween(float time, UIAnimator animator, TweenNode tweenNode)
        {
            var binding = animator.GetBindingByKey(BindingKey);
            if (binding is {} && binding.bindObject.graphic)
                binding.bindObject.graphic.color = Color.Lerp(startColor, endColor, time);
        }

        public void Draw()
        {
            startColor = EditorGUILayout.ColorField("Start Color", startColor);
            endColor = EditorGUILayout.ColorField("End Color", endColor);
        }
    }
}
using UnityEngine;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class MousePositionTween : ITween
    {
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
                binding.bindObject.rectTransform.position = Input.mousePosition;
        }

        public void Draw(){}
    }
}
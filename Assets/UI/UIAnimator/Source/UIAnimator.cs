using System.Collections.Generic;
using System.Linq;
using Nash1m.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Nash1m.UI.Animator
{
    public class UIAnimator : MonoBehaviour
    {
        public delegate void AnimationEndCallback();
        public AnimationEndCallback onAnimationEnd;

        public List<UIAnimation> animations = new List<UIAnimation>();
        public List<Binding> animationBindings = new List<Binding>();

        public bool IsPlaying { get; private set; }
        public float CurrentTime { get; private set; }
        public UIAnimation CurrentAnimation { get; private set; }

        private void Update()
        {
            if (!IsPlaying) return;

            CurrentTime += Time.deltaTime;
            UpdateAnimation();
        }
        private void UpdateAnimation()
        {
            var animationTime = GetAnimationTime(CurrentTime, CurrentAnimation);
            CurrentAnimation.UpdateAnimation(animationTime);
        }
        public void OnCurrentAnimationEnd()
        {
            IsPlaying = false;
            onAnimationEnd?.Invoke();
            onAnimationEnd = null;
        }

        public void Play(string animationKey)
        {
            var animationByKey = GetAnimationByKey(animationKey);
            if (animationByKey is null) return;

            onAnimationEnd = null;
            CurrentTime = 0;
            CurrentAnimation = animationByKey;
            IsPlaying = true;
            UpdateAnimation();
        }
        public void Stop(string animationKey)
        {
        }

        
        public UIAnimation GetAnimationByKey(string key)
        {
            return animations.FirstOrDefault(x => x.key == key);
        }
        public Binding AddBindingObject(GameObject go)
        {
            var binding = GetBindingByTargetObject(go);
            if (binding == null)
            {
                binding = new Binding {bindObject = new Binding.BindingObject(go), key = go.name};
                animationBindings.Add(binding);
            }

            return binding;
        }
        public Binding GetBindingByKey(string key)
        {
            return animationBindings.FirstOrDefault(x => x.key == key);
        }
        public Binding GetBindingByTargetObject(GameObject go)
        {
            return animationBindings.FirstOrDefault(x => x.bindObject.gameObject == go);
        }

        public static float GetAnimationTime(float _time, UIAnimation animation)
        {
            var animationTime = _time;
            
            if (animation.animationType == AnimationType.PingPong)
                animationTime = _time.PingPong(0, animation.Duration);
            else if (animation.animationType == AnimationType.Loop)
                animationTime = _time.Loop(0, animation.Duration);

            animationTime = Mathf.Clamp(animationTime, 0, animation.Duration);

            return animationTime;
        }
    }

    [System.Serializable]
    public class Binding
    {
        public string key;
        public BindingObject bindObject;
        
        
        [System.Serializable]
        public class BindingObject
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public CanvasGroup canvasGroup;
            public Graphic graphic;

            public BindingObject(GameObject go)
            {
                gameObject = go;
                rectTransform = go.GetComponent<RectTransform>();
                canvasGroup = go.GetComponent<CanvasGroup>();
                graphic = go.GetComponent<Graphic>();
            }
        }
    }
}
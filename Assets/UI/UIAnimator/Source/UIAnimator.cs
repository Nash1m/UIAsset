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


        private bool _isPlaying = false;
        private UIAnimation _currentAnimation;
        private float _time = 0;

        public bool IsPlaying => _isPlaying;
        public float CurrentTime => _time;
        public UIAnimation CurrentAnimation => _currentAnimation;

        private void Update()
        {
            if (!_isPlaying) return;

            _time += Time.deltaTime;
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            var animationTime = GetAnimationTime(_time, _currentAnimation);
            _currentAnimation.UpdateAnimation(animationTime);
        }

        public void OnCurrentAnimationEnd()
        {
            _isPlaying = false;
            onAnimationEnd?.Invoke();
            onAnimationEnd = null;
        }

        public void Play(string animationKey)
        {
            var animationByKey = GetAnimationByKey(animationKey);
            if (animationByKey is null) return;

            onAnimationEnd = null;
            _currentAnimation = animationByKey;
            _time = 0;
            UpdateAnimation();
            _isPlaying = true;
        }
        public void Stop(string animationKey)
        {
        }
        public void StopAll()
        {
        }


        private UIAnimation GetAnimationByKey(string key)
        {
            return animations.FirstOrDefault(x => x.key == key);
        }
        public Binding GetBindingByKey(string key)
        {
            return animationBindings.FirstOrDefault(x => x.key == key);
        }
        private Binding GetBindingByTargetObject(GameObject go)
        {
            return animationBindings.FirstOrDefault(x => x.bindObject.gameObject == go);
        }
        public Binding AddBindingObject(GameObject go)
        {
            var binding = GetBindingByTargetObject(go);
            if (binding == null)
            {
                binding = new Binding() {bindObject = new Binding.BindingObject(go), key = go.name};
                animationBindings.Add(binding);
            }

            return binding;
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
            public Image image;
            public Text text;

            public BindingObject(GameObject go)
            {
                gameObject = go;
                rectTransform = go.GetComponent<RectTransform>();
                canvasGroup = go.GetComponent<CanvasGroup>();
                image = go.GetComponent<Image>();
                text = go.GetComponent<Text>();
            }
        }
    }
}
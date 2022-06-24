using System.Collections.Generic;
using System.Linq;
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
        private bool _timeReversed = false;
        private float _time = 0;

        public bool IsPlaying => _isPlaying;
        public float CurrentTime => _time;
        public UIAnimation CurrentAnimation => _currentAnimation;

        private void Update()
        {
            if (!_isPlaying) return;

            FlowTime(ref _time,ref _timeReversed, _currentAnimation);

            _currentAnimation.UpdateAnimation(_time);
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
            _timeReversed = false;
            _time = 0;
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

        public static void FlowTime(ref float time, ref bool reverseTime, UIAnimation animation)
        {
            time += Time.deltaTime * (reverseTime ? -1 : 1);
            
            if (animation.animationType == AnimationType.PingPong)
            {
                if (time >= animation.Duration && !reverseTime)
                    reverseTime = true;
                else if (time <= 0 && reverseTime)
                    reverseTime = false;
            }
            else if (animation.animationType == AnimationType.Loop)
            {
                if (time >= animation.Duration)
                    time = 0;
            }
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
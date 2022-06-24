using System.Collections.Generic;
using System.Linq;
using Nash1m.Extensions;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class UIAnimation
    {
        public string key;
        public UIAnimator animator;

        public AnimationType animationType;

        //List of tween nodes
        public List<TweenNode> tweenNodes = new List<TweenNode>();

        public float Duration => tweenNodes.Count > 0 ? tweenNodes.Max(x => x.endTime) : 0;

        public void UpdateAnimation(float animationTime)
        {
            foreach (var tweenNode in tweenNodes)
            {
                var current = animationTime - tweenNode.startTime;
                tweenNode.tween.UpdateTween(current.Normalized(tweenNode.startTime, tweenNode.endTime), animator, tweenNode);
            }

            if (animationTime >= Duration && animationType == AnimationType.Single)
                animator.OnCurrentAnimationEnd();
        }
    }
    
    public enum AnimationType
    {
        Single,
        PingPong,
        Loop
    }
}
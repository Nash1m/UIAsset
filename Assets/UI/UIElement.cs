using Nash1m.UI.Animator;
using UnityEngine;

namespace Nash1m.UI
{
    public class UIElement : MonoBehaviour
    {
        public UIAnimator animator;

        public void SetActive(bool value)
        {
            animator ??= GetComponent<UIAnimator>();
            
            if (animator)
            {
                if(value)
                {
                    animator.Play("Show");
                    gameObject.SetActive(true);
                }
                else
                {
                    animator.Play("Hide");
                    animator.onAnimationEnd = () => gameObject.SetActive(false);
                }
            }
            else
                gameObject.SetActive(value);
        }
    }
}
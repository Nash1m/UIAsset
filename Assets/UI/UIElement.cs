using Nash1m.UI.Animator;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nash1m.UI
{
    public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private UIAnimator animator;

        private void Start()
        {
            animator ??= GetComponent<UIAnimator>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            animator.Play("Hover");
            animator.onAnimationEnd = () => animator.Play("Hovering");
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            animator.Play("Unhover");
        }
        
        public void SetActive(bool value)
        {
            if (animator)
            {
                if(value)
                {
                    gameObject.SetActive(true);
                    animator.Play("Show");
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
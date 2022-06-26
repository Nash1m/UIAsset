namespace Nash1m.UI.ScreenManager
{
    public abstract class Screen : UIElement
    {
        private bool transitioningIn, transitioningOut;
        public bool IsTransitioning => transitioningIn || transitioningOut;
        
        

        protected override void OnActiveChanged(bool value)
        {
            base.OnActiveChanged(value);
            
            if(animator)
                animator.onAnimationEnd += OnActivationAnimationEnd;
            
            transitioningIn = value;
            transitioningOut = !value;
        }
        private void OnActivationAnimationEnd()
        {
            animator.onAnimationEnd -= OnActivationAnimationEnd;
            transitioningIn = false;
            transitioningOut = false;
        }


        public virtual void OnShow()
        {
        }
        public virtual void OnHide()
        {
        }
    }
}
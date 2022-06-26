namespace Nash1m.UI.Animator
{
    public interface ITween
    {
        string BindingKey { get; set; }
        void UpdateTween(float time, UIAnimator animator, TweenNode tweenNode);
        void Draw();
    }
}
using UnityEngine;

namespace Nash1m.UI.Animator
{
    [System.Serializable]
    public class TweenNode: ISerializationCallbackReceiver
    {
        public float startTime;
        public float endTime;

        public int channel;

        public ITween tween;
        public Color nodeColor;

        [SerializeField] private string tweenType;
        [SerializeField] private string tweenData;

        public float Duration => endTime - startTime;

        public TweenNode(ITween tweenArg)
        {
            tween = tweenArg;
            tweenType = tween.GetType().ToString();
            nodeColor = Random.ColorHSV(0, 1,0.5f, 1, 0.8f,1);
            
            startTime = 0;
            endTime = 10;
        }
        
        public void OnBeforeSerialize()
        {
            tweenType = tween.GetType().ToString();
            tweenData = JsonUtility.ToJson(tween);
        }
        public void OnAfterDeserialize()
        {
            tween = JsonUtility.FromJson(tweenData, System.Type.GetType(tweenType)) as ITween;
        }
        
        public override string ToString() => $"Start Time: {startTime}, End Time: {endTime}, Channel: {channel}";
    }
}

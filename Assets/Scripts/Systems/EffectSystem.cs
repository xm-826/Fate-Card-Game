using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerFormer<PerformEffectGA>();  
    }
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        // 1. 从命令中取出 Effect
        // 2. 调用 Effect.GetGameAction() → 得到真正的命令（比如 DrawCardGA）
        // 3. 把真正的命令加入到执行队列
        GameAction effectAction = performEffectGA.Effect.GetGameAction();
        ActionSystem.Instance.AddReaction(effectAction);
        yield return null;
    }
   
}

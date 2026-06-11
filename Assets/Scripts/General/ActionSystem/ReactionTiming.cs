using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 反应时机枚举 —— 决定订阅者被回调的时间点。
///
/// <para>用于 <see cref="ActionSystem.SubscribeReaction{T}"/> 和
/// <see cref="ActionSystem.UnsubscribeReaction{T}"/> 的 timing 参数。</para>
///
/// <code>
/// // 订阅"在任何攻击动作发生之前"的通知
/// ActionSystem.SubscribeReaction&lt;AttackAction&gt;(OnBeforeAttack, ReactionTiming.PRE);
///
/// // 订阅"在任何攻击动作完成之后"的通知
/// ActionSystem.SubscribeReaction&lt;AttackAction&gt;(OnAfterAttack, ReactionTiming.POST);
/// </code>
/// </summary>
public enum ReactionTiming
{
    /// <summary>
    /// 前置时机 —— 在动作的 Perform 执行<b>之前</b>回调。
    /// 典型用途：数据校验、条件拦截、战前准备。
    /// </summary>
    PRE,

    /// <summary>
    /// 后置时机 —— 在动作的 Perform 执行<b>之后</b>回调。
    /// 典型用途：结果处理、成就判定、日志记录。
    /// </summary>
    POST
}

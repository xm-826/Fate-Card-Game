using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏动作的抽象基类。
/// 所有具体的游戏动作（如攻击、移动、使用技能等）都应该继承此类。
///
/// <para>架构说明：</para>
/// 每个 GameAction 携带三个「反应列表」，形成一个树状执行结构：
/// <list type="number">
///   <item><b>PreReactions（前反应）</b>：在主逻辑执行<b>之前</b>触发</item>
///   <item><b>PerformReactions（执行反应）</b>：与主逻辑<b>同时</b>执行</item>
///   <item><b>PostReactions（后反应）</b>：在主逻辑执行<b>之后</b>触发</item>
/// </list>
///
/// <para>使用方式：</para>
/// <code>
/// // 1. 定义自己的动作类
/// public class AttackAction : GameAction
/// {
///     public int damage;
///     public GameObject target;
/// }
///
/// // 2. 添加反应（在其他系统中，通过 ActionSystem.AddReaction）
/// var attack = new AttackAction { damage = 10, target = enemy };
/// attack.PreReactions.Add(new BuffCheckAction());    // 攻击前检查buff
/// attack.PostReactions.Add(new DamageDisplayAction()); // 攻击后显示伤害数字
/// </code>
/// </summary>
public abstract class GameAction
{
    /// <summary>
    /// 前反应列表 —— 在此动作的 Perform 执行<b>之前</b>，这些反应会按顺序执行。
    /// 典型用途：前置条件检查、消耗资源、播放前摇动画等。
    /// </summary>
    public List<GameAction> PreReactions { get; private set; } = new();

    /// <summary>
    /// 执行反应列表 —— 在此动作的 Perform 执行<b>之后</b>立即执行。
    /// 典型用途：与主逻辑紧密相关的副作用（如伤害计算后的护盾吸收）。
    /// </summary>
    public List<GameAction> PerformReactions { get; private set; } = new();

    /// <summary>
    /// 后反应列表 —— 在 Perform 和 PerformReactions 全部完成后执行。
    /// 典型用途：收尾工作（播放特效、触发剧情、记录日志、通知UI更新等）。
    /// </summary>
    public List<GameAction> PostReactions { get; private set; } = new();
}

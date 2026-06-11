using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动作系统 —— 整个框架的核心引擎，负责调度和执行所有 GameAction。
///
/// <para><b>设计模式：</b>单例 + 响应式管道</para>
///
/// <para><b>核心概念：</b></para>
/// <list type="bullet">
///   <item><b>GameAction（游戏动作）</b>：要执行的操作，如攻击、移动、使用技能。
///       每个动作携带三个反应列表（Pre / Perform / Post），形成树状执行结构。</item>
///   <item><b>Reaction（反应）</b>：附加到某个动作上的子动作。当父动作执行到某个阶段时，
///       子动作会递归地经历自己的完整 Flow（Pre → Perform → Post）。</item>
///   <item><b>Subscriber（订阅者）</b>：外部系统通过 SubscribeReaction 注册的回调。
///       在指定类型动作的 Pre 或 Post 阶段被通知，适合做跨系统联动（如成就系统监听击杀动作）。</item>
///   <item><b>Performer（执行者）</b>：动作的"实际干活"逻辑，通过 AttachPerformer 注册。
///       每个动作类型只能有一个 Performer，它在 Perform 阶段被调用。</item>
/// </list>
///
/// <para><b>执行流程图：</b></para>
/// <code>
/// PerForm(action)
///   └─ Flow(action)
///        ├─ [Pre阶段]  通知订阅者 → 递归执行 PreReactions
///        ├─ [Perform阶段] 执行 Performer → 递归执行 PerformReactions
///        └─ [Post阶段] 通知订阅者 → 递归执行 PostReactions
/// </code>
///
/// <para><b>典型使用示例：</b></para>
/// <code>
/// // 1. 注册动作的执行逻辑
/// ActionSystem.AttachPerformer&lt;AttackAction&gt;(attack =>
/// {
///     // 实际执行攻击的协程
///     target.TakeDamage(attack.damage);
///     yield return new WaitForSeconds(0.5f);
/// });
///
/// // 2. 订阅动作的前/后通知（用于跨系统联动）
/// ActionSystem.SubscribeReaction&lt;AttackAction&gt;(
///     attack => Debug.Log($"攻击前！伤害={attack.damage}"),
///     ReactionTiming.PRE
/// );
///
/// // 3. 创建并执行动作
/// var attack = new AttackAction { damage = 10, target = enemy };
/// attack.PreReactions.Add(new CheckBuffAction());  // 攻击前检查buff
/// attack.PostReactions.Add(new ShowDamageAction()); // 攻击后显示伤害
/// ActionSystem.Instance.PerForm(attack);
/// </code>
/// </summary>
public class ActionSystem : Singleton<ActionSystem>
{
    // ============================================================
    // 实例字段
    // ============================================================

    /// <summary>
    /// 当前正在处理的反应列表引用。
    /// 在 Flow 的不同阶段，它分别指向 action.PreReactions / PerformReactions / PostReactions。
    /// 外部可通过 <see cref="AddReaction"/> 向此列表动态添加反应。
    /// </summary>
    private List<GameAction> reactions = null;

    /// <summary>
    /// 是否正在执行动作流程。
    /// 这是一个简单的防重入锁 —— 如果已有动作在执行中，新的 PerForm 调用会被忽略。
    /// </summary>
    public bool IsPerforming { get; private set; } = false;

    // ============================================================
    // 静态注册表（全局共享，跨所有 ActionSystem 实例生效）
    // ============================================================

    /// <summary>
    /// Pre 阶段订阅者映射表。
    /// Key = 动作类型（如 typeof(AttackAction)），Value = 需要通知的回调列表。
    /// 在对应类型动作的 Pre 阶段被 <see cref="PerformSubscribers"/> 调用。
    /// </summary>
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();

    /// <summary>
    /// Post 阶段订阅者映射表。
    /// 结构同 preSubs，但在 Post 阶段被调用。
    /// </summary>
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();

    /// <summary>
    /// Performer 执行者映射表。
    /// Key = 动作类型，Value = 执行该动作主逻辑的协程函数。
    /// 每种动作类型最多只有一个 Performer，通过 <see cref="AttachPerformer{T}"/> 注册。
    /// </summary>
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    /// <summary>
    /// 包装委托映射表 —— 用于取消订阅时找到对应的包装实例。
    ///
    /// <para>为什么需要这个？</para>
    /// SubscribeReaction 接受的是泛型委托 Action&lt;T&gt;（如 Action&lt;AttackAction&gt;），
    /// 但内部存储的是 Action&lt;GameAction&gt;。所以需要用闭包做一层包装。
    /// 当用户要取消订阅时，传入原始 Action&lt;T&gt;，我们通过此映射表找到
    /// 对应的包装委托，再从订阅列表中移除。
    /// </summary>
    private static Dictionary<Delegate, Action<GameAction>> wrapperMap = new();

    // ============================================================
    // 公共入口
    // ============================================================

    /// <summary>
    /// 执行一个游戏动作，启动完整的 Pre → Perform → Post 流程。
    ///
    /// <para>如果当前已有动作在执行中（<see cref="IsPerforming"/> == true），
    /// 此调用会被忽略。这是为了防止动作嵌套导致的不可预期行为。</para>
    ///
    /// <para>流程是异步的（通过协程），方法本身会立即返回。</para>
    /// </summary>
    /// <param name="action">要执行的游戏动作，不能为 null</param>
    /// <param name="OnPerformFinished">可选回调，在整个动作流程（含所有子反应）全部完成后调用</param>
    public void PerForm(GameAction action, System.Action OnPerformFinished = null)
    {
        // 防重入：如果正在执行中，忽略新请求
        if (IsPerforming) return;
        IsPerforming = true;

        // 启动协程驱动整个流程
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    /// <summary>
    /// 向当前正在执行的动作添加一个反应。
    ///
    /// <para>这个方法通常在 Performer 或 Subscriber 回调中调用，
    /// 它会把 newAction 追加到当前 Flow 正在遍历的反应列表中。</para>
    ///
    /// <para>注意：如果在非执行期间调用（reactions 为 null），不会有任何效果。</para>
    /// </summary>
    /// <param name="gameAction">要添加的反应动作</param>
    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    // ============================================================
    // 核心流程引擎
    // ============================================================

    /// <summary>
    /// 动作流程的核心协程。
    ///
    /// <para><b>三个阶段：</b></para>
    /// <list type="number">
    ///   <item><b>Pre（前置）</b>：通知所有 Pre 订阅者 → 递归执行所有 PreReactions</item>
    ///   <item><b>Perform（执行）</b>：调用 Performer 执行主逻辑 → 递归执行所有 PerformReactions</item>
    ///   <item><b>Post（后置）</b>：通知所有 Post 订阅者 → 递归执行所有 PostReactions</item>
    /// </list>
    ///
    /// <para>每个阶段的 Reactions 执行都是<b>递归</b>的：
    /// 反应本身也是 GameAction，它们会再次进入完整的 Flow，
    /// 形成深度优先的树状遍历。</para>
    /// </summary>
    /// <param name="action">当前要处理的动作</param>
    /// <param name="OnFlowFinished">整个流程完成后的回调</param>
    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        // ---------- Pre 阶段 ----------
        // 将 reactions 指针切换到当前动作的前反应列表
        reactions = action.PreReactions;
        // 先通知所有订阅了此类动作 Pre 时机的监听者
        PerformSubscribers(action, preSubs);
        // 再递归执行列表中的每一个前反应
        yield return PerformReactions();

        // ---------- Perform 阶段 ----------
        // 切换到执行反应列表
        reactions = action.PerformReactions;
        // 先执行此动作的主逻辑（Performer）
        yield return PerformPerformer(action);
        // 再递归执行列表中的每一个执行反应
        yield return PerformReactions();

        // ---------- Post 阶段 ----------
        // 切换到后反应列表
        reactions = action.PostReactions;
        // 先通知所有订阅了此类动作 Post 时机的监听者
        PerformSubscribers(action, postSubs);
        // 再递归执行列表中的每一个后反应
        yield return PerformReactions();

        // 三个阶段的递归全部完成，触发流程结束回调
        OnFlowFinished?.Invoke();
    }

    // ============================================================
    // 内部执行方法
    // ============================================================

    /// <summary>
    /// 执行动作的 Performer（主逻辑）。
    /// 根据动作的实际类型，从 <see cref="performers"/> 表中查找对应的协程函数并执行。
    /// 如果该类型没有注册 Performer，则什么都不做（直接跳过）。
    /// </summary>
    /// <param name="action">要执行主逻辑的动作</param>
    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.ContainsKey(type))
        {
            // 调用注册的协程函数，等待其完成
            yield return performers[type](action);
        }
    }

    /// <summary>
    /// 通知指定动作类型的所有订阅者。
    /// 从 subs 字典中查找订阅了该动作类型的回调列表，逐一调用。
    /// </summary>
    /// <param name="action">触发了订阅的动作实例</param>
    /// <param name="subs">订阅者字典（preSubs 或 postSubs）</param>
    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            foreach (var sub in subs[type])
            {
                // 直接调用回调，传入具体的 action 实例
                sub(action);
            }
        }
    }

    /// <summary>
    /// 递归执行当前 <see cref="reactions"/> 列表中的所有反应。
    ///
    /// <para>这是整个系统的递归核心：</para>
    /// 对列表中的每一个 reaction，调用 Flow(reaction)，而 Flow 内部又会
    /// 再次经历 Pre → Perform → Post 三个阶段，每个阶段又会再次调用此方法。
    /// 这样就形成了<b>深度优先的树状遍历</b>。
    /// </summary>
    private IEnumerator PerformReactions()
    {
        foreach (var reaction in reactions)
        {
            // 递归：每个反应都是完整的 GameAction，走完整的 Flow
            yield return Flow(reaction);
        }
    }

    // ============================================================
    // 公共静态 API —— Performer 注册 / 注销
    // ============================================================

    /// <summary>
    /// 为指定类型的 GameAction 注册执行者（Performer）。
    ///
    /// <para>Performer 是动作的"实际干活"逻辑。每种动作类型最多只能有一个 Performer，
    /// 重复注册会覆盖之前的。</para>
    ///
    /// <para>Performer 是一个协程函数（返回 IEnumerator），这意味着你可以：
    /// <list type="bullet">
    ///   <item>使用 yield return 实现延时、动画等待等</item>
    ///   <item>在协程中调用 <see cref="AddReaction"/> 动态添加反应</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="T">要注册的动作类型（必须继承自 GameAction）</typeparam>
    /// <param name="performer">
    /// 执行者协程函数，接受具体类型的 T 实例作为参数。
    /// 例如：<c>(AttackAction a) => { a.target.TakeDamage(a.damage); yield return null; }</c>
    /// </param>
    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        // 将泛型 Func<T, IEnumerator> 包装为 Func<GameAction, IEnumerator>
        // 以便统一存储在 Dictionary<Type, Func<GameAction, IEnumerator>> 中
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);

        if (performers.ContainsKey(type))
            performers[type] = wrappedPerformer;   // 覆盖已有
        else
            performers.Add(type, wrappedPerformer); // 新增
    }

    /// <summary>
    /// 注销指定类型的 Performer。
    /// 注销后，该类型的动作在执行时将跳过 Perform 阶段的主逻辑。
    /// </summary>
    /// <typeparam name="T">要注销的动作类型</typeparam>
    public static void DetachPerFormer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type))
            performers.Remove(type);
    }

    // ============================================================
    // 公共静态 API —— 反应订阅 / 取消订阅
    // ============================================================

    /// <summary>
    /// 订阅指定类型动作的 Pre 或 Post 通知。
    ///
    /// <para>这是实现<b>跨系统解耦联动</b>的关键机制。
    /// 比如成就系统不需要知道攻击系统的内部细节，只需要订阅 AttackAction 即可。</para>
    ///
    /// <para><b>与 Reaction 的区别：</b>
    /// <list type="bullet">
    ///   <item>Reaction（反应）：添加到具体 action 实例上的子动作，跟随该实例执行</item>
    ///   <item>Subscribe（订阅）：全局监听，任何该类型的动作触发时都会被通知，
    ///       但不直接参与执行树</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>使用示例：</b></para>
    /// <code>
    /// // 全局监听所有攻击动作完成后的事件
    /// ActionSystem.SubscribeReaction&lt;AttackAction&gt;(
    ///     attack => AchievementSystem.OnAttackComplete(attack),
    ///     ReactionTiming.POST
    /// );
    /// </code>
    /// </summary>
    /// <typeparam name="T">要订阅的动作类型</typeparam>
    /// <param name="reaction">回调函数，接收具体类型的动作实例</param>
    /// <param name="timing">订阅时机：PRE（动作执行前）或 POST（动作执行后）</param>
    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        // 根据 timing 选择对应的订阅者字典
        Dictionary<Type, List<Action<GameAction>>> subs =
            timing == ReactionTiming.PRE ? preSubs : postSubs;

        // 将泛型 Action<T> 包装为 Action<GameAction>，统一存储
        void wrappedReaction(GameAction action) => reaction((T)action);

        // 记录映射关系，以便后续取消订阅时能找到包装委托
        wrapperMap[reaction] = wrappedReaction;

        // 将包装委托加入对应类型的订阅者列表
        if (subs.ContainsKey(typeof(T)))
        {
            subs[typeof(T)].Add(wrappedReaction);
        }
        else
        {
            subs.Add(typeof(T), new List<Action<GameAction>>());
            subs[typeof(T)].Add(wrappedReaction);
        }
    }

    /// <summary>
    /// 取消之前通过 <see cref="SubscribeReaction{T}"/> 注册的订阅。
    ///
    /// <para>重要：传入的 reaction 必须与订阅时使用的<b>是同一个委托实例</b>，
    /// 否则无法找到对应的包装委托，取消订阅会静默失败。</para>
    ///
    /// <para>最佳实践：</para>
    /// <code>
    /// // 订阅时保存委托引用
    /// Action&lt;AttackAction&gt; handler = attack => Debug.Log("攻击了！");
    /// ActionSystem.SubscribeReaction(handler, ReactionTiming.POST);
    ///
    /// // 取消时使用同一个引用
    /// ActionSystem.UnsubscribeReaction(handler, ReactionTiming.POST);
    /// </code>
    /// </summary>
    /// <typeparam name="T">要取消订阅的动作类型</typeparam>
    /// <param name="reaction">与订阅时相同的委托实例</param>
    /// <param name="timing">与订阅时相同的时机</param>
    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        // 根据 timing 选择对应的字典
        Dictionary<Type, List<Action<GameAction>>> subs =
            timing == ReactionTiming.PRE ? preSubs : postSubs;

        // 通过 wrapperMap 找到之前包装的 Action<GameAction> 委托
        if (subs.ContainsKey(typeof(T)) && wrapperMap.TryGetValue(reaction, out var wrapped))
        {
            subs[typeof(T)].Remove(wrapped);
            wrapperMap.Remove(reaction);
        }
    }
}

//卡牌效果
[System.Serializable]
//一个翻译器,  数据 → 命令的转换器
public abstract class Effect
{
    //每个效果都会自己创建一个游戏动作。。还是有点不理解，
    // 好像就是效果会自己使用这个来创建游戏动作，继续吧。。。。
    public abstract GameAction GetGameAction();


}

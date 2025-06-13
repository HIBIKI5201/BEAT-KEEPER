namespace BeatKeeper
{
    /// <summary>
    /// SetVerticalMix(string name)の指定用列挙型
    /// </summary>
    public enum BGMLayerEnum
    {
        /// <summary>初期</summary>
        Base,
        
        /// <summary>リズム共鳴1・2段階</summary>
        Layer1,
        
        /// <summary>リズム共鳴3・4段階</summary>
        Layer2,
        
        /// <summary>リズム共鳴5・6段階</summary>
        Layer3,
        
        /// <summary>リズム共鳴7段階・フローゾーン</summary>
        Layer4,
    }
}

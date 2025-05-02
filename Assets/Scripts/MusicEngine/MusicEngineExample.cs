using System;
using BeatKeeper;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;

/// <summary>
/// MusicEngineのテストコード
/// </summary>
public class MusicEngineExample : MonoBehaviour
{
    private MusicEngineHelper _helper;
    
    private void Start()
    {
        _helper = ServiceLocator.GetInstance <MusicEngineHelper> ();
        
        // BGM再生
        Music.Play("MusicEngineCore");
        
        // テスト用メソッドの呼び出し
        RegisterEventHandlers();
        TestRepeatAction();
        TestTimingAction();
    }

    #region MusicEngineHelper - 音楽タイミングの取得/変更時のイベント

    /// <summary>
    /// イベントハンドラの登録
    /// </summary>
    private void RegisterEventHandlers()
    {
        _helper.OnJustChangedBar += OnBarChanged;
        _helper.OnJustChangedBeat += OnBeatChanged;
        _helper.OnNearChangedBar += OnNearBarChanged;
        _helper.OnNearChangedBeat += OnNearBeatChanged;
    }
    
    /// <summary>
    /// イベントハンドラの登録解除
    /// </summary>
    private void UnregisterEventHandlers()
    {
        _helper.OnJustChangedBar -= OnBarChanged;
        _helper.OnJustChangedBeat -= OnBeatChanged;
        _helper.OnNearChangedBar -= OnNearBarChanged;
        _helper.OnNearChangedBeat -= OnNearBeatChanged;
    }
    
    private void OnBarChanged()
    {
        Debug.Log($"小節変更: {_helper.GetCurrentBarCount()}");
    }

    private void OnBeatChanged()
    {
        Debug.Log($"拍変更: {_helper.GetCurrentBarCount()}, {_helper.GetCurrentBeatCount()}");
    }

    private void OnNearBarChanged()
    {
        Debug.Log("小節変更が近づいています");
    }

    private void OnNearBeatChanged()
    {
        Debug.Log("拍変更が近づいています");
    }

    #endregion

    #region MusicEngineHelper - タイミングアクションの登録/解除

    /// <summary>
    /// 繰り返さないタイミングアクション
    /// </summary>
    private void TestTimingAction()
    {
        var timing = new TimingKey(3, 0, 0);
        var guid = _helper.RegisterTimingAction(timing, () => Debug.Log("TimingAction(3,0,0)")); // 登録
        _helper.ClearTimingActionsAt(timing); // 指定の拍の処理を全て削除
        _helper.UnregisterTimingAction(3, 0,0, guid); // 拍とIDを指定して、その処理だけ解除
    }

    /// <summary>
    /// 指定した小節ごとに繰り返すタイミングアクション
    /// </summary>
    private void TestRepeatAction()
    {
        // 引数は（繰り返し間隔, 開始タイミング, アクション, 繰り返し回数）
        var guid = _helper.RegisterBarCycleAction(1, 1, 0, 0, () => Debug.Log("RepeatAction　Timing: (0,0)"), 10);
        _helper.UnregisterRepeatAction(guid); // 繰り返し全てを解除
    }

    #endregion 
    
    private void OnDestroy()
    {
        UnregisterEventHandlers();
    }
}
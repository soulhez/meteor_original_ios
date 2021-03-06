﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.UI;

//for loadingui updateui
public interface LoadingUI
{
    void UpdateProgress(float percent);
}

public class LevelHelper : MonoBehaviour
{
	AsyncOperation mAsync;
    public struct LevelParam
    {
        public int id;
        public int gate;
    }

    public void Load()
    {
        StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        int displayProgress = 0;
        int toProgress = 0;
        yield return 0;
        Level lev = Global.Instance.GLevelItem;
        ResMng.LoadScene(lev.Scene);
        mAsync = SceneManager.LoadSceneAsync(lev.Scene);
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            toProgress = (int)mAsync.progress * 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                if (LoadingWnd.Exist)
                    LoadingWnd.Instance.UpdateProgress(displayProgress / 100.0f);
                yield return 0;
            }
            yield return 0;
        }
        toProgress = 100;
        //WSLog.LogInfo("displayProgress < toProgress");
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            if (LoadingWnd.Exist)
                LoadingWnd.Instance.UpdateProgress(displayProgress / 100.0f);
            yield return 0;
        }
        //WSLog.LogInfo("displayProgress < toProgress");
        mAsync.allowSceneActivation = true;
        yield return 0;
        //WSLog.LogInfo("OnLoadFinishedEx");
        OnLoadFinishedEx(lev);
        if (LoadingWnd.Exist)
            LoadingWnd.Instance.Close();
        Destroy(this);
    }

    LevelScriptBase GetLevelScript(string sn)
    {
        string typeIden = string.Format("LevelScript_{0}", sn);
        Type type = Type.GetType(typeIden);
        if (type == null)
        {
            //尝试在chapter的dll里加载
            if (System.IO.File.Exists(Global.Instance.Chapter.Dll))
            {
                Assembly ass = Assembly.Load(System.IO.File.ReadAllBytes(Global.Instance.Chapter.Dll));
                Type[] t = ass.GetTypes();
                for (int i = 0; i < t.Length; i++)
                {
                    if (t[i].Name == typeIden)
                    {
                        //LevelScriptBase l = System.Activator.CreateInstance(t[i]) as LevelScriptBase;
                        type = t[i];
                        break;
                    }
                }
            }
            if (type == null)
            {
                Log.WriteError(string.Format("Load sn failed {0}, Meteor Version:{1}", sn, AppInfo.Instance.AppVersion()));
                return null;
            }
        }
        Global.Instance.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    //只加载地图/地图物件.要令所有客户端初始化完毕后状态一致，然后用指令播放器，播放帧指令.
    //如果是单机，就使用帧播放
    void OnLoadFinishedEx(Level lev)
    {
        SoundManager.Instance.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.ID, lev.LevelScript));
            return;
        }

        Global.Instance.GScript = script;
        //加载场景配置数据
        SceneMng.Instance.OnEnterLevel();

        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(script);

        FrameReplay.Instance.OnBattleStart();
    }
}

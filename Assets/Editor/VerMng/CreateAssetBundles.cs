﻿using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public class CreateAssetBundle
{
    public static List<string> strBuilded = new List<string>();
    public static bool PackageAllPrefab(BuildTarget target, string newVersion)
	{
		string SavePath = "";
        //取得路径内含Resources的全部预设文件，依次打包
        string SelectPath = "Assets/";
        string []files = null;
        try
        {
            files = Directory.GetFiles(SelectPath, "*.prefab", SearchOption.AllDirectories);
        }
        catch (Exception exp)
        {
            WSLog.LogError(exp.Message);
        }

        strBuilded.Clear();
		int packagefile = 0;
		int unpackagefile = 0;
		foreach (string eachfile in files)
		{
            string file = eachfile.Replace('\\', '/');
			string path = file;
			packagefile++;
			path = SavePath + path;
			path = path.Substring(0, path.LastIndexOf('.'));
			path += ".assetbundle";
            if (strBuilded.Contains(file))
                continue;
            UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(file);
            //BuildPipeline.BuildAssetBundles();
            BuildPipeline.BuildAssetBundle(o, null, path, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies, target);
            strBuilded.Add(file);
            UnityEngine.Object[] depend = EditorUtility.CollectDependencies(new UnityEngine.Object[] { o });
            foreach (UnityEngine.Object inner in depend)
            {
                string str = AssetDatabase.GetAssetPath(inner);
                if (str.EndsWith(".cs") || str == "")
                    continue;
                if (str == file)
                    continue;
                if (inner != null)
                    PackageOnePrefab(target, inner);
            }
		}
        EditorUtility.DisplayDialog("Tip", "Package file : " + packagefile.ToString() + "\r\nunPackage file : " + unpackagefile.ToString(), "OK");
        AssetDatabase.Refresh();
		return true;
	}

    static void PackageOnePrefab(UnityEditor.BuildTarget target, UnityEngine.Object SelectObject)
    {
        string file = AssetDatabase.GetAssetPath(SelectObject);
		UnityEngine.Object []depend = EditorUtility.CollectDependencies(new UnityEngine.Object[]{SelectObject});
		string [] strdepend = AssetDatabase.GetDependencies(new string[]{file});
        file = file.Replace('\\', '/');
        string path = file;
        string name = "";
        int nNameBegin = path.LastIndexOf('/');
        int nNameEnd = path.LastIndexOf('.');
        name = path.Substring(nNameBegin + 1, nNameEnd - nNameBegin - 1);
		//SavePath = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target)+ "/";
        path = VerMng.SavePath + path;
        path = path.Substring(0, path.LastIndexOf('.'));
        path += ".assetbundle";
		//Directory.CreateDirectory(SavePath);
        //if (strBuilded.Contains(file))
        //    return;
		//BuildPipeline.BuildAssetBundle(SelectObject, null, SavePath, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.CollectDependencies, target);
    }
}
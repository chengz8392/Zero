﻿using Jing;
using System;
using System.Collections;
using UnityEngine;

namespace Zero
{
    /// <summary>
    /// 内嵌资源包解压
    /// </summary>
    public class PackageUpdate
    {
        public void Start(Action onComplete, Action<float, long> onProgress)
        {
            Log.CI(Log.COLOR_BLUE, "「PackageUpdate」内嵌资源解压检查...");
            ILBridge.Ins.StartCoroutine(Run(onComplete, onProgress));
        }

        IEnumerator Run(Action onComplete, Action<float, long> onProgress)
        {
            do
            {
                //检查程序是否第一次启动
                if (Runtime.Ins.localData.IsInit)
                {
                    break;
                }

                Runtime.Ins.localData.IsInit = true;

                //检查是否存在Package.zip
                string path =FileSystem.CombinePaths(ZeroConst.STREAMING_ASSETS_PATH , "Package.zip");
                WWW www = new WWW(path);
                while (false == www.isDone)
                {
                    onProgress(0f, 0);
                    yield return new WaitForEndOfFrame();
                }                

                //Package.zip不存在
                if (null != www.error)
                {
                    Log.I("解压[Package.zip]:{0}", www.error);
                    break;
                }

                //解压Zip
                ZipHelper zh = new ZipHelper();
                zh.UnZip(www.bytes, Runtime.Ins.localResDir);
                while (false == zh.isDone)
                {
                    Log.I("[Package.zip]解压进度:{0}%", zh.progress * 100);
                    onProgress(zh.progress, www.bytes.Length);                    
                    yield return new WaitForEndOfFrame();
                }
                www.Dispose();

                Log.I("[Package.zip]解压完成");

                //重新加载一次版本号文件，因为可能被覆盖了
                Runtime.Ins.localResVer.Load();
            } while (false);
            onComplete();
            yield break;
        }
    }
}

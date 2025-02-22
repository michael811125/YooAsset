﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    public sealed class UnloadUnusedAssetsOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            UnloadUnused,
            Done,
        }

        private readonly ResourceManager _resManager;
        private readonly int _loopCount;
        private ESteps _steps = ESteps.None;

        internal UnloadUnusedAssetsOperation(ResourceManager resourceManager, int loopCount)
        {
            _resManager = resourceManager;
            _loopCount = loopCount;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.UnloadUnused;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.UnloadUnused)
            {
                for (int i = 0; i < _loopCount; i++)
                {
                    LoopUnloadUnused();
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        /// <summary>
        /// 说明：资源包之间会有深层的依赖链表，需要多次迭代才可以在单帧内卸载！
        /// </summary>
        private void LoopUnloadUnused()
        {
            var removeList = new List<LoadBundleFileOperation>(_resManager.LoaderDic.Count);

            // 注意：优先销毁资源提供者
            foreach (var loader in _resManager.LoaderDic.Values)
            {
                loader.TryDestroyProviders();
            }

            // 获取销毁列表
            foreach (var loader in _resManager.LoaderDic.Values)
            {
                if (loader.CanDestroyLoader())
                {
                    removeList.Add(loader);
                }
            }

            // 销毁文件加载器
            foreach (var loader in removeList)
            {
                string bundleName = loader.LoadBundleInfo.Bundle.BundleName;
                loader.DestroyLoader();
                _resManager.LoaderDic.Remove(bundleName);
            }
        }
    }
}
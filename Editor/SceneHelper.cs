﻿/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GameCanvas.Editor
{
    public sealed class SceneHelper
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const string k_GameScenePath = "Assets/Game.unity";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static void OnLaunch()
        {
            if (EditorSettings.CurrentSettings.OpenGameSceneOnLaunchEditor)
            {
                foreach (var scene in EditorSceneManager.GetSceneManagerSetup())
                {
                    if (scene.path == k_GameScenePath) return;
                }

                EditorSceneManager.OpenScene(k_GameScenePath);
            }
        }

        #endregion
    }
}

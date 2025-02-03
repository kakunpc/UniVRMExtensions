#nullable enable
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using VRM;
using Esperecyan.UniVRMExtensions.CopyVRMSettingsComponents;
using SwayingObjectsConverterWizard = Esperecyan.UniVRMExtensions.SwayingObjects.Wizard;

namespace Esperecyan.UniVRMExtensions
{
    /// <summary>
    /// メニューアイテムへの追加とバリデート。
    /// </summary>
    public static class MenuItems
    {
        /// <summary>
        /// 当エディタ拡張の名称。
        /// </summary>
        internal const string Name = "UniVRMExtensions";

        /// <summary>
        /// 追加するメニューアイテムの、「VRM」メニュー内の位置。
        /// </summary>
        private const int Priority = 1100;

        /// <summary>
        /// 当エディタ拡張のバージョンを取得します。
        /// </summary>
        /// <returns></returns>
        public static Task<string> GetVersion()
        {
            var request = Client.List(offlineMode: true, includeIndirectDependencies: true);
            var taskCompleteSource = new TaskCompletionSource<string>();
            void Handler()
            {
                if (!request.IsCompleted)
                {
                    return;
                }

                EditorApplication.update -= Handler;

                taskCompleteSource.SetResult(
                    request.Result.FirstOrDefault(info => info.name == "jp.pokemori.univrm-extensions").version
                );
            }

            EditorApplication.update += Handler;

            return taskCompleteSource.Task;
        }

        [MenuItem("VRM0/プレハブバリアントを作ってVRMプレハブ化", false, MenuItems.Priority)]
        private static async void Initialize()
        {
            var gameObject = (GameObject)Selection.activeObject;
            var animator = gameObject.GetComponent<Animator>();
            if (animator == null || !animator.isHuman)
            {
                EditorUtility.DisplayDialog(
                    MenuItems.Name + "-" + await MenuItems.GetVersion(),
                    "HumanoidのAnimatorコンポーネントがアタッチされたGameObjectを選択した状態で実行してください。",
                    "OK"
                );
                return;
            }

            if (gameObject.GetComponent<VRMMeta>() != null)
            {
                EditorUtility.DisplayDialog(
                    MenuItems.Name + "-" + await MenuItems.GetVersion(),
                    "選択中のアバターはVRMプレハブです。",
                    "OK"
                );
                return;
            }

            var path = AssetUtility.CreatePrefabVariant(gameObject);

            VRMInitializer.Initialize(path);

            EditorUtility.DisplayDialog(
                MenuItems.Name + "-" + await MenuItems.GetVersion(),
                $"「{path}」へVRMプレハブを生成しました。",
                "OK"
            );
        }

        /// <summary>
        /// 選択されているオブジェクトがGameObject、かつサブアセットではなければ <c>true</c> を返します。
        /// </summary>
        /// <returns></returns>
        [MenuItem("VRM0/プレハブバリアントを作ってVRMプレハブ化", true)]
        private static bool ActiveObjectIsGameObject()
        {
            return Selection.activeObject is GameObject gameObject && !AssetDatabase.IsSubAsset(gameObject);
        }

        /// <summary>
        /// 選択されているアバターの変換ダイアログを開きます。
        /// </summary>
        [MenuItem("VRM0/Open CopyVRMSettings Wizard", false, MenuItems.Priority + 1)]
        private static void OpenWizard()
        {
            Wizard.Open();
        }

        /// <summary>
        /// 揺れ物の相互変換ダイアログを開きます。
        /// </summary>
        [MenuItem("VRM0/Open Swaying Objects Converter Wizard", false, MenuItems.Priority + 2)]
        private static void OpenSwayingObjectsConverterWizard()
        {
            SwayingObjectsConverterWizard.Open();
        }
    }
}

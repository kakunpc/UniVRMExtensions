using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF;
using VRM;

namespace Esperecyan.UniVRMExtensions.Utilities
{
    /// <summary>
    /// HumanoidボーンとTransformの対応関係。
    /// </summary>
    internal class BoneMapper
    {
        /// <summary>
        /// すべてのスケルトンボーンを取得します。
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        internal static Dictionary<HumanBodyBones, Transform> GetAllSkeletonBones(GameObject avatar)
        {
            var animator = avatar.GetComponent<Animator>();
            return avatar.GetComponent<VRMHumanoidDescription>().Description.human
                .Select(boneLimit => boneLimit.humanBone)
                .ToDictionary(
                    keySelector: humanoidBone => humanoidBone,
                    elementSelector: humanoidBone => animator.GetBoneTransform(humanoidBone)
                );
        }

        /// <summary>
        /// コピー元のアバターの指定ボーンと対応する、コピー先のアバターのボーンを返します。
        /// </summary>
        /// <param name="sourceBoneRelativePath"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceSkeletonBones"></param>
        /// <returns>見つからなかった場合は <c>null</c> を返します。</returns>
        internal static Transform FindCorrespondingBone(
            Transform sourceBone,
            GameObject source,
            GameObject destination,
            Dictionary<HumanBodyBones, Transform> sourceSkeletonBones
        )
        {
            if (!sourceBone.IsChildOf(source.transform))
            {
                return null;
            }

            string sourceBoneRelativePath = sourceBone.RelativePathFrom(root: source.transform);
            Transform destinationBone = destination.transform.Find(sourceBoneRelativePath);
            if (destinationBone)
            {
                return destinationBone;
            }

            if (!sourceBone.IsChildOf(source.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips)))
            {
                return null;
            }

            var humanoidAndSkeletonBone
                = BoneMapper.ClosestSkeletonBone(bone: sourceBone, skeletonBones: sourceSkeletonBones);
            Animator destinationAniamtor = destination.GetComponent<Animator>();
            Transform destinationSkeletonBone = destinationAniamtor.GetBoneTransform(humanoidAndSkeletonBone.Key);
            if (!destinationSkeletonBone)
            {
                return null;
            }

            destinationBone
                = destinationSkeletonBone.Find(sourceBone.RelativePathFrom(root: humanoidAndSkeletonBone.Value));
            if (destinationBone)
            {
                return destinationBone;
            }

            return destinationSkeletonBone.GetComponentsInChildren<Transform>()
                .FirstOrDefault(bone => bone.name == sourceBone.name);
        }

        /// <summary>
        /// 祖先方向へたどり、指定されたボーンを含む直近のスケルトンボーンを取得します。
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="avatar"></param>
        /// <param name="skeletonBones"></param>
        /// <returns></returns>
        private static KeyValuePair<HumanBodyBones, Transform> ClosestSkeletonBone(
            Transform bone,
            Dictionary<HumanBodyBones, Transform> skeletonBones
        )
        {
            foreach (Transform parent in bone.Ancestors())
            {
                if (!skeletonBones.ContainsValue(parent))
                {
                    continue;
                }

                return skeletonBones
                    .FirstOrDefault(predicate: humanoidAndSkeletonBone => humanoidAndSkeletonBone.Value == parent);
            }

            throw new ArgumentException();
        }
    }
}

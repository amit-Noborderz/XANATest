//with the exception of some minor additions, this is not the work of GameDevStudent. The script was downloaded from here: https://github.com/masterprompt/ModelStitching 

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using MagicaCloth2;

public class Stitcher
{
    /// <summary>
    /// Stitch clothing onto an avatar.  Both clothing and avatar must be instantiated however clothing may be destroyed after.
    /// </summary>
    /// <param name="sourceClothing"></param>
    /// <param name="targetAvatar"></param>
    /// <returns>Newly created clothing on avatar</returns>
    CharacterBodyParts body;

    public GameObject Stitch(GameObject sourceClothing, GameObject targetAvatar)
    {
        TransformCatalog boneCatalog = new TransformCatalog(targetAvatar.transform);
        SkinnedMeshRenderer[] skinnedMeshRenderers = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
        GameObject targetClothing = AddChild(sourceClothing, targetAvatar.transform);
        foreach (SkinnedMeshRenderer sourceRenderer in skinnedMeshRenderers)
        {
            SkinnedMeshRenderer targetRenderer = AddSkinnedMeshRenderer(sourceRenderer, targetClothing);
            targetRenderer.bones = TranslateTransforms(sourceRenderer.bones, boneCatalog);
        }
        return targetClothing;
    }

    #region Megica Cloths

    public GameObject Stitch_Vtuber_Megica(GameObject sourceClothing, GameObject targetAvatar)
    {
        //TransformCatalog boneCatalog = new TransformCatalog(targetAvatar.transform);
        //SkinnedMeshRenderer[] skinnedMeshRenderers = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
        //GameObject targetClothing = AddChild(sourceClothing, targetAvatar.transform);

        //foreach (SkinnedMeshRenderer sourceRenderer in skinnedMeshRenderers)
        //{
        //    SkinnedMeshRenderer targetRenderer = AddSkinnedMeshRenderer(sourceRenderer, targetClothing);
        //    targetRenderer.bones = TranslateTransforms_Vtuber(sourceRenderer.bones, boneCatalog, targetAvatar.transform);
        //}
        //return targetClothing;
        return Equip_Megica(sourceClothing, targetAvatar );
    }
    GameObject Equip_Megica(GameObject equipPrefab, GameObject targetAvatar)
    {
        TransformCatalog targetAvatarBoneMap = new TransformCatalog(targetAvatar.transform);
        Transform parentObj = targetAvatar.gameObject.GetComponent<AvatarController>().AvatarAndClothParent.transform;

        // Generate a prefab with cloth set up.
        var gobj = UnityEngine.Object.Instantiate(equipPrefab,parentObj);
        
        // All cloth components included in the prefab.
        var clothList = new List<MagicaCloth>(gobj.GetComponentsInChildren<MagicaCloth>());

        // All collider components included in the prefab.
        var colliderList = new List<ColliderComponent>(gobj.GetComponentsInChildren<ColliderComponent>());

        //　All renderers included in the prefab.
        var skinList = new List<SkinnedMeshRenderer>(gobj.GetComponentsInChildren<SkinnedMeshRenderer>());

        // First stop the automatic build that is executed with Start().
        // And just in case, it does some initialization called Awake().
        foreach (var cloth in clothList)
        {
            // Normally it is called with Awake(), but if the component is disabled, it will not be executed, so call it manually.
            // Ignored if already run with Awake().
            cloth.Initialize();

            // Turn off auto-build on Start().
            cloth.DisableAutoBuild();
        }

        // Swap the bones of the SkinnedMeshRenderer.
        // This process is a general dress-up process for SkinnedMeshRenderer.
        // Comment out this series of processes when performing this process with functions such as other assets.
        foreach (var sren in skinList)
        {
            if(!parentObj.gameObject.activeInHierarchy)
                sren.enabled = false;

            var bones = sren.bones;
            Transform[] newBones = new Transform[bones.Length];

            for (int i = 0; i < bones.Length; ++i)
            {
                Transform bone = bones[i];
                if (!targetAvatarBoneMap.TryGetValue(bone.name, out newBones[i]))
                {
                    // Is the bone the renderer itself?
                    if (bone.name == sren.name)
                    {
                        newBones[i] = sren.transform;
                    }
                    else
                    {
                        // bone not found
                        Debug.Log($"[SkinnedMeshRenderer({sren.name})] Unable to map bone [{bone.name}] to target skeleton.");
                    }
                }
            }
            sren.bones = newBones;

            // root bone
            if (targetAvatarBoneMap.ContainsKey(sren.rootBone?.name))
            {
                sren.rootBone = targetAvatarBoneMap[sren.rootBone.name];
            }
        }

        // Here, replace the bones used by the MagicaCloth component.
        foreach (var cloth in clothList)
        {
            // Replaces a component's transform.
            cloth.ReplaceTransform(targetAvatarBoneMap);
        }

        // Move all colliders to the new avatar.
        foreach (var collider in colliderList)
        {
            Transform parent = collider.transform.parent;
            if (parent && targetAvatarBoneMap.ContainsKey(parent.name))
            {
                Transform newParent = targetAvatarBoneMap[parent.name];

                // After changing the parent, you need to write back the local posture and align it.
                var localPosition = collider.transform.localPosition;
                var localRotation = collider.transform.localRotation;
                collider.transform.SetParent(newParent);
                collider.transform.localPosition = localPosition;
                collider.transform.localRotation = localRotation;
            }
        }

        // Finally let's start building the cloth component.
        foreach (var cloth in clothList)
        {
            // I disabled the automatic build, so I build it manually.
            cloth.BuildAndRun();
        }

        MoveHere(clothList[0].gameObject, targetAvatar);
        return clothList[0].gameObject;

        // Record information for release.
        //einfo.equipObject = gobj;
        //einfo.colliderList = colliderList;
    }
    void MoveHere(GameObject cloth, GameObject avatar)
    {
        GameObject oldParent = cloth.transform.parent.gameObject;
        cloth.transform.SetParent(avatar.transform);

        UnityEngine.Object.DestroyImmediate(oldParent);
    }
    private void CopyMegicaComponent(GameObject source, GameObject target)
    {
        var sourceDynamicBoneCollider = source.GetComponent<MagicaCloth>();
        if (sourceDynamicBoneCollider != null)
        {
            System.Type type = sourceDynamicBoneCollider.GetType();
            Component targetComponent = target.AddComponent(type);
            foreach (var field in type.GetFields())
            {
                field.SetValue(targetComponent, field.GetValue(sourceDynamicBoneCollider));
            }
        }
    }

    #endregion




    private GameObject AddChild(GameObject source, Transform parent)
    {
        GameObject target = new GameObject(source.name);
        //to resolve naked avatar cloths are instantiate inside a new created parent 
        target.transform.parent = parent.gameObject.GetComponent<AvatarController>().AvatarAndClothParent.transform;
        //target.transform.parent = parent;
        target.transform.localPosition = source.transform.localPosition;
        target.transform.localRotation = source.transform.localRotation;
        target.transform.localScale = source.transform.localScale;
        return target;
    }
    private SkinnedMeshRenderer AddSkinnedMeshRenderer(SkinnedMeshRenderer source, GameObject parent)
    {
        SkinnedMeshRenderer target = parent.AddComponent<SkinnedMeshRenderer>();
        target.sharedMesh = source.sharedMesh;
        target.materials = source.sharedMaterials;

        if(source.GetComponent<MagicaCloth>())
        {
            CopyMegicaComponent(target.gameObject, source.gameObject);
        }
        return target;
    }
    private Transform[] TranslateTransforms(Transform[] sources, TransformCatalog transformCatalog)
    {
        Transform[] targets = new Transform[sources.Length];
        for (int index = 0; index < sources.Length; index++)
        {
            targets[index] = DictionaryExtensions.Find(transformCatalog, sources[index].name);
            if (targets[index] == null)
            {
                Debug.LogWarning($"Transform '{sources[index].name}' not found in target avatar.");
            }
        }
        return targets;
    }
    private Transform FindOrCreateParentTransform(Transform sourceParent, TransformCatalog transformCatalog, Transform targetRoot)
    {
        if (sourceParent == null)
        {
            return targetRoot;
        }

        Transform targetParent = DictionaryExtensions.Find(transformCatalog, sourceParent.name);
        if (targetParent == null)
        {
            Debug.LogWarning($"Parent transform '{sourceParent.name}' not found. Creating a new parent transform.");
            GameObject newParent = new GameObject(sourceParent.name);
            newParent.transform.parent = FindOrCreateParentTransform(sourceParent.parent, transformCatalog, targetRoot);
            newParent.transform.localPosition = sourceParent.localPosition;
            newParent.transform.localRotation = sourceParent.localRotation;
            newParent.transform.localScale = sourceParent.localScale;
            targetParent = newParent.transform;
            transformCatalog.Add(newParent.name, newParent.transform); // Add the new parent to the catalog
        }

        return targetParent;
    }


    #region TransformCatalog
    private class TransformCatalog : Dictionary<string, Transform>
    {
        #region Constructors
        public TransformCatalog(Transform transform)
        {
            Catalog(transform);
        }
        #endregion

        #region Catalog
        private void Catalog(Transform transform)
        {
            if (ContainsKey(transform.name))
            {
                Remove(transform.name);
                Add(transform.name, transform);
            }
            else
                Add(transform.name, transform);
            foreach (Transform child in transform)
                Catalog(child);
        }
        #endregion
    }
    #endregion


    #region DictionaryExtensions
    private class DictionaryExtensions
    {
        public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
        {
            TValue value;
            source.TryGetValue(key, out value);
            return value;
        }
    }
    #endregion

}


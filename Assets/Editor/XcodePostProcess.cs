#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class XcodePostProcess
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            string targetGuid = proj.GetUnityMainTargetGuid();
            string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();

            // Add required frameworks
            proj.AddFrameworkToProject(targetGuid, "Photos.framework", false);
            proj.AddFrameworkToProject(targetGuid, "PhotosUI.framework", false);
            proj.AddFrameworkToProject(frameworkTargetGuid, "Photos.framework", false);
            proj.AddFrameworkToProject(frameworkTargetGuid, "PhotosUI.framework", false);

            // Add required libraries
            proj.AddFileToBuild(targetGuid, proj.AddFile("usr/lib/libc++.tbd", "libc++.tbd", PBXSourceTree.Sdk));

            // Write the changes to the project
            proj.WriteToFile(projPath);

            // Update Info.plist
            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // Add usage description for Photos
            rootDict.SetString("NSPhotoLibraryUsageDescription", "This app requires access to the photo library to share screenshots.");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "This app requires access to add photos to the photo library.");

            // Write the changes to the Info.plist
            plist.WriteToFile(plistPath);
        }
    }
}
#endif
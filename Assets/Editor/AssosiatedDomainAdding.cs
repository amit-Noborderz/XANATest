using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

using UnityEditor.Callbacks;
using UnityEditor;

public class AssosiatedDomainAdding
{
    static string[] associatedDomains;

    private static void OnProstProcessBuildIOS(string pathToBuiltProject)
    {
#if UNITY_IOS
        // Modify entitlements (as you're already doing)
        string projectPath = "/Unity-iPhone.xcodeproj/project.pbxproj";
        string targetName = "Unity-iPhone";
        string entitlementsFileName = "Unity-iPhone.entitlements";

        var entitlements = new ProjectCapabilityManager(pathToBuiltProject + projectPath, entitlementsFileName, targetName);
        entitlements.AddAssociatedDomains(new string[] { "applinks:unitytesting.page.link", "applinks:xanasummit.page.link", "applinks:xanapord.page.link" });
        entitlements.WriteToFile();

        // ? Modify Info.plist to change app name in system dialogs
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;
        rootDict.SetString("CFBundleDisplayName", "XANA 2.0"); // <-- This is what changes the name on popups and home screen

        plist.WriteToFile(plistPath);
#endif
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
            OnProstProcessBuildIOS(pathToBuiltProject);
    }
}

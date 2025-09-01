#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class ClipboardManagerGenerator : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
            GenerateClipboardManager();
        }
    }

    private void GenerateClipboardManager()
    {
        // Define the directory path for the iOS-specific plugin
        string directoryPath = Path.Combine(Application.dataPath, "Plugins/iOS");
        string filePath = Path.Combine(directoryPath, "ClipboardManager.m");

        // Ensure the directory exists, if not create it
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Content of the ClipboardManager.m file (corrected)
        string fileContent = @"
#import <UIKit/UIKit.h>

extern void CopyToClipboardiOS(const char* text);

void CopyToClipboardiOS(const char* text)
{
    NSString *textToCopy = [NSString stringWithUTF8String:text];
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    pasteboard.string = textToCopy;
}
";

        // Write the content to the ClipboardManager.m file
        File.WriteAllText(filePath, fileContent);

        // Refresh the Asset Database in Unity to recognize the new file
        AssetDatabase.Refresh();

        // Log message for the user to confirm the file generation
        Debug.Log("ClipboardManager.m has been generated at: " + filePath);
    }
}
#endif

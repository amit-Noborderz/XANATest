using UnityEngine;
using UnityEngine.UI;
using System;
using Paroxe.PdfRenderer;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using Aspose.Slides;
using Aspose.Slides.Export;
using DG.Tweening.Plugins.Core.PathCore;

public class FilePicker : MonoBehaviour
{
    public Button uploadButton;
    public GameObject ProgressParent;
    public GameObject MiniControllerParent;
    public Image UploadProgressBar;
    public TMPro.TextMeshProUGUI uploadText;
    public TMPro.TextMeshProUGUI uploadTextProgress;
    public MediaPresentationManager MediaPresentationManagerInstance;

    private string fileServerPath;
    private MediaType fileType;
    Coroutine DownloadCoroutine;

    public static FilePicker FilePickerInstance;
    private void Awake()
    {
        FilePickerInstance = this;
    }
    // iOS Native Plugin Methods
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _PickFile(string gameObjectName);
    
    [DllImport("__Internal")]
    private static extern bool _IsFilePickerSupported();
#endif

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        uploadButton.onClick.AddListener(OpenFileAndroid);
#endif
#if UNITY_IOS && !UNITY_EDITOR
uploadButton.onClick.AddListener(PickFile);
#endif
#if UNITY_EDITOR
        uploadButton.onClick.AddListener(PickFile);
#endif

    }

    void OpenFileAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        NativeGallery.GetMixedMediaFromGallery(SetFilePath, NativeGallery.MediaType.Pdf, "Select");
#endif
#if UNITY_IOS && !UNITY_EDITOR

#endif
    }


    public void PickFile()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (_IsFilePickerSupported())
        {
            UpdateStatus("Opening file picker...");
            _PickFile(gameObject.name);
        }
        else
        {
            UpdateStatus("File picker not supported on this device");
        }
#elif UNITY_EDITOR
        // For testing in editor
        string editorPath = UnityEditor.EditorUtility.OpenFilePanel("Select File", "", "");
        if (!string.IsNullOrEmpty(editorPath))
        {
            OnFileSelected(editorPath);
        }
#else
        UpdateStatus("File picker only supported on iOS");
#endif
    }

    // This method will be called from iOS native code
    public void OnFileSelected(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            UpdateStatus($"File selected: {filePath}");
            SetFilePath(filePath);
        }
        else
        {
            UpdateStatus("No file selected");
        }
    }

    public void OnFilePickerCancelled()
    {
        UpdateStatus("File selection cancelled");
    }

    private void UpdateStatus(string message)
    {
        Debug.LogError(message);
    }


    void SetFilePath(string path)
    {
        ShowFileUploadingPanel();
        //Debug.LogError("File path :- " + path);
        fileType = GetFileExtension(path);
        if (fileType == MediaType.Undefined)
        {
            uploadText.text = "File not supported.";
            return;
        }
        else
            uploadText.text = "Uploading file...";
        byte[] fileData = File.ReadAllBytes(path);

        DownloadCoroutine = StartCoroutine(UploadImageOnServer(fileData, (a) =>
          {
              MediaPresentationManagerInstance.Init(fileServerPath, fileType);
              //Debug.LogError("File uploaded"+fileServerPath);
          }));
    }

    FilePicker.MediaType GetFileExtension(string path)
    {
        if (string.IsNullOrEmpty(path))
            return (MediaType)0;

        string extension = System.IO.Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
            return (MediaType)0;

        if (extension[0] == '.')
        {
            if (extension.Length == 1)
                return (MediaType)0;

            extension = extension.Substring(1);
        }

        extension = extension.ToLowerInvariant();
        if (extension == "png" || extension == "jpg" || extension == "jpeg" || extension == "gif" || extension == "bmp" || extension == "tiff")
            return MediaType.Image;
        else if (extension == "mp4" || extension == "mov" || extension == "wav" || extension == "avi" || extension == "m4v")
            return MediaType.Video;
        else if (extension == "mp3" || extension == "aac" || extension == "flac")
            return MediaType.Audio;
        else if (extension == "pdf")
            return MediaType.PDF;
        else if (extension == "ppt")
            return MediaType.PPT;

        return MediaType.Undefined;
    }

    IEnumerator UploadImageOnServer(Byte[] imageData, Action<bool> CallBack)
    {
        WWWForm wWWForm = new WWWForm();
        wWWForm.AddBinaryData("file", imageData);

        UnityWebRequest unityWebRequest = UnityWebRequest.Post(ConstantsGod.API_BASEURL + ConstantsGod.UPLOADFILECLOUDIMAGE, wWWForm);
        unityWebRequest.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);
        //unityWebRequest.SetRequestHeader("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Nzk1OCwiaWF0IjoxNzU1ODYxMjI2LCJleHAiOjE3NTU5NDc2MjZ9.Rv0eaemwaD7N6sRO2tdtPEfGgAj68LQQuy9jw1QH9-U");
        UnityWebRequestAsyncOperation www = unityWebRequest.SendWebRequest();

        while (!www.isDone)
        {
            UpdateProgressBar(unityWebRequest.uploadProgress);
            yield return null;
        }
        UpdateProgressBar(1);
        ImageData data = new ImageData();
        data = JsonUtility.FromJson<ImageData>(unityWebRequest.downloadHandler.text);
        Debug.LogError(unityWebRequest.downloadHandler.text);
        fileServerPath = data.cdn_link;
        if (data.success == "true")
        {
            CallBack(true);
        }
        else
            CallBack(false);

        unityWebRequest.Dispose();

    }

    void ShowFileUploadingPanel()
    {
        MiniControllerParent.SetActive(true);
        ProgressParent.SetActive(true);
    }

    public void StopUploading()
    {
        if (DownloadCoroutine != null)
            StopCoroutine(DownloadCoroutine);
        ProgressParent.SetActive(false);
        MiniControllerParent.SetActive(false);
        UploadProgressBar.fillAmount = 0;
        uploadTextProgress.text = (0).ToString();
    }

    void UpdateProgressBar(float _progress)
    {
        uploadText.text = "Uploading file...";
        UploadProgressBar.fillAmount = _progress;
        uploadTextProgress.text = ((int)(_progress * 100)).ToString();

        if (_progress > .9)
        {
            ProgressParent.SetActive(false);
            UploadProgressBar.fillAmount = 0;
            uploadTextProgress.text = (0).ToString();
        }
    }

    //void OpenFile()
    //{
    //    string filePath = fileServerPath;
    //    if (!string.IsNullOrEmpty(filePath))
    //    {
    //        // Get the file extension
    //        string fileExtension = Path.GetExtension(filePath).ToLower();  // Convert to lowercase for consistency

    //        // Check the file extension and handle accordingly
    //        if (fileExtension == ".ppt" || fileExtension == ".pptx")
    //        {
    //            Debug.Log("PowerPoint file selected: " + filePath);
    //            ConvertPPTToPDF(filePath);
    //            // Handle PowerPoint file processing here
    //        }
    //        else if (fileExtension == ".pdf")
    //        {
    //            Debug.Log("PDF file selected: " + filePath);
    //            LoadPDF(filePath);
    //            // Handle PDF file processing here
    //        }
    //        else if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
    //        {
    //            Debug.Log("Image file selected: " + filePath);
    //            LoadImage(filePath);
    //            // Handle image file processing here
    //        }
    //        else if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
    //        {
    //            Debug.Log("Video file selected: " + filePath);
    //            MediaPresentationManager.PlayVideo(filePath);
    //            // Handle video file processing here
    //        }
    //        else
    //        {
    //            Debug.LogError("Unsupported file type selected: " + fileExtension);
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("No file selected.");
    //    }
    //    if (!string.IsNullOrEmpty(filePath))
    //    {
    //        Debug.Log("Selected File: " + filePath);
    //    }
    //}

    //void LoadPDF(string path)
    //{
    //    // Logic to load PPT here
    //    PDFViewer.FilePath = path;
    //    PDFViewer.gameObject.SetActive(true);
    //}


    //void LoadImage(string path)
    //{
    //    // Read the image file bytes
    //    byte[] imageBytes = File.ReadAllBytes(path);

    //    // Create a Texture2D from the image bytes
    //    Texture2D texture = new Texture2D(2, 2);
    //    texture.LoadImage(imageBytes);  // This will load the image from the byte array into the texture

    //    // Assign the texture to the RawImage component to display it
    //    DisplayImage.texture = texture;

    //    DisplayImage.gameObject.SetActive(true);
    //}

    //public void ConvertPPTToPDF(string pptFilePath)
    //{
    //    // Get the persistent data path for the application
    //    string persistentDataPath = Application.persistentDataPath;

    //    // Define the output PDF file path
    //    string outputPdfPath = Path.Combine(persistentDataPath, "converted_presentation.pdf");

    //    // Load the PowerPoint presentation
    //    Presentation presentation = new Presentation(pptFilePath);

    //    // Save the presentation as a PDF at the output path
    //    presentation.Save(outputPdfPath, SaveFormat.Pdf);

    //    // Log the saved PDF path (for testing)
    //    Debug.Log("PDF saved at: " + outputPdfPath);

    //    LoadPDF(outputPdfPath);
    //}

    public enum MediaType
    {
        Image,
        Video,
        PDF,
        PPT,
        Audio,
        Undefined
    }
}

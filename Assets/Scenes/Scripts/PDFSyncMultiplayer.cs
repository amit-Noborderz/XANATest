using ExitGames.Client.Photon;
using Paroxe.PdfRenderer;
using Photon.Pun;
using Photon.Realtime;
using System.Threading.Tasks;
using UnityEngine;
using static XANASummitDataContainer;


public class PDFSyncMultiplayer : MonoBehaviourPunCallbacks
{
    public int MediaIndex;
    public PDFViewer PDFViewer;

    private GameObject MutipleScreens;
    public XANASummitDataContainer XANASummitDataContainer;

    private float currentPage = 0;

    public async void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);

        if (!photonView.IsMine)
        {
            PDFViewer.m_Internal.ScrollRect.scrollSensitivity = 0;
        }
        MediaPresentationManager.LoadMediaAcrossAllPlayers += LoadPDFRPC;
        MediaPresentationManager.SyncAcrossAllPlayers += SyncPDFState;

        while (FilePicker.FilePickerInstance == null)
        {
            await Task.Delay(1000);
        }
        CheckForMutipleDisplays();
        gameObject.transform.localScale = FilePicker.FilePickerInstance.MediaPresentationManagerInstance.PdfViewerParent.transform.localScale;

    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        MediaPresentationManager.LoadMediaAcrossAllPlayers -= LoadPDFRPC;
        MediaPresentationManager.SyncAcrossAllPlayers -= SyncPDFState;
        BuilderEventManager.StopMediaSharing?.Invoke();
        
    }

    void CheckForMutipleDisplays()
    {
        MutipleScreens = GameObject.FindGameObjectWithTag("VideoDisplay");
    }

    public void LoadPDFUrl()
    {
        XANASummitDataContainer.DomeGeneralData domeGeneralData = XANASummitDataContainer.GetDomeData(ConstantsHolder.domeId);
        if (domeGeneralData != null)
        {
            if (domeGeneralData.domes_media_v2 != null && domeGeneralData.domes_media_v2.mediajson.Length > 0)
            {
                if (domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaType == "PDF")
                {
                    // Load the PDF document from the URL
                    PDFViewer.LoadDocumentFromWeb(domeGeneralData.domes_media_v2.mediajson[MediaIndex].mediaUrl);
                }
            }
        }
    }

    public async void LoadPDFUrl(string mediaUrl, FilePicker.MediaType mediaType)
    {
        PDFViewer.LoadDocumentFromWeb(mediaUrl);
        Wait:
        await Task.Delay(1000);
        if (PDFViewer.IsLoaded)
            await Task.Delay(1000);
        else
            goto Wait;
        SetPDFPageOnAllDisplays();
    }

    public void LoadPDFRPC(string mediaUrl, FilePicker.MediaType mediaType)
    {
        LoadPDFUrl(mediaUrl, mediaType);
    }

    public void SyncPDFState(FilePicker.MediaType _mediaType, float _currentVideoTime, bool _videoPlay, bool _videoPause, float _pageNumber, float _zoomValue)
    {
        if (_zoomValue != 0)
        {
            PDFViewer.ZoomFactor = PDFViewer.ZoomFactor * (1 + _zoomValue / 100);
        }
        PDFViewer.m_Internal.ScrollRect.verticalNormalizedPosition = _pageNumber;
        SetPDFPageOnAllDisplays();
    }

    void SetPDFPageOnAllDisplays()
    {
        if (MutipleScreens == null)
            return;
        PDFDocument pdfDocument = PDFViewer.Document;
        if (pdfDocument.IsValid)
        {
            int pageCount = pdfDocument.GetPageCount();

            PDFRenderer renderer = new PDFRenderer();
            Texture2D tex = renderer.RenderPageToTexture(pdfDocument.GetPage(PDFViewer.CurrentPageIndex % pageCount));
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 8;

            MutipleScreens.GetComponent<MeshRenderer>().material.mainTexture = tex;
        }
    }
}

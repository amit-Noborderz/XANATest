using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;

public class EnhancedPlayerVoiceItem : PlayerVoiceItem
{
    [Header("Voice Level")]
    public Slider voiceLevelSlider;
    public Image voiceLevelFill;
    public Color lowVolumeColor = Color.green;
    public Color highVolumeColor = Color.red;

    private Speaker playerSpeaker;
    private float sensitivity = 30f;

    public override void Initialize(Player playerData, VoicePlayerManager voiceManager)
    {
        base.Initialize(playerData, voiceManager);

        // Find speaker for this player
        UpdateSpeakerReference();
    }

    void UpdateSpeakerReference()
    {
        if (VoiceConnectionHelper.Instance != null && player != null)
        {
            playerSpeaker = VoiceConnectionHelper.Instance.GetSpeakerForPlayer(player.ActorNumber);
        }
    }

    void Update()
    {
        base.Update();

        // Update voice level indicator
        if (voiceLevelFill != null && playerSpeaker != null)
        {
            float voiceLevel=0;
            AudioSource audioSource = playerSpeaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // Calculate voice level from audio data
                voiceLevel = GetAudioLevel(audioSource);
                Debug.LogError("voice level :- "+voiceLevel);
                //voiceLevelSlider.value = Mathf.Lerp(voiceLevelSlider.value, voiceLevel, Time.deltaTime * 5f);
                voiceLevelFill.fillAmount= Mathf.Lerp(voiceLevelFill.fillAmount, voiceLevel, Time.deltaTime * 5f);
            }

            if (voiceLevelFill != null)
            {
                voiceLevelFill.color = Color.Lerp(lowVolumeColor, highVolumeColor, voiceLevel);
            }
        }
        else
        {
            //voiceLevelSlider.value = Mathf.Lerp(voiceLevelSlider.value, 0f, Time.deltaTime * 5f);
            voiceLevelFill.fillAmount = Mathf.Lerp(voiceLevelFill.fillAmount, 0f, Time.deltaTime * 5f);
        }

        // Update speaker reference if null
        if (playerSpeaker == null)
        {
            UpdateSpeakerReference();
        }
    }

    private float GetAudioLevel(AudioSource audioSource)
    {
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += Mathf.Abs(samples[i]);
        }

        float average = sum / samples.Length;
        return Mathf.Clamp01(average * sensitivity);
    }
}
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Tests for sound options functionality including On/Off button behavior
/// </summary>
public class SoundOptionsTests
{
    private GameObject soundManagerObj;
    private SoundManager soundManager;
    private GameObject musicManagerObj;
    private MusicManager musicManager;
    private GameObject audioListenerObj;

    [SetUp]
    public void Setup()
    {
        // Clear PlayerPrefs for consistent testing
        PlayerPrefs.DeleteAll();

        // Create AudioListener - required for audio to play in the scene
        audioListenerObj = new GameObject("AudioListener");
        audioListenerObj.AddComponent<AudioListener>();

        // Create SoundManager - add AudioSource BEFORE SoundManager
        soundManagerObj = new GameObject("SoundManager");
        soundManagerObj.AddComponent<AudioSource>();
        soundManager = soundManagerObj.AddComponent<SoundManager>();
        SoundManager.Instance = soundManager;

        // Create MusicManager - add AudioSource and clip BEFORE MusicManager
        musicManagerObj = new GameObject("MusicManager");
        var audioSource = musicManagerObj.AddComponent<AudioSource>();
        audioSource.clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        musicManager = musicManagerObj.AddComponent<MusicManager>();
        MusicManager.Instance = musicManager;
    }

    [TearDown]
    public void Teardown()
    {
        if (audioListenerObj != null)
            Object.DestroyImmediate(audioListenerObj);
        if (soundManagerObj != null)
            Object.DestroyImmediate(soundManagerObj);
        if (musicManagerObj != null)
            Object.DestroyImmediate(musicManagerObj);
        SoundManager.Instance = null;
        MusicManager.Instance = null;
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void ClickingOnButton_EnablesSound()
    {
        // Arrange
        soundManager.SetMuted(true);
        Assert.IsTrue(soundManager.IsMuted, "Sound should initially be muted");

        // Act
        soundManager.SetMuted(false);

        // Assert
        Assert.IsFalse(soundManager.IsMuted, "Sound should be unmuted after clicking On");
        Assert.AreEqual(1f, AudioListener.volume, "AudioListener volume should be 1");
    }

    [Test]
    public void ClickingOffButton_DisablesSound()
    {
        // Arrange
        soundManager.SetMuted(false);
        Assert.IsFalse(soundManager.IsMuted, "Sound should initially be unmuted");

        // Act
        soundManager.SetMuted(true);

        // Assert
        Assert.IsTrue(soundManager.IsMuted, "Sound should be muted after clicking Off");
        Assert.AreEqual(0f, AudioListener.volume, "AudioListener volume should be 0");
    }

    [UnityTest]
    public IEnumerator MusicPlays_WhenSoundIsEnabled()
    {
        // Arrange
        soundManager.SetMuted(false);
        var audioSource = musicManager.GetComponent<AudioSource>();

        // Act
        musicManager.PlayMusic();
        yield return null;

        // Assert
        Assert.IsTrue(audioSource.isPlaying, "Music should be playing when sound is enabled");
        Assert.IsFalse(soundManager.IsMuted, "Sound should not be muted");
    }

    [Test]
    public void OnButton_DoesNothing_WhenSoundAlreadyEnabled()
    {
        // Arrange
        soundManager.SetMuted(false);
        float initialVolume = AudioListener.volume;

        // Act
        soundManager.SetMuted(false); // Click On again

        // Assert
        Assert.AreEqual(initialVolume, AudioListener.volume,
            "Volume should remain unchanged when clicking On while already enabled");
        Assert.IsFalse(soundManager.IsMuted, "Sound should remain unmuted");
    }

    [Test]
    public void OffButton_DoesNothing_WhenSoundAlreadyDisabled()
    {
        // Arrange
        soundManager.SetMuted(true);
        float initialVolume = AudioListener.volume;

        // Act
        soundManager.SetMuted(true); // Click Off again

        // Assert
        Assert.AreEqual(initialVolume, AudioListener.volume,
            "Volume should remain unchanged when clicking Off while already disabled");
        Assert.IsTrue(soundManager.IsMuted, "Sound should remain muted");
    }

    [Test]
    public void SoundState_PersistsInPlayerPrefs()
    {
        // Arrange & Act
        soundManager.SetMuted(true);
        int savedState = PlayerPrefs.GetInt("SoundMuted", 0);

        // Assert
        Assert.AreEqual(1, savedState, "Muted state should be saved as 1 in PlayerPrefs");

        // Act - Unmute
        soundManager.SetMuted(false);
        savedState = PlayerPrefs.GetInt("SoundMuted", 0);

        // Assert
        Assert.AreEqual(0, savedState, "Unmuted state should be saved as 0 in PlayerPrefs");
    }

    [Test]
    public void SoundManager_RestoresMutedStateOnStartup()
    {
        // Arrange
        PlayerPrefs.SetInt("SoundMuted", 1);
        PlayerPrefs.Save();

        // Act - Create new SoundManager instance (simulating game restart)
        var newSoundManagerObj = new GameObject("NewSoundManager");
        var newSoundManager = newSoundManagerObj.AddComponent<SoundManager>();
        newSoundManagerObj.AddComponent<AudioSource>();

        // Assert
        Assert.AreEqual(0f, AudioListener.volume,
            "AudioListener should be muted on startup when PlayerPrefs indicates muted");

        // Cleanup
        Object.DestroyImmediate(newSoundManagerObj);
    }
}

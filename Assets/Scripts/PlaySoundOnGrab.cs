using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class PlaySoundWhenRotated : MonoBehaviour
{
    public AudioSource audioSource;
    public XRGrabInteractable grabInteractable;

    private bool isGrabbed = false;
    private Quaternion lastRotation;
    private float rotationSpeed = 0f;
    private float rotationThreshold = 30f; // derajat per detik
    public float decayTime; // lamanya getaran sisa
    private Coroutine fadeOutRoutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        lastRotation = transform.rotation;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        StartFadeOut(decayTime);
    }

    void Update()
    {
        if (!isGrabbed)
            return;

        // Hitung kecepatan rotasi (seberapa cepat berubah)
        Quaternion delta = transform.rotation * Quaternion.Inverse(lastRotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        rotationSpeed = Mathf.Abs(angle) / Time.deltaTime;

        // Jika rotasi cepat → mainkan suara
        if (rotationSpeed > rotationThreshold)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            // Reset fade out jika masih aktif
            if (fadeOutRoutine != null)
            {
                StopCoroutine(fadeOutRoutine);
                fadeOutRoutine = null;
                audioSource.volume = 1f;
            }
        }
        else
        {
            // Jika pelan tapi masih belum cukup lama diam, kasih efek decay
            if (audioSource.isPlaying && fadeOutRoutine == null)
            {
                StartFadeOut(decayTime);
            }
        }

        lastRotation = transform.rotation;
    }

    void StartFadeOut(float duration)
    {
        if (fadeOutRoutine != null)
            StopCoroutine(fadeOutRoutine);
        fadeOutRoutine = StartCoroutine(FadeOutAudio(duration));
    }

    IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = audioSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
        fadeOutRoutine = null;
    }
}

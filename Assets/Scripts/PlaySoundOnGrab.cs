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
    public float decayTime = 0.5f; // lamanya getaran sisa
    private Coroutine fadeOutRoutine;
    private Coroutine returnRoutine;

    // 🧭 posisi & rotasi awal
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Simpan posisi & rotasi awal
        initialPosition = transform.position;
        initialRotation = transform.rotation;

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

        // Stop return ke posisi awal kalau sedang jalan
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        StartFadeOut(decayTime);

        // Mulai animasi balik ke posisi awal
        returnRoutine = StartCoroutine(ReturnToStart(1f)); // 1 detik durasi
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

    IEnumerator ReturnToStart(float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = t / duration;
            transform.position = Vector3.Lerp(startPos, initialPosition, progress);
            transform.rotation = Quaternion.Slerp(startRot, initialRotation, progress);
            yield return null;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;
        returnRoutine = null;
    }
}

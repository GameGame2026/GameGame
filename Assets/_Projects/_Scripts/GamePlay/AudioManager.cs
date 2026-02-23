using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("背景音乐")]
    public AudioClip normalMusic;
    public AudioClip bossMusic;

    [Header("设置")]
    [Range(0, 1)] public float musicVolume = 0.5f;
    public float fadeDuration = 1f;  // 淡入淡出时间

    private AudioSource musicSource;
    private bool isBossMusicPlaying = false;

    [Header("boss战前对话")]
    public  DialogueData_SO theDialogue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 可选，让音乐跨场景
        }
        else
        {
            Destroy(gameObject);
        }

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.clip = normalMusic;
        musicSource.Play();

        // 订阅对话
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.OnDialogueClosed += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.OnDialogueClosed -= OnDialogueEnded;
    }

    private void OnDialogueEnded(DialogueData_SO last_dlg)
    {
        // 判断这个对话是否是After_fight
        if (last_dlg != null && last_dlg == theDialogue)
            StartBossMusicWithFade();
    }

    /// <summary>
    /// 切换到Boss音乐
    /// </summary>
    public void StartBossMusic()
    {
        if (isBossMusicPlaying) return;
        isBossMusicPlaying = true;
        // 简单的直接切换，如需淡入淡出可扩展协程
        musicSource.clip = bossMusic;
        musicSource.Play();
    }

    /// <summary>
    /// 恢复普通音乐
    /// </summary>
    public void ResumeNormalMusic()
    {
        if (!isBossMusicPlaying) return;
        isBossMusicPlaying = false;
        musicSource.clip = normalMusic;
        musicSource.Play();
    }

    // 可选：带淡入淡出的版本
    public void StartBossMusicWithFade()
    {
        StartCoroutine(FadeToMusic(bossMusic, true));
    }

    public void ResumeNormalMusicWithFade()
    {
        StartCoroutine(FadeToMusic(normalMusic, false));
    }

    private System.Collections.IEnumerator FadeToMusic(AudioClip newClip, bool isBoss)
    {
        float startVolume = musicSource.volume;
        // 淡出
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = 0;
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        // 淡入
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = musicVolume;
        isBossMusicPlaying = isBoss;
    }
}
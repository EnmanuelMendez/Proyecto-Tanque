using UnityEngine;

/// <summary>
/// Reproduce música de fondo (marcha fúnebre) al iniciar el juego.
/// Se debe asignar el AudioClip de la marcha fúnebre desde el inspector.
/// Este objeto persiste entre escenas.
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    public AudioClip m_BackgroundMusic;      // Asignar el clip de marcha fúnebre desde el inspector
    [Range(0f, 1f)]
    public float m_Volume = 0.3f;           // Volumen de la música de fondo

    private AudioSource m_AudioSource;
    private static BackgroundMusicManager s_Instance;

    private void Awake()
    {
        // Singleton: solo una instancia de música de fondo
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar AudioSource
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        m_AudioSource.clip = m_BackgroundMusic;
        m_AudioSource.volume = m_Volume;
        m_AudioSource.loop = true;
        m_AudioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Iniciar la música de fondo
        if (m_BackgroundMusic != null)
        {
            m_AudioSource.Play();
        }
        else
        {
            Debug.LogWarning("BackgroundMusicManager: No se ha asignado un AudioClip de música de fondo.");
        }
    }

    /// <summary>
    /// Detiene la música de fondo.
    /// </summary>
    public void StopMusic()
    {
        if (m_AudioSource != null && m_AudioSource.isPlaying)
        {
            m_AudioSource.Stop();
        }
    }

    /// <summary>
    /// Reanuda la música de fondo.
    /// </summary>
    public void PlayMusic()
    {
        if (m_AudioSource != null && !m_AudioSource.isPlaying)
        {
            m_AudioSource.Play();
        }
    }
}

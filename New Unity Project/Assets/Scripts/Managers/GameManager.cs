using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;                // Número de rondas que un jugador debe ganar para ganar el juego
    public int m_MaxLives = 5;                      // Número máximo de derrotas antes de perder el juego
    public float m_StartDelay = 3f;                 // Delay entre las fases de RoundStarting y RoundPlaying
    public float m_EndDelay = 3f;                   // Delay entre las fases de RoundPlaying y RoundEnding
    public float m_MaxGameTime = 240f;              // Tiempo máximo del juego en segundos (4 minutos)
    public CameraControl m_CameraControl;           // Referencia al script de CameraControl
    public Text m_MessageText;                      // Referencia al texto para mostrar mensajes
    public Text m_TimerText;                        // Referencia al texto del temporizador (asignar desde el inspector)
    public Text m_LivesText;                        // Referencia al texto de vidas (asignar desde el inspector)
    public GameObject m_TankPrefab;                 // Referencia al Prefab del Tanque
    public TankManager[] m_Tanks;                   // Array de TankManagers para controlar cada tanque

    private int m_RoundNumber;                      // Número de ronda
    private WaitForSeconds m_StartWait;             // Delay hasta que la ronda empieza
    private WaitForSeconds m_EndWait;               // Delay hasta que la ronda acaba
    private TankManager m_RoundWinner;              // Referencia al ganador de la ronda para anunciar quién ha ganado
    private TankManager m_GameWinner;               // Referencia al ganador del juego para anunciar quién ha ganado
    private float m_GameTimer;                      // Temporizador del juego
    private float m_GameStartTime;                  // Tiempo en que inició la partida
    private bool m_GameOver;                        // Flag de fin de juego
    private int[] m_Losses;                         // Contador de derrotas por jugador

    // --- UI creada por código (si no se asignan desde el inspector) ---
    private Text m_TimerTextInternal;
    private Text m_LivesTextInternal;
    private GameObject m_PlayerInfoObject;

    private void Start()
    {
        // Creamos los delays para que solo se apliquen una vez
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        // Inicializar temporizador
        m_GameTimer = m_MaxGameTime;
        m_GameStartTime = Time.time;
        m_GameOver = false;

        // Inicializar contadores de derrotas
        m_Losses = new int[m_Tanks.Length];
        for (int i = 0; i < m_Losses.Length; i++)
        {
            m_Losses[i] = 0;
        }

        // Crear UI adicional si no está asignada desde el inspector
        CreateAdditionalUI();

        SpawnAllTanks();                            // Generar tanques
        SetCameraTargets();                         // Ajustar cámara
        StartCoroutine(GameLoop());                 // Iniciar juego
    }

    private void Update()
    {
        if (!m_GameOver)
        {
            // Actualizar temporizador
            m_GameTimer -= Time.deltaTime;
            UpdateTimerUI();

            // Actualizar texto de vidas
            UpdateLivesUI();

            // Comprobar si se acabó el tiempo
            if (m_GameTimer <= 0f)
            {
                m_GameTimer = 0f;
                m_GameOver = true;
                StartCoroutine(TimeOutEnding());
            }
        }
    }

    /// <summary>
    /// Crea elementos de UI adicionales (temporizador, vidas, nombre) por código si no están asignados.
    /// </summary>
    private void CreateAdditionalUI()
    {
        // Buscar o crear un Canvas para los elementos de UI
        Canvas mainCanvas = FindExistingOverlayCanvas();
        GameObject canvasObj;

        if (mainCanvas == null)
        {
            canvasObj = new GameObject("GameUICanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasObj = mainCanvas.gameObject;
        }

        // --- Texto del temporizador (esquina superior izquierda) ---
        if (m_TimerText == null)
        {
            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(canvasObj.transform, false);
            m_TimerTextInternal = timerObj.AddComponent<Text>();
            m_TimerTextInternal.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            m_TimerTextInternal.fontSize = 28;
            m_TimerTextInternal.color = Color.white;
            m_TimerTextInternal.alignment = TextAnchor.UpperLeft;

            Shadow timerShadow = timerObj.AddComponent<Shadow>();
            timerShadow.effectColor = new Color(0, 0, 0, 0.8f);

            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0, 1);
            timerRect.anchorMax = new Vector2(0, 1);
            timerRect.pivot = new Vector2(0, 1);
            timerRect.anchoredPosition = new Vector2(10, -10);
            timerRect.sizeDelta = new Vector2(300, 50);
        }
        else
        {
            m_TimerTextInternal = m_TimerText;
        }

        // --- Texto de vidas (esquina superior centro-izquierda) ---
        if (m_LivesText == null)
        {
            GameObject livesObj = new GameObject("LivesText");
            livesObj.transform.SetParent(canvasObj.transform, false);
            m_LivesTextInternal = livesObj.AddComponent<Text>();
            m_LivesTextInternal.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            m_LivesTextInternal.fontSize = 22;
            m_LivesTextInternal.color = Color.white;
            m_LivesTextInternal.alignment = TextAnchor.UpperLeft;

            Shadow livesShadow = livesObj.AddComponent<Shadow>();
            livesShadow.effectColor = new Color(0, 0, 0, 0.8f);

            RectTransform livesRect = livesObj.GetComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(0, 1);
            livesRect.anchorMax = new Vector2(0, 1);
            livesRect.pivot = new Vector2(0, 1);
            livesRect.anchoredPosition = new Vector2(10, -50);
            livesRect.sizeDelta = new Vector2(400, 80);
        }
        else
        {
            m_LivesTextInternal = m_LivesText;
        }

        // --- Nombre y matrícula (esquina superior derecha) ---
        GameObject infoObj = new GameObject("PlayerInfoUI");
        infoObj.transform.SetParent(canvasObj.transform, false);

        Text infoText = infoObj.AddComponent<Text>();
        infoText.text = "Enmanuel Mendez\n2-19-0262";
        infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        infoText.fontSize = 22;
        infoText.color = Color.white;
        infoText.alignment = TextAnchor.UpperRight;
        infoText.horizontalOverflow = HorizontalWrapMode.Overflow;
        infoText.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow infoShadow = infoObj.AddComponent<Shadow>();
        infoShadow.effectColor = new Color(0, 0, 0, 0.8f);
        infoShadow.effectDistance = new Vector2(1, -1);

        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(1, 1);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.pivot = new Vector2(1, 1);
        infoRect.anchoredPosition = new Vector2(-10, -10);
        infoRect.sizeDelta = new Vector2(300, 60);

        m_PlayerInfoObject = infoObj;
    }

    private Canvas FindExistingOverlayCanvas()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        for (int i = 0; i < allCanvases.Length; i++)
        {
            if (allCanvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return allCanvases[i];
            }
        }
        return null;
    }

    private void UpdateTimerUI()
    {
        if (m_TimerTextInternal != null)
        {
            int minutes = Mathf.FloorToInt(m_GameTimer / 60f);
            int seconds = Mathf.FloorToInt(m_GameTimer % 60f);
            m_TimerTextInternal.text = string.Format("TIEMPO: {0:00}:{1:00}", minutes, seconds);

            // Cambiar color a rojo cuando quede menos de 30 segundos
            if (m_GameTimer <= 30f)
                m_TimerTextInternal.color = Color.red;
            else
                m_TimerTextInternal.color = Color.white;
        }
    }

    private void UpdateLivesUI()
    {
        if (m_LivesTextInternal != null)
        {
            string livesInfo = "";
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                int remaining = m_MaxLives - m_Losses[i];
                livesInfo += m_Tanks[i].m_ColoredPlayerText + " Vidas: " + remaining + "/" + m_MaxLives + "\n";
            }
            m_LivesTextInternal.text = livesInfo;
        }
    }

    private void SpawnAllTanks()
    {
        // Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ...los creo, ajusto el número de jugador y las referencias necesarias para controlarlo
            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        // Creo un array de Transforms del mismo tamaño que el número de tanques
        Transform[] targets = new Transform[m_Tanks.Length];

        // Recorro los Transforms...
        for (int i = 0; i < targets.Length; i++)
        {
            // ...lo ajusto al transform del tanque apropiado
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        // Estos son los targets que la cámara debe seguir
        m_CameraControl.m_Targets = targets;
    }

    // Llamado al principio y en cada fase del juego después de otra
    private IEnumerator GameLoop()
    {
        // Empiezo con la corutina RoundStarting y no retorno hasta que finalice
        yield return StartCoroutine(RoundStarting());

        // Cuando finalice RoundStarting, empiezo con RoundPlaying y no retorno hasta que finalice
        yield return StartCoroutine(RoundPlaying());

        // Cuando finalice RoundPlaying, empiezo con RoundEnding y no retorno hasta que finalice
        yield return StartCoroutine(RoundEnding());

        // Si el juego terminó (por tiempo o por vidas)
        if (m_GameOver)
        {
            // No reiniciar automáticamente, mostrar pantalla final
            yield break;
        }

        // Si hay un ganador del juego
        if (m_GameWinner != null)
        {
            // Mostrar pantalla de victoria con tiempo y puntaje
            m_GameOver = true;
            ShowWinnerScreen();
        }
        else
        {
            // Si no, reinicio las corutinas para que continúe el bucle
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting()
    {
        // Cuando empiece la ronda reseteo los tanques e impido que se muevan.
        ResetAllTanks();
        DisableTankControl();

        // Ajusto la cámara a los tanques reseteados.
        m_CameraControl.SetStartPositionAndSize();

        // Incremento la ronda y muestro el texto informativo.
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        // Cuando empiece la ronda dejo que los tanques se muevan.
        EnableTankControl();

        // Borro el texto de la pantalla.
        m_MessageText.text = string.Empty;

        // Mientras haya más de un tanque y no se haya acabado el juego...
        while (!OneTankLeft() && !m_GameOver)
        {
            // ... vuelvo al frame siguiente.
            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        // Deshabilito el movimiento de los tanques.
        DisableTankControl();

        // Borro al ganador de la ronda anterior.
        m_RoundWinner = null;

        // Miro si hay un ganador de la ronda.
        m_RoundWinner = GetRoundWinner();

        // Si lo hay, incremento su puntuación y las derrotas del perdedor.
        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;

            // Incrementar derrotas de los perdedores
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i] != m_RoundWinner)
                {
                    m_Losses[i]++;
                }
            }
        }

        // Comprobar si algún jugador alcanzó el máximo de derrotas (5 intentos)
        TankManager loserByLives = CheckMaxLosses();
        if (loserByLives != null)
        {
            // El otro jugador gana
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i] != loserByLives)
                {
                    m_GameWinner = m_Tanks[i];
                    break;
                }
            }
        }
        else
        {
            // Compruebo si alguien ha ganado el juego por rondas.
            m_GameWinner = GetGameWinner();
        }

        // Genero el mensaje según si hay un ganador del juego o no.
        string message = EndMessage();
        m_MessageText.text = message;

        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_EndWait;
    }

    /// <summary>
    /// Corrutina que se ejecuta cuando se acaba el tiempo (4 minutos).
    /// Ambos jugadores pierden.
    /// </summary>
    private IEnumerator TimeOutEnding()
    {
        // Deshabilitar controles
        DisableTankControl();

        // Calcular tiempo jugado
        float timePlayed = Time.time - m_GameStartTime;
        int minutes = Mathf.FloorToInt(timePlayed / 60f);
        int seconds = Mathf.FloorToInt(timePlayed % 60f);

        // Mostrar mensaje de tiempo agotado - ambos pierden
        string message = "<color=#FF0000>¡TIEMPO AGOTADO!</color>\n\n";
        message += "Ambos jugadores pierden.\n";
        message += "Ningún jugador logró ganar en 4 minutos.\n\n";
        message += "Tiempo jugado: " + string.Format("{0:00}:{1:00}", minutes, seconds) + "\n\n";

        // Mostrar puntajes finales
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " victorias\n";
        }

        message += "\n<size=18>La escena se reiniciará en 5 segundos...</size>";

        m_MessageText.text = message;

        // Esperar 5 segundos y reiniciar
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Muestra la pantalla de victoria con puntaje y tiempo.
    /// </summary>
    private void ShowWinnerScreen()
    {
        // Deshabilitar controles
        DisableTankControl();

        // Calcular tiempo que tomó ganar
        float timePlayed = Time.time - m_GameStartTime;
        int minutes = Mathf.FloorToInt(timePlayed / 60f);
        int seconds = Mathf.FloorToInt(timePlayed % 60f);

        // Construir mensaje de victoria
        string message = m_GameWinner.m_ColoredPlayerText + "\n";
        message += "<size=32>¡GANA EL JUEGO!</size>\n\n";
        message += "Puntaje: " + m_GameWinner.m_Wins + " rondas ganadas\n";
        message += "Tiempo: " + string.Format("{0:00}:{1:00}", minutes, seconds) + "\n\n";

        // Puntajes de todos los jugadores
        message += "--- Resultados Finales ---\n";
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " victorias, "
                     + m_Losses[i] + " derrotas\n";
        }

        message += "\n<size=18>La escena se reiniciará en 5 segundos...</size>";

        m_MessageText.text = message;

        // Reiniciar después de 5 segundos
        StartCoroutine(RestartAfterDelay(5f));
    }

    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Comprueba si algún jugador ha alcanzado el máximo de derrotas (5 intentos).
    /// </summary>
    private TankManager CheckMaxLosses()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Losses[i] >= m_MaxLives)
            {
                return m_Tanks[i];
            }
        }
        return null;
    }

    // Usado para comprobar si queda más de un tanque.
    private bool OneTankLeft()
    {
        // Contador de tanques.
        int numTanksLeft = 0;

        // Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si está activo, incremento el contador.
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        // Devuelvo true si queda 1 o menos, false si queda más de uno.
        return numTanksLeft <= 1;
    }

    // Comprueba si algún tanque ha ganado la ronda (si queda un tanque o menos).
    private TankManager GetRoundWinner()
    {
        // Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si solo queda uno, es el ganador y lo devuelvo.
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        // Si no hay ninguno activo es un empate, así que devuelvo null.
        return null;
    }

    // Comprueba si hay algún ganador del juego.
    private TankManager GetGameWinner()
    {
        // Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si alguno tiene las rondas necesarias, ha ganado y lo devuelvo.
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        // Si no, devuelvo null.
        return null;
    }

    // Devuelve el texto del mensaje a mostrar al final de cada ronda.
    private string EndMessage()
    {
        // Por defecto no hay ganadores, así que es empate.
        string message = "EMPATE!";

        // Si hay un ganador de ronda cambio el mensaje.
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        // Retornos de carro.
        message += "\n\n\n\n";

        // Recorro los tanques y añado sus puntuaciones y vidas restantes.
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            int remaining = m_MaxLives - m_Losses[i];
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA | Vidas: " + remaining + "\n";
        }

        // Si hay un ganador del juego, cambio el mensaje entero para reflejarlo.
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!";

        return message;
    }

    // Para resetear los tanques (propiedades, posiciones, etc.).
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    // Habilita el control del tanque
    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    // Deshabilita el control del tanque
    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}

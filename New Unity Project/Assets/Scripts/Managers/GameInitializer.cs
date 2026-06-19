using UnityEngine;

/// <summary>
/// Inicializador del juego que configura todos los nuevos sistemas:
/// - Música de fondo (marcha fúnebre)
/// - Enemigos obstáculo
/// - UI de información del jugador
/// 
/// Adjuntar a un GameObject vacío en la escena principal.
/// Este script se asegura de que todo esté configurado correctamente al iniciar.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Música de Fondo")]
    public AudioClip m_MarchaFunebre;               // Asignar el clip de marcha fúnebre
    [Range(0f, 1f)]
    public float m_MusicVolume = 0.3f;

    [Header("Enemigos")]
    public bool m_SpawnEnemies = true;              // Activar/desactivar generación de enemigos
    public int m_NumberOfEnemies = 3;               // Cantidad de enemigos
    public float m_EnemySpawnRadius = 20f;          // Radio de spawn de enemigos
    public Color m_EnemyColor = new Color(0.5f, 0f, 0.5f);
    public LayerMask m_TankLayerMask;               // Layer de los tanques

    private void Awake()
    {
        // Configurar música de fondo
        SetupBackgroundMusic();

        // Generar enemigos
        if (m_SpawnEnemies)
        {
            SpawnEnemyObstacles();
        }
    }

    private void SetupBackgroundMusic()
    {
        // Buscar si ya existe un BackgroundMusicManager
        BackgroundMusicManager existingManager = FindObjectOfType<BackgroundMusicManager>();
        if (existingManager != null)
        {
            return; // Ya existe, no crear otro
        }

        // Crear el objeto de música de fondo
        GameObject musicObj = new GameObject("BackgroundMusicManager");
        BackgroundMusicManager musicManager = musicObj.AddComponent<BackgroundMusicManager>();
        musicManager.m_BackgroundMusic = m_MarchaFunebre;
        musicManager.m_Volume = m_MusicVolume;
    }

    private void SpawnEnemyObstacles()
    {
        for (int i = 0; i < m_NumberOfEnemies; i++)
        {
            // Calcular posición aleatoria
            Vector2 randomPoint = Random.insideUnitCircle * m_EnemySpawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomPoint.x, 0f, randomPoint.y);

            // Crear el enemigo
            CreateEnemy(spawnPos, i);
        }
    }

    private void CreateEnemy(Vector3 position, int index)
    {
        // Crear el objeto padre
        GameObject enemy = new GameObject("Enemy_" + (index + 1));
        enemy.transform.position = position;

        // Agregar Rigidbody
        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.mass = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

        // Agregar Collider
        BoxCollider col = enemy.AddComponent<BoxCollider>();
        col.size = new Vector3(2f, 1.2f, 3f);
        col.center = new Vector3(0f, 0.6f, 0f);

        // Agregar el script de enemigo
        EnemyObstacle enemyScript = enemy.AddComponent<EnemyObstacle>();
        enemyScript.m_PatrolSpeed = 3f + Random.Range(0f, 2f);
        enemyScript.m_ChaseSpeed = 5f + Random.Range(0f, 2f);
        enemyScript.m_DetectionRadius = 12f + Random.Range(0f, 5f);
        enemyScript.m_PatrolRadius = 8f + Random.Range(0f, 5f);
        enemyScript.m_PushForce = 400f + Random.Range(0f, 200f);
        enemyScript.m_TankMask = m_TankLayerMask;

        // Crear el cuerpo visual (un cubo que simula un vehículo enemigo)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(enemy.transform);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(1.8f, 1f, 2.5f);
        Destroy(body.GetComponent<Collider>()); // Remover collider duplicado
        body.GetComponent<Renderer>().material.color = m_EnemyColor;

        // Crear la "torreta" visual
        GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Cube);
        turret.name = "Turret";
        turret.transform.SetParent(enemy.transform);
        turret.transform.localPosition = new Vector3(0f, 1.3f, 0.2f);
        turret.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
        Destroy(turret.GetComponent<Collider>());
        turret.GetComponent<Renderer>().material.color = m_EnemyColor * 0.7f; // Un tono más oscuro

        // Crear indicador frontal (ojo rojo)
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "FrontIndicator";
        indicator.transform.SetParent(enemy.transform);
        indicator.transform.localPosition = new Vector3(0f, 1.3f, 1f);
        indicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Destroy(indicator.GetComponent<Collider>());
        indicator.GetComponent<Renderer>().material.color = Color.red;

        // Asignar tag si es necesario
        enemy.tag = "Untagged";
    }
}

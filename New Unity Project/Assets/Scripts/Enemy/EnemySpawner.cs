using UnityEngine;

/// <summary>
/// Genera enemigos obstáculo en posiciones aleatorias del mapa.
/// Adjuntar al GameManager o a un GameObject vacío en la escena.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject m_EnemyPrefab;             // Prefab del enemigo (debe tener EnemyObstacle y un modelo visual)
    public int m_NumberOfEnemies = 3;            // Cantidad de enemigos a generar
    public Transform[] m_SpawnPoints;            // Puntos de spawn para los enemigos (opcionales)
    public float m_SpawnRadius = 20f;            // Radio de spawn si no hay puntos específicos
    public LayerMask m_TankMask;                 // Layer de los tanques para asignar a los enemigos

    private GameObject[] m_EnemyInstances;

    private void Start()
    {
        SpawnEnemies();
    }

    /// <summary>
    /// Genera los enemigos en las posiciones configuradas.
    /// </summary>
    public void SpawnEnemies()
    {
        if (m_EnemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: No se ha asignado un prefab de enemigo.");
            return;
        }

        m_EnemyInstances = new GameObject[m_NumberOfEnemies];

        for (int i = 0; i < m_NumberOfEnemies; i++)
        {
            Vector3 spawnPosition;

            if (m_SpawnPoints != null && i < m_SpawnPoints.Length && m_SpawnPoints[i] != null)
            {
                spawnPosition = m_SpawnPoints[i].position;
            }
            else
            {
                // Posición aleatoria dentro del radio
                Vector2 randomPoint = Random.insideUnitCircle * m_SpawnRadius;
                spawnPosition = transform.position + new Vector3(randomPoint.x, 0f, randomPoint.y);
            }

            m_EnemyInstances[i] = Instantiate(m_EnemyPrefab, spawnPosition, Quaternion.identity);

            // Asignar el layer mask de tanques al enemigo
            EnemyObstacle enemyScript = m_EnemyInstances[i].GetComponent<EnemyObstacle>();
            if (enemyScript != null && m_TankMask != 0)
            {
                enemyScript.m_TankMask = m_TankMask;
            }
        }
    }

    /// <summary>
    /// Destruye todos los enemigos generados.
    /// </summary>
    public void DestroyAllEnemies()
    {
        if (m_EnemyInstances == null) return;

        for (int i = 0; i < m_EnemyInstances.Length; i++)
        {
            if (m_EnemyInstances[i] != null)
            {
                Destroy(m_EnemyInstances[i]);
            }
        }
    }

    /// <summary>
    /// Resetea los enemigos (destruye y vuelve a generar).
    /// </summary>
    public void ResetEnemies()
    {
        DestroyAllEnemies();
        SpawnEnemies();
    }
}

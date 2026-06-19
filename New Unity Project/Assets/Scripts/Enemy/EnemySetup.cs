using UnityEngine;

/// <summary>
/// Script auxiliar para crear un enemigo prefab básico por código.
/// Adjuntar a un GameObject vacío en la escena y ejecutar en modo Play para ver un enemigo de prueba.
/// También se puede usar desde el editor para crear el prefab.
/// 
/// INSTRUCCIONES PARA CREAR EL PREFAB DE ENEMIGO:
/// 1. Crear un GameObject vacío llamado "Enemy"
/// 2. Agregar un componente Rigidbody
/// 3. Agregar un BoxCollider (o SphereCollider)
/// 4. Agregar el script EnemyObstacle
/// 5. Agregar un hijo con un modelo visual (cubo escalado, esfera, o modelo 3D)
/// 6. Asignar la Layer "Players" al LayerMask del EnemyObstacle
/// 7. Guardar como prefab en Assets/Prefabs/Enemy.prefab
/// </summary>
public class EnemySetup : MonoBehaviour
{
    [Header("Configuración visual del enemigo")]
    public Color m_EnemyColor = new Color(0.5f, 0f, 0.5f); // Color púrpura para los enemigos
    public float m_EnemyScale = 1.5f;

    /// <summary>
    /// Crea un GameObject enemigo básico en la escena.
    /// Útil para pruebas rápidas.
    /// </summary>
    public GameObject CreateEnemyObject(Vector3 position)
    {
        // Crear el objeto padre
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.position = position;

        // Agregar Rigidbody
        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.mass = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

        // Agregar Collider
        BoxCollider col = enemy.AddComponent<BoxCollider>();
        col.size = new Vector3(m_EnemyScale, m_EnemyScale, m_EnemyScale);

        // Agregar el script de enemigo
        EnemyObstacle enemyScript = enemy.AddComponent<EnemyObstacle>();
        enemyScript.m_PatrolSpeed = 4f;
        enemyScript.m_ChaseSpeed = 6f;
        enemyScript.m_DetectionRadius = 15f;
        enemyScript.m_PatrolRadius = 10f;
        enemyScript.m_PushForce = 500f;

        // Crear el modelo visual (un cubo)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(enemy.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(m_EnemyScale, m_EnemyScale * 0.6f, m_EnemyScale * 1.5f);

        // Eliminar el collider del modelo visual (ya tenemos el del padre)
        Destroy(visual.GetComponent<Collider>());

        // Cambiar el color
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.color = m_EnemyColor;

        // Agregar un "ojo" para que se vea la dirección
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.transform.SetParent(enemy.transform);
        eye.transform.localPosition = new Vector3(0f, 0.2f, m_EnemyScale * 0.7f);
        eye.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Destroy(eye.GetComponent<Collider>());
        eye.GetComponent<Renderer>().material.color = Color.red;

        return enemy;
    }
}

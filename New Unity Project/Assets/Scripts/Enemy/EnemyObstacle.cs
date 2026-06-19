using UnityEngine;

/// <summary>
/// Enemigo que patrulla entre puntos y obstaculiza a los tanques.
/// Se mueve hacia los tanques cuando están cerca y los empuja.
/// Adjuntar a un GameObject con Rigidbody y Collider.
/// </summary>
public class EnemyObstacle : MonoBehaviour
{
    public float m_PatrolSpeed = 4f;            // Velocidad de patrullaje
    public float m_ChaseSpeed = 6f;             // Velocidad al perseguir un tanque
    public float m_DetectionRadius = 15f;       // Radio de detección de tanques
    public float m_PatrolRadius = 10f;          // Radio de patrullaje alrededor del punto de inicio
    public float m_PushForce = 500f;            // Fuerza aplicada al empujar un tanque
    public float m_ChangeDirectionTime = 3f;    // Tiempo entre cambios de dirección de patrullaje
    public LayerMask m_TankMask;                // Layer mask para detectar tanques

    private Rigidbody m_Rigidbody;
    private Vector3 m_StartPosition;
    private Vector3 m_PatrolTarget;
    private Transform m_CurrentTarget;
    private float m_DirectionTimer;
    private bool m_IsChasing;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    private void Start()
    {
        m_StartPosition = transform.position;
        SetNewPatrolTarget();
        m_DirectionTimer = m_ChangeDirectionTime;
    }

    private void FixedUpdate()
    {
        // Buscar tanques cercanos
        m_CurrentTarget = FindNearestTank();

        if (m_CurrentTarget != null)
        {
            // Perseguir al tanque más cercano
            ChaseTarget();
            m_IsChasing = true;
        }
        else
        {
            // Patrullar
            Patrol();
            m_IsChasing = false;
        }
    }

    private Transform FindNearestTank()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_DetectionRadius, m_TankMask);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < colliders.Length; i++)
        {
            // Solo perseguir tanques activos
            if (!colliders[i].gameObject.activeSelf)
                continue;

            float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = colliders[i].transform;
            }
        }

        return nearest;
    }

    private void ChaseTarget()
    {
        Vector3 direction = (m_CurrentTarget.position - transform.position).normalized;
        direction.y = 0f; // Mantener en el plano horizontal

        Vector3 movement = direction * m_ChaseSpeed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);

        // Rotar hacia el objetivo
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_Rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime));
        }
    }

    private void Patrol()
    {
        // Temporizador para cambiar de dirección
        m_DirectionTimer -= Time.deltaTime;
        if (m_DirectionTimer <= 0f || Vector3.Distance(transform.position, m_PatrolTarget) < 1f)
        {
            SetNewPatrolTarget();
            m_DirectionTimer = m_ChangeDirectionTime;
        }

        // Moverse hacia el punto de patrullaje
        Vector3 direction = (m_PatrolTarget - transform.position).normalized;
        direction.y = 0f;

        Vector3 movement = direction * m_PatrolSpeed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);

        // Rotar hacia el punto de patrullaje
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            m_Rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime));
        }
    }

    private void SetNewPatrolTarget()
    {
        // Punto aleatorio dentro del radio de patrullaje
        Vector2 randomPoint = Random.insideUnitCircle * m_PatrolRadius;
        m_PatrolTarget = m_StartPosition + new Vector3(randomPoint.x, 0f, randomPoint.y);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Empujar tanques al colisionar
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
        TankHealth tankHealth = collision.gameObject.GetComponent<TankHealth>();

        if (otherRb != null && tankHealth != null)
        {
            // Aplicar fuerza de empuje
            Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
            pushDirection.y = 0f;
            otherRb.AddForce(pushDirection * m_PushForce);

            // Aplicar un poco de daño al tanque
            tankHealth.TakeDamage(5f);
        }
    }

    // Visualizar el radio de detección en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);

        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? m_StartPosition : transform.position;
        Gizmos.DrawWireSphere(center, m_PatrolRadius);
    }
}

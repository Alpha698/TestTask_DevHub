using UnityEngine;
using UnityEngine.EventSystems;

public class CannonController : MonoBehaviour
{
    [Header("Prefabs & Effects")]
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private AudioSource cannonAudioSource;
    [SerializeField] private AudioClip fireAudioClip;
    [SerializeField] private AudioClip defeatAudioClip;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private ParticleSystem loseEffect;
    [SerializeField] private Transform muzzle;

    [Header("Trajectory")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float arcHeight = 5f;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    [SerializeField] private int trajectoryPoints = 30;

    private Vector3 targetPoint;
    private bool isLose = false;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver() || !GameManager.Instance.IsGameStarted()) return;

        // Turn cannon and make the trajectory of the cannonball flight to the place of the click
        if (Input.GetMouseButton(0))
        {
            bool isOverUI = false;

            // Check if click by UI and not by 3D location
#if UNITY_ANDROID || UNITY_IOS
            if (!Application.isEditor && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                isOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
            else
            {
                isOverUI = EventSystem.current.IsPointerOverGameObject();
            }
#else
    isOverUI = EventSystem.current.IsPointerOverGameObject();
#endif

            if (!isOverUI)
            {
                SetTargetPoint();
                RotateCannonToTarget();
                DrawTrajectory();
            }
        }

    }

    private void SetTargetPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 point = hit.point;
            point.y = Terrain.activeTerrain.SampleHeight(point) + Terrain.activeTerrain.transform.position.y;
            targetPoint = point;
        }
    }

    private void RotateCannonToTarget()
    {
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private Vector3 CalculateBallisticVelocity(Vector3 start, Vector3 end, float arcHeight)
    {
        Vector3 direction = end - start;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
        float distanceXZ = directionXZ.magnitude;
        float heightDifference = direction.y;
        float gravity = Mathf.Abs(Physics.gravity.y);

        float h = Mathf.Max(arcHeight, heightDifference + 0.1f);
        float tUp = Mathf.Sqrt(2 * h / gravity);
        float tDown = Mathf.Sqrt(2 * Mathf.Max(0.1f, h - heightDifference) / gravity);
        float totalTime = tUp + tDown;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * gravity * h);
        Vector3 velocityXZ = directionXZ / totalTime;

        return velocityXZ + velocityY;
    }

    private void DrawTrajectory()
    {
        Vector3 velocity = CalculateBallisticVelocity(muzzle.position, targetPoint, arcHeight);
        Vector3[] points = new Vector3[trajectoryPoints];

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;
            points[i] = muzzle.position + velocity * t + 0.5f * Physics.gravity * t * t;
        }

        lineRenderer.positionCount = trajectoryPoints;
        lineRenderer.SetPositions(points);
    }

    public void Shoot()
    {
        GameObject ball = Instantiate(cannonBallPrefab, muzzle.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = CalculateBallisticVelocity(muzzle.position, targetPoint, arcHeight);
        rb.useGravity = true;

        cannonAudioSource.clip = fireAudioClip;
        cannonAudioSource.Play();
        fireEffect.Play();
    }

    public void HandleLose()
    {
        if (isLose) return;

        //If the cannon is destroyed then off the activity
        cannonAudioSource.clip = defeatAudioClip;
        cannonAudioSource.Play();

        isLose = true;
        loseEffect.Play();
        lineRenderer.positionCount = 0;
    }
}

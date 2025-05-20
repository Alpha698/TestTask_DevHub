using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CannonController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Button fireButton;
    [SerializeField]
    private TextMeshProUGUI fireButtonText;
    [SerializeField]
    private Slider reloadSlider;
    [SerializeField] private float reloadTime = 3f;

    [Space]
    [SerializeField]
    private GameObject cannonPrefab;
    [SerializeField]
    private GameObject cannonBallPrefab;
    [SerializeField]
    private AudioSource fireAudio;
    [SerializeField]
    private ParticleSystem fireEffect;
    [SerializeField]
    private ParticleSystem LoseEffect;
    [SerializeField]
    private Transform muzzle;

    [Space]
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private float arcHeight = 5f;
    [SerializeField]
    private float trajectoryTimeStep = 0.1f;
    [SerializeField]
    private int trajectoryPoints = 30;

    private Vector3 targetPoint;
    private bool isReloading = false;
    private bool isLose = false;

    private void OnEnable()
    {
        Enemy.EnemyAttack += TakeDamage;
    }

    private void OnDisable()
    {
        Enemy.EnemyAttack -= TakeDamage;
    }

    private void Start()
    {
        fireButton.onClick.AddListener(Fire);
    }

    private void Update()
    {
        if (!isLose)
        {
            if (Input.GetMouseButton(0) && !IsPointerOverUI())
            {
                RotateCannonToTarget();
                SetTargetPoint();
                DrawTrajectory();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
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

    private void SetTargetPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 point = hit.point;

            float terrainY = Terrain.activeTerrain.SampleHeight(point);
            point.y = terrainY + Terrain.activeTerrain.transform.position.y;

            targetPoint = point;
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
            Vector3 point = muzzle.position + velocity * t + 0.5f * Physics.gravity * t * t;
            points[i] = point;
        }

        lineRenderer.positionCount = trajectoryPoints;
        lineRenderer.SetPositions(points);
    }

    private void Fire()
    {
        if (isReloading) return;

        GameObject ball = Instantiate(cannonBallPrefab, muzzle.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.interpolation = RigidbodyInterpolation.None;

        Vector3 velocity = CalculateBallisticVelocity(muzzle.position, targetPoint, arcHeight);
        rb.velocity = velocity;

        //lineRenderer.positionCount = 0;

        fireAudio.Play();
        fireEffect.Play();

        // Reloading
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        fireButton.interactable = false;
        reloadSlider.value = 0f;

        Color originalTextColor = fireButtonText.color;
        Color fadedColor = originalTextColor;
        fadedColor.a = 0.3f;
        fireButtonText.color = fadedColor;

        float timer = 0f;
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            reloadSlider.value = timer / reloadTime;
            yield return null;
        }

        fireButton.interactable = true;
        fireButtonText.color = originalTextColor;
        isReloading = false;
    }

    public void TakeDamage()
    {
        if (isLose) return;

        isLose = true;

        Debug.Log("Lose");
        LoseEffect.Play();

        fireButton.interactable = false;
        Color originalTextColor = fireButtonText.color;
        Color fadedColor = originalTextColor;
        fadedColor.a = 0.3f; 
        fireButtonText.color = fadedColor;

        lineRenderer.positionCount = 0;
    }

}
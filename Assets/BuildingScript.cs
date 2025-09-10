using TMPro;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
    [Header("Prefabs & Layers")]
    public GameObject foundationPrefab;
    public LayerMask groundLayer;
    public LayerMask foundationLayer;
    

    [Header("Build Settings")]
    public float maxBuildDistance = 10f;
    public float snapDistance = 1f;

    [Header("Visuals")]
    public Material greenMaterial;
    public Material redMaterial;
    public TextMeshProUGUI debugText;

    // Приватні змінні
    private GameObject currentGhostObject;
    private Renderer ghostRenderer;
    private bool isBuildingMode = false;
    private bool canPlace = false;
    private RaycastHit hit;
    private Quaternion rotationGhost;
    

    void Start()
    {
        if (currentGhostObject == null)
        {
            currentGhostObject = Instantiate(foundationPrefab);
            Destroy(currentGhostObject.GetComponent<BoxCollider>());
            ghostRenderer = currentGhostObject.GetComponent<Renderer>();
            currentGhostObject.SetActive(false);
            rotationGhost = transform.rotation;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode;
        }

        if (isBuildingMode)
        {
            HandleBuildingMode();
        }
        else
        {
            currentGhostObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPlaceFoundation();
        }
    }

    private void HandleBuildingMode()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, maxBuildDistance, groundLayer))
        {
            currentGhostObject.SetActive(true);

            Vector3 targetPosition = hit.point;
            bool snapped = TrySnapToExistingFoundation(ref targetPosition);

            canPlace = checkFoundation();

            if (!snapped)
            {
                // Піднімаємо на половину висоти, щоб об’єкт стояв на землі
                currentGhostObject.transform.position = targetPosition + Vector3.up * foundationPrefab.transform.localScale.y / 2f;
                rotationGhost  = transform.rotation;
                currentGhostObject.transform.rotation = rotationGhost;
            }
            else
            {
                currentGhostObject.transform.position = targetPosition;
                currentGhostObject.transform.rotation = rotationGhost;

            }

            //debugText.text = $"Can place: {canPlace}";
        }
        else
        {
            currentGhostObject.SetActive(false);
        }
    }

    private bool TrySnapToExistingFoundation(ref Vector3 targetPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(targetPosition, snapDistance);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Foundation") && col.gameObject != currentGhostObject)
            {
                
                // Локальні координати точки хіта
                Vector3 localPoint = col.transform.InverseTransformPoint(hit.point);
                float threshold = 0.3f;
                

                Vector3 localOffset = Vector3.zero;

                if (Mathf.Abs(localPoint.x) > Mathf.Abs(localPoint.z))
                {
                    if (Mathf.Abs(localPoint.x) < threshold)
                        localOffset.x = 0;
                    else
                        localOffset.x = Mathf.Sign(localPoint.x);
                }
                else
                {
                    if (Mathf.Abs(localPoint.z) < threshold)
                        localOffset.z = 0;
                    else
                        localOffset.z = Mathf.Sign(localPoint.z);
                }

                // Світові координати для привида
                Vector3 newWorldPos = col.transform.TransformPoint(localOffset);

                // Орієнтація привида
                rotationGhost = col.transform.rotation;

                // Вивід для debugText
                debugText.text = $"=== Snap Debug ===\n" +
                                 $"Hit point: {hit.point}\n" +
                                 $"Collider center: {col.transform.position}\n" +
                                 
                                 $"Local hit: {localPoint}\n" +
                                 $"LocalOffset: {localOffset}\n" +
                                 $"TargetPosition (world): {newWorldPos}\n" +
                                 $"RotationGhost: {rotationGhost.eulerAngles}";

                // Оновлюємо позицію
                targetPosition = newWorldPos;

                return true;
            }
        }
        return false;
    }


    private void TryPlaceFoundation()
    {
        if (currentGhostObject.activeSelf && canPlace)
        {
            Instantiate(foundationPrefab, currentGhostObject.transform.position, currentGhostObject.transform.rotation);
        }
    }

    void OnDrawGizmos()
    {
        if (currentGhostObject == null) return;

        Gizmos.color = Color.magenta;
        GetTopHalfBox(out Vector3 boxCenter, out Vector3 halfExtents);
        Gizmos.DrawWireCube(boxCenter, halfExtents * 2f);
    }

    private bool checkFoundation()
    {
        GetTopHalfBox(out Vector3 boxCenter, out Vector3 halfExtents);

        Collider[] colliders = Physics.OverlapBox(boxCenter, halfExtents, currentGhostObject.transform.rotation);

        foreach (var col in colliders)
        {
            if (col.gameObject != currentGhostObject)
            {
                ghostRenderer.material = redMaterial;
                return false;
            }
        }

        ghostRenderer.material = greenMaterial;
        return true;
    }

    /// <summary>
    /// Отримує центр і розміри колайдера, який займає тільки верхню половину об’єкта.
    /// </summary>
    private void GetTopHalfBox(out Vector3 center, out Vector3 halfExtents)
    {
        Vector3 fullSize = currentGhostObject.transform.localScale;

        // робимо верхню половину, але зменшуємо трохи для уникнення фальшивих перетинів
        float margin = 0.01f;

        halfExtents = new Vector3(
            (fullSize.x / 2f) - margin,
            (fullSize.y / 4f) - margin,
            (fullSize.z / 2f) - margin
        );

        // Щоб уникнути від'ємних значень, підстрахуємо
        halfExtents = Vector3.Max(halfExtents, Vector3.zero);

        center = currentGhostObject.transform.position + new Vector3(0, fullSize.y / 4f, 0);
    }
}

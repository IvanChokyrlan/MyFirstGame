using UnityEngine;

public class BuildScript : MonoBehaviour
{
    public GameObject foundationPrefab;
    public LayerMask groundLayer;
    public float maxBuildDistance = 10f;
    public float snapDistance = 1f;

    private GameObject currentGhostObject;
    private bool isBuildingMode = false; // Змінна для відстеження режиму будівництва

    void Update()
    {
        // Перемикаємо режим будівництва при натисканні клавіші 'B'
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode; // Перемикаємо значення true/false

            // Якщо виходимо з режиму, ховаємо "примарний" об'єкт
            if (!isBuildingMode)
            {
                if (currentGhostObject != null)
                {
                    currentGhostObject.SetActive(false);
                }
            }
        }

        // Запускаємо логіку будівництва, тільки якщо режим увімкнений
        if (isBuildingMode)
        {
            UpdateGhostObject();

            if (Input.GetKeyDown(KeyCode.F))
            {
                PlaceFoundation();
            }
        }
    }

    void UpdateGhostObject()
    {
        // Якщо "примарного" об'єкта немає, створюємо його і ховаємо
        if (currentGhostObject == null)
        {
            currentGhostObject = Instantiate(foundationPrefab);
            currentGhostObject.SetActive(false); // Ховаємо його одразу після створення
        }

        // Кидаємо промінь від центру екрана
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxBuildDistance, groundLayer))
        {
            currentGhostObject.SetActive(true); // Показуємо, якщо знайшли землю

            Vector3 targetPosition = hit.point;

            Collider[] colliders = Physics.OverlapSphere(targetPosition, snapDistance);
            bool snapped = false;

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Foundation") && col.gameObject != currentGhostObject)
                {
                    // Обчислюємо вектор напрямку від точки hit до центра фундаменту, щоб прилипати збоку
                    Vector3 direction = (col.transform.position - hit.point).normalized;

                    // Обчислюємо, до якої сторони ближче (X чи Z)
                    float xSnap = Mathf.Round(direction.x);
                    float zSnap = Mathf.Round(direction.z);

                    targetPosition = col.transform.position;

                    if (Mathf.Abs(xSnap) > Mathf.Abs(zSnap))
                    {
                        targetPosition += Vector3.right * foundationPrefab.transform.localScale.x * xSnap;
                    }
                    else
                    {
                        targetPosition += Vector3.forward * foundationPrefab.transform.localScale.z * zSnap;
                    }

                    snapped = true;
                    break;
                }
            }

            if (!snapped)
            {
                targetPosition.y = hit.point.y;
            }
            else
            {
                targetPosition.y = colliders[0].transform.position.y; // Вирівнюємо висоту з існуючим фундаментом
            }

            currentGhostObject.transform.position = targetPosition + Vector3.up * foundationPrefab.transform.localScale.y / 2;
        }
        else
        {
            currentGhostObject.SetActive(false); // Ховаємо, якщо промінь нічого не знайшов
        }
    }

    void PlaceFoundation()
    {
        if (currentGhostObject != null && currentGhostObject.activeSelf)
        {
            Instantiate(foundationPrefab, currentGhostObject.transform.position, Quaternion.identity);
        }
    }
}
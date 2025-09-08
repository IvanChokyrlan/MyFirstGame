using TMPro;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{

    public GameObject foundationPrefab;
    public LayerMask groundLayer;
    public float maxBuildDistance = 10f;
    public float snapDistance = 1f;

    // Матеріали для візуального зворотного зв'язку
    public Material greenMaterial;
    public Material redMaterial;
    public TextMeshProUGUI text;

    // Приватні змінні для логіки скрипта
    private GameObject currentGhostObject;
    private Renderer ghostRenderer;
    private bool isBuildingMode = false;
    private bool canPlace = false;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode;
        }

        if (currentGhostObject == null)
        {
            currentGhostObject = Instantiate(foundationPrefab);
            Destroy(currentGhostObject.GetComponent<BoxCollider>());
            currentGhostObject.SetActive(false);
        }

        if (isBuildingMode)
        {


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxBuildDistance, groundLayer))
            {
                currentGhostObject.SetActive(true);

                Vector3 targetPosition = hit.point;


                Collider[] colliders = Physics.OverlapSphere(targetPosition, maxBuildDistance);
                bool snapped = false;

                foreach (Collider col in colliders)
                {
                    if (col.CompareTag("Foundation") && col.gameObject != currentGhostObject)
                    {
                        Vector3 direction = (hit.point - col.transform.position).normalized;
                        text.text = ($"\t\t\t\t\tX\tY\tZ" +
                            $"\nПозиція нашого обєкта:\t{col.transform.position}" +
                            $"\nПозиція мишки:\t\t{hit.point}" +
                            $"\nЇх різниця:\t\t\t{(hit.point - col.transform.position).normalized}" +
                            $"\nxSnap {Mathf.Round(direction.x)}, zSnap {Mathf.Round(direction.z)} ");
                        float xSnap = Mathf.Round(direction.x);
                        float zSnap = Mathf.Round(direction.z);

                        float totalDistanceX = (col.transform.localScale.x / 2) + (foundationPrefab.transform.localScale.x / 2);
                        float totalDistanceZ = (col.transform.localScale.z / 2) + (foundationPrefab.transform.localScale.z / 2);

                        text.text += ($"\nMathf.Abs(xSnap): {Mathf.Abs(xSnap)}, Mathf.Abs(zSnap) {Mathf.Abs(zSnap)}");
                        if (Mathf.Abs(xSnap) > Mathf.Abs(zSnap))
                        {
                            targetPosition = col.transform.position + Vector3.right * totalDistanceX * xSnap;
                            

                        }
                        else
                        {
                            targetPosition = col.transform.position + Vector3.forward * totalDistanceZ * zSnap;
                        }

                        snapped = true;
                        break;

                    }
                }


                if (!snapped)
                {
                    currentGhostObject.transform.position = targetPosition + Vector3.up * foundationPrefab.transform.localScale.y / 2;
                }
                else
                {
                    currentGhostObject.transform.position = targetPosition;
                }


            }
            else
            {
                currentGhostObject.SetActive(false);
            }


        }
        else
        {
            currentGhostObject.SetActive(false);
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentGhostObject.activeSelf)
            {
                Instantiate(foundationPrefab, currentGhostObject.transform.position, Quaternion.identity);
            }
        }



    }
}

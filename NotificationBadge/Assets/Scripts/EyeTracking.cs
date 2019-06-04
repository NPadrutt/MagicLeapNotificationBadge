using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class EyeTracking : MonoBehaviour
{
    public Material FocusedMaterial, NonFocusedMaterial;
    public GameObject DetailObjectToOpen;

    [Tooltip("Specifies the target we will orient to. If no target is specified, the main camera will be used.")]
    private Transform targetTransform;

    public Transform TargetTransform
    {
        get { return targetTransform; }
        set { targetTransform = value; }
    }

    private Vector3 heading;
    private MeshRenderer meshRenderer;
    private bool isDetailOpen;
    private int missedCounter;
    private int hitCounter;

    // Start is called before the first frame update
    void Start()
    {
        if (TargetTransform == null)
        {
            TargetTransform = CameraCache.Main.transform;
        }

        MLEyes.Start();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    void Awake()
    {
        if (DetailObjectToOpen != null)
        {
            DetailObjectToOpen.SetActive(false);
        }
    }

    private void OnDisable()
    {
        Debug.Log("Eye Tracking Disabled");
        MLEyes.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (!MLEyes.IsStarted)
        {
            MLEyes.Start();
            Debug.Log("Eye Tracking reenabled.");
        }

        if (MLEyes.IsStarted)
        {
            RaycastHit rayHit;
            heading = MLEyes.FixationPoint - TargetTransform.position;

            if (Physics.Raycast(TargetTransform.position, heading, out rayHit, 10.0f))
            {
                missedCounter = 0;
                hitCounter++;

                if (rayHit.collider.gameObject == gameObject)
                {
                    if (hitCounter >= 2)
                    {
                        meshRenderer.material = FocusedMaterial;

                        if (DetailObjectToOpen != null && !isDetailOpen)
                        {
                            var heading = TargetTransform.position + TargetTransform.forward * 1.9f;

                            DetailObjectToOpen.transform.position = heading;
                            DetailObjectToOpen.SetActive(true);
                            isDetailOpen = true;
                        }
                    }
                }
                else if (rayHit.collider.gameObject == DetailObjectToOpen || rayHit.collider.gameObject.tag == "ReadingArea")
                {
                    missedCounter = 0;
                    meshRenderer.material = NonFocusedMaterial;
                } 
                else {
                    DetailObjectToOpen.SetActive(false);
                    isDetailOpen = false;
                    missedCounter = 0;
                    hitCounter = 0;
                    meshRenderer.material = NonFocusedMaterial;
                }
            }
            else
            {
                missedCounter++;
                hitCounter = 0;

                if (missedCounter >= 8)
                {
                    DetailObjectToOpen.SetActive(false);
                    isDetailOpen = false;
                    meshRenderer.material = NonFocusedMaterial;
                }
            }
        }
    }
}
using UnityEngine;
using TMPro;

public class PickUpRoll : MonoBehaviour
{
    private RollDiceManager manager;
    void Start()
    {
        manager = Object.FindAnyObjectByType<RollDiceManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {

            if (manager != null)
            {
                manager.ShowRollMenu(); 
            }
            else
            {
                Debug.LogWarning("No RollDiceManager found in scene!");
            }

            Destroy(gameObject); 
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            if (manager != null)
            {
                manager.ShowRollMenu();
            }
            else
            {
                Debug.LogWarning("No RollDiceManager found in scene!");
            }

            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    [Header("Assign the Game Over UI or Object to Activate")]
    public GameObject gameEndUI;

    void Start()
    {
        gameEndUI.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Pause the game
            Time.timeScale = 0f;

            // Activate the game end UI or any assigned GameObject
            if (gameEndUI != null)
            {
                gameEndUI.SetActive(true);
            }

            // Optional: lock cursor or disable player control here if needed
            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;
        }
    }
}

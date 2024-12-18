using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class Level2 : MonoBehaviour
{
    public static Level2 Instance;

    [Header("UI Elements")]
    public Text scoreText;
    public Text questionText;

    // Heart Image references
    public Image heart1;
    public Image heart2;
    public Image heart3;

    [Header("Sound Effects")]
    public AudioClip gameOverSound;   // Sound for game over
    public AudioClip levelCompleteSound; // Sound for level completion
    public AudioClip sliceSound;      // Sound for slicing an object (add this for slices)
    public AudioClip backgroundMusic;
    private AudioSource audioSource;  // AudioSource to play sounds

    [Header("Game Data")]
    private int playerScore = 0;
    private int currentQuestionIndex = 0;
    private int playerLives = 3; // Total hearts/lives

    private string[] questions = {
        "-3 + -7 = ?",
        "-5 + -4 = ?",
        "-6 + -9 = ?",
        "-8 + -3 = ?",
        "-10 + -5 = ?",
        "-7 + -2 = ?",
        "-4 + -6 = ?",
        "-11 + -8 = ?",
        "-12 + -3 = ?",
        "-9 + -10 = ?"
    };

    private int[] answers = { -10, -9, -15, -11, -15, -9, -10, -19, -15, -19 };

    private string filePath;
    private List<UserData> userList;

    private void Awake()
    {
        // Set up singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        filePath = Application.persistentDataPath + "/userdata.json";

        // Load existing user data if the file exists
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>();
        }

        audioSource = GetComponent<AudioSource>(); // Initialize AudioSource component
    }

    private void Start()
    {
        if (scoreText == null || questionText == null || heart1 == null || heart2 == null || heart3 == null)
        {
            Debug.LogError("UI references are not assigned in the Inspector!");
            return;
        }

        UpdateUI();

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.1f;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Background music clip is not assigned in the Inspector.");
        }
    }

    public void AddScore(int amount)
    {
        playerScore += amount;
        Debug.Log($"Score updated: {playerScore}");

       

        // Play slice sound for the correct answer
        if (audioSource != null && sliceSound != null)
        {
            audioSource.PlayOneShot(sliceSound);
        }

        UpdateUI();
    }

    public void LoseHeart()
    {
        playerLives--;

        // Hide a heart based on remaining lives
        switch (playerLives)
        {
            case 2:
                heart3.enabled = false;
                break;
            case 1:
                heart2.enabled = false;
                break;
            case 0:
                heart1.enabled = false;
                GameOver();
                break;
        }

        // Play slice sound (if desired, like for a "slice" event when losing a heart)
        if (audioSource != null && sliceSound != null)
        {
            audioSource.PlayOneShot(sliceSound);
        }

        Debug.Log($"Lives remaining: {playerLives}");
        UpdateUI();
    }

    private void GameOver()
    {
        PlayerManagement.isGameOver = true;
        Debug.Log("Game Over!");
        questionText.text = "Game Over!";

        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
    }

    public int GetCurrentAnswer()
    {
        if (currentQuestionIndex < answers.Length)
        {
            return answers[currentQuestionIndex];
        }
        Debug.LogError("No valid answer available. Out of bounds!");
        return -1; // Return an invalid answer
    }

    public void DisplayNextQuestion()
    {
        currentQuestionIndex++;

        Debug.Log($"Question Index Updated: {currentQuestionIndex}");

        if (currentQuestionIndex < questions.Length)
        {
            UpdateUI();
        }
        else
        {
            PlayerManagement.isVictory = true;
            questionText.text = "Level Complete!";
            Debug.Log("All questions answered. Level complete!");

            // Play level complete sound
            if (audioSource != null && levelCompleteSound != null)
            {
                audioSource.PlayOneShot(levelCompleteSound);
            }

            // Update the user's level, passing the completed level
            UpdateUserLevel(3);

            // Save updated user data
            SaveUserData();
        }
    }

    private void UpdateUserLevel(int completedLevel)
    {
        int userId = PlayerPrefs.GetInt("LoggedInUserId");
        UserData foundUser = userList.Find(user => user.id == userId);

        if (foundUser != null)
        {
            if (completedLevel > foundUser.currentLevel)
            {
                foundUser.currentLevel = completedLevel;
                Debug.Log($"User {foundUser.username} level updated to {foundUser.currentLevel}");
            }
            else
            {
                Debug.Log($"Completed level ({completedLevel}) is not higher than current level ({foundUser.currentLevel}). No update made.");
            }
        }
        else
        {
            Debug.LogError("User not found!");
        }
    }

    private void SaveUserData()
    {
        string json = JsonUtility.ToJson(new UserDataList { users = userList });
        File.WriteAllText(filePath, json);
        Debug.Log("User data saved to file.");
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {playerScore}";
        }

        if (questionText != null && currentQuestionIndex < questions.Length)
        {
            questionText.text = questions[currentQuestionIndex];
            Debug.Log($"Updated Question: {questions[currentQuestionIndex]}");
        }
        else
        {
            Debug.LogWarning("No more questions to display or QuestionText is null.");
        }
    }

    [System.Serializable]
    public class UserDataList
    {
        public List<UserData> users;
    }

    [System.Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public int age;
        public int currentLevel;
    }
}

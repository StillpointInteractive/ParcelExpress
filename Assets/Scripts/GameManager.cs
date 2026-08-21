using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    [SerializeField] private int score = 0;

    [SerializeField] private float timer;
    [SerializeField] private float maxTime = 180f;
    [SerializeField] private bool timerRunning;

    public TMP_Text timertxt;
    public TMP_Text scoretxt;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void StartTimer(float time)
    {
        timer = time;
        timerRunning = true;
    }

    public void ParcelDelivered(Parcel parcel)
    {
        timerRunning = false;
        timer = 0f;
        AddScore(100);

       
    }
    public void MissionFailed()
    {
       
        // GameOver
    }

    private void Update()
    {
        timer = Mathf.Clamp(timer, 0f, maxTime);

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        int hundredths = Mathf.FloorToInt((timer * 100f) % 100f);

        timertxt.text = $"{minutes}:{seconds:00}.{hundredths:00}";
        scoretxt.text = "Score: " + score;

        if (timerRunning) timer -= Time.deltaTime;

        
    }


}

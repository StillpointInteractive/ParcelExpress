using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    [SerializeField] private int score = 0;

    [SerializeField] private float timer;
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

    private void Update()
    {

        timertxt.text = "Time:  " + Mathf.RoundToInt(timer);
        scoretxt.text = "Score: " + score;

        if (timerRunning) timer -= Time.deltaTime;
    }


}

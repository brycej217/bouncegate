using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get { if (_instance == null)
            {
                Debug.Log("game manager uninstantiated");
            }
            return _instance;
        }
    }

    public static int Combo;
    public static int Score;
    private float Velocity;
    private int BaseFontSize = 36;

    private static GameObject Ball;
    private static Rigidbody2D BallBody;
    private static Canvas Canvas;
    private TextMeshProUGUI ScoreText;
    private TextMeshProUGUI SpeedText;
    private GameObject LeaderboardContainer;

    private GameObject WallLeft;
    private GameObject WallRight;
    private GameObject WallUp;
    private GameObject WallDown;

    private GameObject WallLeft2;
    private GameObject WallRight2;
    private GameObject WallUp2;
    private GameObject WallDown2;

    private GameObject[] Bonuses = new GameObject[3];
    private int CurrentBonus = 0;
    private Camera MainCamera;
    private Volume GlobalVolume;
    private Vignette GateEffectsVignette;
    private LensDistortion GateEffectsDistortion;
    private Leaderboard Lb;
    private bool Slowing;
    private bool Speeding;
    private float BaseEffectsFloat = .20f;
    public bool GameOn = false;
    private Vector3 LastDir;

    private int GatesPassed;
    private bool LastPosReset;
    private Vector3 LastPos;
    
    [SerializeField] 
    private VolumeProfile GateEffects;


    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(_instance);

        Combo = 0;
        Score = 0;
        Ball = GameObject.Find("Ball");
        BallBody = Ball.GetComponent<Rigidbody2D>();

        Canvas = FindObjectOfType<Canvas>();
        ScoreText = GameObject.Find("/Canvas/ScoreContainer/Score").GetComponent<TextMeshProUGUI>();
        SpeedText = GameObject.Find("/Canvas/SpeedContainer/Speed").GetComponent<TextMeshProUGUI>();
        LeaderboardContainer = GameObject.Find("/Canvas/LeaderboardContainer");

        Bonuses[0] = GameObject.Find("/Canvas/Bonuses/First");
        Bonuses[1] = GameObject.Find("/Canvas/Bonuses/Second");
        Bonuses[2] = GameObject.Find("/Canvas/Bonuses/Third");

        Lb = GameObject.Find("/Leaderboard").GetComponent<Leaderboard>();

        MainCamera = FindObjectOfType<Camera>();
        GlobalVolume = FindObjectOfType<Volume>();
        GateEffects.TryGet<Vignette>(out GateEffectsVignette);
        GateEffects.TryGet<LensDistortion>(out GateEffectsDistortion);

        GateEffectsVignette.intensity.Override(0f);
        GateEffectsDistortion.intensity.Override(0f);

        Speeding = false;
        Slowing = false;

        GlobalVolume.profile = _instance.GateEffects;

        WallLeft = GameObject.Find("/Boundary/Wall 1");
        WallRight = GameObject.Find("/Boundary/Wall 2");
        WallUp = GameObject.Find("/Boundary/Wall 3");
        WallDown = GameObject.Find("/Boundary/Wall 4");

        WallLeft.GetComponent<Wall>().direction = Vector3.right;
        WallRight.GetComponent<Wall>().direction = Vector3.left;
        WallUp.GetComponent<Wall>().direction = Vector3.down;
        WallDown.GetComponent<Wall>().direction = Vector3.up;

        //backupwalls
        WallLeft2 = GameObject.Find("/Boundary/Wall 5");
        WallRight2 = GameObject.Find("/Boundary/Wall 6");
        WallUp2 = GameObject.Find("/Boundary/Wall 7");
        WallDown2 = GameObject.Find("/Boundary/Wall 8");

        WallLeft2.GetComponent<Wall>().direction = Vector3.right;
        WallRight2.GetComponent<Wall>().direction = Vector3.left;
        WallUp2.GetComponent<Wall>().direction = Vector3.down;
        WallDown2.GetComponent<Wall>().direction = Vector3.up;
    }

    private void Start()
    {
        ScoreText.fontSize = BaseFontSize;
        SpeedText.fontSize = BaseFontSize;
        ScoreText.text = "0";
        CurrentBonus = 0;
        LeaderboardContainer.SetActive(false);
        LastPosReset = true; 
    }

    public void StartGame() {
        _instance.GameOn = true;
        LeaderboardContainer.SetActive(false);
    }

    private void FixedUpdate() {
        SpeedTextUpdate();
    }

    private void ScoreTextUpdate(int NewScore) {
        Score += NewScore;
        ScoreText.fontSize += NewScore/2;
        ScoreText.text = Score.ToString();
        StartCoroutine(DeflateText());
    }

    IEnumerator DeflateText() {
        for(; ScoreText.fontSize > BaseFontSize; ScoreText.fontSize-= .2f) {
            yield return new WaitForSeconds(.001f);
        }
    }

    private void SpeedTextUpdate() { //updates and wobbles speed text
        Velocity = BallBody.velocity.magnitude;
        int SpeedRead;
        if(Velocity >= Ball.GetComponent<Ball>().maxSpeed) {
            SpeedRead = Random.Range(999, 9999);
        }
        else { 
            SpeedRead = (int)(Velocity * 100f - 499);
        } 
        
        SpeedText.text = SpeedRead.ToString(); 
        SpeedText.ForceMeshUpdate();

        Mesh mesh = SpeedText.mesh;
        Vector3[] meshVertices = mesh.vertices;

        for(int i = 0; i < meshVertices.Length; i++) {
            Vector3 offset = WiggleText(Velocity + i);
            meshVertices[i] = meshVertices[i] + offset;
        }

        mesh.vertices = meshVertices;
        SpeedText.canvasRenderer.SetMesh(mesh);
    }

    private Vector2 WiggleText(float wiggle) {
        return new Vector2(Mathf.Sin(Time.time * wiggle), Mathf.Cos(Time.time * wiggle));
    }
 
    public static void BallBounce(Vector3 dir) {
        _instance.Hit(dir);
        _instance.ResetBonuses();
        _instance.CalculateScore();
        _instance.LastPosReset = false;
        _instance.LastPos = Ball.transform.position;
        _instance.LastDir = BallBody.velocity.normalized;
    }

    private void Hit(Vector3 dir) {
        Vector3 prevVel = BallBody.velocity;

        Vector2 perpendicular = Vector2.Perpendicular(new Vector2(dir.x, dir.y));
        Vector3 cross = new Vector3(perpendicular.x, perpendicular.y, 0).normalized;

        Vector3 reflectedVel = Vector3.Reflect(BallBody.velocity, cross);

        BallBody.velocity = reflectedVel * 1.01f;
    }

    
    private void CalculateScore() {
        //something that combines ball velocity and combo
        int NewScore = (int) Velocity;
        NewScore = (int) (NewScore * DisplayCloseCallBonus());
        NewScore = (int)(NewScore * DisplayLongShotBonus());
        if (GatesPassed > 0) {
            NewScore = (int) (NewScore * DisplayGatesPassedBonus());
            GatesPassed = 0; 
        }
        _instance.ScoreTextUpdate(NewScore);
    }

    private float DisplayLongShotBonus() { 
        if(!LastPosReset && Vector3.Distance(LastPos, Ball.transform.position) > 12f) {
            Bonuses[CurrentBonus].SetActive(true);
            Bonuses[CurrentBonus].GetComponent<TextMeshProUGUI>().text = "Longshot | x1.15";
            if(CurrentBonus >= 2) {
            CurrentBonus = 0;
            }
            else {
            CurrentBonus++;
            }
            return 1.15f;
            
        }
        else {
            return 1f;
        } 
    }

    private float DisplayCloseCallBonus() { 
        if(Mathf.Abs(Ball.transform.position.x) > 15 || Mathf.Abs(Ball.transform.position.y) > 7.5) {
            Bonuses[CurrentBonus].SetActive(true);
            Bonuses[CurrentBonus].GetComponent<TextMeshProUGUI>().text = "Close Call | x1.25";
            if(CurrentBonus >= 2) {
                CurrentBonus = 0;
            }
            else {
                CurrentBonus++;
            }
            return 1.25f;
        }
        else {
            return 1f;
        }
    }

    private float DisplayGatesPassedBonus() {
        float multilpier = Mathf.Pow(1.1f, GatesPassed);
        
        Bonuses[CurrentBonus].SetActive(true);
        Bonuses[CurrentBonus].GetComponent<TextMeshProUGUI>().text = GatesPassed.ToString() + " Gates | x" + multilpier.ToString("0.0");

        if(CurrentBonus >= 2) {
            CurrentBonus = 0;
        }
        else {
            CurrentBonus++;
        }

        return multilpier; 
    }   

    public static void SpeedField() {
        if (_instance.Speeding) {
            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.VisualSpeedUp());
        }else if (_instance.Slowing) {
            _instance.GateEffectsVignette.color.Override(Color.red);
        }
        else { 
            _instance.StartCoroutine(_instance.VisualSpeedUp());
        }
        _instance.GatesPassed += 1;
    }

    IEnumerator VisualSpeedUp() {
        Speeding = true;
        GateEffectsVignette.color.Override(Color.red);

        for(float i = GateEffectsDistortion.intensity.value; i > -BaseEffectsFloat - ClampedVelocity(); i -= .01f) { 
            GateEffectsDistortion.intensity.Override(i);
            GateEffectsVignette.intensity.Override(-i);
            yield return new WaitForSeconds(.01f);
        }
        for(float i = GateEffectsDistortion.intensity.value; i<0; i += .01f) { 
            GateEffectsDistortion.intensity.Override(i);
            GateEffectsVignette.intensity.Override(-i);
            yield return new WaitForSeconds(.05f);
        }

        GateEffectsVignette.intensity.Override(0f);
        GateEffectsDistortion.intensity.Override(0f);
        Speeding = false;
    }

    public static void SlowField() {
        Time.timeScale = 0.5f;
        if (_instance.Speeding || _instance.Slowing)
        {
            _instance.StopAllCoroutines();
        }
        _instance.StartCoroutine(_instance.VisualTimeSlow());
        _instance.GatesPassed += 1;
    }


    IEnumerator VisualTimeSlow() {
        Slowing = true;
        GateEffectsVignette.color.Override(Color.blue);

        for(float i = GateEffectsDistortion.intensity.value; i < BaseEffectsFloat + ClampedVelocity(); i += .01f) { 
            GateEffectsDistortion.intensity.Override(i);
            GateEffectsVignette.intensity.Override(i);
            yield return new WaitForSeconds(.005f);
        }
        yield return new WaitForSeconds(.2f + (Velocity/100f));

        for(float i = GateEffectsDistortion.intensity.value; i > 0; i -= .01f) {
            GateEffectsDistortion.intensity.Override(i);
            GateEffectsVignette.intensity.Override(i);
            Time.timeScale = Mathf.Clamp(1f - i, .5f, 1f);
            yield return new WaitForSecondsRealtime(.01f);
        }

        Time.timeScale = 1f;
        GateEffectsVignette.intensity.Override(0f);
        GateEffectsDistortion.intensity.Override(0f);
        Slowing = false;
    }

    public static void WallHit(Vector3 dir) {
        _instance.WallBounce(dir);
        _instance.StartCoroutine(_instance.Lb.SubmitScoreRoutine(_instance.ResetScore()));
        _instance.StartCoroutine(_instance.Lb.FetchScores());
        _instance.LeaderboardContainer.SetActive(true);
        _instance.GameOn = false;
        _instance.LastPosReset = true;
    }
    private void WallBounce(Vector3 dir)
    {
        Debug.Log("wall");
        BallBody.velocity = Vector3.Reflect(BallBody.velocity, dir);
        BallBody.velocity *= .5f;
    }

    private void ResetBonuses() { 
        foreach (GameObject Bonus in Bonuses){
            Bonus.SetActive(false); 
        }
        CurrentBonus = 0;
    }

    private int ResetScore() {
        int total = Score;
        Score = 0;
        GatesPassed = 0;
        ScoreText.text = Score.ToString();
        ScoreText.fontSize = BaseFontSize;
        return total;
    }

    private float ClampedVelocity() {
        float speed = Velocity;
        return Mathf.Clamp((speed / 125), .05f, .25f);
    }
}

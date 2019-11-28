using UnityEngine;
using DG.Tweening;
using Cinemachine;

using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{

    public Transform LevelStartPoint;
    public Transform LastCheckpoint;
    public FreezerController LastCheckpointFreezer;
    public PlayerController player;
    public GameObject playerPrefb;
    public CinemachineVirtualCamera virtualCamera;

    public GameObject victoryMessage;
    public AudioSource bgm;
    public AudioSource victoryAudioSource;

    public bool gameEnded;
    static LevelController instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        player = Instantiate(playerPrefb, LevelStartPoint.position, Quaternion.identity).GetComponent<PlayerController>();
        virtualCamera.Follow = player.transform;

    }
    private void Update()
    {
        if (instance.player.dead)
        {
            RestartLevel();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Game");
        }

        if (player.victory && !gameEnded)
        {
            bgm.Stop();
            victoryAudioSource.Play();
            victoryMessage.SetActive(true);
            gameEnded = true;
        }
    }

    public static void RestartLevel()
    {
        if (instance.LastCheckpoint)
        {
            instance.player.myTrueFreezer = instance.LastCheckpointFreezer;
            instance.player.Restart();
        }
        else
        {
            instance.player.transform.position = instance.LevelStartPoint.position;
            instance.player.life = instance.player.maxLife;
            instance.player.thermalVisionUses = 1;
        }

        instance.player.dead = false;
    }

    public static void SetCheckpoint(FreezerController freezer)
    {
        instance.LastCheckpointFreezer = freezer;
        instance.LastCheckpoint = freezer.transform;
    }
}

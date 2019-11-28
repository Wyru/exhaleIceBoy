using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class LevelController : MonoBehaviour
{

    public Transform LevelStartPoint;
    public Transform LastCheckpoint;
    public FreezerController LastCheckpointFreezer;
    public PlayerController player;
    public GameObject playerPrefb;
    public CinemachineVirtualCamera virtualCamera;
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
        }

        instance.player.dead = false;
    }

    public static void SetCheckpoint(FreezerController freezer)
    {
        instance.LastCheckpointFreezer = freezer;
        instance.LastCheckpoint = freezer.transform;
    }
}

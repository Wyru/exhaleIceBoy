using UnityEngine;


public class FreezerController : MonoBehaviour
{

    public Transform spawnPoint;
    public int thermalVisionAmount;
    Animator animator;
    AudioSource audioSource;

    public AudioClip openSound;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Open()
    {
        PlayOpenSound();
        animator.SetTrigger("open");
    }

    public Vector2 GetSpawnPoint()
    {
        return spawnPoint.position;
    }
    public int getThermalVision()
    {
        return thermalVisionAmount;
    }

    public void PlayOpenSound(){
        audioSource.PlayOneShot(openSound);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{

    [Header("Settings")]
    [Range(0f, 10f)]
    public float speed;
    public float airModifier;
    public float jumpForce;
    public bool canDoubleJump;
    public float fallGravityScale;
    public float normalGravityScale;
    public float baseDamage;

    public float maxLife;
    public float life;

    public int thermalVisionUses = 1;
    public float thermalSensorDuration = 1f;
    public float grapWallTime;
    Wyru.Timer grapWallTimer;
    public AnimationCurve AccelerationCurve;

    [Range(0, 1)] public float accelerationTime;
    [Range(0, 1)] public float desaccelerationTime;
    Wyru.Timer accelerationTimer;
    public AnimationCurve DesaccelerationCurve;
    Wyru.Timer desaccelerationTimer;

    public AnimationCurve scaleWithLife;

    [System.Serializable]
    public class HeatDamageSetting
    {
        public float damageRate;
        public int amountOfParticles;
        public float soundEffectVol;
    }

    public HeatDamageSetting[] damageSettings;

    [Header("Sound Effects")]

    public AudioClip jumpEffect;
    public AudioClip stepEffect;
    public AudioClip deathEffect;
    public AudioClip onTermalSensorStart;
    public AudioClip onTermalSensorEnd;


    [Header("Controll Variables")]
    public bool onGround;
    public bool jumped;
    public bool doubleJumpep;
    public bool freezing;
    public bool hasControll;
    public bool dead;
    bool canFreeze;
    public bool usingThermalSensor;
    public bool victory;
    public bool onTouchWall;
    public bool grapingWall;
    public bool canGrapWall = true;
    float inputHorizontalMovement;
    float horizontalMovement;




    [Header("References")]
    public Controlls controlls;
    public GroundChecker groundChecker;
    public Tilemap heatTilemap;
    public ParticleSystem heatDamageParticles;
    public Transform thermalSensorMask;
    AudioSource audioSource;
    public AudioSource evaporatingAudioSource;


    [HideInInspector]
    public FreezerController myTrueFreezer;
    Rigidbody2D rb;
    Animator animator;
    CircleCollider2D cc2d;
    SpriteRenderer spriteRenderer;
    FreezerController freezer;


    // Start is called before the first frame update
    void Start()
    {
        life = maxLife;
        hasControll = true;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        cc2d = GetComponent<CircleCollider2D>();

        grapWallTimer = gameObject.AddComponent<Wyru.Timer>();
        grapWallTimer.SetTimer(grapWallTime);

        accelerationTimer = gameObject.AddComponent<Wyru.Timer>();
        accelerationTimer.SetTimer(accelerationTime);

        desaccelerationTimer = gameObject.AddComponent<Wyru.Timer>();
        desaccelerationTimer.SetTimer(desaccelerationTime);

        canFreeze = true;
    }


    private void OnEnable()
    {
        controlls = new Controlls();

        controlls.Enable();

        controlls.Player.Horizontal.performed += ctx =>
        {
            inputHorizontalMovement = ctx.ReadValue<float>();
        };

        controlls.Player.Jump.performed += Jump;

        controlls.Player.ThermalSensor.performed += ctx => { ThermalSensor(); };

    }

    // Update is called once per frame
    void Update()
    {
        if (!heatTilemap)
        {
            try
            {
                heatTilemap = (GameObject.FindGameObjectsWithTag("HeatMap")[0]).GetComponent<Tilemap>();
            }
            catch (System.Exception) { }
        }

        UpdateGravity();

        if (!hasControll || dead)
            return;

        UpdateMovement();
        UpdateGrapingWall();
        UpdateThermalDamage();
        UpdateSize();
    }

    void UpdateGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallGravityScale;
        }
        else if (rb.velocity.y > 0.05)
        {
            rb.gravityScale = normalGravityScale;
        }
        else
        {
            rb.gravityScale = 1;
        }
    }


    void UpdateMovement()
    {
        if (!onGround && groundChecker.IsGrounded())
        {
            jumped = false;
            doubleJumpep = false;
            canGrapWall = true;
            onGround = true;
        }
        else
        {
            onGround = groundChecker.IsGrounded();
        }

        if (Mathf.Abs(inputHorizontalMovement) > 0)
        {
            desaccelerationTimer.StopTimer();
            accelerationTimer.StartTimer(false);
            horizontalMovement = AccelerationCurve.Evaluate(accelerationTimer.GetProgress());
            horizontalMovement *= inputHorizontalMovement > 0 ? 1 : -1;
            spriteRenderer.flipX = horizontalMovement > 0;

            if (onGround)
            {
                animator.SetBool("is Walking", true);
                rb.velocity = (new Vector2(horizontalMovement * speed, rb.velocity.y));
            }
            else
            {
                if (!grapingWall)
                {
                    rb.velocity = (new Vector2(Mathf.Clamp(rb.velocity.x + (airModifier * horizontalMovement), -speed, speed), rb.velocity.y));
                }
            }
        }
        else
        {
            accelerationTimer.StopTimer();
            if (!Mathf.Approximately(rb.velocity.x, 0))
            {
                desaccelerationTimer.StartTimer(false);
                animator.SetBool("is Walking", false);
                animator.SetBool("slide", true);
            }
            else
            {
                desaccelerationTimer.StopTimer();
                animator.SetBool("slide", false);
            }
        }
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (freezer && !freezing && canFreeze)
        {
            canFreeze = false;
            myTrueFreezer = freezer;
            LevelController.SetCheckpoint(myTrueFreezer);
            freezing = true;
            hasControll = false;
            freezer.Open();

            transform.DOMoveY(transform.position.y + 0.7f, .2f)
            .SetEase(Ease.OutQuint)
            .OnComplete(() =>
            {
                audioSource.PlayOneShot(jumpEffect);
                spriteRenderer.enabled = false;
                StartCoroutine("onFreezer");
            });
        }
        else if (onGround && !jumped)
        {
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            audioSource.PlayOneShot(jumpEffect);
        }
        else if (jumped && (canDoubleJump && !doubleJumpep))
        {
            audioSource.PlayOneShot(jumpEffect);
            doubleJumpep = true;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (grapingWall && !jumped)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce + Vector2.right * jumpForce * (inputHorizontalMovement > 0 ? -1 : 1), ForceMode2D.Impulse);
            audioSource.PlayOneShot(jumpEffect);
            grapingWall = false;
            canGrapWall = true;
        }
    }


    void UpdateGrapingWall()
    {
        if (controlls.Player.Grap.ReadValue<bool>())
        {
            if (!onGround && canGrapWall)
            {

                RaycastHit2D[] hits = new RaycastHit2D[1];
                cc2d.Raycast(new Vector2(inputHorizontalMovement, 0), hits, (cc2d.radius + 0.1f) * transform.localScale.x, groundChecker.whatIsGround);
                Debug.Log(hits);

                if (hits[0])
                {
                    if (!grapingWall && !onTouchWall)
                    {
                        jumped = false;
                        doubleJumpep = false;
                        onTouchWall = true;
                        grapWallTimer.StartTimer();
                        grapingWall = true;
                    }
                }
            }
        }

        onTouchWall = false;

        if (grapingWall)
        {
            if (grapWallTimer.GetProgress() < 1)
            {
                rb.velocity = new Vector2(0, -0.01f);
            }
            else
            {
                grapingWall = false;
                canGrapWall = false;
            }
        }
    }


    void UpdateThermalDamage()
    {
        int temperature = CheckTemperature();
        TakeHeatDamage(temperature);
    }

    public void UpdateSize()
    {
        float scale = scaleWithLife.Evaluate(life / maxLife);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public int CheckTemperature()
    {
        try
        {
            Vector3Int gridPosition = heatTilemap.WorldToCell(transform.position);
            string tilename = heatTilemap.GetTile(gridPosition).name;
            switch (tilename)
            {
                case "HeatTiles_0":
                    return 0;

                case "HeatTiles_1":
                    return 1;

                case "HeatTiles_2":
                    return 2;

                case "HeatTiles_3":
                    return 3;
            }
        }
        catch (System.Exception)
        {

        }

        return 0;


    }

    public void ThermalSensor()
    {
        if (thermalVisionUses > 0 && !usingThermalSensor)
        {
            usingThermalSensor = true;
            audioSource.PlayOneShot(onTermalSensorStart);

            thermalVisionUses--;
            thermalSensorMask.DOScale(25, .6f)
            .SetEase(Ease.OutQuint)
            .OnComplete(() =>
            {
                thermalSensorMask.DOScale(25, thermalSensorDuration).OnComplete(() =>
                {
                    audioSource.PlayOneShot(onTermalSensorEnd);

                    thermalSensorMask.DOScale(0, .3f)
                    .OnComplete(() =>
                    {
                        usingThermalSensor = false;
                    });
                });

            });
        }
    }

    public void TakeHeatDamage(int level)
    {
        life = Mathf.Clamp(life - damageSettings[level].damageRate * baseDamage * Time.deltaTime, 0, maxLife);
        var emission = heatDamageParticles.emission;

        emission.rateOverTime = damageSettings[level].amountOfParticles;

        if (level > 0)
        {
            if (!evaporatingAudioSource.isPlaying)
                evaporatingAudioSource.Play();
            evaporatingAudioSource.volume = damageSettings[level].soundEffectVol;
        }
        else
        {
            evaporatingAudioSource.Stop();
        }

        if (life == 0 && !dead)
        {
            audioSource.PlayOneShot(deathEffect);
            dead = true;
        }
    }

    IEnumerator onFreezer()
    {
        yield return new WaitForSeconds(1f);
        myTrueFreezer.Open();
        yield return new WaitForSeconds(.1f);
        transform.position = myTrueFreezer.GetSpawnPoint();
        thermalVisionUses = myTrueFreezer.getThermalVision();
        life = maxLife;
        transform.localScale = new Vector3(1, 1, 1);
        spriteRenderer.enabled = true;


        transform.DOMoveY(transform.position.y + 1.5f, .4f)
            .SetEase(Ease.OutQuint).OnComplete(() =>
            {
                audioSource.PlayOneShot(jumpEffect);
                hasControll = true;
                freezing = false;
            });

        yield return new WaitForSeconds(2f);
        canFreeze = true;
    }

    public void Restart()
    {
        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero;
        life = maxLife;

        transform.DOLocalMove(myTrueFreezer.GetSpawnPoint(), 0.7f).OnComplete(() =>
        {
            thermalVisionUses = myTrueFreezer.getThermalVision();
            spriteRenderer.enabled = true;

            dead = false;

            myTrueFreezer.Open();
            transform.DOMoveY(transform.position.y + 1.5f, .4f)
            .SetEase(Ease.OutQuint).OnComplete(() =>
            {
                hasControll = true;
                freezing = false;
            });

        });



    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Freezer"))
        {
            freezer = other.GetComponent<FreezerController>();
        }

        if (other.CompareTag("Death"))
        {
            audioSource.PlayOneShot(deathEffect);
            dead = true;
        }

        if (other.CompareTag("Exit"))
        {
            victory = true;
            hasControll = false;
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Freezer"))
        {
            freezer = null;
        }
    }
}

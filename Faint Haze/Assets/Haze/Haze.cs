using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Haze : MonoBehaviour
{
    //Parametri di base del personaggio.
    public float speed;
    private float speed_aux;
    public float jump_force;
    private Rigidbody2D rb2d;
    public ContactFilter2D contactFilter;
    private Vector2 velocity;

    //Valori di stato del personaggio, usati per la detezione da parte dei nemici.
    //Lo stato Visible è quando il personaggio è pienamente visibile; Caution è per quando è solo parzialmente visibile (intangibile dentro
    //un oggetto che non lo copre totalmente); Hidden è per quando il personaggio è completamente nascosto.
    public enum Visibility { Visible, Caution, Hidden }
    public Visibility stealth_status = Visibility.Visible;
    //La direzione base e del rampino, utilizzate per monitorare sia il movimento che la direzione dello sprite.
    public enum Direction { Left, Right }
    public Direction direct = Direction.Right;
    public Direction hs_direct = Direction.Right;

    //Parametri di supporto per le funzionalità base del personaggio, come il movimento.
    protected Vector2 move_save;
    protected float hookshot_position;
    public Transform groundCheckPos;
    public float circleRadius;

    //Parametri di controllo per l'aspetto grafico del personaggio.
    public SpriteRenderer sprite;
    public Sprite sprite_up;
    public Sprite sprite_down;

    public BoxCollider2D collider_up;
    public CircleCollider2D collider_down;

    public Animator haze_anim;

    private float velocity_ctrl;

    //Parametri di controllo per vedere se il personaggio può compiere l'azione specificata.
    public bool canUseSkill01;
    public bool canUseSkill02;
    public bool canClimb;
    public bool canMove;
    public bool canUseHookshot;

    //Parametri di controllo per vedere le condizioni attuali del personaggio.
    public bool isStuck = false;
    public bool isCrouched;
    public bool isClimbing;
    public bool isIntangible;
    public bool isStunned;
    public bool isGrounded;
    private bool flipSprite;

    //Parametri della Skill 1 (Intangibilità).
    public int skill01_duration;
    public int skill01_cooldown;
    public int skill01_stunduration;

    //Parametri della Skill 2 (Lancio feromoni).
    public GameObject skill02_object;
    public GameObject skill02_object_instance;
    public Transform skill02_throw_point;
    public int skill02_cooldown;

    //Valori di funzionalità del rampino.
    private float rc_distance;
    private float rc_target_distance;
    public float hookshot_speed;
    public LayerMask rc_hookshot_mask = 10;
    private RaycastHit2D rc_hookshot_hit;

    //Parametri di controllo della User Interface.
    public Text hookshot_indicator;

    IEnumerator Skill01Cooldown()
    {
        //Aspetta i secondi specificati nella variabile intera "skill01_cooldown" per riabilitare l'uso dell'intangibilità.

        yield return new WaitForSeconds(skill01_cooldown);

        canUseSkill01 = true;
    }

    IEnumerator Stun()
    {
        //Stordisce il personaggio per la durata specificata nella variabile "skill01_stunduration".
        //Il movimento e la capacità di compiere azioni e usare oggetti o abilità vengono disabilitati tramite le due variabili booleane "canMove" e "isStunned".
        //canMove disabilita il movimento. isStunned previene che il giocatore compia azioni come rampino o feromoni durante lo stordimento.

        canMove = false;
        speed = 0;
        isStunned = true;

        yield return new WaitForSeconds(skill01_stunduration);

        //Dopo aver esaurito il tempo di stordimento, le capacità del personaggio vengono ripristinate.

        canMove = true;
        speed = speed_aux;
        isStunned = false;
    }

    IEnumerator Skill01()
    {
        //Questa coroutine gestisce l'uso dell'abilità dell'Intangibilità.
        //Per prima cosa disabilita l'uso ripetuto dell'abilità tramite la booleana "canUseSkill01". Poi setta la booleana isIntangible per
        //comunicare al gioco che il personaggio è adesso intangibile. Ciò serve agli oggetti per far partire i rispettivi script.

        canUseSkill01 = false;
        isIntangible = true;

        //anim.SetTrigger("Intangible");


        //Cambia il layer del personaggio, alterando la sua capacità di collidere con determinati oggetti. Serve principalmente per entrare
        //negli oggetti designati. Dopodiché aspetta per la durata dell'abilità specificata nella variabile skill01_duration per far proseguire
        //lo script e terminare gli effetti dell'intangibilità.
        gameObject.layer = 9;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));

        yield return new WaitForSeconds(skill01_duration);

        //Ripristina la normale capacità di collisione.
        gameObject.layer = 0;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));

        //La booleana isStuck viene settata a vero dagli ostacoli quando il personaggio ci entra dentro, e viene messa a falso quando il personaggio esce.
        //Se il personaggio è ancora all'interno dell'ostacolo quando l'abilità termina, fa partire la funzione dello stordimento.
        if (isStuck)
        {
            StartCoroutine("Stun");
        }

        //Disabilita la condizione di intangibilità, facendo tornare il personaggio nello stato normale e facendo partire il cooldown.
        isIntangible = false;
        StartCoroutine("Skill01Cooldown");
    }

    IEnumerator Skill02()
    {
        //Questa coroutine gestisce l'uso dell'abilità dei Feromoni.
        //Per prima cosa disabilita l'uso ripetuto dell'abilità tramite la booleana "canUseSkill02". Poi instanzia il Game Object dei feromoni, prendendo come
        //coordinate la posizione del Game Object vuoto legato al personaggio di Haze.
        canUseSkill02 = false;

        skill02_object_instance = Instantiate(skill02_object, skill02_throw_point.position, skill02_throw_point.rotation);

        //Attende la durata specificata nella variabile skill02_cooldown per far terminare lo script.

        yield return new WaitForSeconds(skill02_cooldown);

        //Riabilita l'uso dell'abilità.
        
        canUseSkill02 = true;
        
    }

    IEnumerator JumpDirection()
    {
        //Questa coroutine gestisce la funzione del salto.
        //La coroutine aspetta un decimo di secondo per far partire la funzione del salto. Ciò evita eventuali bug o contrasti con
        //l'abilità del movimento.
        //yield return new WaitForSeconds(0.15f);

        //Disabilita il normale movimento del personaggio.
        canMove = false;

        //Attende che il personaggio torni a terra per riabilitare il movimento.
        yield return new WaitUntil(GroundedCheck);

        //Riabilita il movimento del personaggio e gli restituisce la fisica personalizzata.
        canMove = true;
        //rb2d.bodyType = RigidbodyType2D.Kinematic;
    }

    IEnumerator ActiveHookshot()
    {
        //Questa coroutine gestisce la funzione del rampino.
        //Per prima cosa azzera completamente la velocità del personaggio, per evitare eventuali bug dovuti alla fisica personalizzata e al movimento.
        isGrounded = false;
        move_save = Vector2.zero;

        //Verifica se c'è un contrasto tra la direzione del personaggio e quella del punto d'appiglio. Se le due non coincidono, il personaggio viene
        //fatto ruotare nella stessa direzione del punto d'appiglio. Ciò crea un effetto visivo più coerente.
        if (hs_direct==Direction.Right && direct==Direction.Left)
        {
            direct = Direction.Right;
            sprite.flipX = !sprite.flipX;
        }
        if(hs_direct==Direction.Left && direct == Direction.Right)
        {
            direct = Direction.Left;
            sprite.flipX = !sprite.flipX;
        }

        //Una volta corretto (se necessario) la direzione del personaggio, si disabilita il movimento del personaggio.
       

        //Al personaggio viene data una velocità puramente verticale.
        rb2d.velocity = new Vector2(rb2d.velocity.x, hookshot_speed);

        //Aspetta fin quando il personaggio non raggiunge l'altezza designata.

        yield return new WaitUntil(HeightCheck);

        //In base alla direzione (corretta in precedenza) del personaggio, esso viene spinto sulla piattaforma.
        if (hs_direct == Direction.Right)
            rb2d.velocity = new Vector2(10, 0);
        else
            rb2d.velocity = new Vector2(-10, 0);

        //Aspetta un decimo di secondo per stabilizzare le funzioni del gioco.
        yield return new WaitForSeconds(0.1f);

        //Azzera nuovamente la velocità per contrastare eventuali bug con la fisica personalizzata. Dopodiché ripristina l'abilità di movimento.
        rb2d.velocity = Vector2.zero;
        canMove = true;
    }

    public void Death()
    {
        haze_anim.SetTrigger("death_anim");
        Time.timeScale = 0;
        //gameOverPanel.SetActive(true);
    }

    public void OnDrawGizmos()
    {
        //Una funzione puramente di debug che serve a controllare la funzione del rampino.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 10, 0));

        Gizmos.DrawWireSphere(groundCheckPos.position, circleRadius);
    }

    public bool GroundedCheck()
    {
        //Una funzione puramente di supporto che controlla se il personaggio è a terra o meno.

        if (isGrounded)
            return true;
        else
            return false;
    }

    public void GroundControl()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, circleRadius, 1 << LayerMask.NameToLayer("Ground"));


    }

    /*public bool HeightCompare(Vector3 first, Vector3 second)
    {
        if (first.y >= second.y + 5f)
        {
            Debug.Log(first.y);
            Debug.Log(second.y);
            return true;
        }

        else
            return false;
    }*/

    public bool HeightCheck()
    {
        //Una funzione di supporto per il rampino. Controlla l'altezza del personaggio rispetto all'altezza specificata dal punto d'appiglio.
        //Serve ad arrestare la salita del personaggio quando arriva all'altezza giusta.

        if (transform.position.y > hookshot_position)
            return true;
        else
            return false;
    }

    private void Awake()
    {
        haze_anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //La funzione Start inizializza alcune importanti variabili quando il giocatore spawna nel livello.

        speed = 5;
        speed_aux = speed;

        canUseSkill01 = true;
        canUseSkill02 = true;
        canMove = true;

        rc_distance = 10f;
        Physics2D.IgnoreLayerCollision(11, 13);
        sprite = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()                                                  
    {
        //Controlla, ogni frame, se è il personaggio è a terra o meno.
        GroundControl();

        //Controlla se il personaggio può muoversì. Se può, questa funzione entra in azione e gestisce il movimento standard.
        if (isGrounded)
        {
            //Stabilisce la velocità calcolandola secondo la direzione presa in input e la velocità del personaggio.
            //Inoltre salva la direzione in una variabile ausiliaria.
            Vector2 move = Vector2.zero;
            move.x = Input.GetAxisRaw("Horizontal");
            rb2d.velocity = new Vector2(move.x * speed, 0);
            speed = speed_aux;
            move_save = move;
            haze_anim.SetFloat("velocity_anim", Mathf.Abs(rb2d.velocity.x));

            //Gira il personaggio quando cambia direzione.
            flipSprite = (sprite.flipX ? (move.x > 0.01f) : (move.x < -0.01f));
            
            if (flipSprite)
            {
                sprite.flipX = !sprite.flipX;
                if (direct == Direction.Right)
                    direct = Direction.Left;
                else
                    direct = Direction.Right;
            }

        }
     //Per sopra: la seconda targetVelocity viene usata durante il salto: quando il personaggio non può muoversi ma ha comunque una velocità,
        //continua nel suo moto senza aver bisogno dell'input del giocatore.


        //Il salto. Per saltare controlla che il personaggio sia a terra e non sia stordito. In input prende il pulsante del salto.
        if (Input.GetButtonDown("Jump") && isGrounded && !isStunned)
        {
            Debug.Log("Jumping");

            StartCoroutine("JumpDirection");
            //Il salto varia in base al fatto se il personaggio si sia muovendo o meno.
            if (rb2d.velocity != Vector2.zero)
            {
                //Se si sta muovendo, il salto viene calcolato in base alla direzione e la forza del salto.
                rb2d.velocity = ((Vector2.up + move_save) * jump_force * Time.fixedDeltaTime);

            }
            else
            {
                //Altrimenti, il salto viene calcolato in base alla sola forza del salto, con direzione standard in sù.
                rb2d.velocity = (Vector2.up * jump_force * Time.fixedDeltaTime);
                // rb2d.AddForce(Vector2.up * jump_force, ForceMode2D.Impulse);
            }

            //Fa partire la coroutine che gestisce il movimento durante il salto.
        }


       
        //Il rampino. Per essere usato controlla se il personaggio sia a terra e non sia stordito. Prende in input il pulsante specificato.
        if (Input.GetKeyDown(KeyCode.B) && isGrounded && !isStunned && !isCrouched)
        {
            //Quando azionato, controlla se effettivamente c'è qualcosa che possa essere usato nella funzione. Il rampino parte solo e unicamente
            //quando c'è un elemento sopra il personaggio che ne permette l'uso.
            if (Physics2D.Raycast(transform.position, Vector2.up, rc_distance, rc_hookshot_mask.value))
            {
                //Prende le informazioni riguardo l'oggetto colpito, specificamente la sua posizione.
                hookshot_position = Physics2D.Raycast(transform.position, Vector2.up, rc_distance, rc_hookshot_mask.value).transform.position.y;
                rc_hookshot_hit = Physics2D.Raycast(transform.position, Vector2.up, rc_distance, rc_hookshot_mask.value);


                //Gli effetti variano a seconda dell'elemento colpito. Se è un punto d'appiglio, controlla la sua direzione e fa partire lo script
                //del rampino. Altrimenti, se è un vespaio (interazione ambientale), fa partire lo script dell'interazione.
                if (rc_hookshot_hit.collider.gameObject.tag == "hookshot_point_right")
                {
                    hs_direct = Direction.Right;
                    StartCoroutine("ActiveHookshot");
                }

                else if (rc_hookshot_hit.collider.gameObject.tag == "hookshot_point_left")
                {
                    hs_direct = Direction.Left;
                    StartCoroutine("ActiveHookshot");
                }

                else if (rc_hookshot_hit.collider.gameObject.tag == "hookshot_interaction")
                    rc_hookshot_hit.collider.gameObject.GetComponent<InteractionWasps>().ReduceEpsilonDetection();

            }

        }

        


       

    }

    //La funzione LateUpdate viene chiamata 1 frame dopo ComputeVelocity. Ciò serve ad evitare contrasti con le varie funzioni che possono essere
    //attivate nello stesso frame.
    private void LateUpdate()
    {
        //Se il pulsante dell'intangibilità viene premuto, e se si può effettivamente usare l'abilità, aziona il rispettivo script.
        if (Input.GetButtonDown("Intangibility"))
        {
            if (canUseSkill01)
            {
                StartCoroutine("Skill01");
            }
        }

        //Stessa cosa per il lancio dei feromoni.
        if (Input.GetButtonDown("Throw"))
        {
            if (canUseSkill02)
            {
                StartCoroutine("Skill02");
            }
        }

        //Il crouching.
        //Se il personaggio può muoversi normalmente, alla pressione del pulsante specificato verifica se il personaggio sia già abbassato.
        //In base allo stato del personaggio, cambia il collider e lo stato di crouching del personaggio.
        if (Input.GetKeyDown(KeyCode.Q) && isGrounded == true && canMove == true && isCrouched == false)
        {
            Debug.Log("Crouch");

            speed = speed / 2;
            collider_down.enabled = true;
            collider_up.enabled = false;
            isCrouched = true;
            sprite.sprite = sprite_down;

            haze_anim.SetBool("isCrouching_anim", true);

        }
        else if (Input.GetKeyDown(KeyCode.Q) && isGrounded == true && canMove == true && isCrouched == true)
        {
            Debug.Log("Crouch");
            speed = speed * 2;
            collider_up.enabled = true;
            collider_down.enabled = false;
            isCrouched = false;
            sprite.sprite = sprite_up;

            haze_anim.SetBool("isCrouching_anim", false);
        }
    }

   /* private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("floor"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("floor"))
        {
            isGrounded = false;
        }
    }*/

}

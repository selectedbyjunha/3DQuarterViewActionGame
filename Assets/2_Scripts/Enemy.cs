using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C };           // Enemy Type
    public Type enemyType;                  // Enemy Type 을 집어넣을 Type 변수

    public int maxHealth;
    public int curHealth;
    public Transform target;            // 몬스터는 플레이어를 따라온다.
    public BoxCollider meleeArea;       // 몬스터가 플레이어를 피격하는 Collider
    public GameObject bullet;           // 원거리 몬스터가 쏘는 총알
    public bool isChase;                // 추적하고 있는지 Check
    public bool isAttack;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;                   // 몬스터가 플레이어를 따라갈 수 있게 하는 NavMeshAgent
    Animator anim;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;    //  Material은 Mesh Renderer 컴포넌트에서 접근 가능!
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);       // Invoke("함수명", 시간);
    }

    void ChaseStart() {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update() {     // 다양한 몬스터 만들기 편에서 업데이트 하는 이유 다시 듣기!
        if ( nav.enabled ) {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }


    // 물리력이 NavAgent 이동을 방해하지 않도록 로직 추가
    void FreezeVelocity() {
        if ( isChase ) {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void Targeting() {
        float targetRadius = 0;
        float targetRange = 0;

        switch ( enemyType ) {
            case Type.A :
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
            case Type.B :
                targetRadius = 1f;
                targetRange = 12f;
                break;
            case Type.C :
                targetRadius = 0.5f;
                targetRange = 25f;
                break;
        }

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if ( rayHits.Length > 0 && !isAttack ) {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack() {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);
        
        switch ( enemyType ) {
            case Type.A :
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;

            case Type.B :
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce( transform.forward * 20, ForceMode.Impulse );
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                
                yield return new WaitForSeconds(2f);
                break;

            case Type.C :
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }


        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }


    void FixedUpdate() {
        Targeting();
        FreezeVelocity();
    }


    void OnTriggerEnter(Collider other) {

        // Debug.Log("other.tag : " + other.tag);
        /*
         * 근접 공격 구현 못했음!! (버그 있음 ㅠㅠ)
         */
        if ( other.tag == "Melee" ) {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            // Debug.Log("Melee : " + curHealth);
            
            // Knock-back 을 위한 변수 reactVec 선언
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec));
        } else if ( other.tag == "Bullet" ) {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            // Debug.Log("Range : " + curHealth);

            // Knock-back 을 위한 변수 reactVec 선언
            Vector3 reactVec = transform.position - other.transform.position;
            
            // 총알이 관통하지 못하도록 제어
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec) {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if ( curHealth > 0 ) {
            mat.color = Color.white;
        } else {
            mat.color = Color.gray;
            gameObject.layer = 14;
            
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);

            Destroy(gameObject, 4);
        }
    }
}

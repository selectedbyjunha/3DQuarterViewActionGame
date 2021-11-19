using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    
    public GameObject[] weapons;        // 플레이어가 들고 있는 무기의 개수
    public bool[] hasWeapons;           // 무기를 들고 있는지 여부
    
    public GameObject[] grenades;       // 공전물체(수류탄) 를 컨트롤하기 위해 배열 변수 생성
    public int hasGrenades;             // 공전물체(수류탄) 개수
    public GameObject grenadeObj;    // 필살기로 던질 수류탄

    public Camera followCamera;         // 총알 쏠 때 카메라 방향

    public int ammo;                    // 총알
    public int coin;                    // 코인
    public int health;                  // HP

    // 플레이어가 가진 자산 max 값 설정
    public int maxAmmo;                 
    public int maxCoin;                 
    public int maxHealth;               
    public int maxHasGrenade;           

    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool fDown;             // 공격하기 (Fire)
    bool gDown;             // 수류탄 던지기
    bool rDown;             // 장전하기 (reload)
    bool iDown;             // 무기 입수
    bool sDown1;            // 무기 줍기1 (Swap)
    bool sDown2;            // 무기 줍기2 (Swap)
    bool sDown3;            // 무기 줍기3 (Swap)

    [SerializeField]
    private float jumpPower;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;      // 공격 delay 종료 후, 공격 준비 완료 시그널
    bool isBorder;
    bool isDamage;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator animator;
    MeshRenderer[] meshs;

    GameObject nearObject;
    Weapon equipWeapon;         // 장착 중인 무기
    int equipWeaponIndex = -1;
    float fireDelay;


    // Awake() == 초기화
    private void Awake() {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interaction();
        //FixedUpdate();
        Grenade();
    }

    void GetInput() {
        hAxis = Input.GetAxisRaw("Horizontal");     // 수평 이동
        vAxis = Input.GetAxisRaw("Vertical");       // 수직 이동
        wDown = Input.GetButton("Walk");            // Shift를 누르고 있을 때만 작동하도록!
        jDown = Input.GetButtonDown("Jump");        // Space바를 누르는 즉시 점프가 되도록
        fDown = Input.GetButton("Fire1");       // 마우스 왼쪽 버튼을 누르면 공격
        gDown = Input.GetButtonDown("Fire2");       // 마우스 오른쪽 버튼 누르면 필살기 (수류탄 던지기)
        rDown = Input.GetButtonDown("Reload");       // 장전
        iDown = Input.GetButtonDown("Interaction");        // e 키를 누르면 무기 입수
        sDown1 = Input.GetButtonDown("Swap1");        // e 키를 누르면 무기 입수
        sDown2 = Input.GetButtonDown("Swap2");        // e 키를 누르면 무기 입수
        sDown3 = Input.GetButtonDown("Swap3");        // e 키를 누르면 무기 입수
    }

    void Move() {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;      // x, y, z축 이동 후, normalized로 정규화

        if ( isDodge ) {
            moveVec = dodgeVec;
        }
        if ( isSwap || isReload || !isFireReady ) {
            moveVec = Vector3.zero;
        }

        // 벽에 부딪혔을 때, 더 가는 동작만 못하도록 제어를 해준다.
        if ( !isBorder ) {
            if ( wDown ) {
                transform.position += moveVec * speed * 0.3f * Time.deltaTime;
            } else {
                transform.position += moveVec * speed * Time.deltaTime;
            }
        }

        animator.SetBool("isRun", moveVec != Vector3.zero);
        animator.SetBool("isWalk", wDown); 
    }

    void Turn() {
        // 1. 키보드에 의한 회전 (WASD, 방향키)
        transform.LookAt(transform.position + moveVec);     // 몸 회전하기

        // 2. 마우스에 의한 회전 (이해 잘 안되니깐 반복해서 볼 것!)
        if ( fDown ) {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);          // 스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if ( Physics.Raycast(ray, out rayHit, 100) ) {                         // out : return처럼 반환값을 주어진 변수에 저장하는 키워드
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
        
    }

    void Jump() {
        if ( jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap ) {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);      // ForceMode.Impulse : 즉각적으로 반응할 수 있음(?)
            animator.SetBool("isJump", true);
            animator.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Attack() {
        // 무기 안들고 있으면 그냥 return
        if ( equipWeapon == null ) {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        // 공격속도 쿨타임이 지나면 다시 휘두를 수 있음 (doSwing)
        if ( fDown && isFireReady && !isDodge && !isSwap ) {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Grenade() {

        if ( hasGrenades == 0 ) {
            
            Debug.Log("hasGrenades == 0?");
        
            return;
        }

        if ( gDown && !isReload && !isSwap ) {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);          // 스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if ( Physics.Raycast(ray, out rayHit, 100) ) {                         // out : return처럼 반환값을 주어진 변수에 저장하는 키워드
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 2;
                // transform.LookAt(transform.position + nextVec);

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Reload() {
        if ( equipWeapon == null ) {
            return;
        }
        if ( equipWeapon.type == Weapon.Type.Melee ) {
            return;
        }
        if ( ammo == 0 ) {
            return;
        }

        // 조건부터 걸어주고, Animator 구현!
        if ( rDown && !isJump && !isDodge && !isSwap && isFireReady ) {
            animator.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut() {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge() {
        if ( jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap) {
            dodgeVec = moveVec;
            speed *= 2;
            animator.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);       // Invoke("함수이름", 시간차이);
        }
    }

    void DodgeOut() {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap() {
        // 무기 들고 있는지 || 이미 해당 무기 들고 있는지 체크하기
        if ( sDown1 && ( !hasWeapons[0] || equipWeaponIndex == 0 ) ) {
            return;
        }
        if ( sDown2 && ( !hasWeapons[1] || equipWeaponIndex == 1 ) ) {
            return;
        }
        if ( sDown3 && ( !hasWeapons[2] || equipWeaponIndex == 2 ) ) {
            return;
        }


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        // 무기 바꿔주기
        if ( ( sDown1 || sDown2 || sDown3 ) && !isJump && !isDodge ) {
            if ( equipWeapon != null ) {
                //equipWeapon.SetActive(false);               // nullException 에러 유발 가능해서 if절로 감쌈
            equipWeapon.gameObject.SetActive(false);      // GameObject 객체가 아니라 Weapon 으로 변경했기 때문
            }

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            //weapons[weaponIndex].SetActive(true);     // 첫 번째 작성했던 소스
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.5f);
        }
    }

    void SwapOut() {
        isSwap = false;
    }

    void Interaction() {
        if ( iDown && nearObject != null && !isJump && !isDodge ) {
            if ( nearObject.tag == "Weapon" ) {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                // 먹은 아이템 사라지기
                Destroy(nearObject);
            }
        }
    }

    // 플레이어의 자동회전 제어하기
    void FreezeRotation() {
        rigid.angularVelocity = Vector3.zero;           // 물리 회전 속도
    }

    void StopToWall() {
        Debug.DrawRay( transform.position, transform.forward * 5, Color.green );
        isBorder = Physics.Raycast( transform.position, transform.forward, 5, LayerMask.GetMask("Wall") );
    }

    void FixedUpdate() {
        FreezeRotation();
        StopToWall();
    }

    // 태그 설정해주는 원리를 잘 모르겠음 ;;
    void OnCollisionEnter(Collision collision) {
        if ( collision.gameObject.tag == "Floor" ) {
            animator.SetBool("isJump", false);
            isJump = false;
        }
    }

    // Weapon 획득 이벤트!
    void OnTriggerStay(Collider other) {
        if ( other.tag == "Weapon" ) {
            nearObject = other.gameObject;
        }

        //Debug.Log(nearObject.name);
    }
    
    void OnTriggerExit(Collider other) {
        if ( other.tag == "Weapon" ) {
            nearObject = null;
        }

        // Debug.Log(nearObject.name);
    }

    void OnTriggerEnter(Collider other) {

        if ( other.tag == "Item" ) {
            
            // 테스트
            Debug.Log("Player - OnTriggerEnterEvent : Item Eat test");

            Item item = other.GetComponent<Item>();
            switch( item.type ) {
                case Item.Type.Ammo :
                    ammo += item.value;
                    if ( ammo > maxAmmo ) {
                        ammo = maxAmmo;
                    }
                    break;
                case Item.Type.Coin :
                    coin += item.value;
                    if ( coin > maxCoin ) {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Heart :
                    health += item.value;
                    if ( health > maxHealth ) {
                        health = maxHealth;
                    }
                    break;
                case Item.Type.Grenade :
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if ( hasGrenades > maxHasGrenade ) { 
                        hasGrenades = maxHasGrenade;
                    }
                    break;
            }
            
            // 먹은 아이템 삭제
            Destroy(other.gameObject);
        }
        
        else if ( other.tag == "EnemyBullet" ) {
            if ( !isDamage ) {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                // 원거리 몬스터에게 총알 맞으면 총알 사라지도록 설정
                if ( other.GetComponent<Rigidbody>() != null) {
                    Destroy(other.gameObject);
                }

                StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage() {
        
        isDamage = true;

        // 1초 동안 무적 타임 발생 시키기
        foreach ( MeshRenderer mesh in meshs ) {
            mesh.material.color = Color.yellow;
        }

        yield return new WaitForSeconds(1f);

        isDamage = false;
        
        // 무적 타임 종료 후, 정상으로 되돌리기
        foreach ( MeshRenderer mesh in meshs ) {
            mesh.material.color = Color.white;
        }
    }
}

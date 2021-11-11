using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed;
    float hAxis;
    float vAxis;
    bool wDown;

    Vector3 moveVec;

    Animator animator;

    // Awake() == 초기화
    private void Awake() {
        animator = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal");     // 수평 이동
        vAxis = Input.GetAxisRaw("Vertical");       // 수직 이동
        wDown = Input.GetButton("Walk");            // Shift를 누르고 있을 때만 작동하도록!

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;      // x, y, z축 이동 후, normalized로 정규화

        if ( wDown ) {
            transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        } else {
            transform.position += moveVec * speed * Time.deltaTime;
        }

        animator.SetBool("isRun", moveVec != Vector3.zero);
        animator.SetBool("isWalk", wDown);

        transform.LookAt(transform.position + moveVec);     // 몸 회전하기
        
    }
}

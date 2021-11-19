using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;            // 공전하게 될 객체
    public float orbitSpeed;            // 공전 속도
    Vector3 offSet;


    // Start is called before the first frame update
    void Start()
    {
        // 거리 유지를 위한 offSet 변수 생성
        offSet = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        // target이 움직일 때 마다 offSet 만큼 더한 값을 transform 에 더해줌
        transform.position = target.position + offSet;

        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);           // 타겟 주위를 회전하는 함수
        
        // 이동한 만큼 다시 한번 offSet 업데이트 : Walk/Run/Jump/Dodge 등에 따라 target 포지션이 드라마틱하게 바뀔 수 있으므로
        offSet = transform.position - target.position;
    }
}

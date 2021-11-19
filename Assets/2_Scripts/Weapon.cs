using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;         // 최대 탄창
    public int curAmmo;         // 현재 총알 남은 수

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;         // 총알 나가는 위치
    public GameObject bullet;           // 총알
    public Transform bulletCasePos;     // 탄피 떨어지는 위치
    public GameObject bulletCase;       // 탄피

    public void Use() {
        if ( type == Type.Melee ) {
            // Swing();     // Co-Routine 함수 사용하는 방법은 살짝 다름!
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        } else if ( type == Type.Range && curAmmo > 0 ) {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing() {

        // 1. 번 구역 실행
        // yield return null;      // 1프레임 대기
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        // 2. 번 구역 실행
        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;
        
        // 3. 번 구역 실행
        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;

        yield break;
    }

    // Use() 함수 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴
    // Use() 함수 메인루틴 + Swing() 코루틴 (Co-Op)
    // yield : 결과를 전달하는 키워드 > Co-Routine 은 yield 키워드를 꼭 필요로 함
    // yield 키워드를 여러 개 사용하여 시간차 로직 작성 가능함

    
    IEnumerator Shot() {
        // 1. 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
        yield return null;      // 1프레임 휴식

        // 2. 탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}

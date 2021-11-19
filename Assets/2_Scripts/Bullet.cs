using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;

    // Bullet 이 Floor 에 떨어졌을 때
    void OnCollisionEnter(Collision other) {
        if ( other.gameObject.tag == "Floor" ) {
            Destroy(gameObject, 3);     // 3초 후에 사라진다.
        }
    }

    // Bullet이 벽에 부딫혔을 때,
    void OnTriggerEnter(Collider other) {
        // 근접 공격인 몬스터가 벽에 붙어있는 플레이어를 공격했을 때, Collider가 사라지지 않도록 !isMelee
        if ( !isMelee && other.gameObject.tag == "Wall" ) {
            Destroy(gameObject);        // 딜레이 없이 바로 사라진다.
        }
    }
}

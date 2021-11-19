using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type type;
    public int value;
    [SerializeField]
    private float rotateVelocity = 10;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    private void Awake() {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();    // GetComponent는 Unity Editor의 첫번째 컴포넌트만 가져오기 때문에 순서 설정을 잘해주어야 한다!!!!
    }

    void Update() {
        transform.Rotate(Vector3.up * 10 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other) {
        if ( other.gameObject.tag == "Floor" ) {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}

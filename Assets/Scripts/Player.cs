using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float JumpPower;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool isJump;
    bool sDown1; //숫자 1를 눌렀을때
    bool sDown2; //숫자 2를 눌렀을때

    //플레이어 무기 관련 배열
    public GameObject[] weapons;
    public bool[] hasWeapons;

    public int health;
    public int maxHealth;

    //중력 변수
    public float gravity = -20.0f;
    Vector3 moveVec;
    //수직 속도 변수
    float yVelocity = 0;

    Rigidbody rigid;
    Animator anim;

    GameObject nearWeapon; //트리거된 무기들을 저장하기 위한 변수
    Weapon equipWeapon; //기존에 장착된 무기를 저장하는 변수
    int equipWeaponIndex=-1;
   

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Jump();
        //캐릭터의 수직속도(중력)을 적용한다
        yVelocity += gravity * Time.deltaTime;
        moveVec.y = yVelocity;
        Interaction_W();//무기 상호작용
        Swap();
        Attack();
    }
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        sDown1 = Input.GetButtonDown("sDown1");
        sDown2 = Input.GetButtonDown("sDown2");
    }

    //플레이어 움직이게 하는 함수
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        moveVec = Camera.main.transform.TransformDirection(moveVec);

        if (wDown)
            transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        else
            transform.position += moveVec * speed * Time.deltaTime;
       

        transform.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    
    //플레이어 점프하게 하는 함수
    void Jump()
    {
        if(jDown&&!isJump)
        {
            rigid.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
            anim.SetBool("isJump",false);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Swap() //무기 교체
    {
        // 무기를 갖고 있지 않는 상태에서 무기가 활성화되는 것을 방지
        if (sDown1 && (!hasWeapons[0]||equipWeaponIndex==0))
        {
            return;
        }
        if (sDown2 && (!hasWeapons[1]||equipWeaponIndex==1))
        {
            return;
        }

        int weaponIndex = -1;
        //숫자 1을 누르면 총, 숫자2를 누르면 수류탄
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;

        //1,2를 누르면 무기가 보이게 함.
        if (sDown1 || sDown2)
        {
            if (equipWeapon != null) //빈손이 아니라면 
            {
                equipWeapon.gameObject.SetActive(false); //전에 손에 쥐고 있었던 무기를 비활성화함
            }

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true); // 손에 쥐고 있는 무기를 활성화하여 보이게함.
        }
    }

    void Attack() //공격
    {
        if (equipWeapon == null) //무기를 갖고 있는 경우에만 실행
            return;
        if (Input.GetMouseButtonDown(0))
        {
            equipWeapon.Use();
            anim.SetTrigger("doShot");
        }
    }

    void Interaction_W() //닿은 무기들을 배열에 저장
    {
        if (nearWeapon != null)
        {
            if (nearWeapon.tag == "Weapon")
            {
                Item item = nearWeapon.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true; //해당 무기가 플레이어한테 갖고 있다고 설정

                Destroy(nearWeapon);
            }
        }
    }

    //플레이어가 Floor로 태그된 물체와 닿으면 착지하는 모션을 멈춤
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", true);
            isJump = false;
        }

    }

    //플레이어가 아이템이라고 태그된 물체와 닿으면 일어나는 일
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                //아이템 스크립트에서 지정한 타입과 value 값에 따라 health 가 늘어남
                //health 가 maxhealth를 넘어서면 health 값이 maxhealth 값이 됨
                case Item.Type.Ammo:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
            }
            //아이템은 닿으면 사라짐
            Destroy(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other) //무기 콜라이더에 닿으면(=무기 오브젝트 감지)
    {
        if (other.tag == "Weapon")
        {
            nearWeapon = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other) //무기 콜라이더에서 빠져나오면
    {
        if (other.tag == "Weapon")
        {
            nearWeapon = null;
        }
    }
}

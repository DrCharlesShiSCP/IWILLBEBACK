using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private HitscanWeapon currentWeapon;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float adsFov = 55f;
    float defaultFov;

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera) defaultFov = playerCamera.fieldOfView;
    }

    void Update()
    {
        if (!currentWeapon) return;

        // ADS
        bool isADS = Input.GetMouseButton(1);
        currentWeapon.IsADS = isADS;
        if (playerCamera)
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, isADS ? adsFov : defaultFov, Time.deltaTime * 12f);

        // Fire
        if (currentWeapon.CanHoldToFire())
        {
            if (Input.GetMouseButton(0)) currentWeapon.TryFire();
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) currentWeapon.TryFire();
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R)) currentWeapon.StartReload();
    }

    //public API to swap/equip
    public void Equip(HitscanWeapon weapon)
    {
        currentWeapon = weapon;
    }
}

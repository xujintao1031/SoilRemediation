using GasChromatography.Manager;
using UnityEngine;

public class RoleHandle : Singleton<RoleHandle>
{
    public Transform Role;

    public Transform Face;

    public Transform Hand;

    public Transform Eye;

    public Transform Foot;

    public Transform Head;

    public Transform Body;

    public Transform Up;

    public Transform Down;

    public Camera RoleCamera;
    void Start()
    {
        // RoleCamera.targetTexture = ResLoader.Allocate().LoadSync<RenderTexture>("RoleTexture");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// 旋转
    /// </summary>
    /// <param name="p"></param>
    public void TranslateTheTrans(float p) 
    {
        Role.transform.localEulerAngles += new Vector3(0, p, 0);
    }

    /// <summary>
    /// 穿戴装备
    /// </summary>
    /// <param name="sort"></param>
    /// <param name="ClothesName"></param>
    public void ChuanDaiZhuangBei(string sort ,string ClothesName) 
    {
        switch (sort)
        {
            case "全身":
                for (int i = 0; i < Body.childCount; i++) 
                {
                    Body.transform.GetChild(i).gameObject.SetActive(false);
                }
                Body.transform.Find(ClothesName).gameObject.SetActive(true);
                if (ClothesName=="防护服")
                {
                    HideObj(Head);
                    HideObj(Up);
                    HideObj(Down);
                    HideObj(Foot);
                }
                break;
            case "头部":
                for (int i = 0; i < Head.childCount; i++)
                {
                    Head.transform.GetChild(i).gameObject.SetActive(false);
                }
                Head.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
            case "面部":
                for (int i = 0; i < Face.childCount; i++)
                {
                    Face.transform.GetChild(i).gameObject.SetActive(false);
                }
                Face.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
            case "手部":
                for (int i = 0; i < Hand.childCount; i++)
                {
                    Hand.transform.GetChild(i).gameObject.SetActive(false);
                }
                Hand.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
            case "上身":
                for (int i = 0; i < Up.childCount; i++)
                {
                    Up.transform.GetChild(i).gameObject.SetActive(false);
                }
                Up.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
            case "脚部":
                for (int i = 0; i < Foot.childCount; i++)
                {
                    Foot.transform.GetChild(i).gameObject.SetActive(false);
                }
                Foot.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
            case "下身":
                for (int i = 0; i < Down.childCount; i++)
                {
                    Down.transform.GetChild(i).gameObject.SetActive(false);
                }
                Down.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
            case "眼部":
                for (int i = 0; i < Eye.childCount; i++)
                {
                    Eye.transform.GetChild(i).gameObject.SetActive(false);
                }
                Eye.transform.Find(ClothesName).gameObject.SetActive(true);
                break;
                
        }
    }

    public void HideObj(Transform Obj) 
    {
        for (int i = 0; i < Obj.childCount; i++) 
        {
            Obj.GetChild(i).gameObject.SetActive(false);
        }
    }
}

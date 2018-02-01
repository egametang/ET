using UnityEngine;
using UnityEngine.UI;

public class SharkTurn : MonoBehaviour
{
    //动物数组
    private Transform[] animals;
    //启动转盘
    public bool isStart;
    //目标索引
    public int targetIdx;
    //当前索引
    private int currentIdx;
    //转轮速度 值越大 速度越慢
    private float turnSpeed = 0.5f;
    //最大速度
    private float maxSpeed = 0.02f;
    //最小速度
    private float minSpeed = 0.5f;
    //减速度
    private float reduceSpeed = 0.1f;
    //转轮跟随
    private int turnFollow;
    //最大跟随数
    private int maxFollow = 3;
    //转轮时间
    private float workTime;
    //转轮圈数
    private int count;
    //最大圈数
    private int maxCount = 5;
    //加速度状态
    private bool isAddSpeed;
    //跟随颜色过度
    private float fade = 1;
    private void Start()
    {
        animals = new Transform[transform.childCount];
        for (int i = 0; i < animals.Length; i++)
        {
            animals[i] = transform.GetChild(i);
            animals[i].Find("FlashImg").gameObject.SetActive(false);
        }
        animals[currentIdx].Find("FlashImg").gameObject.SetActive(true);
        animals[currentIdx].GetComponent<Animator>().SetBool("Play",true);
    }

    private void Update()
    {
        workTime += Time.deltaTime;
        if (isStart && workTime >= turnSpeed)
        {
            animals[0].GetComponent<Animator>().enabled=false;
            workTime = 0;
            fade = 1;
            currentIdx = (++currentIdx) % 28;
            //启动加速
            if (!isAddSpeed && turnSpeed > maxSpeed)
            {
                turnSpeed -= reduceSpeed;
                if (turnSpeed < maxSpeed)
                {
                    turnSpeed = maxSpeed;
                }
            }
            //圈数统计
            if (currentIdx == targetIdx)
            {
                count++;
            }
            //设置速度状态
            if (count == maxCount && (currentIdx + 6) % 28 == targetIdx)
            {
                isAddSpeed = true;
            }
            //转轮恢复默认
            for (int j = 0; j < animals.Length; j++)
            {
              
                animals[j].Find("FlashImg").gameObject.SetActive(false);
            }

            if (!isAddSpeed && turnFollow < maxFollow)
            {
                turnFollow++;
            }
            //转轮跟随
            for (int i = 0; i <= turnFollow; i++)
            {
                animals[(currentIdx - i + 28) % 28].Find("FlashImg").gameObject.SetActive(true);
                animals[(currentIdx - i + 28) % 28].Find("FlashImg").GetComponent<Image>().color = new Color(1, 1, 1, fade);
                fade -= 0.2f;
            }
            //开奖前减速
            if (isAddSpeed && turnSpeed < minSpeed)
            {
                turnSpeed += reduceSpeed;
                if (turnSpeed > minSpeed)
                {
                    turnSpeed = minSpeed;
                }

                if (turnFollow > 0)
                {
                    turnFollow--;
                }
            }
            //参数重置
            if (isAddSpeed && currentIdx == targetIdx)
            {
                animals[currentIdx].GetComponent<Animator>().SetTrigger("Trigger" + currentIdx);
                animals[currentIdx].GetComponent<Animator>().SetTrigger("TriggerFlash");
                isAddSpeed = false;
                isStart = false;
                count = 0;
                turnSpeed = 0.5f;
            }
        }
    }
}

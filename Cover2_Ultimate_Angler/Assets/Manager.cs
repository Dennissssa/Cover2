using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FishingGame : MonoBehaviour
{
    public GameObject fishingRod; // 鱼竿对象
    public GameObject splashEffect; // 水花效果Prefab
    public Transform hookSpawnPoint; // 鱼钩发射点
    public GameObject hookPrefab; // 鱼钩的Prefab
    public AudioClip backgroundMusic; // 背景音乐
    public AudioClip splashSound; // 水花音效
    public AudioClip reelSound; // 拉鱼音效
    public float hookWaitMin = 2f;
    public float hookWaitMax = 4f;
    public int rotationCountMin = 5; // 最小转动圈数
    public int rotationCountMax = 10; // 最大转动圈数
    public Slider progressSlider; // 进度条Slider
    public Slider inputCountSlider; // 当前输入计数Slider
    public int maxInputCount = 10; // 最大滚轮输入数量限制
    public float inputDecayRate = 1f; // 输入减少的速率
    public float maxReelSpeed = 1f; // 拉鱼的最大速度

    private GameObject currentHook; // 保存当前钩子的实例
    private AudioSource audioSource; // AudioSource组件
    private bool fishCaught = false;
    private bool isReeling = false; // 用于管理拉鱼状态
    private float requiredProgress;
    private bool isCountingDown = true; // 用于管理计时器的状态
    private int currentInputCount = 0; // 当前滚轮输入计数
    private bool hookThrown = false; // 用于检查是否已投钩

    void Start()
    {
        // 获取AudioSource组件并播放背景音乐
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();

        requiredProgress = Random.Range(rotationCountMin, rotationCountMax);
        progressSlider.maxValue = requiredProgress;
        progressSlider.value = 0f;

        inputCountSlider.maxValue = maxInputCount; // 设置输入计数Slider的最大值
        inputCountSlider.value = 0f; // 初始化输入计数Slider

        StartCoroutine(InputCountMonitor()); // 启动输入计数监控
    }

    public void ThrowHook()
    {
        if (!hookThrown) // 检查是否已经投钩
        {
            hookThrown = true; // 设置为已投钩
            isCountingDown = true; // 开始计时
            StartCoroutine(ThrowHookCoroutine());
        }
    }

    private IEnumerator ThrowHookCoroutine()
    {
        // 创建钩子并保存引用
        currentHook = Instantiate(hookPrefab, hookSpawnPoint.position, Quaternion.identity);
        fishingRod.SetActive(true);

        float waitTime = Random.Range(hookWaitMin, hookWaitMax);
        yield return new WaitForSeconds(waitTime);

        GameObject splash = Instantiate(splashEffect, currentHook.transform.position, Quaternion.identity);
        splash.SetActive(true);
        audioSource.PlayOneShot(splashSound); // 播放水花音效

        float splashDuration = 0.5f;
        yield return new WaitForSeconds(splashDuration);

        Destroy(splash);

        // 检查计时器状态
        if (isCountingDown)
        {
            LoseGame();
        }
    }

    public void ReelFish()
    {
        if (!isReeling) // 只有在没有拉鱼时才允许调用
        {
            isCountingDown = false; // 停止计时器
            isReeling = true; // 设置为正在拉鱼状态
            StartCoroutine(PullUpFish());
        }
    }

    private IEnumerator PullUpFish()
    {
        float progress = 0f; // 初始化进度
        audioSource.Stop(); // 停止背景音乐
        audioSource.PlayOneShot(reelSound); // 播放拉鱼音效

        while (progress < requiredProgress)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (scrollInput < 0) // 滚轮向下滚动
            {
                // 增加进度
                progress += Time.deltaTime * maxReelSpeed;
                progressSlider.value = progress; // 更新进度条
                MoveHookTowardsRod(progress);
            }
            else if (scrollInput > 0) // 滚轮向上滚动
            {
                // 减少进度
                progress -= Time.deltaTime * maxReelSpeed * 0.5f;
                if (progress < 0)
                {
                    progress = 0; // 防止负值
                }
                progressSlider.value = progress; // 更新进度条
            }

            // 检查并更新输入计数
            if (scrollInput != 0)
            {
                currentInputCount++;
                inputCountSlider.value = currentInputCount; // 更新输入计数Slider
            }

            // 检查是否成功抓到鱼
            if (progress >= requiredProgress)
            {
                fishCaught = true;
                WinGame(); // 成功抓到鱼
                yield break;
            }

            yield return null; // 等待下一帧
        }

        if (!fishCaught)
        {
            LoseGame();
        }

        isReeling = false; // 重置为未拉鱼状态
    }

    private IEnumerator InputCountMonitor()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // 检查是否超过最大输入限制
            if (currentInputCount > maxInputCount)
            {
                LoseGame(); // 输入数量超过限制，游戏失败
                yield break; // 退出协程
            }

            // 持续减少输入计数
            currentInputCount = Mathf.Max(0, currentInputCount - (int)inputDecayRate); // 逐渐减少输入计数
        }
    }

    private void MoveHookTowardsRod(float progress)
    {
        float lerpFactor = Mathf.Clamp01(progress / requiredProgress);
        // 使用当前钩子的实例进行位置更新
        if (currentHook != null)
        {
            currentHook.transform.position = Vector3.Lerp(hookSpawnPoint.position, fishingRod.transform.position, lerpFactor);
        }
    }

    private void LoseGame()
    {
        // 在失败时进行必要的清理
        SceneManager.LoadScene("LoseScene");
    }

    private void WinGame()
    {
        // 在成功时进行必要的清理
        SceneManager.LoadScene("WinScene");
    }

    // 当按下第二个按钮时调用此方法
    public void StopCountingDown()
    {
        isCountingDown = false; // 停止计时
    }
}
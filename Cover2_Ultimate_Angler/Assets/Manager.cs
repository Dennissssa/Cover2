using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FishingGame : MonoBehaviour
{
    public GameObject fishingRod; // ��Ͷ���
    public GameObject splashEffect; // ˮ��Ч��Prefab
    public Transform hookSpawnPoint; // �㹳�����
    public GameObject hookPrefab; // �㹳��Prefab
    public AudioClip backgroundMusic; // ��������
    public AudioClip splashSound; // ˮ����Ч
    public AudioClip reelSound; // ������Ч
    public float hookWaitMin = 2f;
    public float hookWaitMax = 4f;
    public int rotationCountMin = 5; // ��Сת��Ȧ��
    public int rotationCountMax = 10; // ���ת��Ȧ��
    public Slider progressSlider; // ������Slider
    public Slider inputCountSlider; // ��ǰ�������Slider
    public int maxInputCount = 10; // ������������������
    public float inputDecayRate = 1f; // ������ٵ�����
    public float maxReelSpeed = 1f; // ���������ٶ�

    private GameObject currentHook; // ���浱ǰ���ӵ�ʵ��
    private AudioSource audioSource; // AudioSource���
    private bool fishCaught = false;
    private bool isReeling = false; // ���ڹ�������״̬
    private float requiredProgress;
    private bool isCountingDown = true; // ���ڹ����ʱ����״̬
    private int currentInputCount = 0; // ��ǰ�����������
    private bool hookThrown = false; // ���ڼ���Ƿ���Ͷ��

    void Start()
    {
        // ��ȡAudioSource��������ű�������
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();

        requiredProgress = Random.Range(rotationCountMin, rotationCountMax);
        progressSlider.maxValue = requiredProgress;
        progressSlider.value = 0f;

        inputCountSlider.maxValue = maxInputCount; // �����������Slider�����ֵ
        inputCountSlider.value = 0f; // ��ʼ���������Slider

        StartCoroutine(InputCountMonitor()); // ��������������
    }

    public void ThrowHook()
    {
        if (!hookThrown) // ����Ƿ��Ѿ�Ͷ��
        {
            hookThrown = true; // ����Ϊ��Ͷ��
            isCountingDown = true; // ��ʼ��ʱ
            StartCoroutine(ThrowHookCoroutine());
        }
    }

    private IEnumerator ThrowHookCoroutine()
    {
        // �������Ӳ���������
        currentHook = Instantiate(hookPrefab, hookSpawnPoint.position, Quaternion.identity);
        fishingRod.SetActive(true);

        float waitTime = Random.Range(hookWaitMin, hookWaitMax);
        yield return new WaitForSeconds(waitTime);

        GameObject splash = Instantiate(splashEffect, currentHook.transform.position, Quaternion.identity);
        splash.SetActive(true);
        audioSource.PlayOneShot(splashSound); // ����ˮ����Ч

        float splashDuration = 0.5f;
        yield return new WaitForSeconds(splashDuration);

        Destroy(splash);

        // ����ʱ��״̬
        if (isCountingDown)
        {
            LoseGame();
        }
    }

    public void ReelFish()
    {
        if (!isReeling) // ֻ����û������ʱ���������
        {
            isCountingDown = false; // ֹͣ��ʱ��
            isReeling = true; // ����Ϊ��������״̬
            StartCoroutine(PullUpFish());
        }
    }

    private IEnumerator PullUpFish()
    {
        float progress = 0f; // ��ʼ������
        audioSource.Stop(); // ֹͣ��������
        audioSource.PlayOneShot(reelSound); // ����������Ч

        while (progress < requiredProgress)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (scrollInput < 0) // �������¹���
            {
                // ���ӽ���
                progress += Time.deltaTime * maxReelSpeed;
                progressSlider.value = progress; // ���½�����
                MoveHookTowardsRod(progress);
            }
            else if (scrollInput > 0) // �������Ϲ���
            {
                // ���ٽ���
                progress -= Time.deltaTime * maxReelSpeed * 0.5f;
                if (progress < 0)
                {
                    progress = 0; // ��ֹ��ֵ
                }
                progressSlider.value = progress; // ���½�����
            }

            // ��鲢�����������
            if (scrollInput != 0)
            {
                currentInputCount++;
                inputCountSlider.value = currentInputCount; // �����������Slider
            }

            // ����Ƿ�ɹ�ץ����
            if (progress >= requiredProgress)
            {
                fishCaught = true;
                WinGame(); // �ɹ�ץ����
                yield break;
            }

            yield return null; // �ȴ���һ֡
        }

        if (!fishCaught)
        {
            LoseGame();
        }

        isReeling = false; // ����Ϊδ����״̬
    }

    private IEnumerator InputCountMonitor()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // ����Ƿ񳬹������������
            if (currentInputCount > maxInputCount)
            {
                LoseGame(); // ���������������ƣ���Ϸʧ��
                yield break; // �˳�Э��
            }

            // ���������������
            currentInputCount = Mathf.Max(0, currentInputCount - (int)inputDecayRate); // �𽥼����������
        }
    }

    private void MoveHookTowardsRod(float progress)
    {
        float lerpFactor = Mathf.Clamp01(progress / requiredProgress);
        // ʹ�õ�ǰ���ӵ�ʵ������λ�ø���
        if (currentHook != null)
        {
            currentHook.transform.position = Vector3.Lerp(hookSpawnPoint.position, fishingRod.transform.position, lerpFactor);
        }
    }

    private void LoseGame()
    {
        // ��ʧ��ʱ���б�Ҫ������
        SceneManager.LoadScene("LoseScene");
    }

    private void WinGame()
    {
        // �ڳɹ�ʱ���б�Ҫ������
        SceneManager.LoadScene("WinScene");
    }

    // �����µڶ�����ťʱ���ô˷���
    public void StopCountingDown()
    {
        isCountingDown = false; // ֹͣ��ʱ
    }
}
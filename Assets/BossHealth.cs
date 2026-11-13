using UnityEngine;
using UnityEngine.UI;

namespace Platformer.Mechanics
{
    public class BossHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 20;
        [SerializeField] private Slider healthSlider;

        [Header("Death Sprite")]
        public Sprite deadSprite;

        [Header("Ending Images")]
        public GameObject[] endingImages;
        [SerializeField] private float endingImageDelay = 1f;

        [Header("Death Effect")]
        [SerializeField] private float blinkDuration = 1f;
        [SerializeField] private float blinkInterval = 0.08f;
        [SerializeField] private float fadeDuration = 1f;

        private int currentHealth;
        private bool isDead;
        private bool isSpriteVisible = true;
        private int endingImageIndex;

        private SpriteRenderer spriteRenderer;
        private Collider2D bossCollider;

        /// <summary>
        /// 초기 설정 및 컴포넌트 참조를 가져오고 엔딩 이미지를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            bossCollider = GetComponent<Collider2D>();

            SetEndingImages(false);

            currentHealth = maxHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }

        /// <summary>
        /// 보스에게 피해를 입히고 체력 슬라이더를 갱신합니다. 체력이 0 이하가 되면 사망 처리합니다.
        /// </summary>
        /// <param name="damage">받는 피해량</param>
        public void TakeDamage(int damage)
        {
            if (isDead)
                return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);

            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            Debug.Log($"보스 피격! 현재 HP: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 보스의 사망 처리를 수행합니다. 콜라이더를 끄고 사망 스프라이트로 변경하며 사망 연출을 시작합니다.
        /// </summary>
        private void Die()
        {
            if (isDead)
                return;

            isDead = true;

            Debug.Log("보스 사망");

            if (bossCollider != null)
            {
                bossCollider.enabled = false;
            }

            if (deadSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = deadSprite;
            }

            StartDeathEffect();
        }

        /// <summary>
        /// 보스 사망 시 깜빡이는 연출(Blink Effect)을 시작합니다.
        /// </summary>
        private void StartDeathEffect()
        {
            Debug.Log("Invoke 방식 사망 연출 시작");

            InvokeRepeating(nameof(ToggleBossSprite), 0f, blinkInterval);
            Invoke(nameof(StopBlink), blinkDuration);
        }

        /// <summary>
        /// 스프라이트 렌더러를 켜고 꺼서 깜빡거리는 효과를 구현합니다.
        /// </summary>
        private void ToggleBossSprite()
        {
            if (spriteRenderer == null)
                return;

            isSpriteVisible = !isSpriteVisible;
            spriteRenderer.enabled = isSpriteVisible;
        }

        /// <summary>
        /// 깜빡임 연출을 정지하고, 페이드아웃 연출을 예약(호출)합니다.
        /// </summary>
        private void StopBlink()
        {
            Debug.Log("깜빡임 종료");

            CancelInvoke(nameof(ToggleBossSprite));

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            Invoke(nameof(FadeOutBoss), 0f);
        }

        /// <summary>
        /// 보스 이미지의 페이드아웃(점점 투명해짐) 연출을 시작합니다.
        /// </summary>
        private void FadeOutBoss()
        {
            if (spriteRenderer == null)
            {
                Invoke(nameof(DisableBossGameObject), 0f);
                return;
            }

            Debug.Log("페이드아웃 시작");

            float alphaStep = blinkInterval / fadeDuration;

            InvokeRepeating(nameof(ReduceBossAlpha), 0f, blinkInterval);
            Invoke(nameof(StopFadeOut), fadeDuration);
        }

        /// <summary>
        /// 보스의 알파(투명도) 값을 지속적으로 감소시킵니다.
        /// </summary>
        private void ReduceBossAlpha()
        {
            if (spriteRenderer == null)
                return;

            Color color = spriteRenderer.color;
            color.a -= blinkInterval / fadeDuration;
            color.a = Mathf.Clamp01(color.a);

            spriteRenderer.color = color;
        }

        /// <summary>
        /// 페이드아웃 연출을 정지하고 보스 오브젝트를 비활성화 단계로 넘깁니다.
        /// </summary>
        private void StopFadeOut()
        {
            Debug.Log("페이드아웃 종료");

            CancelInvoke(nameof(ReduceBossAlpha));

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0f;
                spriteRenderer.color = color;
            }

            DisableBossGameObject();
        }

        /// <summary>
        /// 보스 게임 오브젝트를 완전히 비활성화(SetActive(false))시키고 엔딩 이미지 표시를 예약합니다.
        /// </summary>
        private void DisableBossGameObject()
        {
            Debug.Log("보스 GameObject SetActive(false) 실행 직전");

            Invoke(nameof(TestInvokeAfterBossDisabled), 1f);

            gameObject.SetActive(false);

            Debug.Log("보스 GameObject SetActive(false) 실행 완료");

            Invoke(nameof(StartEndingImages), 0.5f);
        }

        /// <summary>
        /// 보스 오브젝트 비활성화 후에도 Invoke 메서드가 성공적으로 실행되는지 확인하는 테스트용 메서드입니다.
        /// </summary>
        private void TestInvokeAfterBossDisabled()
        {
            Debug.Log("테스트 성공: 보스가 꺼진 뒤에도 Invoke가 실행됨");
        }

        /// <summary>
        /// 엔딩 이미지들의 순차적 출력을 준비하고 시작합니다.
        /// </summary>
        private void StartEndingImages()
        {
            Debug.Log("엔딩 이미지 출력 시작");

            endingImageIndex = 0;
            ShowNextEndingImage();
        }

        /// <summary>
        /// 다음 엔딩 이미지를 활성화하여 화면에 표시합니다.
        /// </summary>
        private void ShowNextEndingImage()
        {
            if (endingImages == null || endingImages.Length == 0)
                return;

            if (endingImageIndex >= endingImages.Length)
                return;

            if (endingImages[endingImageIndex] != null)
            {
                endingImages[endingImageIndex].SetActive(true);
                Debug.Log($"엔딩 이미지 {endingImageIndex + 1}번 표시");
            }

            endingImageIndex++;

            if (endingImageIndex < endingImages.Length)
            {
                Invoke(nameof(ShowNextEndingImage), endingImageDelay);
            }
        }

        /// <summary>
        /// 엔딩 이미지 배열에 등록된 모든 오브젝트들의 활성화 상태를 한 번에 일괄 설정합니다.
        /// </summary>
        /// <param name="active">활성화 여부 (true/false)</param>
        private void SetEndingImages(bool active)
        {
            if (endingImages == null)
                return;

            for (int i = 0; i < endingImages.Length; i++)
            {
                if (endingImages[i] != null)
                {
                    endingImages[i].SetActive(active);
                }
            }
        }
    }
}
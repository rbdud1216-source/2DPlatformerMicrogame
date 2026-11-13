using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class PetController : MonoBehaviour
    {
        [Tooltip("따라갈 플레이어 오브젝트를 지정합니다.")]
        public PlayerController player;

        [Tooltip("플레이어를 따라갈 때의 지연 시간(초). 기본 0.1s")]
        public float delay = 0.1f;

        [Header("Pet Position Offset")]
        public Vector3 playerPositionOffset = new Vector3(-1f, 0f, 0f);

        [Header("Attack Offset")]
        [SerializeField] private float attackOffsetX = 10f;
        [SerializeField] private float moveOutDuration = 0.4f;
        [SerializeField] private float moveBackDuration = 0.4f;

        [Header("Attack Spin")]
        [SerializeField] private float spinSpeed = 1080f;

        [Header("Attack Damage")]
        [SerializeField] private int damage = 1;

        private Animator animator;
        private Collider2D attackCollider;
        private Coroutine attackCoroutine;
        private bool isAttacking;

        void Awake()
        {
            animator = GetComponent<Animator>();
            attackCollider = GetComponent<Collider2D>();

            attackCollider.isTrigger = true;
            attackCollider.enabled = false;

            if (player == null)
            {
                Debug.LogError("[PetController] Player 참조가 비어 있습니다. 인스펙터에서 할당해주세요.");
                enabled = false;
            }
        }

        void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Attack();
            }
        }

        void LateUpdate()
        {
            if (!enabled || player == null) return;
            if (player.stateHistory == null || player.stateHistory.Count == 0) return;

            float targetTime = Time.time - delay;

            PlayerState past = player.stateHistory.LastOrDefault(state => state.Timestamp <= targetTime);

            if (past.Timestamp > 0f)
            {
                ApplyState(past);
            }
            else
            {
                ApplyState(player.stateHistory[0]);
            }
        }

        private void ApplyState(PlayerState playerState)
        {
            transform.position = playerState.Position + playerPositionOffset;

            if (animator != null)
            {
                animator.SetFloat("velocityX", playerState.VelocityX);
                animator.SetBool("grounded", playerState.IsGrounded);
            }
        }

        public void Attack()
        {
            if (player == null)
                return;

            if (!player.controlEnabled)
            {
                Debug.Log("플레이어 조작 불가 상태라 펫 공격 불가");
                return;
            }

            if (player.health != null && !player.health.IsAlive)
            {
                Debug.Log("플레이어 사망 상태라 펫 공격 불가");
                return;
            }

            if (isAttacking)
            {
                Debug.Log("펫이 공격 중이라 클릭 무시");
                return;
            }

            attackCoroutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            attackCollider.enabled = true;

            Vector3 originalOffset = playerPositionOffset;
            Vector3 targetOffset = originalOffset + new Vector3(attackOffsetX, 0f, 0f);

            float totalDuration = moveOutDuration + moveBackDuration;
            Coroutine spinCoroutine = StartCoroutine(SpinRoutine(totalDuration));

            yield return MoveOffsetRoutine(originalOffset, targetOffset, moveOutDuration);
            yield return MoveOffsetRoutine(targetOffset, originalOffset, moveBackDuration);

            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
            }

            playerPositionOffset = originalOffset;
            transform.rotation = Quaternion.identity;
            attackCollider.enabled = false;

            isAttacking = false;
            attackCoroutine = null;

            Debug.Log("펫 공격 종료. 다시 클릭 가능");
        }

        private IEnumerator MoveOffsetRoutine(Vector3 startOffset, Vector3 endOffset, float duration)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;

                float t = Mathf.Clamp01(time / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                playerPositionOffset = Vector3.Lerp(startOffset, endOffset, smoothT);

                yield return null;
            }

            playerPositionOffset = endOffset;
        }

        private IEnumerator SpinRoutine(float duration)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;

                transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

                yield return null;
            }

            transform.rotation = Quaternion.identity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAttacking)
                return;

            BossHealth bossHealth = other.GetComponent<BossHealth>();

            if (bossHealth == null)
            {
                bossHealth = other.GetComponentInParent<BossHealth>();
            }

            if (bossHealth == null)
                return;

            bossHealth.TakeDamage(damage);

            attackCollider.enabled = false;

            Debug.Log("펫이 보스를 때림. 공격 콜라이더 비활성화");
        }
    }
}
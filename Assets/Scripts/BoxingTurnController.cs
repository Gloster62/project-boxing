using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class BoxingTurnController : MonoBehaviour
{
    [SerializeField] private HandView handView;
    [SerializeField] private CardViewCreator cardViewCreator;
    [SerializeField] private CardData[] handCards;
    [SerializeField] private GameObject enemyIntent;
    [SerializeField] private TMP_Text intentDamage;
    [SerializeField] private GameObject guardIndicator;
    [SerializeField] private TMP_Text guardAmount;
    [SerializeField] private TMP_Text turnStatus;
    [SerializeField] private GameObject matchResult;
    [SerializeField] private TMP_Text matchResultText;
    [SerializeField] private Image playerHealthFill;
    [SerializeField] private Image playerStaminaFill;
    [SerializeField] private Image enemyHealthFill;
    [SerializeField] private Image enemyStaminaFill;
    [SerializeField] private TMP_Text playerHealthLabel;
    [SerializeField] private TMP_Text playerStaminaLabel;
    [SerializeField] private TMP_Text enemyHealthLabel;
    [SerializeField] private TMP_Text enemyStaminaLabel;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField, Min(1)] private int maxHealth = 30;
    [SerializeField, Min(1)] private int maxStamina = 10;
    [SerializeField] private int[] enemyAttacks = { 4, 7, 10 };
    [SerializeField, Min(0.05f)] private float animationDelay = 0.2f;
    [SerializeField, Range(0.1f, 1f)] private float combatAnimationSpeed = 0.85f;

    private BoxingMatch match;
    private bool playingCard;
    private string defenseAnimation;

    private void Start()
    {
        match = new BoxingMatch(maxHealth, maxStamina, enemyAttacks);
        BeginPlayerTurn();
    }

    private void Update()
    {
        if (!playingCard && Keyboard.current?.spaceKey.wasPressedThisFrame == true && match.EndPlayerTurn())
            StartCoroutine(EnemyTurn());
    }

    private void BeginPlayerTurn()
    {
        defenseAnimation = null;
        RefreshGuard();
        var views = new List<CardView>(handCards.Length);
        foreach (var data in handCards)
        {
            var view = cardViewCreator.CreateCardView(new Card(data), transform.position, Quaternion.identity);
            view.Clicked += PlayCard;
            views.Add(view);
        }
        handView.AddCards(views);
        intentDamage.text = match.EnemyIntent.ToString();
        ShowPlayerStatus();
        RefreshBars();
    }

    private void PlayCard(CardView view)
    {
        if (playingCard || match.CurrentPhase != BoxingMatch.Phase.PlayerTurn) return;

        var card = view.Card;
        int previousEnemyHealth = match.EnemyHealth;
        if (!match.TryPlay(card.Effect, card.Amount, card.Stamina))
        {
            if (card.Stamina > match.Stamina)
                turnStatus.text = "Not enough stamina · Space to end turn";
            return;
        }

        playingCard = true;
        HidePreview();
        handView.SetCardsVisible(false);
        handView.RemoveCard(view);
        UpdateBar(playerStaminaFill, playerStaminaLabel, "STAMINA", match.Stamina, maxStamina);
        if (card.Effect != CardEffect.Attack)
        {
            defenseAnimation = card.AnimationState;
            RefreshGuard();
        }
        StartCoroutine(AnimateCard(card, previousEnemyHealth));
    }

    private IEnumerator AnimateCard(Card card, int previousEnemyHealth)
    {
        turnStatus.text = card.Title;
        yield return new WaitForSeconds(animationDelay);

        if (card.Effect == CardEffect.Attack)
        {
            float duration = PlayAnimation(playerAnimator, card.AnimationState);
            float windup = duration * 0.5f;
            if (windup > 0f) yield return new WaitForSeconds(windup);
            RefreshBars();
            var reaction = BoxingMatch.GetHitReaction(previousEnemyHealth, match.EnemyHealth, maxHealth);
            yield return AnimateImpact(playerAnimator, enemyAnimator, duration - windup, reaction);
        }
        playingCard = false;
        if (match.CurrentPhase == BoxingMatch.Phase.Won)
            FinishMatch("Victory");
        else
        {
            handView.SetCardsVisible(true);
            ShowPlayerStatus();
        }
    }

    private IEnumerator EnemyTurn()
    {
        HidePreview();
        handView.ClearCards();
        turnStatus.text = "Enemy turn";
        RefreshBars();
        yield return new WaitForSeconds(animationDelay);

        float duration = PlayAnimation(enemyAnimator, "jab");
        float windup = duration * 0.5f;
        yield return new WaitForSeconds(windup);

        bool defended = match.Guard > 0 && match.EnemyIntent > 0;
        int previousHealth = match.PlayerHealth;
        int damage = match.ResolveEnemyAttack();
        var reaction = BoxingMatch.GetHitReaction(previousHealth, match.PlayerHealth, maxHealth);
        RefreshGuard();
        RefreshBars();
        turnStatus.text = damage == 0 ? "Attack defended" : $"Enemy hit for {damage}";
        yield return AnimateImpact(enemyAnimator, playerAnimator, duration - windup, reaction,
            defended ? defenseAnimation : null);
        if (match.CurrentPhase == BoxingMatch.Phase.Lost)
        {
            FinishMatch("Defeat");
            yield break;
        }

        yield return new WaitForSeconds(0.35f);
        match.StartNextRound();
        BeginPlayerTurn();
    }

    private IEnumerator AnimateImpact(Animator attacker, Animator defender, float attackRemaining,
        BoxingMatch.HitReaction reaction, string defense = null)
    {
        string hitAnimation = reaction switch
        {
            BoxingMatch.HitReaction.Light => "damage1",
            BoxingMatch.HitReaction.Medium => "damage2",
            BoxingMatch.HitReaction.Heavy => "damage3",
            BoxingMatch.HitReaction.Knockdown => "knockdown",
            BoxingMatch.HitReaction.Knockout => "knockout",
            _ => null
        };

        float reactionDuration = PlayAnimation(defender, defense ?? hitAnimation);
        yield return new WaitForSeconds(Mathf.Max(attackRemaining, reactionDuration));
        PlayAnimation(attacker, "idle");
        if (defense != null && hitAnimation != null)
            yield return new WaitForSeconds(PlayAnimation(defender, hitAnimation));

        if (reaction == BoxingMatch.HitReaction.Knockout) yield break;
        if (reaction == BoxingMatch.HitReaction.Knockdown)
            yield return new WaitForSeconds(PlayAnimation(defender, "knockdownUp"));
        if (defense != null || hitAnimation != null)
            PlayAnimation(defender, "idle");
    }

    private void RefreshGuard()
    {
        guardAmount.text = match.Guard.ToString();
        guardIndicator.SetActive(match.Guard > 0);
    }

    private float PlayAnimation(Animator animator, string state)
    {
        if (!animator || !animator.runtimeAnimatorController || string.IsNullOrEmpty(state))
            return 0f;

        int stateHash = Animator.StringToHash("Base Layer." + state);
        if (!animator.HasState(0, stateHash)) return 0f;

        animator.speed = state == "idle" ? 1f : combatAnimationSpeed;
        animator.Play(stateHash, 0, 0f);
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == state)
                return clip.length / animator.speed;

        return 0f;
    }

    private void ShowPlayerStatus()
    {
        turnStatus.text = match.Guard > 0
            ? $"Guard: {match.Guard} · Space to end turn"
            : "Click a card · Space to end turn";
    }

    private void RefreshBars()
    {
        UpdateBar(playerHealthFill, playerHealthLabel, "HP", match.PlayerHealth, maxHealth);
        UpdateBar(enemyHealthFill, enemyHealthLabel, "HP", match.EnemyHealth, maxHealth);
        UpdateBar(playerStaminaFill, playerStaminaLabel, "STAMINA", match.Stamina, maxStamina);
        UpdateBar(enemyStaminaFill, enemyStaminaLabel, "STAMINA", match.EnemyStamina, maxStamina);
    }

    private static void UpdateBar(Image fill, TMP_Text label, string name, int value, int maximum)
    {
        fill.fillAmount = (float)value / maximum;
        label.text = $"{name}  {value} / {maximum}";
    }

    private void FinishMatch(string message)
    {
        HidePreview();
        handView.ClearCards();
        enemyIntent.SetActive(false);
        guardIndicator.SetActive(false);
        turnStatus.gameObject.SetActive(false);
        matchResultText.text = message.ToUpperInvariant();
        matchResultText.color = message == "Victory"
            ? new Color(1f, 0.82f, 0.51f)
            : new Color(0.92f, 0.38f, 0.34f);
        matchResult.SetActive(true);
    }

    private static void HidePreview()
    {
        if (CardViewHoverSystem.Instance != null)
            CardViewHoverSystem.Instance.Hide();
    }
}

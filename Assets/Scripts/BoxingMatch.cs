using System;

public sealed class BoxingMatch
{
    public enum Phase
    {
        PlayerTurn,
        EnemyTurn,
        RoundEnd,
        Won,
        Lost
    }

    public enum HitReaction
    {
        None,
        Light,
        Medium,
        Heavy,
        Knockdown,
        Knockout
    }

    private readonly int maxStamina;
    private readonly int[] enemyAttacks;
    private int attackIndex;

    public Phase CurrentPhase { get; private set; }
    public int PlayerHealth { get; private set; }
    public int EnemyHealth { get; private set; }
    public int Stamina { get; private set; }
    public int EnemyStamina { get; private set; }
    public int Guard { get; private set; }
    public int EnemyIntent => enemyAttacks[attackIndex];

    public BoxingMatch(int maxHealth, int maxStamina, int[] enemyAttacks)
    {
        if (maxHealth < 1 || maxStamina < 1 || enemyAttacks == null || enemyAttacks.Length == 0)
            throw new ArgumentException("A match needs health, stamina and at least one enemy attack.");

        PlayerHealth = EnemyHealth = maxHealth;
        Stamina = EnemyStamina = this.maxStamina = maxStamina;
        this.enemyAttacks = (int[])enemyAttacks.Clone();
        for (int i = 0; i < this.enemyAttacks.Length; i++)
            this.enemyAttacks[i] = Math.Max(0, this.enemyAttacks[i]);
        CurrentPhase = Phase.PlayerTurn;
    }

    public bool TryPlay(CardEffect effect, int amount, int cost)
    {
        if (CurrentPhase != Phase.PlayerTurn || cost < 0 || cost > Stamina || amount < 1)
            return false;

        switch (effect)
        {
            case CardEffect.Attack:
                EnemyHealth = Math.Max(0, EnemyHealth - amount);
                if (EnemyHealth == 0) CurrentPhase = Phase.Won;
                break;
            case CardEffect.Block:
            case CardEffect.Dodge:
                Guard += amount;
                break;
            default:
                return false;
        }

        Stamina -= cost;
        return true;
    }

    public bool EndPlayerTurn()
    {
        if (CurrentPhase != Phase.PlayerTurn) return false;
        CurrentPhase = Phase.EnemyTurn;
        EnemyStamina = maxStamina;
        return true;
    }

    public int ResolveEnemyAttack()
    {
        if (CurrentPhase != Phase.EnemyTurn) return 0;

        EnemyStamina = Math.Max(0, EnemyStamina - EnemyIntent);
        int damage = Math.Min(PlayerHealth, Math.Max(0, EnemyIntent - Guard));
        PlayerHealth -= damage;
        Guard = 0;
        CurrentPhase = PlayerHealth == 0 ? Phase.Lost : Phase.RoundEnd;
        return damage;
    }

    public void StartNextRound()
    {
        if (CurrentPhase != Phase.RoundEnd) return;
        attackIndex = (attackIndex + 1) % enemyAttacks.Length;
        Stamina = maxStamina;
        CurrentPhase = Phase.PlayerTurn;
    }

    public static HitReaction GetHitReaction(int previousHealth, int health, int maxHealth)
    {
        if (health >= previousHealth) return HitReaction.None;
        if (health <= 0) return HitReaction.Knockout;

        float ratio = (float)health / maxHealth;
        if (ratio <= 0.25f && previousHealth > maxHealth * 0.25f) return HitReaction.Knockdown;
        if (ratio <= 1f / 3f) return HitReaction.Heavy;
        return ratio <= 2f / 3f ? HitReaction.Medium : HitReaction.Light;
    }
}
